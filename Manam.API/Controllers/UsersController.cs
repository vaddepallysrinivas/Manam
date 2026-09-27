using Manam.Models;
using Manam.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manam.API.Controllers;

/// <summary>
/// Users API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        var response = new ApiResponse<IReadOnlyList<UserDto>>
        {
            Success = true,
            Message = "Users retrieved successfully.",
            Data = users,
            TraceId = HttpContext.TraceIdentifier
        };

        return Ok(response);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

        if (user == null)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "User not found.",
                TraceId = HttpContext.TraceIdentifier
            };
            return NotFound(errorResponse);
        }

        var response = new ApiResponse<UserDto>
        {
            Success = true,
            Message = "User retrieved successfully.",
            Data = user,
            TraceId = HttpContext.TraceIdentifier
        };

        return Ok(response);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value!.Errors.Count > 0)
                .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            var errorResponse = new ErrorResponse
            {
                Message = "Validation failed.",
                Errors = errors,
                TraceId = HttpContext.TraceIdentifier
            };

            return BadRequest(errorResponse);
        }

        var userId = await _userService.CreateUserAsync(request, cancellationToken);

        var response = new ApiResponse<Guid>
        {
            Success = true,
            Message = "User created successfully.",
            Data = userId,
            TraceId = HttpContext.TraceIdentifier
        };

        return CreatedAtAction(nameof(GetUserById), new { id = userId }, response);
    }

    /// <summary>
    /// Update a user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var success = await _userService.UpdateUserAsync(id, request, cancellationToken);

        if (!success)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "User not found.",
                TraceId = HttpContext.TraceIdentifier
            };
            return NotFound(errorResponse);
        }

        var response = new ApiResponse<string>
        {
            Success = true,
            Message = "User updated successfully.",
            Data = "OK",
            TraceId = HttpContext.TraceIdentifier
        };

        return Ok(response);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var success = await _userService.DeleteUserAsync(id, cancellationToken);

        if (!success)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "User not found.",
                TraceId = HttpContext.TraceIdentifier
            };
            return NotFound(errorResponse);
        }

        var response = new ApiResponse<string>
        {
            Success = true,
            Message = "User deleted successfully.",
            Data = "OK",
            TraceId = HttpContext.TraceIdentifier
        };

        return Ok(response);
    }
}
