using MelonLoader;

namespace ShalazamPlugin;

// Central logging gate. Happy-path / diagnostic output goes through Verbose and is suppressed unless
// verbose logging is enabled (MelonPreferences ShalazamApi.VerboseLogging). Warnings and errors always
// log. Prefer these over MelonLogger.Msg directly so logging stays quiet by default.
public static class Log
{
    public static void Verbose(string message)
    {
        if (Globals.VerboseLogging)
        {
            MelonLogger.Msg(message);
        }
    }

    public static void Warning(string message) => MelonLogger.Warning(message);

    public static void Error(string message) => MelonLogger.Error(message);
}
