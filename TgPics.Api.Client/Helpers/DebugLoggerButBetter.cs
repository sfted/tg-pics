namespace TgPics.Api.Client.Helpers;

using HttpTracer.Logger;
using System.Diagnostics;

public class DebugLoggerButBetter : ILogger
{
    public void Log(string aFuckingMessage) => Debug.WriteLine(aFuckingMessage);
}