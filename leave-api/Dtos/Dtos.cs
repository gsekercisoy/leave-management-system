namespace LeaveApi.Dtos;

public record RegisterDto(string Username, string Password, string? Role);
public record LoginDto(string Username, string Password);
public record AuthResponse(string Token, string Username, string Role);
public record CreateLeaveDto(DateTime StartDate, DateTime EndDate, string Reason);
