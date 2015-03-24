using System;
using System.Collections.Generic;

using log4net;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class MetaLoadController : IMetaConsumer
    {
        public MetaLoadController(ICurrentMetaProvider currentMetaProvider, IMetaConsumer metaConsumer, IMetaLoaderFactory metaLoaderFactory, long syncLoadInterval, string name)
        {
            this.currentMetaProvider = currentMetaProvider;
            this.metaConsumer = metaConsumer;
            metaLoader = metaLoaderFactory.CreateLoader(name);
            this.syncLoadInterval = syncLoadInterval;
            loggerName = name + "Controller";
            logger = LogManager.GetLogger(loggerName);
            state = State.Unknown;
        }

        public void ProcessQueue()
        {
            lock(processLock)
                switch(state)
                {
                case State.Reset:
                case State.Loading:
                    LoadAndUpdate();
                    break;
                case State.LoadingLast:
                    {
                        var metas = GetMetas();
                        var toTicks = metas.Count > 0 ? metas[0].readTicks : currentMetaProvider.NowTicks;
                        logger.LogInfoFormat(loggerName, "Sync Load begin");
                        Load(loadStartTicks, toTicks);
                        loadStartTicks = toTicks;
                        state = State.ProcessingQueue;
                        logger.LogInfoFormat(loggerName, "Processing first metas");
                        ProcessMetas(metas);
                    }
                    break;
                case State.ProcessingQueue:
                    {
                        var metas = GetMetas();
                        ProcessMetas(metas);
                    }
                    break;
                }
        }

        public bool IsProcessingQueue { get { return state == State.ProcessingQueue; } }

        private void LoadAndUpdate()
        {
            var ticks = currentMetaProvider.NowTicks;
            if(ticks - loadStartTicks > syncLoadInterval)
            {
                Load(loadStartTicks, ticks);
                loadStartTicks = ticks;
                state = State.Loading;
            }
            else
            {
                logger.LogInfoFormat(loggerName, "Going to sync load state. Current position is {0}. Now is {1}. ", DateTimeFormatter.FormatWithMsAndTicks(loadStartTicks), DateTimeFormatter.FormatWithMsAndTicks(ticks));
                state = State.LoadingLast;
                currentMetaProvider.Subscribe(this);
            }
        }

        private List<MetasPack> GetMetas()
        {
            lock(metasLock)
            {
                var metas = unprocessedMetas;
                unprocessedMetas = new List<MetasPack>();
                return metas;
            }
        }

        private enum State
        {
            Unknown,
            Reset,
            Loading,
            LoadingLast,
            ProcessingQueue
        }

        public void ResetTo(long startTicks)
        {
            logger.LogInfoFormat(loggerName, "ResetTo {0}", DateTimeFormatter.FormatWithMsAndTicks(startTicks));
            metaLoader.CancelLoadingAsync();
            lock(processLock)
            {
                logger.LogInfoFormat(loggerName, "ResetTo - lock acquired");
                //if(state == State.Loading || state == State.LoadingLast) //todo
                //    throw new Exception("not supported");
                lock(metasLock)
                    unprocessedMetas = new List<MetasPack>();
                currentMetaProvider.Unsubscribe(this);
                state = State.Reset;
                loadStartTicks = startTicks;
                metaLoader.Reset(startTicks);
                logger.LogInfoFormat(loggerName, "ResetTo - done");
            }
        }

        private void ProcessMetas(IEnumerable<MetasPack> metas)
        {
            foreach(var metasPack in metas)
                PutMetas(metasPack);
        }

        private void Load(long startTicks, long endTicks)
        {
            logger.LogInfoFormat(loggerName, "Loading from {0} to {1}", DateTimeFormatter.FormatWithMsAndTicks(startTicks), DateTimeFormatter.FormatWithMsAndTicks(endTicks));
            metaLoader.Load(metaConsumer, endTicks);
            logger.LogInfoFormat(loggerName, "Loading completed");
        }

        private void PutMetas(MetasPack metasPack)
        {
            metaConsumer.ProcessMetas(metasPack.metas, metasPack.readTicks);
        }

        public void ProcessMetas(TaskMetaInformation[] metas, long readTicks)
        {
            switch(state)
            {
            case State.LoadingLast:
            case State.ProcessingQueue:
                lock(metasLock)
                    unprocessedMetas.Add(new MetasPack(metas, readTicks));
                break;
            }
        }

        private readonly object metasLock = new object();
        private readonly object processLock = new object();

        private readonly long syncLoadInterval = TimeSpan.FromHours(1).Ticks;
        private readonly string loggerName;

        private volatile State state = State.Unknown;

        private readonly ICurrentMetaProvider currentMetaProvider;
        private readonly IMetaConsumer metaConsumer;
        private readonly IMetasLoader metaLoader;

        private readonly ILog logger;

        private volatile List<MetasPack> unprocessedMetas;

        private long loadStartTicks = long.MinValue;

        private class MetasPack
        {
            public MetasPack(TaskMetaInformation[] metas, long readTicks)
            {
                this.metas = metas;
                this.readTicks = readTicks;
            }

            public readonly TaskMetaInformation[] metas;
            public readonly long readTicks;
        }
    }
}