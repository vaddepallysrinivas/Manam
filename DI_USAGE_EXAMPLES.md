# DI Implementation Examples

## How to Use the DI Container

### Example 1: Using Services in a Controller

```csharp
using Manam.Models;
using Manam.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Manam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IAuthenticationService _authService;

	// Constructor Injection - Dependencies are automatically resolved
	public UsersController(IUserService userService, IAuthenticationService authService)
	{
		_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		_authService = authService ?? throw new ArgumentNullException(nameof(authService));
	}

	// GET: api/users/{id}
	[HttpGet("{id}")]
	public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken cancellationToken)
	{
		var user = await _userService.GetUserByIdAsync(id, cancellationToken);

		if (user == null)
			return NotFound();

		return Ok(user);
	}

	// GET: api/users
	[HttpGet]
	public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
	{
		var users = await _userService.GetAllUsersAsync(cancellationToken);
		return Ok(users);
	}

	// POST: api/users
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateUser(
		[FromBody] CreateUserRequest request,
		CancellationToken cancellationToken)
	{
		var userId = await _userService.CreateUserAsync(request, cancellationToken);
		return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
	}

	// PUT: api/users/{id}
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateUser(
		Guid id,
		[FromBody] UpdateUserRequest request,
		CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateUserAsync(id, request, cancellationToken);

		if (!result)
			return NotFound();

		return NoContent();
	}

	// DELETE: api/users/{id}
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
	{
		var result = await _userService.DeleteUserAsync(id, cancellationToken);

		if (!result)
			return NotFound();

		return NoContent();
	}

	// POST: api/users/login
	[HttpPost("login")]
	public async Task<ActionResult<string>> Login(
		[FromBody] LoginRequest request,
		CancellationToken cancellationToken)
	{
		// Verify user credentials
		var user = await _userService.GetUserByUsernameAsync(request.Username, cancellationToken);

		if (user == null)
			return Unauthorized("Invalid credentials");

		// Generate token using injected auth service
		var token = await _authService.GenerateTokenAsync(user.Id, user.Username);

		return Ok(new { token });
	}
}
```

### Example 2: Middleware Usage

```csharp
using Manam.Services.Abstractions;

namespace Manam.API.Middleware;

public class AuthenticationMiddleware
{
	private readonly RequestDelegate _next;

	public AuthenticationMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context, IAuthenticationService authService)
	{
		// Middleware can also use constructor injection
		// DI container passes authService automatically

		var token = context.Request.Headers["Authorization"].ToString();

		if (!string.IsNullOrEmpty(token))
		{
			var isValid = authService.ValidateToken(token);
			if (!isValid)
			{
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("Invalid token");
				return;
			}
		}

		await _next(context);
	}
}
```

### Example 3: Adding New Service with DI

**Step 1: Create the interface** (`Manam.Services/Abstractions/IOrderService.cs`)
```csharp
using Manam.Models;

namespace Manam.Services.Abstractions;

public interface IOrderService
{
	Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
	Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
	Task<bool> UpdateOrderAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default);
	Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**Step 2: Create the repository interface** (`Manam.Storage/Abstractions/IOrderRepository.cs`)
```csharp
using Manam.Models;

namespace Manam.Storage.Abstractions;

public interface IOrderRepository : IRepository<Order>
{
	Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
```

**Step 3: Implement the repository** (`Manam.Storage/Implementations/OrderRepository.cs`)
```csharp
using Manam.DatabaseClient;
using Manam.DatabaseClient.Abstractions;
using Manam.Models;
using Manam.Storage.Abstractions;
using System.Data;

namespace Manam.Storage.Implementations;

public sealed class OrderRepository : IOrderRepository
{
	private readonly ISqlDapperBroker _broker;

	public OrderRepository(ISqlDapperBroker broker)
	{
		_broker = broker ?? throw new ArgumentNullException(nameof(broker));
	}

	public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _broker.QuerySingleOrDefaultAsync<Order>(
			"sp_GetOrderById",
			new { orderId = id },
			CommandType.StoredProcedure,
			cancellationToken);
	}

	public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _broker.QueryAsync<Order>(
			"sp_GetOrdersByUserId",
			new { userId },
			CommandType.StoredProcedure,
			cancellationToken);
	}

	// ... implement other IRepository<T> methods
}
```

**Step 4: Implement the service** (`Manam.Services/Implementations/OrderService.cs`)
```csharp
using Manam.Models;
using Manam.Services.Abstractions;
using Manam.Storage.Abstractions;

namespace Manam.Services.Implementations;

public sealed class OrderService : IOrderService
{
	private readonly IOrderRepository _repository;
	private readonly IUserRepository _userRepository;

