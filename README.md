# Portal

This is a minimal Blazor Server sample that demonstrates how to register and use `ProtectedSessionStorage` from the `Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage` package.

## Prerequisites

Install the [.NET 8.0 SDK](https://dotnet.microsoft.com/download) so the project can be restored and built locally.

## Building

```bash
dotnet restore
dotnet build
```

## Running

```bash
dotnet run
```

Then navigate to `https://localhost:5001` (or the URL printed by the command) and experiment with the buttons on the home page to save, load, and clear values stored in protected session storage.
