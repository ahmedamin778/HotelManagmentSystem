# Rate Limiting — Part 2

## Selected endpoints

### Sensitive action: `POST /api/Bookings`

Creating a booking checks room availability, calculates the price, writes to the database, and may send an email notification. Repeated requests can create duplicate demand and unnecessary database and email work.

The endpoint uses a per-client token bucket with these settings:

- Token limit: 3
- Refill: 1 token every minute
- Queue limit: 0

The short burst of three tokens allows normal retries or quick user interactions. The slow refill prevents sustained booking spam. A queue is not useful for booking creation because a delayed write request may no longer match the user's intent or current room availability.

### Expensive operation: `GET /api/Reports/dashboard`

The dashboard reads the room and booking collections and calculates several totals. It uses a per-client concurrency limiter with:

- Permit limit: 1
- Queue limit: 0

Only one dashboard calculation from a client can run at a time. A second concurrent request is rejected immediately instead of duplicating the same database and aggregation work.

## Why the algorithms are different

Token bucket controls how frequently booking requests may be started over time while still permitting a small legitimate burst. The concurrency limiter controls how many dashboard calculations are running at the same moment. Using token bucket for the dashboard would not directly prevent overlapping expensive work, while using only concurrency limiting for booking creation would allow unlimited sequential booking attempts.

## Unrestricted endpoint: `GET /health`

The basic health endpoint remains unrestricted. It is lightweight, read-only, and used by monitoring or orchestration systems that may call it frequently. Restricting it could produce false outage signals when the application itself is healthy.

## Client identification

Both limited endpoints are authenticated. The partition key is the authenticated user's `NameIdentifier` claim, which contains the user ID. IP address is used only as a fallback if no user ID is available. This keeps different authenticated users behind the same network from consuming each other's limits.

## Middleware order

The middleware order is:

1. Authentication
2. Rate limiting
3. Authorization
4. Controller execution

Authentication must run first so the limiter can read the user ID claim. Rate limiting runs before authorization and controller execution, so rejected requests do not enter the controller, application service, repository, or database.

## Work performed before rejection

ASP.NET Core performs routing and authentication before the rate limiter. This is necessary to identify the endpoint policy and authenticated client. A rejected request does not perform model validation, controller work, business logic, database queries, database writes, report aggregation, or email sending.

## Why these limits

Three booking attempts followed by one additional attempt per minute is intentionally small because booking creation changes business data and can trigger notification work. One concurrent dashboard operation is enough for a single user because overlapping dashboard requests calculate the same type of report; the next request can be made immediately after the first completes.

## Rejected requests

Rejected requests return HTTP 429 with a small JSON response. `Retry-After` is added only when the selected limiter supplies valid retry metadata. Each rejection is logged with its UTC timestamp, client identifier, endpoint path, HTTP method, and HTTP status code.
