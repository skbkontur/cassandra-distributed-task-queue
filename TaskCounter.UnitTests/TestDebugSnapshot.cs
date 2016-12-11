using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using GroBuf;
using GroBuf.DataMembersExtracters;

using Newtonsoft.Json;

using NUnit.Framework;

using RemoteTaskQueue.TaskCounter.Implementation;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.UnitTests
{
    [Ignore]
    public class TestDebugSnapshot
    {
        public void TestReadSnapshot()
        {
            var decompress = SnapshotStorageUtils.Decompress(File.ReadAllBytes(@""));
            var serializer = new Serializer(new AllPropertiesExtractor(), null, GroBufOptions.MergeOnRead);
            var counterControllerSnapshot = serializer.Deserialize<CounterControllerSnapshot>(decompress);
            var s = JsonConvert.SerializeObject(counterControllerSnapshot);
            Console.WriteLine(s);
        }

        [Test]
        public void TestCountByLogs()
        {
            var r = new Regex("Task\\s+(\\S+)\\s+is\\s+(\\S+)", RegexOptions.Compiled);
            var lines = File.ReadAllLines(@"c:\work\taskcounter_bug\Catalogue.EDI.RemoteTaskQueueTaskCounterService.TotalLog.2015.11.24.11.38.46.889.log", Encoding.GetEncoding(1251));
            var h = new HashSet<string>();
            foreach(var line in lines)
            {
                var match = r.Match(line);
                if(match.Success && match.Groups.Count == 3)
                {
                    var id = match.Groups[1].Value;
                    var action = match.Groups[2].Value;
                    if(action == "added")
                        h.Add(id);
                    else
                        h.Remove(id);
                }
            }

            Console.WriteLine("count=" + h.Count);
            Console.WriteLine(string.Join("\r\n", h.ToArray()));
        }
    }
}