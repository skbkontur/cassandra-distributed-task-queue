using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.Formatting;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public static class LoggingConfigurationHelpers
    {
        public static void Init()
        {
            var totalFileLog = CreateFileLog("TotalLog")
                               .WithMinimumLevel(LogLevel.Info)
                               .WithMinimumLevelForSourceContexts(LogLevel.Warn, "HandlerTask", "CassandraDistributedTaskQueue")
                               .WithMinimumLevelForSourceContext("HandlerManager", LogLevel.Warn);

            var errorFileLog = CreateFileLog("ErrorLog")
                .WithMinimumLevel(LogLevel.Error);

            var consoleLog = new ConsoleLog().WithMinimumLevel(LogLevel.Info);
            var defaultLog = new CompositeLog(totalFileLog, errorFileLog, consoleLog).WithThreadName();

            LogProvider.Configure(defaultLog, canOverwrite : true);
        }

        private static ILog CreateFileLog([NotNull] string logName)
        {
            return new FileLog(new FileLogSettings
                {
                    FilePath = Path.Combine("logs", $"{logName}.{{RollingSuffix}}.{DateTime.Now:HH-mm-ss}.log"),
                    RollingStrategy = new RollingStrategyOptions
                        {
                            Type = RollingStrategyType.ByTime,
                            Period = RollingPeriod.Day,
                            MaxFiles = 7,
                        },
                    FileOpenMode = FileOpenMode.Append,
                    OutputTemplate = OutputTemplate.Parse("{Timestamp} {Level} [{threadName}] L:{sourceContext} {traceId:w}{operationContext:w}{BillingTransaction:w}{BoxEvent:w}{Message}{NewLine}{Exception}")
                });
        }

        public static void SetupUnhandledExceptionLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "Unhandled exception in current AppDomain");

            TaskScheduler.UnobservedTaskException += (sender, args) =>
                LogUnhandledException(args.Exception, "Unobserved TaskException");
        }

        private static void LogUnhandledException([NotNull] Exception exception, [NotNull] string logMessage)
        {
            Debug.WriteLine(exception.ToString());
            Console.WriteLine(exception.ToString());
            Log.DefaultLogger.Fatal(exception, logMessage);
        }

        [NotNull]
        public static ILog WithThreadName(this ILog log)
        {
            return log.WithProperty("threadName", () => Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString());
        }
    }
}