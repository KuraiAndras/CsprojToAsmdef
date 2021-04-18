using Serilog;
using Serilog.Sinks.Unity3D;
using SomeOtherDependency;
using ILogger = Serilog.ILogger;

namespace SampleUnityDependency
{
    public static class CustomLogger
    {
        private static readonly ILogger MyLogger = new LoggerConfiguration()
            .WriteTo.Unity3D()
            .CreateLogger();

        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        public static void Log(string message) => MyLogger.Information($"{Messenger.Message} {message}");
    }
}
