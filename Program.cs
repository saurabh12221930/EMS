using System;
using System.Collections.Generic;

namespace EMS
{
    class Program
    {
        static void Main(string[] args)
        {
            // ---------------------------------------------------------
            // DATA SOURCE: JSON file (employees.json)
            // We use JSON file storage so the app runs on any computer
            // without needing SQL Server.
            //
            // Why not SQL Server here?
            // On Mac/Linux, "Trusted_Connection=True" causes this error:
            //   "The target principal name is incorrect. Cannot generate SSPI context."
            // SSPI / Windows Authentication only works on Windows.
            // JSON file mode fixes that error completely.
            //
            // NOTE for training/interview:
            // SQL Server version is still available in EmployeeRepository.cs
            // + EmployeeDB.sql (stored procedures). To use SQL Server again,
            // replace EmployeeJsonRepository with EmployeeRepository + connection string.
            // ---------------------------------------------------------
            EmployeeJsonRepository repo = new EmployeeJsonRepository("employees.json");

            Console.WriteLine("Using data file: employees.json");
            Console.WriteLine();

            bool running = true;
            while (running)
            {
                try
                {
                    ShowMenu();
                    Console.Write("Enter your choice (1-7): ");
                    string? choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddEmployee(repo);
                            break;
                        case "2":
                            ViewAllEmployees(repo);
                            break;
                        case "3":
                            SearchEmployee(repo);
                            break;
                        case "4":
                            UpdateEmployee(repo);
                            break;
                        case "5":
                            DeleteEmployee(repo);
                            break;
                        case "6":
                            ImportData(repo);
                            break;
                        case "7":
                            running = false;
                            Console.WriteLine("Thank you! Exiting...");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please enter a number from 1 to 7.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Basic exception handling - show simple error message
                    Console.WriteLine("Error: " + ex.Message);
                }

                if (running)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }

        // Show main menu
        static void ShowMenu()
        {
            Console.WriteLine("===== Employee Management System =====");
            Console.WriteLine("1. Add Employee");
            Console.WriteLine("2. View All Employees");
            Console.WriteLine("3. Search Employee by ID");
            Console.WriteLine("4. Update Employee");
            Console.WriteLine("5. Delete Employee");
            Console.WriteLine("6. Import from employee_management_data.json");
            Console.WriteLine("7. Exit");
            Console.WriteLine("======================================");
        }

        // Option 1: Add new employee
        static void AddEmployee(EmployeeJsonRepository repo)
        {
            Console.WriteLine("--- Add Employee ---");

            int id = ReadInt("Enter Employee ID: ");

            // Validation: check duplicate ID before insert
            try
            {
                if (repo.IsEmployeeIDExists(id))
                {
                    Console.WriteLine("Error: Employee ID already exists. Please use a different ID.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking duplicate ID: " + ex.Message);
                return;
            }

            Employee emp = ReadEmployeeDetails(id);

            try
            {
                bool success = repo.InsertEmployee(emp);
                if (success)
                    Console.WriteLine("Employee added successfully!");
                else
                    Console.WriteLine("Failed to add employee.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding employee: " + ex.Message);
            }
        }

        // Option 2: View all employees (data comes from JSON file)
        static void ViewAllEmployees(EmployeeJsonRepository repo)
        {
            Console.WriteLine("--- All Employees ---");

            try
            {
                List<Employee> list = repo.GetAllEmployees();

                if (list.Count == 0)
                {
                    Console.WriteLine("No employees found.");
                    return;
                }

                Console.WriteLine("ID | Name | Department | Salary | Email | HireDate");
                Console.WriteLine("------------------------------------------------------------");
                foreach (Employee emp in list)
                {
                    Console.WriteLine(emp.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching employees: " + ex.Message);
            }
        }

        // Option 3: Search by ID
        static void SearchEmployee(EmployeeJsonRepository repo)
        {
            Console.WriteLine("--- Search Employee ---");
            int id = ReadInt("Enter Employee ID to search: ");

            try
            {
                Employee? emp = repo.GetEmployeeByID(id);
                if (emp == null)
                {
                    Console.WriteLine("Employee with ID " + id + " does not exist.");
                }
                else
                {
                    Console.WriteLine("Employee found:");
                    Console.WriteLine(emp.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error searching employee: " + ex.Message);
            }
        }

        // Option 4: Update employee
        static void UpdateEmployee(EmployeeJsonRepository repo)
        {
            Console.WriteLine("--- Update Employee ---");
            int id = ReadInt("Enter Employee ID to update: ");

            try
            {
                Employee? existing = repo.GetEmployeeByID(id);
                if (existing == null)
                {
                    Console.WriteLine("Employee with ID " + id + " does not exist.");
                    return;
                }

                Console.WriteLine("Current details: " + existing.ToString());
                Console.WriteLine("Enter new details:");

                Employee updated = ReadEmployeeDetails(id);

                bool success = repo.UpdateEmployee(updated);
                if (success)
                    Console.WriteLine("Employee updated successfully!");
                else
                    Console.WriteLine("Failed to update employee.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating employee: " + ex.Message);
            }
        }

        // Option 5: Delete employee
        static void DeleteEmployee(EmployeeJsonRepository repo)
        {
            Console.WriteLine("--- Delete Employee ---");
            int id = ReadInt("Enter Employee ID to delete: ");

            try
            {
                Employee? existing = repo.GetEmployeeByID(id);
                if (existing == null)
                {
                    Console.WriteLine("Employee with ID " + id + " does not exist.");
                    return;
                }

                bool success = repo.DeleteEmployee(id);
                if (success)
                    Console.WriteLine("Employee deleted successfully!");
                else
                    Console.WriteLine("Failed to delete employee.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting employee: " + ex.Message);
            }
        }

        // Option 6: Import data from employee_management_data.json into employees.json
        static void ImportData(EmployeeJsonRepository repo)
        {
            Console.WriteLine("--- Import Data ---");
            Console.Write("Enter import file path (press Enter for employee_management_data.json): ");
            string? input = Console.ReadLine();
            string importFile = string.IsNullOrWhiteSpace(input)
                ? "employee_management_data.json"
                : input.Trim();

            try
            {
                int imported = repo.ImportFromFile(importFile, out int skipped);
                Console.WriteLine($"Import complete. Imported: {imported}, Skipped (duplicates/invalid): {skipped}.");
                Console.WriteLine("Data saved to employees.json.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error importing data: " + ex.Message);
            }
        }

        // Helper: read full employee details from user with validation
        static Employee ReadEmployeeDetails(int id)
        {
            Employee emp = new Employee();
            emp.EmployeeID = id;

            // Name validation - required field
            while (true)
            {
                Console.Write("Enter Employee Name: ");
                string? name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    emp.EmployeeName = name.Trim();
                    break;
                }
                Console.WriteLine("Employee Name is required. Please try again.");
            }

            // Department validation - required field
            while (true)
            {
                Console.Write("Enter Department: ");
                string? dept = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(dept))
                {
                    emp.Department = dept.Trim();
                    break;
                }
                Console.WriteLine("Department is required. Please try again.");
            }

            // Salary validation - must be a valid positive number
            while (true)
            {
                Console.Write("Enter Salary: ");
                string? input = Console.ReadLine();
                if (decimal.TryParse(input, out decimal salary) && salary >= 0)
                {
                    emp.Salary = salary;
                    break;
                }
                Console.WriteLine("Invalid salary. Please enter a valid positive number.");
            }

            Console.Write("Enter Email (optional, press Enter to skip): ");
            emp.Email = Console.ReadLine()?.Trim() ?? "";

            // HireDate validation
            while (true)
            {
                Console.Write("Enter Hire Date (yyyy-mm-dd): ");
                string? input = Console.ReadLine();
                if (DateTime.TryParse(input, out DateTime date))
                {
                    emp.HireDate = date;
                    break;
                }
                Console.WriteLine("Invalid date. Please use format yyyy-mm-dd (example: 2024-01-15).");
            }

            return emp;
        }

        // Helper: safely read an integer from console
        static int ReadInt(string message)
        {
            while (true)
            {
                Console.Write(message);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value))
                    return value;
                Console.WriteLine("Invalid number. Please try again.");
            }
        }
    }
}
