# AtRiskTracker

A desktop application for tracking "at risk" Further Education students against their qualification progress. Teaching staff record concerns, grades, assessment deadlines and evidence against criteria, then pull cohort and audit reports to spot students who are falling behind.

Built for BTec Extended Diploma, NCFE and T-Level cohorts, where a student's overall grade is derived from points across many units.

The system is in three parts, all living in this repository:

| Part | Folder | Technology |
|------|--------|------------|
| Desktop front-end | project root (`AtRiskTracker.csproj`) | C# / .NET 8 / Windows Forms |
| REST API back-end | `api/` | PHP 8, PDO, JWT |
| Database schema | `data/` | MySQL / MariaDB |

---

## Architecture

```
+---------------------------+         HTTPS/JSON          +----------------------+        PDO        +-----------------+
|  AtRiskTracker (WinForms) |  ---- Bearer JWT token ----> |  PHP REST API (api/) |  --------------->  |  MySQL: atriskreg |
|  .NET 8, Newtonsoft.Json  |  <---- JSON responses ------  |  firebase/php-jwt    |  <---------------  |  (data/schema.sql)|
+---------------------------+                              +----------------------+                   +-----------------+
```

- The desktop client never touches the database directly. Everything goes through the PHP API.
- Every request carries a short-lived JWT access token in an `Authorization: Bearer` header. On a `401` the client silently tries one refresh using the longer-lived refresh token, then retries the original call.
- Tokens are held in memory only. Closing the app logs the user out.
- All API responses share the shape `{ "success": true|false, "data": ..., "error": "..." }`.

---

## Front-end (Windows desktop client)

A Windows Forms application targeting `net8.0-windows`. Namespace `AtRiskTracker`.

### Layout

| Folder | Contents |
|--------|----------|
| `Forms/` | Top level windows: `LoginForm`, `MainForm`, `ChangePasswordForm`, plus the `Dashboard/` grade grid |
| `Dialogs/` | Modal editors for assessments, notes, objectives, predictions and bulk date setting |
| `Admin/` | Management panels and edit dialogs for every reference entity (students, courses, units, groups, users, concerns, criteria and so on) |
| `Reports/` | Report builder, print helper and the reports panel |
| `Controls/` | Reusable controls, including a `HtmlEditor` for rich-text student notes |
| `Models/` | Data transfer objects grouped by area (`AuthModels`, `AdminModels`, `GridModels`, `ReportModels`) |
| `Services/` | `ApiService` (HTTP + auth) and `AppConfig` (settings loader) |
| `Utils/` | `AtRiskCalc` and `GradeCalc` grade-point calculation helpers, plus `CsvParser` for CSV import |

### Configuration

The client reads `config.json` from the application directory (copied next to the executable on build):

```json
{
  "apiBaseUrl": "http://localhost/api"
}
```

Point `apiBaseUrl` at wherever the PHP API is served. During local development this is the Laragon Apache root on `localhost`.

### Dependencies

- `Newtonsoft.Json` 13.0.3 (only external NuGet package)

---

## Back-end (PHP REST API)

A flat set of PHP endpoint files under `api/`, one folder per resource, with the usual `index.php` (list/read), `create.php`, `update.php` and `delete.php` inside each. Auth and reporting have their own folders.

### Resources

`auth`, `users`, `students`, `courses`, `courseunits`, `units`, `unitsections`, `groups`, `enrollments`, `concerns`, `criteria`, `evidence`, `results`, `assessments`, `assessmentdefs`, `qualtypes`, `reports`.

### Shared includes

Every endpoint includes these two files first, in this order:

```php
require_once __DIR__ . '/cors.php';   // always first, before any output
require_once __DIR__ . '/config.php'; // then config / db
```

- **`cors.php`** sets the JSON content type and CORS headers, reflects the request `Origin` back when it matches `localhost` on any port (so Vite or any dev server on any port is accepted), and returns `200` immediately for `OPTIONS` preflight requests.
- **`config.php`** loads `.config.json` and exposes `getDb()` (a configured PDO connection using prepared statements, exceptions on error) and `getJwtConfig()`.
- **`jwt.php`** wraps `firebase/php-jwt`. It generates access and refresh tokens and exposes `requireAuth()`, which validates the Bearer token and exits with `401` if it is missing or invalid. Protected endpoints call `requireAuth()` at the top.
- **`.htaccess`** disables directory indexing and forwards the `Authorization` header through to PHP (Apache strips it by default).

### Authentication flow

1. `POST /auth/login.php` with `{ email, password }`. Passwords are verified with `password_verify` against bcrypt hashes. On success returns `accessToken`, `refreshToken` and the user record.
2. Include `Authorization: Bearer <accessToken>` on every subsequent request.
3. When the access token expires, `POST /auth/refresh.php` with the refresh token to get a new pair.
4. `POST /auth/change-password.php` to update the signed-in user's password.

### Dependencies

- `firebase/php-jwt` `^6.0`, installed via Composer into `api/vendor/`.

---

## Database

A single MySQL/MariaDB database, `atriskreg`, `utf8mb4`. The full schema, seed data and views live in [`data/schema.sql`](data/schema.sql).

### Key tables

