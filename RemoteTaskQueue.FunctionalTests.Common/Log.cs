using System.Collections.Concurrent;

using JetBrains.Annotations;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public static class Log
    {
        [NotNull]
        public static ILog DefaultLogger => LogProvider.Get();

        [NotNull]
        public static ILog For<T>(T instance)
        {
            return GetLoggerForContext(typeof(T).Name);
        }

        [NotNull]
        public static ILog For([NotNull] string contextName)
        {
            return GetLoggerForContext(contextName);
        }

        [NotNull]
        private static ILog GetLoggerForContext([NotNull] string contextName)
        {
            return logs.GetOrAdd(contextName, t => DefaultLogger.ForContext(contextName));
        }

        private static readonly ConcurrentDictionary<string, ILog> logs = new ConcurrentDictionary<string, ILog>();
    }
}