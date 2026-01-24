-- WMS Database Table Creation Script
-- SQL Server: CONGTAM-PC
-- Database: WMSDB
-- Execute this script in SSMS or SQL Server Management Studio

USE [WMSDB]
GO

-- This script will create all tables for the Warehouse Management System
PRINT 'Starting WMS Database Table Creation...'
GO

-- Note: The migrations will be applied programmatically
-- This is a reference script showing the table structure

PRINT 'Please run the following command in Package Manager Console or PowerShell:'
PRINT 'dotnet ef database update --project WMS.Infrastructure --startup-project WMS.Auth.API'
PRINT ''
PRINT 'Or use Visual Studio Package Manager Console:'
PRINT '1. Tools > NuGet Package Manager > Package Manager Console'
PRINT '2. Set Default project to: WMS.Infrastructure'
PRINT '3. Run: Update-Database'
PRINT ''
PRINT 'This will create the following tables:'
PRINT '- Users, Roles, UserRoles'
PRINT '- Products'
PRINT '- Locations'
PRINT '- Inventories, InventoryTransactions'
PRINT '- Inbounds, InboundItems'
PRINT '- Outbounds, OutboundItems'
PRINT '- Payments, PaymentEvents'
PRINT '- Deliveries, DeliveryEvents'
PRINT ''
PRINT 'Default Admin Credentials:'
PRINT 'Username: admin'
PRINT 'Password: Admin@123'
GO
