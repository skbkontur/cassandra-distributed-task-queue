using System;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

namespace FunctionalTests.RepositoriesTests
{
    public class HandleTaskExceptionInfoStorageTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            exceptionInfoStorage = Container.Get<IHandleTaskExceptionInfoStorage>();
        }

        [Test]
        public void ReadWriteExceptionTest()
        {
            const string taskId = "xxx";
            var exception = new Exception("ppp");
            TaskExceptionInfo returnedException;
            exceptionInfoStorage.TryAddExceptionInfo(taskId, exception);
            Assert.IsTrue(exceptionInfoStorage.TryGetExceptionInfo(taskId, out returnedException));
            Assert.IsTrue(returnedException.EqualsToException(exception));
        }

        private IHandleTaskExceptionInfoStorage exceptionInfoStorage;
    }
}