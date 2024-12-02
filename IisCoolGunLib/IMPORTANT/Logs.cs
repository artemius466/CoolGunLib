using System;

namespace IisCoolGun.Important
{
    public static class Logs
{
    public static void Log(string message)
    {
        Console.WriteLine($"{PluginInfo.GUID}: {message}"); // Prefix message with mod GUID
    }
}
}
