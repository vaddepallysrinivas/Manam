# Before & After: Code Quality Improvements

## Example 1: UserRepository.GetByIdAsync()

### ❌ BEFORE (Minimal/Brittle)
```csharp
public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
	var parameters = new { userId = id };

	return await _broker.QuerySingleOrDefaultAsync<User>(
		StoredProcedures.SpGetUserById,
		parameters,
		CommandType.StoredProcedure,
		cancellationToken);
}
```

**Issues:**
- No logging - can't diagnose issues in production
- No error handling - exceptions bubble up uncontrolled
- Parameter name mismatch (`userId` vs. SQL procedure expects `Id`)
- No documentation
- Silent failures (returns null without context)

### ✅ AFTER (Production-Ready)
```csharp
/// <summary>
/// Gets a user by ID from the database.
/// </summary>
public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
	try
	{
		_logger.LogDebug("Fetching user with ID: {UserId}", id);
		var parameters = new { Id = id };

		var user = await _broker.QuerySingleOrDefaultAsync<User>(
			StoredProcedures.SpGetUserById,
			parameters,
			CommandType.StoredProcedure,
			cancellationToken);

		if (user != null)
			_logger.LogDebug("User found: {Username}", user.Username);
		else
			_logger.LogDebug("User not found with ID: {UserId}", id);

		return user;
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
		throw;
	}
}
```

**Improvements:**
- ✅ Clear documentation
- ✅ Structured logging at multiple levels (Debug, Error)
- ✅ Exception handling with context-aware error logging
- ✅ Correct parameter naming (`Id` matches SQL procedure)
- ✅ Diagnostic output (found/not found outcomes visible in logs)

---

## Example 2: UserService.CreateUserAsync()

### ❌ BEFORE (Minimal Validation)
```csharp
public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
{
	if (string.IsNullOrWhiteSpace(request.Username) ||
		string.IsNullOrWhiteSpace(request.Email) ||
		string.IsNullOrWhiteSpace(request.Password))
	{
		throw new ArgumentException("Username, Email, and Password are required.");
	}

	// Check if user already exists
	var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
	if (existingUser != null)
	{
		throw new InvalidOperationException("Username already exists.");
	}

	var userId = Guid.NewGuid();
	var passwordHash = HashPassword(request.Password);

	var user = new User
	{
		Id = userId,
		Username = request.Username,
		Email = request.Email,
		PasswordHash = passwordHash,
		FirstName = request.FirstName,
		LastName = request.LastName,
		IsActive = true,
		CreatedAt = DateTime.UtcNow,
		UpdatedAt = DateTime.UtcNow
	};

	await _userRepository.CreateAsync(user, cancellationToken);
	return userId;
}
```

**Issues:**
- Checks only USERNAME duplicate, not EMAIL
- No password strength requirements
- No logging
- First/LastName could be null (not safe)
- Missing security fields (FailedLoginAttempts, LockoutUntil)
- Generic error messages

### ✅ AFTER (Enterprise-Grade)
```csharp
/// <summary>
/// Creates a new user with validation and duplicate checking.
/// </summary>
public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
{
	try
	{
		// Validate input
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		if (string.IsNullOrWhiteSpace(request.Username))
			throw new ArgumentException("Username is required.", nameof(request.Username));

		if (string.IsNullOrWhiteSpace(request.Email))
			throw new ArgumentException("Email is required.", nameof(request.Email));

		if (string.IsNullOrWhiteSpace(request.Password))
			throw new ArgumentException("Password is required.", nameof(request.Password));

		if (request.Password.Length < 8)
			throw new ArgumentException("Password must be at least 8 characters long.", nameof(request.Password));

		// Check if username already exists
		_logger.LogDebug("Checking if username already exists: {Username}", request.Username);
		var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
		if (existingUserByUsername != null)
		{
			_logger.LogWarning("Username already exists: {Username}", request.Username);
			throw new InvalidOperationException($"Username '{request.Username}' already exists.");
		}

		// Check if email already exists
		_logger.LogDebug("Checking if email already exists");
		var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
		if (existingUserByEmail != null)
		{
			_logger.LogWarning("Email already registered");
			throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
		}

		var userId = Guid.NewGuid();
		var passwordHash = HashPassword(request.Password);
		var now = DateTime.UtcNow;

		var user = new User
		{
			Id = userId,
			Username = request.Username,
			Email = request.Email,
			PasswordHash = passwordHash,
			FirstName = request.FirstName ?? string.Empty,
			LastName = request.LastName ?? string.Empty,
			IsActive = true,
			FailedLoginAttempts = 0,
			LockoutUntil = null,
			CreatedAt = now,
			UpdatedAt = now
		};

		_logger.LogInformation("Creating new user: {Username}", request.Username);
		await _userRepository.CreateAsync(user, cancellationToken);
		_logger.LogInformation("User created successfully: {UserId}", userId);
		return userId;
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error creating user");
		throw;
	}
}
```

