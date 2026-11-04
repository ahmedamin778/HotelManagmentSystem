# 🏨 Hotel Management System API

A hotel booking backend built with **ASP.NET Core (.NET 10)** on a 4-layer **Clean Architecture**.

## ✨ Features
- Rooms, amenities and bookings, with a booking workflow (confirmation and check-in).
- **JWT** authentication and authorization.
- **Concurrency control** to prevent double-booking.
- **Rate limiting**, **health checks** and **API versioning** (v1 / v2).
- **Docker Compose** with a SQL Server volume.

## 🏗 Architecture
```
HotelBookingSystem.API             → Controllers / features
HotelBookingSystem.Application     → Use cases
HotelBookingSystem.Core            → Entities, interfaces
HotelBookingSystem.Infrastructure  → EF Core, SQL Server, repositories
```

## 🛠 Tech Stack
ASP.NET Core · EF Core · SQL Server · JWT · Swagger · Docker

---

<!-- The existing notes (API versioning, Docker Compose, rate limiting) continue below -->
# Hotel Booking System

## API Versioning — Amenities

### 1. Which endpoint did you choose and what changed?

The selected endpoint is the paginated amenities list because it is a small, existing read endpoint with a clear SQL Server source and an existing update operation.

- V1: `GET /api/v1/amenities` keeps the original response shape. It returns `items`, `pageNumber`, `pageSize`, `totalCount`, and `totalPages`. Each item contains `id` and `name`.
- V2: `GET /api/v2/amenities` returns the same amenity records in a clearer envelope. It contains `apiVersion`, `amenities`, and a nested `pagination` object.

Both versions call the existing `GetAmenitiesHandler`, which uses `IAmenityRepository` and `AmenityRepository` to read the same `Amenities` table in the same SQL Server database. V2 changes only the response shape; it does not use a new table or a separate copy of the data.

### 2. Why not just change the existing endpoint?

Changing the existing JSON response directly would be a breaking change. A frontend, mobile application, admin dashboard, external client, or another service may deserialize the current property names and structure. Changing that structure could make those clients fail even though the underlying data is still valid.

Keeping separate versions allows existing clients to continue using V1 while new clients adopt V2:

```text
V1 -> Existing clients
V2 -> New clients
```

This allows the API response to evolve without breaking clients that depend on the original contract.

## Docker Compose and SQL Server Volume

### Setup

The real API and a SQL Server 2022 instance are defined in `docker-compose.yml`. The API uses a multi-stage .NET 10 build from `Dockerfile`. Its connection string is supplied through the `ConnectionStrings__DefaultConnection` environment variable and uses the Compose service name `database`, not `localhost`.

The two existing endpoints selected for the evidence tests were:

- Write: `POST /api/Amenities` (requires an Admin or Staff bearer token).
- Read: `GET /api/v1/amenities`.

Run the application with:

```powershell
docker compose up --build -d
```

The API is available at `http://localhost:8081` and SQL Server is exposed at `localhost,14330`. SQL data is stored in the named volume `hotelmanagmentsystem_hotelbooking-sql-data`, mounted at SQL Server's `/var/opt/mssql` data directory.

### Startup timing and retry (Gotcha 1)

`depends_on` starts the database container before the API container, but it does not guarantee that SQL Server is ready to accept connections. Before the retry loop, a controlled start with SQL unavailable made the API terminate with:

```text
Database migration or seed failed.
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
Error Number:11001,State:0,Class:20
Unhandled exception.
```

On an ordinary fresh run after downloading/building the images, the timing problem did not occur because SQL Server became ready before the API attempted its connection. To reproduce the race reliably, SQL Server was stopped while the API started.

The final startup code tries migration/seeding up to 10 times, waiting 3 seconds between attempts. In the controlled delayed-database test, attempts 1 through 6 failed, then the API started after SQL Server became available. The log contained `Database is not ready. Attempt 6/10. Retrying in 3 seconds.` followed by `Application started.`

### Persistence evidence (Gotcha 2)

1. Logged in through `POST /api/Auth/login` with the seeded Admin account.
2. Created `Docker Volume Proof 20260908` through `POST /api/Amenities`; the response was `{"id":4,"name":"Docker Volume Proof 20260908"}`.
3. Confirmed it appeared in `GET /api/v1/amenities` and the total count was 4.
4. Ran `docker compose down`, which removed both containers but did not remove the named volume.
5. Ran `docker compose up -d` and called the read endpoint again.
6. The same record was still present and the total count was still 4.

