# Mini Logistics Management System

A comprehensive ASP.NET Core MVC web application designed for managing end-to-end logistics operations. The system handles product inventory, customer order processing, shipment tracking, and automated delivery status updates.

**Live Demo**: [https://mini-logistics-portal-2026-bugmg6ehebc7eqdy.westus3-01.azurewebsites.net](https://mini-logistics-portal-2026-bugmg6ehebc7eqdy.westus3-01.azurewebsites.net)

## Features

- **Product Management**: Create, view, and manage the product catalog with real-time stock level tracking and low-stock indicators.
- **Order Processing**: Complete order creation workflow with cart functionality, server-side validation, and automatic stock deduction.
- **Shipment Management**: View pending shipments, assign drivers, and generate unique tracking codes.
- **Real-Time Tracking**: Public-facing tracking portal allowing customers to monitor their shipment status without authentication.
- **Background Processing**: Automated hosted service that continuously monitors and marks shipments as "Delivered" when the estimated delivery time is reached.
- **Dynamic Search**: Real-time, debounced search functionality across products, orders, and shipments that intelligently ignores spaces and special characters.

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: Microsoft SQL Server with Entity Framework Core 8.0
- **Frontend**: Razor Views, Bootstrap 5, Bootstrap Icons, Vanilla JavaScript (Fetch API)
- **Background Services**: .NET Hosted Services (IHostedService)
- **Hosting**: Microsoft Azure App Service

## Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK
- Microsoft SQL Server 2019 or later (or SQL Server Express)
- SQL Server Management Studio (SSMS) - optional but recommended

## Installation and Setup

### 1. Clone the Repository

```bash
[git clone https://github.com//logistics.git](https://github.com/usman-7455/logistics.git)
cd logistics
```

### 2. Configure Database Connection String

Open the `appsettings.json` file in the root directory and update the connection string to match your local SQL Server environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=LogisticsDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Replace `YOUR_SERVER_NAME` with your SQL Server instance name:
- For local SQL Server: `(localdb)\mssqllocaldb`, `localhost`, or `.`
- For SQL Server Express: `localhost\SQLEXPRESS`

### 3. Apply Entity Framework Core Migrations

The project includes pre-configured migrations. To apply them and create the database:

**Using Visual Studio Package Manager Console:**
```powershell
# Ensure 'logistics' is selected as the default project
Update-Database
```

**Using .NET CLI:**
```bash
cd logistics
dotnet ef database update
```

This will create the `LogisticsDB` database with all required tables: Products, Customers, Orders, OrderItems, and Shipments.

### 4. Build and Run the Application

**Using Visual Studio:**
1. Open `logistics.sln` in Visual Studio.
2. Press F5 or click the Run button.
3. The application will launch in your default web browser.

**Using .NET CLI:**
```bash
cd logistics
dotnet build
dotnet run
```

The application will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Database Schema

- **Products**: Id, Name, Price, StockQuantity
- **Customers**: Id, FullName, Email, PhoneNumber
- **Orders**: Id, CustomerId, OrderDate, Status, TotalAmount
- **OrderItems**: Id, OrderId, ProductId, Quantity, UnitPrice
- **Shipments**: Id, OrderId, TrackingCode, DriverName, ShipmentStatus, EstimatedDeliveryTime

## Background Services

### Automatic Delivery Completion

The application includes a background service (`ShipmentDeliveryService`) that operates continuously:
- Executes a check every 1 minute.
- Queries the database for shipments with a status of "OutForDelivery" where the `EstimatedDeliveryTime` has passed.
- Automatically updates the shipment status to "Delivered".
- Cascades the status update to the related Order record.
- Logs all actions for auditing and monitoring purposes.

## Application Pages

- **Products**: View all products with dynamic search and stock status indicators. Add new products to the inventory.
- **Orders**: View all historical and active orders with customer details and tracking codes. Create new orders via a multi-step cart interface.
- **Shipments**: View pending shipments requiring driver assignment. Assign drivers and set estimated delivery times.
- **Track Package**: Public-facing page for entering a tracking code and viewing real-time shipment details, driver information, and package contents.

## Key Features Explained

### Dynamic Search
All list pages feature real-time search that:
- Filters results as the user types (300ms debounce to prevent server overload).
- Intelligently ignores spaces and special characters in the search query.
- Searches across multiple fields simultaneously (e.g., customer name, order ID).

### Stock Management
- Automatic stock deduction when orders are successfully created.
- Prevention of orders exceeding available stock via transaction rollback.
- Visual indicators for low stock (less than 5 units) and out-of-stock items.

### Order Workflow
1. User selects products and quantities in the cart.
2. System validates stock availability.
3. Order is created with "Pending" status.
4. Stock is automatically deducted.
5. Shipment is created with "InTransit" status.
6. Admin assigns a driver and sets the estimated delivery time.
7. Shipment status changes to "OutForDelivery".
8. Background service auto-completes the delivery at the scheduled time.

### Tracking System
- Unique tracking codes generated automatically upon driver assignment.
- Customers can track shipments without requiring authentication.
- Displays current status, estimated delivery time, driver name, and package contents.

## Deployment

The application is configured for seamless deployment to Microsoft Azure App Service.

1. Right-click the project in Visual Studio and select "Publish".
2. Choose "Azure App Service" (Windows).
3. Follow the wizard to configure the resource group and app service plan.
4. Update the Connection String in the Azure Portal Configuration settings to point to the production SQL Database.
5. Ensure Entity Framework Core migrations are applied to the production database.

## Project Structure

```text
logistics/
├── Controllers/
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   └── ShipmentsController.cs
├── Models/
│   ├── Product.cs
│   ├── Customer.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Shipment.cs
├── ViewModels/
│   ├── CreateProductViewModel.cs
│   ├── OrderCreateViewModel.cs
│   ├── AssignDriverViewModel.cs
│   └── TrackingResultViewModel.cs
├── Services/
│   ├── IProductService.cs
│   ├── IOrderService.cs
│   ├── IShipmentService.cs
│   ├── ProductService.cs
│   ├── OrderService.cs
│   ├── ShipmentService.cs
│   └── Background/
│       └── ShipmentDeliveryService.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Migrations/
│   └── (EF Core migrations)
├── Views/
│   ├── Products/
│   ├── Orders/
│   ├── Shipments/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   └── js/
├── appsettings.json
├── Program.cs
└── logistics.csproj
```
