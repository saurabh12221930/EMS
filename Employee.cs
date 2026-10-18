using System;

namespace EMS
{
    // Simple class to store employee details.
    // This is a plain data holder, easy to understand for beginners.
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Department { get; set; } = "";
        public decimal Salary { get; set; }
        public string Email { get; set; } = "";
        public DateTime HireDate { get; set; }

        // Display one employee in a readable line format.
        public override string ToString()
        {
            return $"{EmployeeID} | {EmployeeName} | {Department} | {Salary} | {Email} | {HireDate:yyyy-MM-dd}";
        }
    }
}