The configuration alone would not prove that the internal path and volume name were correct; this write/down/up/read test supplied that evidence.

### Credential mismatch (Gotcha 3)

A temporary API container was deliberately given the wrong SQL password while the database kept the correct password. It failed with the exact SQL error:

```text
Microsoft.Data.SqlClient.SqlException (0x80131904): Login failed for user 'sa'.
Error Number:18456,State:1,Class:14
```

`Login failed for user 'sa'` and SQL error 18456 identify an authentication/credential mismatch. No bad credential was left in the Compose file. Starting the normal API with the shared password succeeded and the amenities endpoint returned HTTP 200.

### Port conflict (Gotcha 4)

Host port 8080 was already occupied by the existing `legalai-api` container. A deliberate test produced:

```text
Bind for 0.0.0.0:8080 failed: port is already allocated
```

The Hotel Booking API therefore uses host port 8081 (`8081:8080`). SQL Server uses host port 14330 to avoid the commonly occupied 1433 while still listening on 1433 inside the Compose network.

### Duplicate volume target (Gotcha 5)

A temporary test declared two differently named volumes for the same `/var/opt/mssql` target. Compose 5.4.0 did not reject it. `docker compose config` silently retained only the second mount and omitted the first. This is a quiet and dangerous result: a Compose file can be syntactically accepted while using a different volume from the one a developer expected. The temporary test definition and volume were removed afterward.

### Direct volume removal (Gotcha 6)

After the persistence test and backup, the exact volume was removed directly:

```powershell
docker volume rm hotelmanagmentsystem_hotelbooking-sql-data
```

The next `docker compose up -d` created a new empty volume, migrations/seeding ran again, and the custom record was gone. The read endpoint returned only the three seeded amenities. This differs from `docker compose down`: normal `down` removes containers and networks but keeps a named volume, whereas `docker volume rm` deletes the stored database files themselves.

### API scaling with a fixed host port (Gotcha 7)

The command used was:

```powershell
docker compose up -d --scale api=2
```

The second API container could not start and Docker returned:

```text
Bind for 0.0.0.0:8081 failed: port is already allocated
```

Only one container can own a specific host IP/port pair. Both replicas may listen on container port 8080, but both cannot publish it as host port 8081. Without a fixed published host port, or with a reverse proxy/load balancer assigning traffic, multiple replicas would not compete for the same host binding.

### External volume backup (Gotcha 8)

With the database stopped for a consistent file copy, a temporary Alpine container mounted the existing SQL volume read-only at `/source` and a normal host directory at `/backup`, then copied the files:

```powershell
docker run --rm `
  -v hotelmanagmentsystem_hotelbooking-sql-data:/source:ro `
  -v "<host-backup-path>:/backup" `
  alpine:3.22 sh -c "cp -a /source/. /backup/"
