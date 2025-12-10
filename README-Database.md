# Database Setup Guide

## Connection Information
The application is now configured to connect to your SQL Server database using the connection string in `appsettings.json`.

## Database: IT_13FinalProject
**Server:** LAPTOP-HN26GHDH  
**Database:** IT_13FinalProject  
**Authentication:** Windows Integrated Security

## Features Implemented
1. **User Account Management** - Full CRUD operations for user accounts
2. **Password Security** - BCrypt password hashing
3. **Database Initialization** - Automatic seeding of default users
4. **Account Creation** - Create Account page now saves to database
5. **User Authentication** - Login page validates against database

## Default Users (automatically created)
- **Admin** - Username: `Admin`, Password: `12345`
- **Doctor** - Username: `Doctor`, Password: `12345`
- **Nurse** - Username: `Nurse`, Password: `12345`
- **Billing Staff** - Username: `Billing`, Password: `12345`
- **Inventory Staff** - Username: `Inventory`, Password: `12345`

## How to Test
1. Run the application
2. Navigate to Create Account page (`/create-account`)
3. Fill in the form and submit
4. The new user will be saved to the SQL Server database
5. You can then login with the new credentials

## Database Schema
The `Users` table contains:
- Id (int, Primary Key)
- Username (nvarchar(50), Unique)
- Email (nvarchar(100), Unique)
- Password (nvarchar(255), Hashed)
- FirstName (nvarchar(50))
- LastName (nvarchar(50))
- Role (nvarchar(20))
- CreatedAt (datetime)
- UpdatedAt (datetime)
- IsActive (bit)

## Next Steps
- The database will be automatically created on first run
- All user operations now persist to SQL Server
- Passwords are securely hashed using BCrypt
- The application maintains backward compatibility with existing components