	public OrderService(IOrderRepository repository, IUserRepository userRepository)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
	}

	public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var order = await _repository.GetByIdAsync(id, cancellationToken);
		return MapToDto(order);
	}

	public async Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
	{
		// Validate user exists
		var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
		if (user == null)
			throw new InvalidOperationException("User not found");

		var order = new Order
		{
			Id = Guid.NewGuid(),
			UserId = request.UserId,
			// ... other properties
		};

		await _repository.CreateAsync(order, cancellationToken);
		return order.Id;
	}

	// ... implement other methods
}
```

**Step 5: Register in extension method** (Update `Manam.Services/Extensions/ServicesServiceCollectionExtensions.cs`)
```csharp
using Manam.Services.Abstractions;
using Manam.Services.Implementations;
using Manam.Storage.Abstractions;
using Manam.Storage.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Manam.Services.Extensions;

public static class ServicesServiceCollectionExtensions
{
	public static IServiceCollection AddBusinessServices(this IServiceCollection services)
	{
		// Existing services
		services.AddScoped<IUserService, UserService>();

		// New services
		services.AddScoped<IOrderService, OrderService>();
		services.AddScoped<IOrderRepository, OrderRepository>();

		return services;
	}
}
```

**Step 6: Update Storage extension** (Update `Manam.Storage/Extensions/StorageServiceCollectionExtensions.cs`)
```csharp
public static IServiceCollection AddStorageServices(this IServiceCollection services)
{
	services.AddScoped<IUserRepository, UserRepository>();
	services.AddScoped<IOrderRepository, OrderRepository>();

	return services;
}
```

### Example 4: Unit Testing with DI

```csharp
using Moq;
using Xunit;
using Manam.Models;
using Manam.Services.Abstractions;
using Manam.Services.Implementations;
using Manam.Storage.Abstractions;

namespace Manam.Tests;

public class UserServiceTests
{
	[Fact]
	public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var expectedUser = new User
		{
			Id = userId,
			Username = "testuser",
			Email = "test@example.com",
			IsActive = true
		};

		var mockRepository = new Mock<IUserRepository>();
		mockRepository
			.Setup(r => r.GetByIdAsync(userId, default))
			.ReturnsAsync(expectedUser);

		var service = new UserService(mockRepository.Object);

		// Act
		var result = await service.GetUserByIdAsync(userId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(userId, result.Id);
		Assert.Equal("testuser", result.Username);

		// Verify the repository was called
		mockRepository.Verify(r => r.GetByIdAsync(userId, default), Times.Once);
	}

	[Fact]
	public async Task CreateUserAsync_WithValidRequest_ReturnsUserId()
	{
		// Arrange
		var request = new CreateUserRequest
		{
			Username = "newuser",
			Email = "new@example.com",
			Password = "SecurePassword123",
			FirstName = "John",
			LastName = "Doe"
		};

		var mockRepository = new Mock<IUserRepository>();
		mockRepository
			.Setup(r => r.GetByUsernameAsync(request.Username, default))
			.ReturnsAsync((User)null);

		mockRepository
			.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
			.ReturnsAsync(1);

		var service = new UserService(mockRepository.Object);

		// Act
		var userId = await service.CreateUserAsync(request);

		// Assert
		Assert.NotEqual(Guid.Empty, userId);

		// Verify both methods were called
		mockRepository.Verify(r => r.GetByUsernameAsync(request.Username, default), Times.Once);
		mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), default), Times.Once);
	}

	[Fact]
	public async Task CreateUserAsync_WithDuplicateUsername_ThrowsException()
	{
		// Arrange
		var existingUser = new User { Username = "existing" };
		var request = new CreateUserRequest { Username = "existing" };

		var mockRepository = new Mock<IUserRepository>();
		mockRepository
			.Setup(r => r.GetByUsernameAsync(request.Username, default))
			.ReturnsAsync(existingUser);

		var service = new UserService(mockRepository.Object);

		// Act & Assert
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => service.CreateUserAsync(request));
	}
}
```

### Example 5: Program.cs Configuration

```csharp
using Manam.API.Middleware;
using Manam.Auth.Extensions;
using Manam.DatabaseClient.Extensions;
using Manam.Services.Extensions;
using Manam.Storage.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console()
	.WriteTo.File("logs/manam-{Date}.txt", rollingInterval: RollingInterval.Day)
	.Enrich.FromLogContext()
	.CreateLogger();

builder.Host.UseSerilog();

// Build configuration
var config = builder.Configuration;
var connectionString = config.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add DI services
builder.Services.AddDatabaseClientServices(connectionString);
builder.Services.AddStorageServices();
builder.Services.AddBusinessServices();
builder.Services.AddAuthenticationServices();

// Add CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader();
	});
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

await app.RunAsync();
```

## Summary

The DI implementation follows these principles:

1. ✅ **Depend on abstractions**, not concrete types
2. ✅ **Inject dependencies** via constructor
3. ✅ **Use extension methods** to register services
4. ✅ **Separate interfaces from implementations**
5. ✅ **Organize by responsibility** (Abstractions, Implementations, Extensions folders)
6. ✅ **Enable easy testing** with mock implementations
7. ✅ **Support scalability** by following consistent patterns
