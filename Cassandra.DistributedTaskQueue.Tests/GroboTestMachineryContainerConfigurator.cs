using System;
using System.IO;
using System.Linq;
using System.Reflection;

using GroboContainer.Core;
using GroboContainer.Impl;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class GroboTestMachineryContainerConfigurator
    {
        [NotNull]
        public static ContainerConfiguration GetContainerConfiguration([NotNull] string testSuiteName)
        {
            var rtqAssemblies = LoadRtqAssemblies();
            return new ContainerConfiguration(rtqAssemblies, testSuiteName, ContainerMode.UseShortLog);
        }

        [NotNull, ItemNotNull]
        private static Assembly[] LoadRtqAssemblies()
        {
            return EnumerateFiles().Where(fullFileName => rtqAssemblyNames.Contains(Path.GetFileName(fullFileName)))
                                   .Select(Assembly.LoadFrom)
                                   .ToArray();
        }

        [NotNull, ItemNotNull]
        private static string[] EnumerateFiles()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var baseDirectoryFiles = string.IsNullOrEmpty(baseDirectory)
                                         ? new string[0]
                                         : Directory.EnumerateFiles(baseDirectory, "*", SearchOption.TopDirectoryOnly).ToArray();
            return baseDirectoryFiles;
        }

        private static readonly string[] rtqAssemblyNames =
            {
                "SkbKontur.Cassandra.DistributedTaskQueue.dll",
                "SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.dll",
                "RemoteTaskQueue.FunctionalTests.Common.dll",
            };
    }
}