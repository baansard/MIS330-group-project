# TrailBuddy Hiking Services (MIS 330)

## Program Overview

TrailBuddy is a class project web application built as a Vue 3 single-page app (SPA) served by an ASP.NET Core 9 static file host.

The app simulates a hiking services platform where:

- Customers can browse trips, view trip details, register/login, and make or cancel reservations.
- Employees can log in to an employee dashboard to view metrics, manage trips, and manage reservation statuses.
- All data is stored in Vue `data()` memory (frontend prototype only, no backend database integration in runtime).

## Tech Stack

- ASP.NET Core 9 minimal static host
- Vue 3 (CDN)
- Bootstrap 5 (CDN)
- Single-page frontend in `wwwroot/index.html`
- SQL design scripts in project root (`CreationFIle.sql`, `AnalysisQueries.sql`)

## Core Features

- Trip listing with filters and sorting
- Trip detail pages with reservation flow
- Customer authentication and profile editing
- Customer reservation tracking ("My Reservations")
- Employee dashboard with reservation/revenue visibility
- Employee trip CRUD modal
- Employee reservation status management

## Recent Updates (Phase 2)

### 1) SQL Schema Fixes in `CreationFIle.sql`

- Fixed table creation order to prevent FK dependency errors:
  - `employee` -> `employeephone` -> `customer` -> `trip` -> `reservation` -> `reservationtrip`
- Removed derived columns from `trip`:
  - `numenrolled`
  - `isfullstatus`
- Updated `reservation` table:
  - Removed `tripid` FK column
  - Added `reservationdate DATE`
  - Added `reservationstatus VARCHAR(15)`
- Added many-to-many junction table:
  - `reservationtrip(reservationid, tripid)`
- Updated seed inserts to include:
  - reservation status/date values
  - `reservationtrip` rows for reservation-trip links

### 2) Added `AnalysisQueries.sql` (10 Business Queries)

Includes commented analytical SQL queries for:

- Sales & Revenue (4 queries)
  - Revenue per trip
  - Monthly revenue trend (non-cancelled)
  - Average revenue per reservation
  - Trips with zero reservations
- Customer Insights (3 queries)
  - Top 5 customers by spend
  - Repeat customers (>1 reservation)
  - Customers with no reservations
- Trip/Product Insights (3 queries)
  - Most popular trips by spots reserved
  - Trips at/over capacity
  - Employee guide with most reservations

### 3) Frontend Schema Alignment in `wwwroot/index.html`

- Replaced trip guide text field with `empid` relationship.
- Added seeded `employees[]` matching SQL employee records.
- Merged employee records into `users[]` so both customers and employees authenticate through one login array.
- Added reservation fields:
  - `reservationDate`
  - `reservationStatus`
- Displayed reservation date/status in both customer and employee reservation views.
- Removed stored `spotsLeft` from trip data.
- Added computed-style method:
  - `spotsLeftForTrip(tripId)` using non-cancelled reservations.
- Added in-memory `reservationTrips[]` junction array to mirror SQL M:N model.
- Updated employee trip modal fields to schema-aligned inputs:
  - `empid`, `street`, `city`, `state`, `zip`
  - `lengthHours`, `distanceMiles`
  - `tripStatus`, `startTime`
  - kept `price`, `maxParticipants`, `nextDate`, `description`, `imageUrl`

## Project Constraints Followed

- No backend controllers/API endpoints added
- No npm packages or build pipeline added
- No `localStorage`/`sessionStorage`
- Kept app in one `index.html` frontend file
- Kept Bootstrap and Vue CDN imports
- Passwords remain plaintext for class demo purposes

## How to Run

1. Open the project in your IDE.
2. Run the ASP.NET Core project.
3. Open the app in your browser (served from `wwwroot/index.html`).
4. Use seeded customer or employee credentials to test role-based flows.

## Important Files

- `Program.cs` - minimal ASP.NET Core static host
- `wwwroot/index.html` - entire Vue SPA frontend
- `CreationFIle.sql` - schema + seed script
- `AnalysisQueries.sql` - phase 2 analysis query set
