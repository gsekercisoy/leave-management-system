namespace LeaveApi.Models;

public class LeaveBalance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Year { get; set; }
    public int TotalDays { get; set; } = 20;
    public int UsedDays { get; set; }
}
