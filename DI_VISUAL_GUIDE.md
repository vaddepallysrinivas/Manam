# Visual DI Architecture Diagrams

## 1. Dependency Injection Resolution Chain

```
┌─────────────────────────────────────────────────────────────┐
│                    Program.cs Entry Point                   │
│                                                              │
│  builder.Services.AddDatabaseClientServices(connString)    │
│  builder.Services.AddStorageServices()                     │
│  builder.Services.AddBusinessServices()                    │
│  builder.Services.AddAuthenticationServices()              │
└───────────────────────┬─────────────────────────────────────┘
						│
		┌───────────────┴───────────────┐
		│  DI Container Initialization   │
		└───────────────┬───────────────┘
						│
		┌───────────────┴───────────────────────────────────────┐
		│                                                       │
		▼                                                       ▼
   ┌──────────────────────────────────────┐  ┌────────────────────┐
   │  Database Access Layer (Singleton)   │  │  Service Lifetimes │
   │                                      │  │                    │
   │  ISqlDapperBroker                   │  │  • Singleton: 1×   │
   │    → SqlDapperBroker               │  │  • Scoped: Per Req │
   │                                      │  └────────────────────┘
   └──────────────────────┬───────────────┘
						  │ (injected as dependency)
		┌─────────────────┴─────────────────┐
		│   Repository Layer (Scoped)       │
		│                                    │
		│  IUserRepository                  │
		│    → UserRepository               │
		└──────────────────┬──────────────────┘
						  │ (injected as dependency)
		┌─────────────────┴────────────────────┐
		│   Business Logic Layer (Scoped)     │
		│                                      │
		│  IUserService                       │
		│    → UserService                    │
		│                                     │
		│  IAuthenticationService            │
		│    → AuthenticationService         │
		└──────────────────┬───────────────────┘
						  │ (injected as dependency)
		┌─────────────────┴──────────────────┐
		│   API Layer (Controllers)          │
		│                                    │
		│  UsersController                  │
		│  OrdersController                 │
		│  (all use injected services)       │
		└────────────────────────────────────┘
```

---

## 2. Class Hierarchy & Interfaces

```
					┌─────────────────────────────────┐
					│   Microsoft.Extensions.DI       │
					│   IServiceCollection            │
					└────────────┬────────────────────┘
								 │ (extended by)
				   ┌─────────────┴────────────────┐
				   │ Extension Methods            │
				   │ • AddDatabaseClientServices()│
				   │ • AddStorageServices()       │
				   │ • AddBusinessServices()      │
				   │ • AddAuthenticationServices()│
				   └──────────────────────────────┘


┌──────────────────────────────────────────────────────────────┐
│              DATABASE ACCESS LAYER ABSTRACTIONS              │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ISqlDapperBroker (Interface)                                │
│  ├─ ExecuteAsync(sql, params, commandType, token)          │
│  ├─ QueryAsync<T>(sql, params, commandType, token)         │
│  ├─ QuerySingleOrDefaultAsync<T>(...)                      │
│  └─ QueryFirstOrDefaultAsync<T>(...)                       │
│                                                               │
│  SqlDapperBroker : ISqlDapperBroker (Implementation)        │
│  ├─ _connectionString: string                               │
│  ├─ ctor(connectionString)                                  │
│  └─ [Implementations of all ISqlDapperBroker methods]      │
│                                                               │
└──────────────────────────────────────────────────────────────┘


┌──────────────────────────────────────────────────────────────┐
│              REPOSITORY LAYER ABSTRACTIONS                   │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  IRepository<T> (Generic Interface)                         │
│  ├─ GetByIdAsync(id, cancellationToken)                    │
│  ├─ GetAllAsync(cancellationToken)                         │
│  ├─ CreateAsync(entity, cancellationToken)                 │
│  ├─ UpdateAsync(entity, cancellationToken)                 │
│  └─ DeleteAsync(id, cancellationToken)                     │
│                                                               │
│  IUserRepository : IRepository<User> (Specialized)          │
│  ├─ GetByUsernameAsync(username, cancellationToken)        │
│  ├─ GetByEmailAsync(email, cancellationToken)              │
│  ├─ UpdateLockoutAsync(userId, lockoutUntil, token)       │
│  └─ ResetFailedLoginAttemptsAsync(userId, token)           │
│                                                               │
│  UserRepository : IUserRepository (Implementation)          │
│  ├─ _broker: ISqlDapperBroker                              │
│  ├─ ctor(ISqlDapperBroker)                                 │
│  └─ [Implements all repository methods]                    │
│                                                               │
└──────────────────────────────────────────────────────────────┘


┌──────────────────────────────────────────────────────────────┐
│             BUSINESS LOGIC LAYER ABSTRACTIONS                │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  IUserService (Interface)                                    │
│  ├─ GetUserByIdAsync(id, cancellationToken)                │
│  ├─ GetAllUsersAsync(cancellationToken)                    │
│  ├─ GetUserByUsernameAsync(username, cancellationToken)    │
│  ├─ GetUserByEmailAsync(email, cancellationToken)          │
│  ├─ CreateUserAsync(request, cancellationToken)            │
│  ├─ UpdateUserAsync(id, request, cancellationToken)        │
│  └─ DeleteUserAsync(id, cancellationToken)                 │
│                                                               │
│  UserService : IUserService (Implementation)                │
│  ├─ _userRepository: IUserRepository                        │
│  ├─ ctor(IUserRepository)                                  │
│  └─ [Business logic with validation & mapping]            │
│                                                               │
└──────────────────────────────────────────────────────────────┘


┌──────────────────────────────────────────────────────────────┐
│           AUTHENTICATION LAYER ABSTRACTIONS                  │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  IAuthenticationService (Interface)                          │
│  ├─ GenerateTokenAsync(userId, username)                   │
│  └─ ValidateToken(token)                                   │
│                                                               │
│  AuthenticationService : IAuthenticationService            │
│  └─ [Token generation & validation logic]                  │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## 3. Dependency Flow Diagram

```
Request → Controller
		   │
		   ├─ Inject IUserService
		   │  └─ UserService instance
		   │     │
		   │     ├─ Inject IUserRepository
		   │     │  └─ UserRepository instance
		   │     │     │
		   │     │     ├─ Inject ISqlDapperBroker
		   │     │     │  └─ SqlDapperBroker (Singleton)
		   │     │     │     │
		   │     │     │     ├─ Open SqlConnection
		   │     │     │     ├─ Execute Dapper Query
		   │     │     │     └─ Close Connection
		   │     │     │
		   │     │     └─ Return Data
		   │     │
		   │     └─ Process Business Logic
		   │        ├─ Validate Input
		   │        ├─ Perform Calculations
		   │        ├─ Map to DTOs
		   │        └─ Return Result
		   │
		   └─ Return Response to Client
