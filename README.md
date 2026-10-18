# Employee Management System

A simple menu-driven **Employee Management System** built by a .NET trainee using:

- C# .NET Console Application
- JSON file storage (`employees.json`) — **current default, works everywhere**
- Microsoft SQL Server + ADO.NET (`Microsoft.Data.SqlClient`) + Stored Procedures — **kept for training/interview**

> **Why JSON by default?**
> On Mac/Linux, `Trusted_Connection=True` causes:
> `The target principal name is incorrect. Cannot generate SSPI context.`
> SSPI / Windows Authentication only works on Windows.
> JSON file mode fixes this — no SQL Server needed to run the app.

---

## Project Files

| File | Purpose |
|------|---------|
| `Employee.cs` | Simple class to store employee details |
| `EmployeeJsonRepository.cs` | **Active** CRUD + Import using `employees.json` (System.Text.Json) |
| `Program.cs` | Menu display and user input handling (uses JSON repo) |
| `employees.json` | Data file (500 employees after import) |
| `employee_management_data.json` | Source file with 500 raw records (extra fields + NaN) |
| `EmployeeRepository.cs` | SQL Server version using stored procedures (kept for learning) |
| `EmployeeDB.sql` | Database, table + 6 stored procedures |
| `EMS.csproj` | Project file |

---

## 1. Requirements

- .NET SDK 8 or later (`dotnet --version` to check)
- Visual Studio Code with C# extension
- (Optional, SQL mode only) Microsoft SQL Server + SSMS / sqlcmd

---

## 2. Run with JSON file (Default — No Setup Needed)

```bash
dotnet restore
dotnet build
dotnet run
```

The app reads/writes `employees.json` in the project folder.
Use menu option `6. Import` to load data from `employee_management_data.json`.

### Menu

```
===== Employee Management System =====
1. Add Employee
2. View All Employees
3. Search Employee by ID
4. Update Employee
5. Delete Employee
6. Import from employee_management_data.json
7. Exit
======================================
```

### Import data (500 records)

`employee_management_data.json` contains 500 employees but with extra fields
(`ManagerID`, `Phone`, `Status`, `City`) and invalid `NaN` values.
Import keeps only the 6 fields the app needs and skips duplicates:

1. Run `dotnet run`.
2. Choose `6` → press Enter to accept default file `employee_management_data.json`.
3. See `Import complete. Imported: 500, Skipped: ...`.
4. Choose `2` to view, `3` to search (try ID 1 or 500).
5. Check `employees.json` — now holds 500 clean records.

Re-running import is safe — existing IDs are skipped.

### Example Test Flow

1. Run `dotnet run`.
2. Choose `6` → import 500 records from `employee_management_data.json`.
3. Choose `3` → search ID=1 or ID=500.
4. Choose `1` → Add ID=501 (new ID works; ID=1 shows "already exists").
5. Choose `4` → update ID=501.
6. Choose `5` → delete ID=501.
7. Choose `7` → exit.

### How JSON storage works (easy interview answer)

- `File.ReadAllText` loads `employees.json`.
- `JsonSerializer.Deserialize<List<Employee>>` converts JSON → objects.
- `JsonSerializer.Serialize` + `File.WriteAllText` saves back.
- `IsEmployeeIDExists` uses LINQ `Any(e => e.EmployeeID == id)` (same idea as `sp_CheckDuplicateEmployeeID`).
- Import uses `JsonDocument` + `TryGetProperty` to read only needed fields, replaces `NaN` with `null`, skips duplicates via `HashSet<int>`.
- Validation (required name/dept, positive salary, valid date) + try-catch stays the same.

---

## 3. Run with SQL Server (Optional — Training Version)

Files `EmployeeRepository.cs` + `EmployeeDB.sql` show the ADO.NET + stored procedure version.

### Step 1: Create Database

Open `EmployeeDB.sql` in SSMS and press **F5**, or:

```bash
sqlcmd -S localhost -U sa -P YourPassword -i EmployeeDB.sql
```

This creates `EmployeeDB`, table `Employee`, and 6 procedures:
`sp_InsertEmployee`, `sp_GetAllEmployees`, `sp_GetEmployeeByID`,
`sp_UpdateEmployee`, `sp_DeleteEmployee`, `sp_CheckDuplicateEmployeeID`.

### Step 2: Switch Program.cs to SQL

In `Program.cs`, replace:

```csharp
EmployeeJsonRepository repo = new EmployeeJsonRepository("employees.json");
```

with:

```csharp
string? connectionString = Environment.GetEnvironmentVariable("EMS_CONNECTION_STRING")
    ?? "Server=localhost;Database=EmployeeDB;Trusted_Connection=True;TrustServerCertificate=True;";
EmployeeRepository repo = new EmployeeRepository(connectionString);
```

And change all method parameter types from `EmployeeJsonRepository` to `EmployeeRepository`.
Then `dotnet build` + `dotnet run`.

### Step 3: Connection string (never hardcode password)

```bash
# macOS/Linux
export EMS_CONNECTION_STRING="Server=localhost,1433;Database=EmployeeDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

```powershell
# Windows PowerShell
$env:EMS_CONNECTION_STRING="Server=localhost;Database=EmployeeDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### How SQL version works (interview answer)

- `SqlConnection` — connects C# to SQL Server.
- `SqlCommand` + `CommandType.StoredProcedure` — calls a stored procedure.
- `SqlParameter` — passes values safely.
- `ExecuteNonQuery()` — INSERT/UPDATE/DELETE.
- `ExecuteReader()` — SELECT.
- `ExecuteScalar()` — single value (`sp_CheckDuplicateEmployeeID` returns 1/0).
- `using` statements + `try-catch` for cleanup and friendly errors.

---

## 4. Common Issues

| Problem | Fix |
|---------|-----|
| `Cannot generate SSPI context` / `target principal name is incorrect` | You are using `Trusted_Connection=True` on Mac/Linux. Use JSON mode (default), or set `EMS_CONNECTION_STRING` with SQL login + `TrustServerCertificate=True` |
| `employees.json` not found when running .exe | File is auto-copied to output via `EMS.csproj`. Run from project folder with `dotnet run` |
| Login failed / cannot connect (SQL mode) | Check server name, SQL Server is running, and `EMS_CONNECTION_STRING` |
| Stored procedure not found (SQL mode) | Re-run `EmployeeDB.sql` with `USE EmployeeDB;` |
