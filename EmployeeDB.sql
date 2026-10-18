-- =============================================
-- EmployeeDB.sql
-- Database script for Employee Management System
-- Run this script in SQL Server Management Studio (SSMS)
-- or using sqlcmd to create database, table and
-- stored procedures.
-- =============================================

-- Create database if it does not exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EmployeeDB')
BEGIN
    CREATE DATABASE EmployeeDB;
END
GO

USE EmployeeDB;
GO

-- =============================================
-- Create Employee table
-- =============================================
IF OBJECT_ID('dbo.Employee', 'U') IS NOT NULL
    DROP TABLE dbo.Employee;
GO

CREATE TABLE dbo.Employee (
    EmployeeID INT PRIMARY KEY,          -- Primary Key, no duplicates allowed
    EmployeeName VARCHAR(100) NOT NULL,  -- Employee full name
    Department VARCHAR(50) NOT NULL,     -- Department name
    Salary DECIMAL(10, 2) NULL,          -- Monthly/Annual salary
    Email VARCHAR(100) NULL,             -- Email address
    HireDate DATE NULL                   -- Date of joining
);
GO

-- =============================================
-- 1. sp_InsertEmployee - Insert a new employee
-- =============================================
IF OBJECT_ID('dbo.sp_InsertEmployee', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertEmployee;
GO

CREATE PROCEDURE dbo.sp_InsertEmployee
    @EmployeeID INT,
    @EmployeeName VARCHAR(100),
    @Department VARCHAR(50),
    @Salary DECIMAL(10, 2),
    @Email VARCHAR(100),
    @HireDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Employee (EmployeeID, EmployeeName, Department, Salary, Email, HireDate)
    VALUES (@EmployeeID, @EmployeeName, @Department, @Salary, @Email, @HireDate);
END
GO

-- =============================================
-- 2. sp_GetAllEmployees - Get all employees
-- =============================================
IF OBJECT_ID('dbo.sp_GetAllEmployees', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAllEmployees;
GO

CREATE PROCEDURE dbo.sp_GetAllEmployees
AS
BEGIN
    SET NOCOUNT ON;

    SELECT EmployeeID, EmployeeName, Department, Salary, Email, HireDate
    FROM dbo.Employee
    ORDER BY EmployeeID;
END
GO

-- =============================================
-- 3. sp_GetEmployeeByID - Search employee by ID
-- =============================================
IF OBJECT_ID('dbo.sp_GetEmployeeByID', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetEmployeeByID;
GO

CREATE PROCEDURE dbo.sp_GetEmployeeByID
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT EmployeeID, EmployeeName, Department, Salary, Email, HireDate
    FROM dbo.Employee
    WHERE EmployeeID = @EmployeeID;
END
GO

-- =============================================
-- 4. sp_UpdateEmployee - Update employee details
-- =============================================
IF OBJECT_ID('dbo.sp_UpdateEmployee', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateEmployee;
GO

CREATE PROCEDURE dbo.sp_UpdateEmployee
    @EmployeeID INT,
    @EmployeeName VARCHAR(100),
    @Department VARCHAR(50),
    @Salary DECIMAL(10, 2),
    @Email VARCHAR(100),
    @HireDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Employee
    SET EmployeeName = @EmployeeName,
        Department = @Department,
        Salary = @Salary,
        Email = @Email,
        HireDate = @HireDate
    WHERE EmployeeID = @EmployeeID;
END
GO

-- =============================================
-- 5. sp_DeleteEmployee - Delete employee by ID
-- =============================================
IF OBJECT_ID('dbo.sp_DeleteEmployee', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DeleteEmployee;
GO

CREATE PROCEDURE dbo.sp_DeleteEmployee
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Employee
    WHERE EmployeeID = @EmployeeID;
END
GO

-- =============================================
-- 6. sp_CheckDuplicateEmployeeID - Check if ID exists
-- Returns 1 if exists, 0 if not exists
-- =============================================
IF OBJECT_ID('dbo.sp_CheckDuplicateEmployeeID', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CheckDuplicateEmployeeID;
GO

CREATE PROCEDURE dbo.sp_CheckDuplicateEmployeeID
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE EmployeeID = @EmployeeID)
        SELECT 1;  -- ID already exists
    ELSE
        SELECT 0;  -- ID does not exist
END
GO
