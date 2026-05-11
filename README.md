# SentinelPulse — Law Enforcement Operations Portal

A full-stack ASP.NET Core MVC (.NET 9) web application for police station management with role-based access, real-time crime mapping, and database-connected operations.

## Tech Stack
- ASP.NET Core MVC (.NET 9) + C# 12
- Entity Framework Core + SQL Server (LocalDB)
- Razor Views + Bootstrap 5 + custom dark theme
- Chart.js for crime analytics
- Leaflet.js + OpenStreetMap for crime mapping
- Nominatim API for geocoding

## Features
- Role-based login (DSP Admin / Officers)
- Dual dashboards (DSP Command Dashboard / Officer Dashboard)
- 3-step FIR registration wizard with map-based location marking
- Auto priority assignment based on crime type and urgency analysis
- CNIC duplicate check on FIR filing
- Case management with status flow: Open > Under Investigation > Pending Approval > Closed
- Officer can update case status and add investigation notes
- DSP approves/rejects case closure
- Evidence locker - attach evidence to cases
- Suspect management per case
- Interactive crime map with live pins (Leaflet.js + OpenStreetMap)
- Officer management panel (DSP only)
- Print-ready FIR documents
- Emergency alert broadcast system
- Dark/light theme toggle
- Live ticking clock
- Mobile responsive (sidebar collapses < 768px)

## API Integrations
- OpenStreetMap Tile API - map rendering
- Nominatim Geocoding API - location to coordinates
- Leaflet.js - interactive map with click-to-mark

## Login Credentials
| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | DSP (Admin) |
| SP-1042 | kamran123 | ASI Officer |
| SP-1187 | hina123 | ASI Officer |
| SP-2231 | bilal123 | Constable |
| SP-2305 | zara123 | Constable |

## Setup
1. Clone the repo
2. Open in Visual Studio 2022
3. Update connection string in appsettings.json if needed
4. Run dotnet ef database update in Package Manager Console
5. Press F5 to run 
