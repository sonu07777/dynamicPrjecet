# Multi-Branch Inventory, Sales & Business Management System

A comprehensive enterprise-grade inventory and sales management system supporting multiple companies, branches, and user roles with complete POS functionality.

## 🌟 Features Implemented

### Backend (.NET 9.0 Web API)
- ✅ **JWT Authentication & Authorization** - Secure token-based auth
- ✅ **4 User Roles** - SuperAdmin, CompanyAdmin, BranchManager, Cashier
- ✅ **Multi-Company & Multi-Branch** - Support for multiple organizations
- ✅ **Complete Database Schema** - 15+ tables with proper relationships
- ✅ **Role-Based Access Control** - Endpoint-level permissions
- ✅ **RESTful API** - Companies, Branches, Products, Categories
- ✅ **Identity Integration** - ASP.NET Core Identity for user management
- ✅ **Swagger Documentation** - Interactive API documentation
- ✅ **Audit Logging Microservice** - Standalone `AuditLogService` with its own database that records who did what (every successful write is automatically audited)
- ✅ **Password Management** - Forgot/reset password via email link, change your own password, and admin password reset (SuperAdmin/CompanyAdmin)

### Frontend (React 18 + TypeScript)
- ✅ **Authentication Flow** - Login with JWT tokens
- ✅ **Protected Routes** - Role-based route access
- ✅ **Context API** - Global state management
- ✅ **Modern UI** - Gradient design with responsive layout
- ✅ **Role-Based Dashboard** - Different views per role
- ✅ **Navigation** - Smart navigation based on permissions

### Database (SQL Server LocalDB)
- Companies & Branches
- Users with Identity (AspNetUsers, AspNetRoles, etc.)
- Products & Categories
- Inventory (branch-level)
- Sales, SaleItems, Payments
- Purchase Orders & Items
- Stock Transfers
- Customers & Suppliers

## 📋 User Roles & Permissions

### Super Admin
- Full system access
- Manage companies and branches
- View all data across all companies
- User management

### Company Admin
- Manage all branches within their company
- Product and inventory management
- Staff management
- View company-wide reports

### Branch Manager
- Manage assigned branch
- Inventory management
- Local staff management
- Branch-level reports

### Cashier
- POS/Sales operations
- Customer management
- Payment processing
- Limited data access

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- SQL Server LocalDB (included with Visual Studio)

### Backend Setup

1. Navigate to backend directory:
```bash
cd BikeShowroom/Backend/BikeShowroomAPI
```

2. Restore packages (if needed):
```bash
dotnet restore
```

3. Database is already created and migrated. To recreate:
```bash
dotnet ef database drop
dotnet ef database update
```

4. Run the backend:
```bash
dotnet run
```

Backend runs at:
- HTTP: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

### AuditLogService (Microservice) Setup

The audit logging microservice runs independently on port `5002` with its own `AuditLogDB` database. The main API forwards audit events to it automatically.

```bash
cd BikeShowroom/Backend/AuditLogService
dotnet restore
dotnet ef database update   # creates the AuditLogDB database
dotnet run                  # runs at http://localhost:5002
```

- Swagger: `http://localhost:5002/swagger`
- Health check: `http://localhost:5002/api/auditlogs/health`
- The main API forwards every successful write (POST/PUT/DELETE) as an audit event via the `AuditService` (see `AuditService:BaseUrl` in `Backend/BikeShowroomAPI/appsettings.json`).

> **Note:** Audit log service must be running for audit events to be recorded. If it is down, the main API still works — audit failures are silently logged, never breaking business requests.

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd BikeShowroom/Frontend/bike-showroom-frontend
```

2. Install dependencies (already done):
```bash
npm install
```

3. Start development server:
```bash
npm run dev
```

Frontend runs at: `http://localhost:5173`

## 🔐 Default Login Credentials

**Super Admin Account:**
- Email: `admin@bikeshowroom.com`
- Password: `Admin@123`

## 📚 API Documentation

### Authentication Endpoints

**POST** `/api/auth/login`
```json
{
  "email": "admin@bikeshowroom.com",
  "password": "Admin@123"
}
```

**GET** `/api/auth/me` (Requires Bearer token)

**POST** `/api/auth/register` (SuperAdmin/CompanyAdmin only)

### Companies Endpoints (SuperAdmin only)

- **GET** `/api/companies` - Get all companies
- **GET** `/api/companies/{id}` - Get company by ID
- **POST** `/api/companies` - Create company
- **PUT** `/api/companies/{id}` - Update company
- **DELETE** `/api/companies/{id}` - Soft delete company

### Branches Endpoints