```

---

## 4. Service Registration Timeline

```
Application Startup
│
├─ [1] Program.cs starts
│
├─ [2] WebApplication.CreateBuilder()
│      └─ Creates IServiceCollection
│
├─ [3] AddDatabaseClientServices()
│      ├─ Register: ISqlDapperBroker → Singleton
│      └─ Store SqlDapperBroker factory in container
│
├─ [4] AddStorageServices()
│      ├─ Register: IUserRepository → Scoped
│      └─ Store UserRepository factory (needs ISqlDapperBroker)
│
├─ [5] AddBusinessServices()
│      ├─ Register: IUserService → Scoped
│      └─ Store UserService factory (needs IUserRepository)
│
├─ [6] AddAuthenticationServices()
│      ├─ Register: IAuthenticationService → Scoped
│      └─ Store AuthenticationService factory
│
├─ [7] app.Build()
│      └─ Locks service registration (no more AddX calls)
│
└─ [8] app.RunAsync()
	   └─ Ready to handle requests

HTTP Request Arrives
│
├─ [1] Controller needs IUserService
│      └─ Check container
│
├─ [2] Scope created for request
│      └─ Mark as "per request" lifetime
│
├─ [3] Resolve IUserService
│      ├─ Create UserService instance
│      ├─ Needs IUserRepository
│      │  ├─ Create UserRepository instance
│      │  ├─ Needs ISqlDapperBroker
│      │  │  └─ Get Singleton instance (already created or create now)
│      │  └─ Inject into UserRepository
│      └─ Inject into UserService
│
├─ [4] Inject into Controller
│      └─ Use service
│
└─ [5] Request completes
	   └─ Dispose scoped services (repositories & services)
```

---

## 5. File Organization

```
Manam.DatabaseClient/
│
├── Abstractions/
│   └── ISqlDapperBroker.cs
│       └── Methods: Execute, Query, QuerySingle, QueryFirst
│
├── Implementations/
│   └── SqlDapperBroker.cs
│       └── Uses: SqlConnection, Dapper
│
└── Extensions/
	└── DatabaseClientServiceCollectionExtensions.cs
		└── AddDatabaseClientServices(connectionString)
			└── Registers: ISqlDapperBroker → SqlDapperBroker (Singleton)


Manam.Storage/
│
├── Abstractions/
│   ├── IRepository.cs
│   │   └── Generic CRUD methods
│   │
│   └── IUserRepository.cs
│       └── User-specific operations
│
├── Implementations/
│   └── UserRepository.cs
│       └── ctor(ISqlDapperBroker) ← Dependency injected
│
└── Extensions/
	└── StorageServiceCollectionExtensions.cs
		└── AddStorageServices()
			└── Registers: IUserRepository → UserRepository (Scoped)


