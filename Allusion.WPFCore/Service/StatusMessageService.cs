using Caliburn.Micro;

namespace Allusion.WPFCore.Service;

public static class StaticLogger
{

    public delegate void LogEventHandler(string message, LogLevel logLevel);

    public static event LogEventHandler LogEvent = delegate { };
   
    public static void Info(string message)
    {
        LogEvent(message, LogLevel.Info);
    }       
    
    public static void Warning(string message)
    {
        LogEvent(message, LogLevel.Warning);
    }

    public static void Error(string message)
    {
        LogEvent(message, LogLevel.Error);
        WriteToLog(message, LogLevel.Error);
    }

    public static void WriteToLog(string message, LogLevel level)
    {
        var logMessage = $"{TimeStamp(DateTime.Now)} - {level}\\t : {message}";
        //write to log file, create if one does not exist
    }


    public static string TimeStamp(DateTime time)
    {
        return time.ToShortTimeString();
    }

    public enum LogLevel
    {
        Info,
        Error,
        Warning,
        Debug,
        Trace

    }

    
}
