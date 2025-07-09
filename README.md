# Resume Management System

## Overview
The **Resume Management System** is a backend application designed to store, process, and manage resumes efficiently. It allows users to upload PDF resumes, extract key information, and store the data in an Oracle database. The system also provides advanced search capabilities to retrieve resumes based on specific criteria.

## Key Features

### 1. Authentication & Security
- Secure API access via **JWT** authentication.
- **Role-Based Access Control (RBAC)** for Admin, HR, and Recruiter roles.
- Data encryption for sensitive information.
- Logging & auditing for tracking uploads and actions.

### 2. Resume Upload & Processing
- Uploads PDF resumes and extracts key details like **name**, **email**, **phone**, **skills**, **experience**, **education**, and **certifications**.
- Stores extracted data in **Oracle Database** and the original **PDF** in **BLOB** storage.
- Prevents duplicate resumes by checking existing records.

### 3. Resume Management & Search
- Advanced **search** and **filtering** by skills, experience, education, etc.
- **Pagination** and **sorting** for large datasets.
- View extracted resume details and **download the original PDF**.
- Logs all changes made to resumes.

### 4. Performance Optimization
- **Entity Framework Core** for efficient database interactions.
- **Redis caching** to speed up resume retrieval.
- **Asynchronous processing** for improved API performance.

## System Architecture

The system follows a **Clean Architecture** approach with the following layers:
- **API Layer**: Exposes secure RESTful APIs with JWT and RBAC.
- **Application Layer**: Implements business logic and validation.
- **Infrastructure Layer**: Handles database interactions, file storage, caching, and logging.
- **Data Layer**: Defines the database schema and manages migrations.

## Database Schema (Oracle)

- **Users Table**: Stores user credentials and roles.
- **Resumes Table**: Stores resume data and original PDF files.

## How to Use

1. **Upload Resumes**: Upload a PDF resume, which automatically extracts and stores the relevant data.
2. **Search & Filter**: Use advanced search to find resumes based on specific criteria.
3. **Role-Based Access**: Access control is based on user roles (Admin, HR, Recruiter).