- **GET** `/api/branches?companyId=1` - Get branches
- **GET** `/api/branches/{id}` - Get branch by ID
- **POST** `/api/branches` - Create branch
- **PUT** `/api/branches/{id}` - Update branch
- **DELETE** `/api/branches/{id}` - Soft delete branch

### Products Endpoints

- **GET** `/api/products?companyId=1&categoryId=1` - Get products
- **GET** `/api/products/{id}` - Get product by ID
- **GET** `/api/products/search?query=bike&companyId=1` - Search products
- **POST** `/api/products` - Create product
- **PUT** `/api/products/{id}` - Update product
- **DELETE** `/api/products/{id}` - Soft delete product

### Audit Logs Endpoints (SuperAdmin/CompanyAdmin only)

- **GET** `/api/auditlogs?entityType=Product&action=Create` - List audit events (filtered via the main API gateway)
- **GET** `/api/auditlogs/{id}` - Get single audit event

Every successful write is automatically recorded by the main API and stored in the `AuditLogService` microservice.

### Password Management Endpoints

**POST** `/api/auth/forgot-password` (public)
```json
{ "email": "user@company.com" }
```
Always returns success. If the email is registered, a reset link is emailed (or logged to the server console in development when email isn't configured).

**POST** `/api/auth/reset-password` (public)
```json
{ "email": "user@company.com", "token": "reset-token-from-link", "newPassword": "NewPass@123" }
```

**POST** `/api/auth/change-password` (Requires Bearer token)
```json
{ "currentPassword": "OldPass@123", "newPassword": "NewPass@123" }
```

**POST** `/api/users/{id}/reset-password` (SuperAdmin/CompanyAdmin only — CompanyAdmin is scoped to their own company)
```json
{ "newPassword": "NewPass@123" }
```

### Email Configuration

Password reset links are sent by SMTP. Configure the `Email` section in `Backend/BikeShowroomAPI/appsettings.json`:

```json
{
  "Email": {
    "Enabled": true,
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "you@gmail.com",
    "Password": "your-app-password",
    "From": "you@gmail.com",
    "FromName": "BikeShowroom",
    "EnableSsl": true
  },
  "App": {
    "FrontendBaseUrl": "http://localhost:5173"
  }
}
```

For Gmail use an [App Password](https://support.google.com/accounts/answer/185833). If `Enabled` is `false` (or sending fails in development), the reset link is logged to the server console so the flow still works locally.

## 🗄️ Database Schema

### Core Tables
- **Companies** - Organization/company data
- **Branches** - Branch locations per company
- **AspNetUsers** - User accounts with Identity
- **AspNetRoles** - User roles (SuperAdmin, CompanyAdmin, etc.)
- **Products** - Product master data
- **Categories** - Hierarchical categories
- **Inventory** - Branch-level stock tracking
- **Sales** - Sale transactions
- **SaleItems** - Line items for sales
- **Customers** - Customer information
- **Suppliers** - Supplier information
- **PurchaseOrders** - Purchase order management
- **StockTransfers** - Inter-branch transfers

## 🏗️ Project Structure

```
BikeShowroom/
├── Backend/
│   ├── BikeShowroomAPI/        # Main API (gateway, port 5000)
│   │   ├── Controllers/        # Thin API endpoints
│   │   ├── Models/            # Database models
│   │   ├── DTOs/              # Data Transfer Objects
│   │   ├── Data/              # DbContext
│   │   ├── Services/          # Business logic — one service per domain
│   │   │   ├── AuthService, EmailService
│   │   │   ├── CompanyService, BranchService, ProductService, CategoryService, InventoryService
│   │   │   ├── UserService    # Includes CompanyAdmin company scoping
│   │   │   └── AuditService   # Microservice client
│   │   ├── Filters/           # MVC filters (AuditActionFilter)
│   │   └── Migrations/        # EF Core migrations
│   └── AuditLogService/        # Audit logging microservice (port 5002)
│       ├── Controllers/        # /api/auditlogs endpoints
│       ├── Models/             # AuditLog entity
│       ├── Data/               # AuditLogDbContext (own AuditLogDB)
│       ├── DTOs/               # CreateAuditLogRequest
│       └── Migrations/         # EF Core migrations
└── Frontend/
    └── bike-showroom-frontend/
        ├── src/
        │   ├── components/     # Reusable components (Navbar, ProtectedRoute)
        │   ├── pages/          # Page components, organized by role:
        │   │   ├── public/          # Login, Forgot/Reset Password
        │   │   ├── shared/          # Dashboard
        │   │   ├── superadmin/      # Companies
        │   │   ├── companyadmin/    # Branches, Users, Audit Logs
        │   │   ├── branchmanager/   # Products, Categories, Inventory, Suppliers, POs, Transfers, Reports
        │   │   └── cashier/         # POS, Customers
        │   ├── routes/         # roleRoutes.tsx — single source of truth for who can access which page
        │   ├── store/          # Redux Toolkit (RTK Query slices)
        │   └── types/          # TypeScript types
        └── public/
```

## 🔧 Configuration

### Backend Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MultiBranchInventoryDB;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "YourSecureSecretKeyHere_ChangeThisInProduction_MinimumLength32Characters!",
    "Issuer": "BikeShowroomAPI",
    "Audience": "BikeShowroomClient"
  }
}
```

### Frontend Configuration (src/services/api.ts)

```typescript
const API_BASE_URL = 'http://localhost:5000/api';
```

## ✅ Features Completed

- [x] **Inventory Management UI** - View and adjust stock levels per branch with low-stock alerts
- [x] **Point of Sale (POS) System** - Full POS with cart, discounts, tax, and payment processing
- [x] **Customers Management** - Full CRUD with search and customer types (Regular, Wholesale, VIP)
- [x] **Suppliers Management** - Full CRUD with search, contact management
- [x] **Purchase Orders** - Create, manage PO lifecycle (Pending → Ordered → Received/Cancelled)
- [x] **Stock Transfers** - Inter-branch stock transfers with status tracking (Pending → InTransit → Received/Cancelled)
- [x] **Reporting System** - Sales stats, KPIs, low-stock alerts, recent sales view
- [x] **Analytics Dashboard** - Revenue, sales counts, average sale amounts
- [x] **User Management UI** - Create users, assign roles, activate/deactivate, inline role changes
- [x] **Categories Management** - Hierarchical categories with parent/child support
- [x] **Companies Management** - Full CRUD for companies (SuperAdmin only)
- [x] **Branches Management** - Full CRUD for branches per company

## 🔐 Security Features

- JWT token authentication
- Password hashing with Identity
- Role-based authorization
- Protected API endpoints
- CORS configuration
- Secure password requirements
- Login blocked for users whose company or branch has been deactivated
- CompanyAdmin user management enforced server-side (can only see/manage users of their own company)

## 📱 Responsive Design

The frontend is fully responsive and works on:
- Desktop (1920px+)
- Laptop (1024px+)
- Tablet (768px+)
- Mobile (320px+)

## 🛠️ Tech Stack

### Backend
- .NET 9.0
- Entity Framework Core 9.0
- ASP.NET Core Identity
- JWT Bearer Authentication
- SQL Server
- Swashbuckle (Swagger)

### Frontend
- React 18
- TypeScript
- React Router v6
- Axios
- Context API
- Vite
- CSS3

## 📝 Development Notes

### Adding New Roles

1. Add role name to seeding in `Program.cs`
2. Update `AuthContext` in frontend
3. Add role to protected routes
4. Update navigation menu logic

### Adding New Entities

1. Create model in `Models/` folder
2. Add DbSet to `BikeShowroomContext`
3. Configure relationships in `OnModelCreating`
4. Create migration: `dotnet ef migrations add AddNewEntity`
5. Apply migration: `dotnet ef database update`
6. Create DTOs in `DTOs/` folder
7. Create controller in `Controllers/` folder
8. Add TypeScript types in frontend
9. Create API service methods
10. Build UI components

### Database Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Drop database
dotnet ef database drop
```

