FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HotelBookingSystem.slnx ./
COPY HotelBookingSystem.Core/HotelBookingSystem.Core.csproj HotelBookingSystem.Core/
COPY HotelBookingSystem.Application/HotelBookingSystem.Application.csproj HotelBookingSystem.Application/
COPY HotelBookingSystem.Infrastructure/HotelBookingSystem.Infrastructure.csproj HotelBookingSystem.Infrastructure/
COPY HotelBookingSystem.API/HotelBookingSystem.API.csproj HotelBookingSystem.API/

RUN dotnet restore HotelBookingSystem.API/HotelBookingSystem.API.csproj

COPY . .
RUN dotnet publish HotelBookingSystem.API/HotelBookingSystem.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelBookingSystem.API.dll"]
