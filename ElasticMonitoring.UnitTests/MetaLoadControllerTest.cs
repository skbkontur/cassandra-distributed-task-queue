using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using Rhino.Mocks;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding;
using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.UnitTests
{
    [TestFixture]
    public class MetaLoadControllerTest : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            consumer = GetMock<IMetaConsumer>();
            metasLoader = GetMock<IMetasLoader>();
            var metaLoaderFactory = GetMock<IMetaLoaderFactory>();
            metaLoaderFactory.Expect(f => f.CreateLoader("zzz")).Return(metasLoader);
            mp = GetMock<ICurrentMetaProvider>();
            mlc = new MetaLoadController(mp, consumer, metaLoaderFactory, 100, "zzz");
        }

        [Test]
        public void TestNotReset()
        {
            EnqueueMetas(100);
            mlc.ProcessQueue();
        }

        [Test]
        public void TestLoadSimple()
        {
            mp.Expect(x => x.Unsubscribe(mlc));
            metasLoader.Expect(x => x.CancelLoadingAsync());
            metasLoader.Expect(x => x.Reset(1000));
            mlc.ResetTo(1000);

            ExpectNow(2000);
            metasLoader.Expect(x => x.Load(consumer, 2000));
            mp.Expect(x => x.Subscribe(mlc));
            mlc.ProcessQueue();

            ExpectNow(2050); //note go to state LoadingLast
            mlc.ProcessQueue();

            var m0 = EnqueueMetas(3000);
            metasLoader.Expect(x => x.Load(consumer, 3000));
            consumer.Expect(c => c.ProcessMetas(m0, 3000));
            mlc.ProcessQueue();
        }

        [Test]
        public void TestLoadHard()
        {
            mp.Expect(x => x.Unsubscribe(mlc));
            metasLoader.Expect(x => x.CancelLoadingAsync());
            metasLoader.Expect(x => x.Reset(1000));
            mlc.ResetTo(1000);

            ExpectNow(2000);
            metasLoader.Expect(x => x.Load(consumer, 2000));
            mlc.ProcessQueue();

            ExpectNow(3000);
            metasLoader.Expect(x => x.Load(consumer, 3000));
            mlc.ProcessQueue();

            ExpectNow(4000);
            metasLoader.Expect(x => x.Load(consumer, 4000));
            mp.Expect(x => x.Subscribe(mlc));
            mlc.ProcessQueue();

            ExpectNow(4099); //note go to state LoadingLast
            mlc.ProcessQueue();

            var m0 = EnqueueMetas(4100);
            metasLoader.Expect(x => x.Load(consumer, 4100));
            consumer.Expect(c => c.ProcessMetas(m0, 4100));
            mlc.ProcessQueue();
        }

        [Test]
        public void TestResetAfterWork()
        {
            TestLoadSimple();

            mp.Expect(x => x.Unsubscribe(mlc));
            metasLoader.Expect(x => x.CancelLoadingAsync());
            metasLoader.Expect(x => x.Reset(500));
            mlc.ResetTo(500);

            ExpectNow(550);
            mp.Expect(x => x.Subscribe(mlc));
            mlc.ProcessQueue();

            ExpectNow(560);
            metasLoader.Expect(x => x.Load(consumer, 560));
            mlc.ProcessQueue();

            var m0 = EnqueueMetas(3100);
            consumer.Expect(c => c.ProcessMetas(m0, 3100));
            mlc.ProcessQueue();
        }

        [Test]
        public void TestLoadSimple2()
        {
            mp.Expect(x => x.Unsubscribe(mlc));
            metasLoader.Expect(x => x.CancelLoadingAsync());
            metasLoader.Expect(x => x.Reset(1000));
            mlc.ResetTo(1000);

            ExpectNow(2000);
            metasLoader.Expect(x => x.Load(consumer, 2000));
            mp.Expect(x => x.Subscribe(mlc));
            mlc.ProcessQueue();

            ExpectNow(2050); //note go to state LoadingLast
            mlc.ProcessQueue();

            ExpectNow(3000);
            metasLoader.Expect(x => x.Load(consumer, 3000));
            mlc.ProcessQueue();

            var m0 = EnqueueMetas(3100);
            consumer.Expect(c => c.ProcessMetas(m0, 3100));
            mlc.ProcessQueue();
        }

        private void ExpectNow(long now)
        {
            mp.Expect(x => x.NowTicks).Return(now);
        }

        private TaskMetaInformation[] EnqueueMetas(long ticks)
        {
            var metas = CreateMetas();
            mlc.ProcessMetas(metas, ticks);
            return metas;
        }

        private static TaskMetaInformation[] CreateMetas()
        {
            return new TaskMetaInformation[] {};
        }

        private IMetaConsumer consumer;
        private IMetasLoader metasLoader;
        private ICurrentMetaProvider mp;
        private MetaLoadController mlc;
    }
}