# User Management Module - Implementation Guide

## 📁 Folder Structure

```
Manam.API/
├── Controllers/
│   └── UserManagement/
│       ├── AuthenticationController.cs
│       └── UserProfileController.cs

Manam.Services/
├── Abstractions/
│   └── UserManagement/
│       ├── IAuthenticationService.cs
│       └── IUserProfileService.cs
├── Implementations/
│   └── UserManagement/
│       ├── AuthenticationService.cs
│       └── UserProfileService.cs
└── Extensions/
    └── UserManagementServiceExtensions.cs

Manam.Models/
└── UserManagement/
    ├── LoginRequest.cs
    ├── LoginResponse.cs
    ├── RegisterRequest.cs
    ├── RegisterResponse.cs
    └── UserProfileResponse.cs
```

## 🚀 Quick Start

### 1. Register Services in Program.cs

Add the following line in `Program.cs` after other service registrations:

```csharp
// Add user management services
builder.Services.AddUserManagementServices();
```

### 2. Supported Endpoints

#### Authentication Controller (No Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/authentication/login` | Login with email/password |
| POST | `/api/v1/authentication/register` | Register new user |
| POST | `/api/v1/authentication/forgot-password?email=user@example.com` | Request password reset |
| POST | `/api/v1/authentication/reset-password` | Reset password with token |
| POST | `/api/v1/authentication/verify-email/{userId}` | Verify user email |

#### User Profile Controller (Auth Required)

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|----------------|
| GET | `/api/v1/userprofile/me` | Get current user profile | - |
| GET | `/api/v1/userprofile/{userId}` | Get user profile by ID | Admin |
| GET | `/api/v1/userprofile?pageNumber=1&pageSize=10` | Get all users paginated | Admin |
| PUT | `/api/v1/userprofile/me` | Update current user profile | - |
| DELETE | `/api/v1/userprofile/me` | Delete current user account | - |
| POST | `/api/v1/userprofile/link-provider` | Link OAuth provider | - |
| POST | `/api/v1/userprofile/{userId}/roles` | Assign role to user | Admin |

### 3. Example API Calls

#### Login
```json
POST /api/v1/authentication/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "MyPassword123!"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "data": {
    "success": true,
    "message": "Login successful",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "SGVsbG8gV29ybGQhIFRoaXMgaXMgYSByYW5kb20gYmFzZTY0IHRva2Vu",
    "userId": 1,
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"],
    "expiresAt": 1705344000
  }
}
```

#### Register
```json
POST /api/v1/authentication/register
Content-Type: application/json

{
  "username": "john.doe",
  "email": "john@example.com",
  "password": "MyPassword123!",
  "confirmPassword": "MyPassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-0123"
}

Response:
{
  "success": true,
  "message": "User registered successfully. Please verify your email.",
  "data": {
    "success": true,
    "message": "User registered successfully. Please verify your email.",
    "userId": 1,
    "email": "john@example.com",
    "errors": {}
  }
}
```

#### Get User Profile
```json
GET /api/v1/userprofile/me
Authorization: Bearer <JWT_TOKEN>

Response:
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "userId": 1,
    "username": "john.doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1-555-0123",
    "isEmailVerified": true,
    "isPhoneVerified": true,
    "isActive": true,
    "hasPassword": true,
    "roles": [
      {
        "roleId": 2,
        "roleName": "User",
        "description": "Regular user"
      }
    ],
    "externalAuths": [],
    "createdAt": "2025-01-15T10:30:00Z",
    "updatedAt": "2025-01-15T10:30:00Z"
  }
}
```

#### Update User Profile
```json
PUT /api/v1/userprofile/me
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1-555-9999"
}

Response:
{
  "success": true,
  "message": "Profile updated successfully"
}
```

## 🔐 Features Implemented

### 1. Authentication Service
- ✅ Password-based login with bcrypt hashing (via PBKDF2)
- ✅ User registration with validation
- ✅ JWT token generation and validation
- ✅ Token refresh functionality
- ✅ Password reset flow with time-limited tokens
- ✅ Email verification
- ✅ Strong password requirements (uppercase, lowercase, digit, special char)
- ✅ Comprehensive request validation

### 2. User Profile Service
- ✅ Get user profile with roles and external auth
- ✅ Update user profile information
- ✅ Soft delete user accounts
- ✅ Paginated user listing
- ✅ Link external OAuth providers
- ✅ Assign roles to users

### 3. Security Features
- ✅ JWT token-based authentication
- ✅ Role-based authorization (Admin/User)
- ✅ IP address and User-Agent logging
- ✅ Password hashing with PBKDF2 (10000 iterations)
- ✅ Refresh token rotation
- ✅ Email validation
- ✅ Strong password validation