**Improvements:**
- ✅ Null check on request object
- ✅ Individual field validation with specific error messages
- ✅ Minimum password length enforcement (8 characters)
- ✅ Duplicate EMAIL check (was missing)
- ✅ Specific, user-friendly error messages
- ✅ Comprehensive logging (Debug for checks, Warning for conflicts, Info for success)
- ✅ Null-coalescing for optional fields (FirstName, LastName)
- ✅ Security field initialization (FailedLoginAttempts, LockoutUntil)
- ✅ Consistent timestamp handling
- ✅ Exception logging with full context

---

## Example 3: UserService.UpdateUserAsync()

### ❌ BEFORE (Naive Updates)
```csharp
public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
	var user = await _userRepository.GetByIdAsync(id, cancellationToken);
	if (user == null)
	{
		return false;
	}

	if (!string.IsNullOrWhiteSpace(request.Email))
		user.Email = request.Email;

	if (!string.IsNullOrWhiteSpace(request.FirstName))
		user.FirstName = request.FirstName;

	if (!string.IsNullOrWhiteSpace(request.LastName))
		user.LastName = request.LastName;

	if (request.IsActive.HasValue)
		user.IsActive = request.IsActive.Value;

	user.UpdatedAt = DateTime.UtcNow;

	var result = await _userRepository.UpdateAsync(user, cancellationToken);
	return result > 0;
}
```

**Issues:**
- No validation that new email isn't already taken
- Updates timestamp even if nothing changed
- No logging of what changed
- Silent null return makes debugging hard
- Always saves to DB even if no actual changes

### ✅ AFTER (Smart Updates)
```csharp
/// <summary>
/// Updates an existing user with selective field updates.
/// </summary>
public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
	try
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		_logger.LogDebug("Fetching user for update: {UserId}", id);
		var user = await _userRepository.GetByIdAsync(id, cancellationToken);
		if (user == null)
		{
			_logger.LogWarning("User not found for update: {UserId}", id);
			return false;
		}

		bool updated = false;

		// Update email if provided
		if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
		{
			// Check if new email is already in use
			var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
			if (existingUser != null && existingUser.Id != id)
			{
				throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
			}
			user.Email = request.Email;
			updated = true;
		}

		// Update first name if provided
		if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != user.FirstName)
		{
			user.FirstName = request.FirstName;
			updated = true;
		}

		// Update last name if provided
		if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName != user.LastName)
		{
			user.LastName = request.LastName;
			updated = true;
		}

		// Update active status if provided
		if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
		{
			user.IsActive = request.IsActive.Value;
			updated = true;
		}

		// Only update if something changed
		if (updated)
		{
			user.UpdatedAt = DateTime.UtcNow;
			_logger.LogInformation("Updating user: {UserId}", id);
			var result = await _userRepository.UpdateAsync(user, cancellationToken);
			return result > 0;
		}

		_logger.LogDebug("No changes to update for user: {UserId}", id);
		return true; // No changes needed
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error updating user: {UserId}", id);
		throw;
	}
}
```

**Improvements:**
- ✅ Null check on request
- ✅ EMAIL uniqueness validation (was missing)
- ✅ Change detection - only updates if values actually changed
- ✅ Timestamps only modified when actual changes occur
- ✅ Comprehensive logging (Debug for fetches, Info for updates, Warning for conflicts)
- ✅ Clear distinction: returns `true` when no changes needed (vs. silent false)
- ✅ Exception handling ensures errors are logged with context
- ✅ Avoids unnecessary database round-trips

---

## Summary of Improvements

| Aspect | Before | After |
|--------|--------|-------|
| **Logging** | None | Structured (Debug/Info/Warning/Error) |
| **Error Handling** | Unhandled exceptions | Try/catch with context-aware logging |
| **Documentation** | Minimal | Comprehensive XML docs |
| **Validation** | Basic required checks | Deep validation + strength requirements |
| **Duplicate Prevention** | Username only | Username AND Email |
| **Database Efficiency** | Always saves | Change detection - only saves if needed |
| **Security** | Basic | Initialization of security fields |
| **Maintainability** | Hard to debug | Full audit trail in logs |
| **User Experience** | Generic errors | Specific, helpful error messages |

---

## Testing Recommendations

To validate these improvements:

1. **Logging**: Monitor application logs in Seq, ELK, or Application Insights
2. **Duplicate Prevention**: 
   - Attempt to create user with existing username → should get clear error
   - Attempt to create user with existing email → should get clear error
3. **Password Validation**: 
   - Try password < 8 chars → should fail
   - Try valid password → should succeed
4. **Update Efficiency**: 
   - Update with no changes → should skip DB save (check logs)
   - Update with one field → should only update that field
5. **Error Scenarios**:
   - Database connection failure → should log error with details
   - Null request parameter → should throw ArgumentNullException
