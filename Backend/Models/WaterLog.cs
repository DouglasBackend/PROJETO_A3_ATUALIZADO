namespace Backend.Models;

public class WaterLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double ConsumptionLiters { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
