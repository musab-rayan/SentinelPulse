# SentinelPulse — Law Enforcement Operations Portal

ASP.NET Core MVC (.NET 9) frontend-only application with in-memory mock data.

## Run

```bash
dotnet run
```

Or open `SentinelPulse.csproj` in Visual Studio 2022/2026 and press F5.

## Demo Credentials

| Role    | Username  | Password    |
|---------|-----------|-------------|
| Admin   | admin     | admin123    |
| Officer | officer   | officer123  |

## Stack

- ASP.NET Core MVC (.NET 9)
- Razor Views + Bootstrap 5 + custom warm dark theme
- Chart.js for crime distribution
- Session-based auth (in-memory, demo only)

## Features

- Live ticking clock + dark/light theme toggle
- Glassmorphism cards with amber glow
- 3-step FIR registration wizard with print-ready output
- Searchable/filterable case registry with role-aware delete
- Case status timeline stepper
- Emergency alert broadcast banner
- Mobile responsive (sidebar collapses < 768px)

No database. All data is hardcoded in `Data/MockData.cs`.
