# Dependency Injection Architecture - Manam Solution

## Overview
This document describes the complete Dependency Injection (DI) implementation across the Manam solution, following SOLID principles and layered architecture patterns.

## Architecture Layers

### 1. **Manam.DatabaseClient** - Data Access Layer
Handles all database communication using Dapper.

**Abstractions** (`Abstractions/`):
- `ISqlDapperBroker` - Interface for SQL/Dapper database operations
  - Query methods
  - Execute methods
  - Single/Multiple result retrieval

**Implementations** (`Implementations/`):
- `SqlDapperBroker` - Concrete implementation of ISqlDapperBroker
  - Manages SQL connections
  - Executes stored procedures and SQL queries
  - Registered as **Singleton** (one instance for application lifetime)

**Configuration** (`Extensions/`):
```csharp
builder.Services.AddDatabaseClientServices(connectionString);
```

---

### 2. **Manam.Storage** - Repository Layer
Implements the Repository Pattern for data access abstraction.

**Abstractions** (`Abstractions/`):
- `IRepository<T>` - Generic repository interface
  - GetByIdAsync, GetAllAsync
  - CreateAsync, UpdateAsync, DeleteAsync

- `IUserRepository` - User-specific repository
  - Inherits from IRepository<User>
  - Additional methods: GetByUsernameAsync, GetByEmailAsync, etc.

**Implementations** (`Implementations/`):
- `UserRepository` - Concrete implementation
  - Depends on ISqlDapperBroker (injected via constructor)
  - Registered as **Scoped** (one instance per HTTP request)

**Configuration** (`Extensions/`):
```csharp
builder.Services.AddStorageServices();
```

---

### 3. **Manam.Services** - Business Logic Layer
Contains business logic and orchestration.

**Abstractions** (`Abstractions/`):
- `IUserService` - User business operations interface
  - GetUserByIdAsync, GetAllUsersAsync
  - CreateUserAsync, UpdateUserAsync, DeleteUserAsync
  - User DTOs mapping

**Implementations** (`Implementations/`):
- `UserService` - Concrete business logic implementation
  - Depends on IUserRepository (injected via constructor)
  - Performs validation, password hashing, data mapping
  - Registered as **Scoped** (one instance per HTTP request)

**Configuration** (`Extensions/`):
```csharp
builder.Services.AddBusinessServices();
```

---

### 4. **Manam.Auth** - Authentication Layer
Handles authentication and authorization.

**Abstractions** (`Abstractions/`):
- `IAuthenticationService` - Authentication operations interface
  - GenerateTokenAsync - JWT token generation
  - ValidateToken - Token validation

**Implementations** (`Implementations/`):
- `AuthenticationService` - Concrete authentication implementation
  - Token generation and validation
  - Registered as **Scoped** (one instance per HTTP request)

**Configuration** (`Extensions/`):
```csharp
builder.Services.AddAuthenticationServices();
```

---

## Dependency Injection Flow

```
Program.cs (Main Entry Point)
	↓
	├─→ AddDatabaseClientServices(connectionString)
	│   └─→ Registers ISqlDapperBroker → SqlDapperBroker (Singleton)
	│
	├─→ AddStorageServices()
	│   └─→ Registers IUserRepository → UserRepository (Scoped)
	│       ↓ (depends on ISqlDapperBroker)
	│
	├─→ AddBusinessServices()
	│   └─→ Registers IUserService → UserService (Scoped)
	│       ↓ (depends on IUserRepository)
	│
	└─→ AddAuthenticationServices()
		└─→ Registers IAuthenticationService → AuthenticationService (Scoped)
```

## Service Lifetimes

- **Singleton**: ISqlDapperBroker
  - Created once for application lifetime
  - Manages connection pooling efficiently

- **Scoped**: All Repository and Service implementations
  - Created once per HTTP request
  - Ensures proper resource management
  - Allows for request-level transaction handling

## Key Design Patterns

### 1. **Dependency Injection Pattern**
- Constructor injection for all dependencies
- Loose coupling between layers
- Easy to test with mock implementations

### 2. **Repository Pattern**
- Abstracts data access logic
- Generic repository for common CRUD operations
- Specific repositories extend for domain entities

### 3. **Service Layer Pattern**
- Business logic separation from data access
- DTOs for API contracts
- Data validation and transformation

### 4. **Layered Architecture**
```
┌─────────────────────────────────────┐
│       Manam.API (Presentation)      │
├─────────────────────────────────────┤
│      Manam.Services (Business)      │
├─────────────────────────────────────┤
│    Manam.Storage (Repository)       │
├─────────────────────────────────────┤
│  Manam.DatabaseClient (Data Access) │
├─────────────────────────────────────┤
│       Manam.Models (Entities)       │
└─────────────────────────────────────┘
```

## Extension Usage in Program.cs

```csharp
using Manam.API.Middleware;
using Manam.Auth.Extensions;
using Manam.DatabaseClient.Extensions;
using Manam.Services.Extensions;
using Manam.Storage.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
	?? throw new InvalidOperationException("Connection string not found.");

// Add services from all layers
builder.Services.AddDatabaseClientServices(connectionString);
builder.Services.AddStorageServices();
builder.Services.AddBusinessServices();
builder.Services.AddAuthenticationServices();

// Rest of configuration...
```

## Adding New Services

### Example: Add a new `IOrderService`

1. **In Manam.Services/Abstractions/IOrderService.cs**:
```csharp
public interface IOrderService
{
	Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
}
```

2. **In Manam.Services/Implementations/OrderService.cs**:
```csharp
public sealed class OrderService : IOrderService
{
	private readonly IOrderRepository _repository;

	public OrderService(IOrderRepository repository)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
	}

	// Implementation...
}
```

3. **Update Manam.Services/Extensions/ServicesServiceCollectionExtensions.cs**:
```csharp
public static IServiceCollection AddBusinessServices(this IServiceCollection services)
{
	services.AddScoped<IUserService, UserService>();
	services.AddScoped<IOrderService, OrderService>();  // Add this line
	return services;
}
```

## Benefits of This Architecture

✅ **Testability** - Easy to inject mock implementations for testing  
✅ **Maintainability** - Clear separation of concerns and responsibilities  
✅ **Scalability** - Easy to add new services, repositories, and layers  
✅ **Flexibility** - Can swap implementations without changing client code  
✅ **Consistency** - Uniform pattern across all services and repositories  
✅ **SOLID Principles**:
   - Single Responsibility - Each class has one reason to change
   - Open/Closed - Open for extension, closed for modification
   - Liskov Substitution - Implementations can be swapped
   - Interface Segregation - Focused, specific interfaces
   - Dependency Inversion - Depend on abstractions, not concrete types

## Configuration Files

All projects have been updated with `Microsoft.Extensions.DependencyInjection` NuGet package to support the DI pattern.

### Project Files:
- `Manam.DatabaseClient/Manam.DatabaseClient.csproj`
- `Manam.Storage/Manam.Storage.csproj`
- `Manam.Services/Manam.Services.csproj`
- `Manam.Auth/Manam.Auth.csproj`

## Next Steps

1. **Fix pre-existing issues**:
   - StoredProcedures.cs - Remove implicit operator to object
   - RequestCorrelationMiddleware.cs - Add using statement for Serilog.Context
   - ExceptionHandlingMiddleware.cs - Fix unreachable switch case

2. **Add Unit Tests** - Create test doubles for interfaces

3. **Add Configuration Services** - Implement IOptions<T> patterns for settings

4. **Add Validation Services** - Create FluentValidation integration

5. **Add Logging Services** - Integrate structured logging across layers
