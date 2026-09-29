# Smart Solar Microgrid Trading System (SE4040 Assignment)

**Solo submission.** All group and individual work in this repository (the API, web application,
Android application, diagrams and report) is 100% my own (Dilsara).

- Repository: https://github.com/Dil123852/SolarGrid
- Walkthrough video: _add link_

## Structure

| Folder | Contents |
|---|---|
| `api/` | ASP.NET Core Web API (.NET 10), clean architecture, MongoDB Atlas, hosted on IIS |
| `web/` | Backoffice / Grid Operator web app: static HTML, Bootstrap 5, vanilla JS. It is a UI layer only and calls the API |
| `android/` | Native Android app (Kotlin, Jetpack Compose): Retrofit, SQLite, Google Maps, ZXing QR |
| `docs/` | Architecture, use case, DFD and database design diagrams |

## Architecture

```
Web app (browser) ──┐
                    ├── HTTP/JSON + JWT ──▶ SolarGrid API (IIS) ──▶ MongoDB Atlas
Android app ────────┘
```

The API follows clean architecture, and dependencies only point inward:

| Project | Role | Depends on |
|---|---|---|
| `SolarGrid.Domain` | Entities, enums, `ReservationPolicy` business rules. Pure C#, no MongoDB | – |
| `SolarGrid.Application` | Services (all business logic), DTOs, `Result` type, repository/security ports | Domain |
| `SolarGrid.Infrastructure` | MongoDB repositories and class maps, JWT issuing, password hashing | Application |
| `SolarGrid.Api` | Thin controllers, role-based `[Authorize]`, Swagger, composition root | Application, Infrastructure |
| `SolarGrid.Tests` | xUnit tests for the rules and the reservation service (in-memory fakes) | Domain, Application |

The web and Android clients hold no business rules. They show whatever `{ message }` the API returns.

### Business rules (enforced in `SolarGrid.Application` / `SolarGrid.Domain`)
- A reservation must be in the future and no more than **7 days** ahead (on create and on reschedule).
- An update or cancellation needs at least **12 hours'** notice before the slot.
- Only **Pending** reservations can be approved. Approval issues a one-time QR token.
- Rescheduling an approved booking sends it back to Pending and invalidates its QR token.
- A node's **battery slots** cap how many live bookings it accepts for the same time slot.
- A node can't be **deactivated** while it has live (pending/approved) upcoming bookings.
- A QR scan completes the transfer exactly once, as an atomic find-and-update.
- Prosumers can only see and change their own records. Only Backoffice can reactivate an account.

### Roles
| Role | Signs in via | Can |
|---|---|---|
| Backoffice | Web (`/api/auth/login`) | Manage staff users, nodes, all bookings (create/reschedule/cancel/approve), reactivate prosumers |
| Grid Operator | Web (read-only) and Android Operator Mode | View bookings and nodes, scan and verify QR codes |
| Prosumer | Android (`/api/auth/prosumer-login`, NIC + password) | Register, edit profile, deactivate account, create/update/cancel own bookings, dashboard, map |

## Running the API

1. Create `api/SolarGrid.Api/appsettings.Local.json` (git-ignored) with the real secrets:
   ```json
   {
     "MongoDB": { "ConnectionString": "mongodb+srv://<user>:<password>@<cluster>/?appName=Cluster0" },
     "Jwt": { "Key": "<random string, 32+ characters>" },
     "Seed": { "BackofficePassword": "<first admin password>" }
   }
   ```
2. `dotnet test SolarGrid.sln` runs the tests.
3. `dotnet run --project api/SolarGrid.Api --launch-profile http`, then open http://localhost:5093/swagger.
   On first start an `admin` Backoffice user is created with the seed password.

### Publishing to IIS
```
dotnet publish api/SolarGrid.Api -c Release -o C:\SolarGridAPI\publish
```
Point the IIS site at `C:\SolarGridAPI\publish`. The output folder must **not** be inside the project folder.
`appsettings.Local.json` is copied into the output automatically when it exists.

## Running the web app
Open `web/index.html` in a browser, or host the `web/` folder as a second IIS site.
The API URL defaults to `http://localhost:5093`. To point at IIS, run this once in the browser console:
```js
localStorage.setItem('sg.apiBaseUrl', 'http://localhost:<iis-port>')
```

## Running the Android app
1. Open the `android/` folder in Android Studio and let it install the SDK and sync Gradle.
2. Copy `android/local.properties.example` to `android/local.properties`, then set `solargrid.apiBaseUrl`
   (`http://10.0.2.2:<port>/` from the emulator) and `MAPS_API_KEY` (Google Cloud Console, Maps SDK for Android).
3. Run on an emulator or a device. Prosumers register in the app. Grid Operators are created by Backoffice in the web app.

SQLite (`SolarGridDbHelper`) caches the signed-in session, so the app reopens on the right home screen,
and it caches the node list, so the map still works offline.