## 🐛 Troubleshooting

### Backend Issues

**Port already in use:**
- Change port in `Properties/launchSettings.json`

**Database connection error:**
- Ensure SQL Server LocalDB is installed
- Check connection string in `appsettings.json`

**JWT token invalid:**
- Check JWT configuration in `appsettings.json`
- Ensure token hasn't expired

### Frontend Issues

**API connection error:**
- Ensure backend is running on port 5000
- Check CORS settings in backend

**Login fails:**
- Use correct credentials
- Check browser console for errors
- Verify backend is running

## 📈 Performance Considerations

- Database indexes on frequently queried fields
- JWT token caching in localStorage
- Lazy loading for large datasets
- Pagination for API responses
- Optimized queries with EF Core

## 🔄 Future Enhancements

- Real-time inventory updates (SignalR)
- Barcode scanning integration
- Receipt printing
- Advanced reporting with charts
- Email notifications
- Mobile app (React Native)
- Multi-language support
- Dark mode
- Export to Excel/PDF
- Backup & restore functionality
- Audit logging
- Two-factor authentication

## 📄 License

This project is created for educational purposes.

## 👥 Support

For issues and questions:
- Check the Swagger documentation at `/swagger`
- Review this README
- Check the PRD in the README for feature requirements

---

**Built with ❤️ using .NET 9.0 & React 18**

*Last Updated: July 29, 2026*
