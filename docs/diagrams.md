# SolarGrid Diagrams

These are Mermaid diagrams. GitHub renders them inline. To put them in the report, paste each block
into https://mermaid.live and export it as PNG/SVG.

## 1. High-level architecture

```mermaid
flowchart LR
    subgraph Clients
        WEB["Web app<br/>HTML + Bootstrap 5 + JS<br/>(Backoffice, Grid Operator)"]
        AND["Android app<br/>Kotlin + Compose<br/>(Prosumer, Grid Operator)"]
        SQL[("SQLite<br/>session + node cache")]
        AND --- SQL
    end

    subgraph IIS["IIS - SolarGrid Web API (.NET 10)"]
        direction TB
        API["SolarGrid.Api<br/>Controllers, JWT auth, Swagger"]
        APP["SolarGrid.Application<br/>Services, DTOs, ports"]
        DOM["SolarGrid.Domain<br/>Entities, ReservationPolicy"]
        INF["SolarGrid.Infrastructure<br/>Mongo repositories, JWT, hashing"]
        API --> APP
        APP --> DOM
        INF --> APP
        API -. wiring .-> INF
    end

    DB[("MongoDB Atlas<br/>SolarGridDB")]
    MAPS["Google Maps SDK"]

    WEB -- "HTTP/JSON + Bearer JWT" --> API
    AND -- "HTTP/JSON + Bearer JWT (Retrofit)" --> API
    INF -- "MongoDB.Driver" --> DB
    AND --> MAPS
```

## 2. Use case diagram

```mermaid
flowchart LR
    BO(["👤 Backoffice"])
    GO(["👤 Grid Operator"])
    PR(["👤 Prosumer"])

    subgraph System["Smart Solar Microgrid Trading System"]
        UC1([Log in])
        UC2([Manage staff users])
        UC3([Manage microgrid nodes])
        UC4([View / filter bookings])
        UC5([Approve booking & issue QR])
        UC6([Create / reschedule / cancel booking])
        UC7([Reactivate prosumer account])
        UC8([Register account])
        UC9([Edit profile])
        UC10([Deactivate own account])
        UC11([View dashboard counts])
        UC12([View nodes on map])
        UC13([Scan & verify QR])
        UC14([View booking summary])
    end

    BO --- UC1 & UC2 & UC3 & UC4 & UC5 & UC6 & UC7 & UC11
    GO --- UC1 & UC4 & UC11 & UC12 & UC13
    PR --- UC1 & UC8 & UC9 & UC10 & UC6 & UC4 & UC11 & UC12 & UC14
```

## 3. Data flow diagram

### Level 0 (context)

```mermaid
flowchart LR
    BO[Backoffice] -- "staff, node, booking changes" --> S((SolarGrid<br/>System))
    S -- "lists, dashboards, QR tokens" --> BO
    PR[Prosumer] -- "registration, profile, bookings" --> S
    S -- "booking status, summaries, QR" --> PR
    GO[Grid Operator] -- "scanned QR token" --> S
    S -- "verification result" --> GO
```

### Level 1

```mermaid
flowchart TB
    PR[Prosumer]
    BO[Backoffice]
    GO[Grid Operator]

    P1(("1.0<br/>Authenticate"))
    P2(("2.0<br/>Manage accounts"))
    P3(("3.0<br/>Manage nodes"))
    P4(("4.0<br/>Manage reservations"))
    P5(("5.0<br/>Verify QR transfer"))

    D1[("D1 Users")]
    D2[("D2 Prosumers")]
    D3[("D3 Nodes")]
    D4[("D4 Reservations")]

    PR -- "NIC + password" --> P1
    BO -- "username + password" --> P1
    GO -- "username + password" --> P1
    P1 -- "verify credentials" --> D1
    P1 -- "verify credentials" --> D2
    P1 -- "JWT" --> PR & BO & GO

    PR -- "register / edit / deactivate" --> P2
    BO -- "create staff, reactivate prosumer" --> P2
    P2 <--> D1
    P2 <--> D2

    BO -- "node details" --> P3
    P3 <--> D3
    P3 -- "check live bookings" --> D4

    PR -- "create / update / cancel" --> P4
    BO -- "approve / manage" --> P4
    P4 -- "node active + slots" --> D3
    P4 <--> D4
    P4 -- "summary, counts, QR token" --> PR & BO

    GO -- "QR token" --> P5
    P5 -- "Approved -> Completed" --> D4
    P5 -- "result" --> GO
```

## 4. Database design (MongoDB collections)

```mermaid
erDiagram
    Users {
        ObjectId _id PK
        string Username UK
        string Email UK
        string PasswordHash
        string Role "Backoffice | GridOperator"
        bool IsActive
        date CreatedAt
    }
    Prosumers {
        string _id PK "NIC"
        string Name
        string Email
        string Phone
        string Address
        string PasswordHash
        bool IsActive
        date CreatedAt
    }
    Nodes {
        ObjectId _id PK
        string Name
        double Latitude
        double Longitude
        double CapacityKWh
        int BatterySlots
        bool IsActive
    }
    Reservations {
        ObjectId _id PK
        string ProsumerNIC FK
        string NodeId FK
        date SlotTime
        string Status "Pending | Approved | Cancelled | Completed"
        string QrToken "set on approval"
        date CreatedAt
        date UpdatedAt
    }

    Prosumers ||--o{ Reservations : books
    Nodes ||--o{ Reservations : hosts
```

Indexes: `Users.Username` and `Users.Email` (unique), `Reservations (ProsumerNIC, SlotTime)`,
`Reservations (NodeId, SlotTime)`, `Reservations.QrToken`.
