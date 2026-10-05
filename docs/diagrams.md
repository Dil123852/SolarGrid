# SolarGrid Diagrams

These are Mermaid diagrams. GitHub renders them inline. Rendered PNG copies used in the report are in
`docs/diagrams/`.

## 1. High-level architecture

```mermaid
flowchart LR
    subgraph Clients
        WEB["Web app<br/>React 19 + TypeScript + Bootstrap 5<br/>(Backoffice, Grid Operator)"]
        AND["Android app<br/>Kotlin + Jetpack Compose<br/>(Prosumer, Grid Operator)"]
        SQL[("SQLite<br/>session + node cache")]
        AND --- SQL
    end

    subgraph IIS["IIS - SolarGrid Web API (ASP.NET Core, .NET 10)"]
        direction TB
        API["SolarGrid.Api<br/>Controllers, JWT auth, Swagger"]
        APP["SolarGrid.Application<br/>Services (all business logic), DTOs, ports"]
        DOM["SolarGrid.Domain<br/>Entities, business rules"]
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
    BO(["Backoffice"])
    GO(["Grid Operator"])
    PR(["Prosumer"])

    subgraph System["Smart Solar Microgrid Trading System"]
        UC1([Sign in])
        UC2([Manage staff users])
        UC3([Manage prosumer accounts])
        UC4([Manage solar stations & hours])
        UC5([Publish / edit booking slots])
        UC6([Update battery slots])
        UC7([View / search bookings])
        UC8([Approve booking & issue QR])
        UC9([Create / reschedule booking])
        UC10([Cancel booking])
        UC11([Register account])
        UC12([Edit profile / deactivate account])
        UC13([View dashboard])
        UC14([View stations on map])
        UC15([Scan & verify QR])
    end

    BO --- UC1 & UC2 & UC3 & UC4 & UC5 & UC7 & UC8 & UC9 & UC10 & UC13
    GO --- UC1 & UC5 & UC6 & UC7 & UC10 & UC13 & UC14 & UC15
    PR --- UC1 & UC11 & UC12 & UC9 & UC10 & UC7 & UC13 & UC14
```

## 3. Data flow diagram

### Level 0 (context)

```mermaid
flowchart LR
    BO[Backoffice] -- "staff, prosumers, stations, slots, approvals" --> S((SolarGrid<br/>System))
    S -- "lists, dashboards, QR tokens" --> BO
    PR[Prosumer] -- "registration, profile, bookings" --> S
    S -- "slot availability, booking status, QR" --> PR
    GO[Grid Operator] -- "slots, battery slots, scanned QR token" --> S
    S -- "bookings, verification result" --> GO
```

### Level 1

```mermaid
flowchart TB
    PR[Prosumer]
    BO[Backoffice]
    GO[Grid Operator]

    P1(("1.0<br/>Authenticate"))
    P2(("2.0<br/>Manage accounts"))
    P3(("3.0<br/>Manage stations"))
    P4(("4.0<br/>Manage booking slots"))
    P5(("5.0<br/>Manage reservations"))
    P6(("6.0<br/>Verify QR transfer"))

    D1[("D1 Users")]
    D2[("D2 Prosumers")]
    D3[("D3 Nodes")]
    D4[("D4 EnergyBookingSlots")]
    D5[("D5 Reservations")]

    PR -- "NIC + password" --> P1
    BO -- "username + password" --> P1
    GO -- "username + password" --> P1
    P1 -- "verify credentials" --> D1 & D2
    P1 -- "JWT" --> PR & BO & GO

    PR -- "register / edit / deactivate" --> P2
    BO -- "create staff, manage prosumers" --> P2
    P2 <--> D1
    P2 <--> D2

    BO -- "station details, hours" --> P3
    GO -- "battery slots" --> P3
    P3 <--> D3
    P3 -- "check live bookings" --> D5

    BO & GO -- "time window, capacity" --> P4
    P4 -- "hours, battery slots" --> D3
    P4 <--> D4
    P4 -- "booked counts" --> D5

    PR -- "create / update / cancel" --> P5
    BO -- "approve / manage" --> P5
    P5 -- "active, hours" --> D3
    P5 -- "slot + capacity" --> D4
    P5 <--> D5
    P5 -- "summary, counts, QR token" --> PR & BO

    GO -- "QR token" --> P6
    P6 -- "Approved -> Completed" --> D5
    P6 -- "result" --> GO
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
        string OpenTime "HH:mm, optional"
        string CloseTime "HH:mm, optional"
        bool IsActive
    }
    EnergyBookingSlots {
        ObjectId _id PK
        string NodeId FK
        date StartTime
        date EndTime
        int Capacity
        bool IsActive
        date CreatedAt
    }
    Reservations {
        ObjectId _id PK
        string ProsumerNIC FK
        string NodeId FK
        string SlotId FK "optional"
        date SlotTime
        string Status "Pending | Approved | Cancelled | Completed"
        string QrToken "set on approval"
        date CreatedAt
        date UpdatedAt
    }

    Prosumers ||--o{ Reservations : books
    Nodes ||--o{ EnergyBookingSlots : publishes
    Nodes ||--o{ Reservations : hosts
    EnergyBookingSlots ||--o{ Reservations : holds
```

Indexes: `Users.Username` and `Users.Email` (unique), `Reservations (ProsumerNIC, SlotTime)`,
`Reservations (NodeId, SlotTime)`, `Reservations.QrToken`, `Reservations.SlotId`,
`EnergyBookingSlots (NodeId, StartTime)`.