| Table | Purpose |
|-------|---------|
| `tbluser` | Staff logins (bcrypt passwords). No student accounts exist. |
| `tblstudent` | Student person records, including rich-text staff notes |
| `tblcourse` | Qualifications, with the point thresholds for Pass / Merit / Distinction and a qualification family (`btec_ext_dip`, `ncfe`, `t_level`, `other`) that drives grade calculation |
| `tblgroup` | Teaching groups belonging to a course |
| `tblstudent_group` | Which students are in which group, plus the concern flag per enrolment |
| `tblunit` / `tblcourseunit` | Unit definitions and their many-to-many link to courses |
| `tblunitsection` / `tblcriteria` | Learning objectives or grade bands within a unit, and the individual criteria inside them |
| `tblevidence` | Per-student sign-off against each criterion |
| `tblresults` | Per-student, per-unit grade (`NS`, `OU`, `U`, `NP`, `P`, `M`, `D`) and raw exam marks |
| `tblassessment_def` / `tblassessment` | Assessment parts per unit and their per-student deadline/status tracking |
| `tblgrade_audit` | Append-only log of every grade change, for auditing |
| `tblconcern` | Concern categories (Attendance, Engagement, and so on), seeded on install |

A view, `vw_criteria_progress`, flattens criteria and evidence per student for reporting.

---

## Getting started (local development)

Local development uses [Laragon](https://laragon.org/) (Apache + MySQL on `localhost`).

### 1. Database

```sql
-- from the MySQL console or Adminer
SOURCE E:/tracker/data/schema.sql;
```

This creates the `atriskreg` database, all tables, the reporting view and the seed concern rows. You will need to insert at least one staff user manually, with a bcrypt-hashed password, before you can log in.

### 2. API

```bash
cd api
composer install                       # pulls firebase/php-jwt into vendor/
cp .backend.example.config.json .config.json
```

Then edit `api/.config.json` with your real values:

```json
{
  "db":  { "host": "localhost", "name": "atriskreg", "user": "...", "pass": "..." },
  "jwt": { "secret": "REPLACE_WITH_STRONG_RANDOM_SECRET_MIN_32_CHARS", "accessExpiry": 3600, "refreshExpiry": 604800 }
}
```

Serve the `api/` folder from Apache (Laragon) so it is reachable at `http://localhost/api`.

> **`.config.json` is git-ignored on purpose.** It holds database credentials and the JWT signing secret. Never commit it. Only `.backend.example.config.json` (with placeholder values) belongs in the repo.

### 3. Desktop client

```bash
dotnet build AtRiskTracker.slnx
dotnet run --project AtRiskTracker.csproj
```

Or open `AtRiskTracker.slnx` in Visual Studio 2022 and press F5. Make sure `config.json` points `apiBaseUrl` at your API before running.

---

## Security notes

- Database credentials and the JWT secret live only in `api/.config.json`, which is git-ignored.
- Rotate the `jwt.secret` before any deployment. A leaked secret lets anyone forge tokens.
- Access tokens are short lived (one hour by default); refresh tokens last a week.
- All database queries use PDO prepared statements.
- The `atriskreg` database holds no student login credentials. Students have no access to the system.

---

## Repository layout

```
tracker/
├── AtRiskTracker.slnx        Visual Studio solution
├── AtRiskTracker.csproj      WinForms project
├── config.json               Client config (API base URL)
├── Program.cs                Entry point
├── Forms/  Dialogs/  Admin/  Reports/  Controls/  Models/  Services/  Utils/
├── api/                      PHP REST API
│   ├── cors.php  config.php  jwt.php  .htaccess
│   ├── .backend.example.config.json   (template; copy to .config.json)
│   ├── composer.json  composer.lock
│   └── <resource folders>/  (students, courses, units, reports, ...)
└── data/
    └── schema.sql            MySQL schema, view and seed data
```

---

## Changelog

### 2026-07-02

- **Dashboard grade grid**: hovering a unit column header now shows the full unit name as a tooltip (unit columns are headed by their short unit code, e.g. `R058`, which doesn't fit the column width).
- **Dashboard grade grid**: fixed a regression where clicking a student's "Predict" cell no longer opened the Grade Prediction dialog. A refactor had accidentally gated the click on a right-click, when it should fire on a left-click like the rest of the grid.
- **Dashboard grade grid**: fixed a bug where bulk-setting an assignment date via "Set Assignment Date for All Students..." updated the database correctly but left the in-memory grid data stale, so reopening a student's Assessment Milestones dialog straight afterwards still showed "Not Set". The dialog now updates its local copy immediately after a successful save.
- **Assessment Milestones dialog**: date fields that are unset (null) no longer display today's date next to an unticked checkbox, which looked like the date had already been set. They now render blank until the checkbox is ticked.
- **Dashboard course selector**: the Course dropdown is now on its own full-width row (widened from 300px to 650px) so long course titles (e.g. "Introduction to the principles of...") are no longer truncated. Group, Year and Print moved to a second row below it, with a little extra padding above the grade grid.
- **Main window**: added a Help → About menu item, and a "Do you really want to exit?" confirmation when closing the app (via the window's close button or File → Exit). Logging out does not trigger the prompt.
- **Admin → Students**: added a CSV bulk-import feature ("Import CSV..." on the toolbar). Accepts a UTF-8 CSV (as exported by Excel's "CSV UTF-8" format) with flexible header matching (e.g. "CIS No", "Forename", "Surname"), previews every row — flagging and excluding rows missing a first or last name — before anything is sent to the server, and optionally enrols the imported students into a group selected via a course → group cascade (avoids ambiguity between same-named groups in different courses). Uses the existing `/students/import.php` endpoint, which previously had no UI wired up to it.

---

## License

Released under the **Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)** license.

You are free to share and adapt this work for non-commercial purposes, provided you give appropriate credit and distribute any derivatives under the same license.

© 2026 Exeter College.

Application icon: "Education graduate cap hat" by Egor Mironov, via [icon-icons.com](https://icon-icons.com/authors/1083-egor-mironov).
