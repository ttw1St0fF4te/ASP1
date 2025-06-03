namespace PR1.Models;

public static class LoyaltyLevels
{
    public const string Basic = "Базовый";
    public const string Silver = "Серебряный";
    public const string Gold = "Золотой";
    
    public static readonly Dictionary<string, (decimal MinSpent, decimal CashbackPercent)> Levels = new()
    {
        { Basic, (30000m, 0.05m) },
        { Silver, (60000m, 0.10m) },
        { Gold, (120000m, 0.15m) }
    };
    
    public static string? GetLoyaltyLevel(decimal totalSpent)
    {
        if (totalSpent >= Levels[Gold].MinSpent) return Gold;
        if (totalSpent >= Levels[Silver].MinSpent) return Silver;
        if (totalSpent >= Levels[Basic].MinSpent) return Basic;
        return null;
    }
    
    public static decimal GetCashbackPercent(string? loyaltyLevel)
    {
        if (loyaltyLevel != null && Levels.ContainsKey(loyaltyLevel))
            return Levels[loyaltyLevel].CashbackPercent;
        return 0m;
    }
}