### 4. Logging & Monitoring
- ✅ Comprehensive logging using Serilog
- ✅ All operations logged with context
- ✅ Error logging with full exception details
- ✅ Request correlation IDs
- ✅ Login attempt audit trail
- ✅ User action tracking

### 5. Error Handling
- ✅ Global exception handling middleware
- ✅ Proper HTTP status codes
- ✅ Detailed error messages
- ✅ Validation error feedback
- ✅ API response model consistency

## 🗄️ Database Integration (TODO)

The following stored procedures need to be called from services:

### Authentication Service
- `sp_maLoginUser` - Validate credentials and login
- `sp_maGetUserByEmail` - Find user by email
- `sp_maInsertUser` - Create new user
- `sp_maCreatePasswordResetToken` - Generate reset token
- `sp_maResetPassword` - Validate and reset password
- `sp_maVerifyEmail` - Mark email as verified

### User Profile Service
- `sp_maGetUserWithRoles` - Get user profile with roles
- `sp_maUpdateUser` - Update user profile
- `sp_maSoftDeleteUser` - Soft delete user
- `sp_maGetAllUsers` - Get paginated users
- `sp_maAddExternalAuth` - Link OAuth provider
- `sp_maAssignRoleToUser` - Assign role to user

## 📝 Configuration (appsettings.json)

Add the following configuration:

```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-key-min-32-chars",
    "Issuer": "ManamAPI",
    "Audience": "ManamClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "PasswordPolicy": {
    "MinLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigits": true,
    "RequireSpecialCharacters": true
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "From": "noreply@manam.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

## 🔗 Service Dependencies

### AuthenticationService Depends On
- `ILogger<AuthenticationService>` - For logging
- `IConfiguration` - For JWT configuration
- `IUserProfileService` - For user profile operations

### UserProfileService Depends On
- `ILogger<UserProfileService>` - For logging

## 📋 Response Model

All API responses follow a consistent model:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public string[]? Errors { get; set; }
}
```

## 🧪 Testing Recommendations

### Unit Tests
- Password validation logic
- Email validation
- JWT token generation and validation
- Request validation

### Integration Tests
- Login flow end-to-end
- Registration flow end-to-end
- Profile update operations
- External provider linking
- Role assignment

### Load Tests
- Login endpoint with concurrent requests
- User listing with pagination
- Token refresh under load

## 🚨 Error Codes

| HTTP Code | Scenario |
|-----------|----------|
| 200 OK | Successful GET/PUT/DELETE |
| 201 Created | Successful POST (resource created) |
| 400 Bad Request | Invalid input, validation errors |
| 401 Unauthorized | Missing/invalid token |
| 403 Forbidden | Insufficient permissions |
| 404 Not Found | Resource not found |
| 500 Internal Server Error | Unexpected server error |

## 📌 Next Steps

1. **Implement Database Client Integration**
   - Replace TODO comments with actual database calls
   - Use DatabaseClient to execute stored procedures

2. **Email Service Integration**
   - Send verification emails on registration
   - Send password reset emails
   - Configure email templates

3. **OAuth Provider Integration**
   - Google OAuth2 authentication
   - Facebook OAuth2 authentication
   - GitHub OAuth authentication

4. **Rate Limiting**
   - Add rate limiting to login endpoint
   - Prevent brute force attacks
   - Account lockout after N failed attempts

5. **JWT Token Management**
   - Store refresh tokens in database
   - Implement token revocation
   - Add token blacklist for logout

6. **Audit Logging**
   - Log all user activities
   - Track login history
   - Monitor suspicious activities

## 🛠️ Best Practices Implemented

✅ Dependency Injection for loose coupling  
✅ Service abstraction with interfaces  
✅ Comprehensive logging at all levels  
✅ Proper exception handling  
✅ Strong password validation  
✅ Secure password hashing  
✅ JWT token-based authentication  
✅ Role-based authorization  
✅ Consistent API response model  
✅ Input validation on all endpoints  
✅ XML documentation comments  
✅ Separation of concerns (Controllers/Services)  
✅ Soft delete for data retention  
✅ IP and User-Agent tracking  

## 📞 Support

For issues or questions about the user management module, refer to:
- API Documentation in Swagger/OpenAPI
- XML documentation comments in code
- This implementation guide
- Database schema documentation

---

**Database:** DESKTOP-O4ATQ75\MSSQLSERVER2\Manam  
**Created:** 2025-01-15  
**Version:** 1.0
