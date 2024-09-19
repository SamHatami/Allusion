namespace Allusion.WPFCore.Interfaces;

public abstract class Configuration
{
    public string GlobalFolder { get; set; }
    public static string DefaultFolder { get; }
    public static string DataFolder { get; }

    public abstract Configuration Read(); 

}