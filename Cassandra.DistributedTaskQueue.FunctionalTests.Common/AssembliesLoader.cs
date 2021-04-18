using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public static class AssembliesLoader
    {
        public static IEnumerable<Assembly> Load()
        {
            return EnumerateFiles()
                   .Where(IsOurAssemblyFile)
                   .Select(Assembly.LoadFrom)
                   .ToArray();
        }

        private static IEnumerable<string> EnumerateFiles()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var entryFiles = entryAssembly == null ? new string[0] : new[] {entryAssembly.Location};

            return entryFiles
                   .Concat(EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory))
                   .Concat(EnumerateFiles(AppDomain.CurrentDomain.RelativeSearchPath))
                   .Distinct();
        }

        private static IEnumerable<string> EnumerateFiles(string directory)
        {
            return string.IsNullOrEmpty(directory)
                       ? new string[0]
                       : Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
        }

        private static bool IsOurAssemblyFile(string fullFileName)
        {
            var fileName = Path.GetFileName(fullFileName);
            if (string.IsNullOrEmpty(fileName))
                return false;
            var extension = Path.GetExtension(fileName);
            return (extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase) ||
                    extension.Equals(".exe", StringComparison.InvariantCultureIgnoreCase)) &&
                   Path.GetFileNameWithoutExtension(fileName).StartsWith("SkbKontur.Cassandra.DistributedTaskQueue", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}