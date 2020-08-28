using System;
using System.IO;
using System.Text;
using System.Threading;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ServiceLib.Logging;

using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.Context;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.Formatting;

namespace RemoteTaskQueue.FunctionalTests
{
    public static class TestLoggingConfigurator
    {
        [NotNull]
        public static ILog SetUpLoggingOnce()
        {
            LazyInitializer.EnsureInitialized(ref defaultLog,
                                              ref isLoggingInitialized,
                                              ref loggingInitializationLock,
                                              SetUpLogging);
            return defaultLog;
        }

        [NotNull]
        private static ILog SetUpLogging()
        {
            LoggingConfigurator.SetupUnhandledExceptionLogging();

            defaultLog = new CompositeLog(CreateConsoleLog(), CreateFileLog())
                         .WithDemystifiedStackTraces()
                         .WithThreadName()
                         .WithAllFlowingContextProperties()
                         .WithMinimumLevel(LogLevel.Info)
                         .WithMinimumLevelForSourceContext("CassandraThriftClient", LogLevel.Warn)
                         .WithMinimumLevelForSourceContexts(LogLevel.Warn, "CassandraDistributedTaskQueue", "HandlerTask")
                         .WithMinimumLevelForSourceContexts(LogLevel.Warn, "CassandraDistributedTaskQueue", "HandlerManager");

            LogProvider.Configure(defaultLog, canOverwrite : false);

            return defaultLog;
        }

        public static void FlushAll()
        {
            if (!isLoggingInitialized)
                return;

            defaultLog.Info("Flushing vostok logs");
            FileLog.FlushAll();
            ConsoleLog.Flush();
        }

        [NotNull]
        private static ILog CreateConsoleLog()
        {
            return new SynchronousConsoleLog(new ConsoleLogSettings
                {
                    OutputTemplate = OutputTemplate.Parse("{Timestamp} {Level} [{threadName}] L:{sourceContext} {Message}{NewLine}{Exception}")
                });
        }

        [NotNull]
        private static ILog CreateFileLog()
        {
            return new FileLog(new FileLogSettings
                {
                    Encoding = Encoding.UTF8,
                    FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "logs", $"TotalLog.{{RollingSuffix}}.{DateTime.Now:HH-mm-ss}.log"),
                    RollingStrategy = new RollingStrategyOptions
                        {
                            Type = RollingStrategyType.ByTime,
                            Period = RollingPeriod.Day,
                            MaxFiles = 14
                        },
                    FileOpenMode = FileOpenMode.Append,
                    OutputTemplate = OutputTemplate.Parse("{Timestamp} {Level} [{threadName}] L:{sourceContext} {traceId:w}{operationContext:w}{Message}{NewLine}{Exception}")
                });
        }

        private static ILog defaultLog;
        private static bool isLoggingInitialized;
        private static object loggingInitializationLock = new object();
    }
}