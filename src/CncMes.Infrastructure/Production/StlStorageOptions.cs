namespace CncMes.Infrastructure.Production;

public class StlStorageOptions
{
    /// <summary>
    /// STL dosyalarının saklandığı kök klasör. MAUI tarafında
    /// FileSystem.AppDataDirectory altında bir alt klasöre gösterir.
    /// </summary>
    public string BaseDirectory { get; init; } = string.Empty;
}