Manam.Services/
│
├── Abstractions/
│   └── IUserService.cs
│       └── Business operations
│
├── Implementations/
│   └── UserService.cs
│       └── ctor(IUserRepository) ← Dependency injected
│
└── Extensions/
	└── ServicesServiceCollectionExtensions.cs
		└── AddBusinessServices()
			└── Registers: IUserService → UserService (Scoped)


Manam.Auth/
│
├── Abstractions/
│   └── IAuthenticationService.cs
│       └── Token operations
│
├── Implementations/
│   └── AuthenticationService.cs
│
└── Extensions/
	└── AuthServiceCollectionExtensions.cs
		└── AddAuthenticationServices()
			└── Registers: IAuthenticationService → AuthenticationService (Scoped)
```

---

## 6. Lifetime Behavior

```
APPLICATION LIFETIME (Singleton)
│
├─ Startup
│  └─ Create ISqlDapperBroker instance ONCE
│     └─ [============ Lives Until Shutdown ==============]
│        ├─ Request 1: Use to query database
│        ├─ Request 2: Reuse same instance
│        ├─ Request 3: Reuse same instance
│        └─ Request N: Reuse same instance
│
└─ Shutdown
   └─ Dispose ISqlDapperBroker


REQUEST LIFETIME (Scoped)
│
├─ Request 1 Starts ──────────────────────────────────────
│  ├─ Create Scope
│  ├─ Create UserService instance [just for this request]
│  ├─ Create UserRepository instance [just for this request]
│  ├─ Use ISqlDapperBroker (shared from singleton pool)
│  └─ Request ends
│     └─ Dispose UserService & UserRepository
│        (ISqlDapperBroker stays alive for next request)
│
├─ Request 2 Starts ──────────────────────────────────────
│  ├─ Create NEW Scope
│  ├─ Create NEW UserService instance [separate from Request 1]
│  ├─ Create NEW UserRepository instance [separate from Request 1]
│  ├─ Use SAME ISqlDapperBroker (singleton, shared)
│  └─ Request ends
│     └─ Dispose UserService & UserRepository
│
└─ Request 3 Starts ──────────────────────────────────────
   (Process repeats for each request)
```

---

## 7. SOLID Principles Visualization

```
┌────────────────────────────────────────────────────────────┐
│ SINGLE RESPONSIBILITY PRINCIPLE                            │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ SqlDapperBroker      → Only database queries              │
│ UserRepository       → Only user data access              │
│ UserService          → Only user business logic           │
│ AuthenticationService → Only token operations             │
│                                                             │
│ Each class has ONE reason to change                       │
└────────────────────────────────────────────────────────────┘


┌────────────────────────────────────────────────────────────┐
│ OPEN/CLOSED PRINCIPLE                                      │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ Open for Extension:                                        │
│ ├─ Add OrderService extending IUserService interface     │
│ ├─ Add PaymentRepository extending IRepository<Payment>  │
│ └─ No need to modify existing classes                    │
│                                                             │
│ Closed for Modification:                                  │
│ ├─ UserService doesn't change                           │
│ ├─ UserRepository doesn't change                        │
│ └─ Only new implementations added                        │
└────────────────────────────────────────────────────────────┘


┌────────────────────────────────────────────────────────────┐
│ LISKOV SUBSTITUTION PRINCIPLE                              │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ IUserRepository interface                                  │
│ ├─ UserRepository (database implementation)               │
│ ├─ MockUserRepository (test implementation)               │
│ ├─ CachedUserRepository (cached implementation)           │
│ └─ Any implementation works the same to UserService     │
│                                                             │
│ Implementations are interchangeable                       │
└────────────────────────────────────────────────────────────┘


┌────────────────────────────────────────────────────────────┐
│ INTERFACE SEGREGATION PRINCIPLE                            │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ IRepository<T>          → CRUD operations                 │
│ IUserRepository         → User-specific queries           │
│ ISqlDapperBroker        → Database operations             │
│ IUserService            → Business logic                  │
│ IAuthenticationService  → Token operations                │
│                                                             │
│ Each interface is small and specific                      │
│ Classes only depend on what they use                      │
└────────────────────────────────────────────────────────────┘


┌────────────────────────────────────────────────────────────┐
│ DEPENDENCY INVERSION PRINCIPLE                             │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ ✓ UserService depends on IUserRepository (abstraction)    │
│ ✗ UserService depends on UserRepository class (concrete)  │
│                                                             │
│ High-level modules (UserService) don't know about        │
│ low-level modules (specific repository implementations)   │
│                                                             │
│ Both depend on abstractions (interfaces)                  │
└────────────────────────────────────────────────────────────┘
```

---

## Summary

This visual guide shows:
1. **Resolution Chain** - How dependencies get resolved
2. **Class Hierarchy** - Interface and implementation structure
3. **Dependency Flow** - How data flows through layers
4. **Registration Timeline** - Service registration sequence
5. **File Organization** - Folder structure for each layer
6. **Lifetime Management** - Singleton vs Scoped behavior
7. **SOLID Compliance** - How principles are implemented
