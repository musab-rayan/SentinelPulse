# SentinelPulse
Law Enforcement Operations Portal

SentinelPulse is a full-stack ASP.NET Core MVC web application developed for police station management. It provides role-based access control, automated FIR prioritization, and real-time interactive crime mapping to streamline case handling. The system allows officers and administrators to track cases from initial reporting to final closure, managing suspects, evidence, and emergency alerts.

## Tech Stack
- **Framework**: ASP.NET Core MVC (.NET 9.0)
- **Database**: Entity Framework Core (v9.0.0), SQL Server (LocalDB)
- **Frontend**: Razor Views, Bootstrap 5.3, Bootstrap Icons, Custom CSS (neuomorphic UI, Light/Dark Theme toggle)
- **Typography**: Geist and Geist Mono fonts
- **Mapping & Geocoding**: Leaflet.js, OpenStreetMap, Nominatim API
- **Analytics**: Chart.js

## Features
- **Dual Dashboards**: Dedicated views for DSP (Admin) and Officers. Admin view includes station-wide statistics, active cases, and a global crime distribution chart. Officer view filters active tasks to their assigned caseload.
- **FIR Registration (3-Step Wizard)**: A multi-step form to collect citizen details, crime details, and location coordinates. Includes a built-in validation check for duplicate CNIC entries.
- **Auto-Prioritization System**: Uses keyword analysis on the incident description to automatically assign an urgency label (Critical, Concerning, Standard, Routine) and priority level (High, Medium, Low) to incoming FIRs.
- **Interactive Crime Map**: Visualizes cases using Leaflet.js. Officers can pinpoint incident locations via click-to-mark during FIR creation, with Nominatim API serving as a fallback geocoder. The map displays standard cases and high-priority Zainab Alerts using distinct marker colors.
- **Case Management Lifecycle**: Cases transition through defined statuses (Open, Under Investigation, Pending Approval, Closed). Officers maintain an investigation log with timestamped notes.
- **Zainab Alert System**: A dedicated emergency broadcast module for missing children. Submitting an alert automatically assigns the case to the two officers with the lowest active caseloads and triggers a UI emergency banner.
- **Evidence & Suspect Logging**: Allows attaching suspect profiles and evidence records to specific cases.

## Screenshots
<img width="1920" height="855" alt="image" src="https://github.com/user-attachments/assets/71f25fdb-ceca-42f9-a5a0-e67c52ea61d0" />
<img width="1920" height="867" alt="image" src="https://github.com/user-attachments/assets/0a21a603-4626-4931-aff1-82b76c65f663" />
<img width="1920" height="860" alt="image" src="https://github.com/user-attachments/assets/8761c06b-aa31-44ec-b744-3ca9cca0d387" />
<img width="1920" height="864" alt="image" src="https://github.com/user-attachments/assets/d24ce802-1e43-4935-a616-1abffb8b1d16" />
<img width="1920" height="859" alt="image" src="https://github.com/user-attachments/assets/934f4b4a-a2ed-464a-89ec-8296c1fcf6d3" />
<img width="1920" height="864" alt="image" src="https://github.com/user-attachments/assets/55e54214-0d33-44a3-af76-4a263df7231e" />


## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server LocalDB (included with Visual Studio workloads)
- Visual Studio 2022 (recommended)

### Installation
1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd SentinelPulse/SentinelPulse
   ```
2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```
3. **Apply EF Core Migrations:**
   The application uses LocalDB configured in `appsettings.json`. Create the database by running:
   ```bash
   dotnet ef database update
   ```
   *(Alternatively, run `Update-Database` in the Visual Studio Package Manager Console).*

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   The application will launch on the ports defined in `launchSettings.json`:
   - `https://localhost:5001`
   - `http://localhost:5000`

## Roles & Authorization
Authorization is enforced manually via `HttpContext.Session` checks across controllers.
- **DSP (Admin)**: Granted full station visibility. Can view all cases, access the global crime map, update officer account statuses, transfer cases between personnel, and exclusively approve or reject case closure requests.
- **Officer**: Granted restricted visibility. Can only view and update cases directly assigned to them. They cannot close a case directly; they must submit a "Pending Approval" closure request to the DSP.

## Login
Authentication is handled by the `AccountController`. The login action queries the `Officers` DbSet in `AppDbContext`, comparing the submitted badge number and plain-text password against the database records. Upon a successful match, the system stores the `OfficerName`, `OfficerBadge`, and `OfficerRole` in the session state.

**Demo Credentials:**
| Role | Username (Badge Number) | Password |
|---|---|---|
| DSP (Admin) | `admin` | `admin123` |
| Officer | `SP-1042` | `kamran123` |



