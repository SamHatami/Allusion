namespace Allusion.WPFCore.Interfaces;

public interface IConfiguration
{
    public string GlobalFolder { get; set; }
    public static string DefaultFolder { get; }
    public static string DataFolder { get; }
}