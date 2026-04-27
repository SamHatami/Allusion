namespace Allusion.WPFCore.Interfaces;

public abstract class Configuration
{
    public string GlobalFolder { get; set; } = string.Empty;
    public static string DefaultFolder { get; } = string.Empty;
    public static string DataFolder { get; } = string.Empty;

    public abstract Configuration Read(); 

}
