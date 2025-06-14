# RaffleApp

This repository contains a minimal raffle application built with .NET. It is organised as a Visual Studio solution with a few projects:

- **RaffleApi** – the ASP.NET web API that exposes raffle endpoints.
- **RaffleDraw** – the library with the raffle domain and business logic.
- **TheTests** – xUnit tests for the RaffleDraw domain.

## Building the solution

Use the .NET SDK (version 9 or later) to restore and build all projects:

```bash
# restore packages and compile
 dotnet build Lalaland.sln
```

## Running the API

To launch the web API during development run:

```bash
# start RaffleApi
 dotnet run --project RaffleApi
```

The API listens on the default ASP.NET ports and provides Swagger UI for exploration.

## API Endpoints

The following endpoints are available:

- `POST /raffles` - create a raffle.
- `GET /raffles/{id}` - retrieve a raffle by id.
- `POST /raffles/{id}/tickets` - purchase a ticket.
- `POST /raffles/{id}/select-winner` - select a winning ticket and return the ticket number.

## Running tests

Unit tests are found in the `TheTests` project and can be executed with:

```bash
 dotnet test TheTests/TheTests.csproj
```