```

The host backup contained 48 files totaling 159,866,017 bytes, including readable filesystem entries `HotelBookingSystemDb.mdf` and `HotelBookingSystemDb_log.ldf`. The verified backup location was:

```text
C:\Users\Ahmed Amin\.codex\visualizations\2026\09\01\01a05f4b-c4f5-7061-af6c-476ca7c76146\hotelbooking-volume-backup-20260908
```

This needs no Compose command because a named volume is a Docker resource that any temporary container can mount by its exact name.

### Reflection

1. The retry strategy is 10 attempts with a fixed 3-second delay. A normal cold run needed no outer retry, while the deliberately delayed database test logged six failed attempts before succeeding.
2. Persistence was proven by creating a real amenity, removing the containers with `docker compose down`, recreating them, and reading the same amenity back. Without this test there would be no reliable proof that the volume target was correct.
3. The mismatch error was `Login failed for user 'sa'` with SQL error number 18456. Those two details point directly to credentials rather than networking or a missing database.
4. Duplicate mounts to one target failed quietly in this Compose version: the second replaced the first in the resolved configuration. This makes reviewing `docker compose config` important.
5. In Gotcha 2 only containers were removed, so the named volume survived. In Gotcha 6 the named volume itself was removed, so its database files and the custom record were lost.
6. Scaling used `docker compose up -d --scale api=2`. It failed because both replicas attempted to bind host port 8081. Container ports can repeat in separate network namespaces; one fixed host port cannot.
7. The backup used plain `docker run --rm` with the existing named volume mounted read-only plus a host bind mount. Compose originally created the resource, but Docker can address it independently by name.
8. Keeping one database container forever is fragile: containers are routinely recreated during image rebuilds, configuration changes, upgrades, or recovery. A volume separates durable database state from the disposable container lifecycle.
9. Credential mismatch is the most realistic accidental live-project mistake here because connection strings and database secrets are often supplied in separate environment/configuration locations. Its SQL 18456 signature is worth recognizing quickly.

## Configuration and Secrets

The real API now keeps safe development defaults in `HotelBookingSystem.API/appsettings.json` and receives local Docker values from the ignored `.env` file. Compose maps nested .NET configuration keys with double underscores:

```yaml
ConnectionStrings__DefaultConnection: "..."
EmailSettings__Host: "${EMAIL_HOST}"
EmailSettings__Port: "${EMAIL_PORT}"
Diagnostics__EnvironmentLabel: "${DIAGNOSTICS_LABEL}"
```

`GET /api/configuration/diagnostic` exposes only non-secret evidence: the environment label, database server/name, and email host/port. It never returns a database password, JWT key, email password, or email username.

### Values selected

- `ConnectionStrings:DefaultConnection` selects the SQL Server instance and database used by the real EF Core data access.
- `EmailSettings:Host` and `EmailSettings:Port` configure the existing email sender.
- `Diagnostics:EnvironmentLabel` is a simple label that makes the active configuration source obvious during this exercise.

JWT and email credentials are also supplied from `.env`, although the diagnostic endpoint deliberately does not reveal them.

### Actual test results

Running the built API directly, outside Docker, used the appsettings email and label defaults. Only the connection string was supplied as an environment variable so this machine could use the already-running test SQL Server instead of its unavailable LocalDB instance:

```json
{
  "environmentLabel": "appsettings.json",
  "database": { "server": "localhost,14330", "name": "HotelBookingSystemDb" },
  "email": { "host": "smtp.example.com", "port": 25 }
}
```

Running through Compose used values substituted from `.env`:

```json
{
  "environmentLabel": "docker-compose (.env)",
  "database": { "server": "database,1433", "name": "HotelBookingSystemDb" },
  "email": { "host": "smtp.gmail.com", "port": 587 }
}
```

#### Gotcha 1: one underscore

`EmailSettings__Host` was temporarily changed to `EmailSettings_Host`. The API still started and returned HTTP 200, but the diagnostic response showed `smtp.example.com`, the appsettings fallback, instead of the `.env` value. There was no configuration error because `EmailSettings_Host` is simply a different, unused key. Restoring the double underscore changed the response back to `smtp.gmail.com`.

#### Gotcha 2: committed secrets

Before this task, appsettings contained a real Gmail address and app password. Committing credentials is unsafe because removing them in a later commit does not remove them from Git history or from existing clones. They were replaced with empty/example defaults and moved to the ignored `.env`. The exposed Gmail app password must also be rotated by its owner because moving it cannot make the old value secret again.

#### Gotcha 3: value type mismatch

`EMAIL_PORT` was temporarily changed from `587` to `True`. Compose accepted the text, but reading `EmailSettings:Port` as `Int32` failed. The endpoint returned HTTP 500 with:

```text
Failed to convert configuration value 'True' at 'EmailSettings:Port' to type 'System.Int32'.
```

.NET therefore did not convert this unexpected value gracefully; binding failed at the point where the typed value was requested. The final `.env` value was restored to `587` and the endpoint returned HTTP 200 again.

#### Gotcha 4: ignore verification

The actual verification command was:

```powershell
git check-ignore -v .env
```

It returned `.gitignore:4:.env .env`, and `.env` did not appear in `git status --short`. This confirms Git treats the real file as ignored. `.env.example` remains trackable because it contains placeholders only.

### New teammate setup

1. Copy `.env.example` to a new file named `.env` in the solution root.
2. Replace every placeholder with that teammate's local SQL, JWT, and email values. Do not share or commit this file.
3. Run `docker compose up --build` from the solution root.
4. Open `http://localhost:8081/api/configuration/diagnostic` and verify the Docker label and expected non-secret values.

### Reflection

The single-underscore mistake is the hardest gotcha to notice in a team: the application starts normally and silently uses a plausible fallback value, so the defect may appear only when behavior differs between environments. .NET cannot assume an unknown environment variable is a typo because a process may legitimately contain variables intended for another library or application. Separating safe defaults from ignored local secrets lets each client or environment provide its own credentials without placing them in repository history.
