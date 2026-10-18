using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace EMS
{
    // This class handles all database work.
    // Every method calls a SQL Server stored procedure.
    // No direct SQL queries are written in C# code.
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        // Connection string is passed from Program.cs
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Check if an EmployeeID already exists.
        // Uses ExecuteScalar() because sp returns a single value (1 or 0).
        public bool IsEmployeeIDExists(int employeeID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("sp_CheckDuplicateEmployeeID", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                // Pass parameter to stored procedure
                command.Parameters.Add(new SqlParameter("@EmployeeID", employeeID));

                connection.Open();
                object? result = command.ExecuteScalar();
                int count = Convert.ToInt32(result);
                return count == 1; // true = exists, false = not exists
            }
        }

        // Insert a new employee. Uses ExecuteNonQuery().
        public bool InsertEmployee(Employee emp)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("sp_InsertEmployee", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@EmployeeID", emp.EmployeeID));
                command.Parameters.Add(new SqlParameter("@EmployeeName", emp.EmployeeName));
                command.Parameters.Add(new SqlParameter("@Department", emp.Department));
                command.Parameters.Add(new SqlParameter("@Salary", emp.Salary));
                command.Parameters.Add(new SqlParameter("@Email", emp.Email));
                command.Parameters.Add(new SqlParameter("@HireDate", emp.HireDate));

                connection.Open();
                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
        }

        // Get all employees. Uses ExecuteReader().
        public List<Employee> GetAllEmployees()
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("sp_GetAllEmployees", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employee emp = new Employee
                        {
                            EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                            EmployeeName = reader["EmployeeName"].ToString() ?? "",
                            Department = reader["Department"].ToString() ?? "",
                            Salary = reader["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Salary"]),
                            Email = reader["Email"].ToString() ?? "",
                            HireDate = reader["HireDate"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["HireDate"])
                        };
                        employees.Add(emp);
                    }
                }
            }

            return employees;
        }

        // Search one employee by ID. Uses ExecuteReader().
        public Employee? GetEmployeeByID(int employeeID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("sp_GetEmployeeByID", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@EmployeeID", employeeID));

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Employee
                        {
                            EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                            EmployeeName = reader["EmployeeName"].ToString() ?? "",
                            Department = reader["Department"].ToString() ?? "",
                            Salary = reader["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Salary"]),
                            Email = reader["Email"].ToString() ?? "",
                            HireDate = reader["HireDate"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["HireDate"])
                        };
                    }
                }
            }

            return null; // Not found
        }

        // Update employee details. Uses ExecuteNonQuery().
        public bool UpdateEmployee(Employee emp)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("sp_UpdateEmployee", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@EmployeeID", emp.EmployeeID));
                command.Parameters.Add(new SqlParameter("@EmployeeName", emp.EmployeeName));
                command.Parameters.Add(new SqlParameter("@Department", emp.Department));
                command.Parameters.Add(new SqlParameter("@Salary", emp.Salary));
                command.Parameters.Add(new SqlParameter("@Email", emp.Email));
                command.Parameters.Add(new SqlParameter("@HireDate", emp.HireDate));

                connection.Open();
                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
        }

        // Delete employee by ID. Uses ExecuteNonQuery().
        public bool DeleteEmployee(int employeeID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("sp_DeleteEmployee", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@EmployeeID", employeeID));

                connection.Open();
                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
        }
    }
}
