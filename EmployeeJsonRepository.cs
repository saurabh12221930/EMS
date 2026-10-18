using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EMS
{
    // This class does the same CRUD work as EmployeeRepository,
    // but stores data in a local JSON file (employees.json)
    // instead of SQL Server. No database connection needed.
    // Simple and beginner-friendly.
    public class EmployeeJsonRepository
    {
        private readonly string _filePath;

        // By default use employees.json in the current folder.
        public EmployeeJsonRepository(string filePath = "employees.json")
        {
            _filePath = filePath;
        }

        // Load all employees from JSON file.
        private List<Employee> LoadEmployees()
        {
            // If file does not exist, return empty list.
            if (!File.Exists(_filePath))
            {
                return new List<Employee>();
            }

            string json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Employee>();
            }

            // Convert JSON text to List<Employee>.
            List<Employee>? list = JsonSerializer.Deserialize<List<Employee>>(json);
            return list ?? new List<Employee>();
        }

        // Save employee list to JSON file.
        private void SaveEmployees(List<Employee> employees)
        {
            // WriteIndented = pretty print JSON file.
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(employees, options);
            File.WriteAllText(_filePath, json);
        }

        // Check if an EmployeeID already exists.
        public bool IsEmployeeIDExists(int employeeID)
        {
            List<Employee> list = LoadEmployees();
            return list.Any(e => e.EmployeeID == employeeID);
        }

        // Insert a new employee.
        public bool InsertEmployee(Employee emp)
        {
            List<Employee> list = LoadEmployees();

            // Duplicate check (same as stored procedure sp_CheckDuplicateEmployeeID)
            if (list.Any(e => e.EmployeeID == emp.EmployeeID))
            {
                return false;
            }

            list.Add(emp);
            SaveEmployees(list);
            return true;
        }

        // Get all employees, ordered by ID.
        public List<Employee> GetAllEmployees()
        {
            List<Employee> list = LoadEmployees();
            return list.OrderBy(e => e.EmployeeID).ToList();
        }

        // Search one employee by ID. Returns null if not found.
        public Employee? GetEmployeeByID(int employeeID)
        {
            List<Employee> list = LoadEmployees();
            return list.FirstOrDefault(e => e.EmployeeID == employeeID);
        }

        // Update employee details.
        public bool UpdateEmployee(Employee emp)
        {
            List<Employee> list = LoadEmployees();

            Employee? existing = list.FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);
            if (existing == null)
            {
                return false;
            }

            existing.EmployeeName = emp.EmployeeName;
            existing.Department = emp.Department;
            existing.Salary = emp.Salary;
            existing.Email = emp.Email;
            existing.HireDate = emp.HireDate;

            SaveEmployees(list);
            return true;
        }

        // Delete employee by ID.
        public bool DeleteEmployee(int employeeID)
        {
            List<Employee> list = LoadEmployees();

            Employee? existing = list.FirstOrDefault(e => e.EmployeeID == employeeID);
            if (existing == null)
            {
                return false;
            }

            list.Remove(existing);
            SaveEmployees(list);
            return true;
        }

        // Import employees from another JSON file (example: employee_management_data.json).
        // That file has extra fields (ManagerID, Phone, Status, City) and NaN values,
        // so we read only the 6 fields our Employee class needs and skip duplicates.
        // Returns number of newly imported employees. Skipped count is given via out parameter.
        public int ImportFromFile(string importFilePath, out int skippedCount)
        {
            skippedCount = 0;

            if (!File.Exists(importFilePath))
            {
                throw new FileNotFoundException("Import file not found: " + importFilePath);
            }

            // Read raw text. Replace NaN (invalid JSON) with null so parsing works.
            string rawText = File.ReadAllText(importFilePath);
            rawText = rawText.Replace("NaN", "null");

            List<Employee> currentList = LoadEmployees();
            HashSet<int> existingIDs = new HashSet<int>(currentList.Select(e => e.EmployeeID));

            int importedCount = 0;

            // Parse JSON array without needing an exact class match.
            using (JsonDocument doc = JsonDocument.Parse(rawText))
            {
                foreach (JsonElement item in doc.RootElement.EnumerateArray())
                {
                    try
                    {
                        // EmployeeID is required. Skip if missing.
                        if (!item.TryGetProperty("EmployeeID", out JsonElement idProp))
                        {
                            skippedCount++;
                            continue;
                        }
                        int id = idProp.GetInt32();

                        // Skip duplicates (already in employees.json or twice in import file).
                        if (existingIDs.Contains(id))
                        {
                            skippedCount++;
                            continue;
                        }

                        // Read only the fields we need. Extra fields are ignored.
                        string name = item.TryGetProperty("EmployeeName", out JsonElement nameProp)
                            ? (nameProp.GetString() ?? "") : "";
                        string dept = item.TryGetProperty("Department", out JsonElement deptProp)
                            ? (deptProp.GetString() ?? "") : "";
                        string email = item.TryGetProperty("Email", out JsonElement emailProp)
                            ? (emailProp.GetString() ?? "") : "";

                        // Skip rows with missing required fields.
                        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(dept))
                        {
                            skippedCount++;
                            continue;
                        }

                        // Salary can be number or string. Default to 0 if invalid.
                        decimal salary = 0;
                        if (item.TryGetProperty("Salary", out JsonElement salProp))
                        {
                            if (salProp.ValueKind == JsonValueKind.Number && salProp.TryGetDecimal(out decimal s))
                                salary = s;
                            else if (salProp.ValueKind == JsonValueKind.String)
                                decimal.TryParse(salProp.GetString(), out salary);
                        }

                        // HireDate is a string like "2022-01-10". Default to today if invalid.
                        DateTime hireDate = DateTime.Today;
                        if (item.TryGetProperty("HireDate", out JsonElement dateProp))
                        {
                            string? dateStr = dateProp.ValueKind == JsonValueKind.String
                                ? dateProp.GetString()
                                : dateProp.ToString();
                            if (!DateTime.TryParse(dateStr, out hireDate))
                                hireDate = DateTime.Today;
                        }

                        Employee emp = new Employee
                        {
                            EmployeeID = id,
                            EmployeeName = name.Trim(),
                            Department = dept.Trim(),
                            Salary = salary,
                            Email = email.Trim(),
                            HireDate = hireDate
                        };

                        currentList.Add(emp);
                        existingIDs.Add(id);
                        importedCount++;
                    }
                    catch
                    {
                        // Skip any bad row, keep importing the rest.
                        skippedCount++;
                    }
                }
            }

            // Save merged list once at the end.
            SaveEmployees(currentList.OrderBy(e => e.EmployeeID).ToList());
            return importedCount;
        }
    }
}
