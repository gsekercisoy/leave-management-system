namespace LeaveApi.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
    public int? DecidedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
