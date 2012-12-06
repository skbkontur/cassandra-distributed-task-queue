using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using FunctionalTests.Logging;

using GroboContainer.Core;
using GroboContainer.Impl;

using NUnit.Framework;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Settings;

using log4net;

namespace FunctionalTests.RemoteLock
{
    [TestFixture]
    public abstract class ThreadsTestBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            Log4NetConfiguration.InitializeOnce();
            IEnumerable<Assembly> assemblies = AssembliesLoader.Load();
            container = new Container(new ContainerConfiguration(assemblies));
            var applicationSettings = ApplicationSettings.LoadDefault("functionalTestsSettings");
            container.Configurator.ForAbstraction<IApplicationSettings>().UseInstances(applicationSettings);

            logger.InfoFormat("Start SetUp, runningThreads = {0}", runningThreads);
            runningThreads = 0;
            isEnd = false;
            threads = new List<Thread>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            logger.InfoFormat("Start TeadDown, runningThreads = {0}", runningThreads);
            foreach(var thread in threads ?? new List<Thread>())
                thread.Abort();
        }

        protected void AddThread(Action<Random> shortAction)
        {
            var seed = Guid.NewGuid().GetHashCode();
            var thread = new Thread(() => MakePeriodicAction(shortAction, seed));
            thread.Start();
            logger.InfoFormat("Add thread with seed = {0}", seed);
            threads.Add(thread);
        }

        protected void JoinThreads()
        {
            logger.Info("JoinThreads. begin");
            isEnd = true;
            running.Set();
            foreach(var thread in threads)
                Assert.That(thread.Join(TimeSpan.FromSeconds(60)), "Не удалось остановить поток");
            logger.Info("JoinThreads. end");
        }

        protected void RunThreads(int timeInterval = 1000)
        {
            logger.InfoFormat("RunThreads. begin, runningThreads = {0}", runningThreads);
            running.Set();
            Thread.Sleep(timeInterval);
            running.Reset();
            while(Interlocked.CompareExchange(ref runningThreads, 0, 0) != 0)
            {
                Thread.Sleep(50);
                logger.InfoFormat("Wait runningThreads = 0. Now runningThreads = {0}", runningThreads);
                foreach(var thread in threads)
                {
                    if(!thread.IsAlive)
                        throw new Exception("Поток сдох");
                }
            }
            logger.Info("RunThreads. end");
        }

        protected Container container;

        private void MakePeriodicAction(Action<Random> shortAction, int seed)
        {
            try
            {
                var localRandom = new Random(seed);
                while(!isEnd)
                {
                    running.WaitOne();
                    Interlocked.Increment(ref runningThreads);
                    shortAction(localRandom);
                    Interlocked.Decrement(ref runningThreads);
                }
            }
            catch(Exception e)
            {
                logger.Error(e);
            }
        }

        private readonly ManualResetEvent running = new ManualResetEvent(false);
        private int runningThreads;
        private volatile bool isEnd;

        private static readonly ILog logger = LogManager.GetLogger(typeof(ThreadsTestBase));
        private List<Thread> threads;
    }
}