using System;
using System.Collections;

using GrEmit;
using GrEmit.Utils;

using JetBrains.Annotations;

using RemoteQueue.Configuration;

using RemoteTaskQueue.Monitoring.Storage.Writing.Contracts;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class TaskDataService
    {
        public TaskDataService(ITaskDataRegistry taskDataRegistry)
        {
            this.taskDataRegistry = taskDataRegistry;
        }

        public object CreateTaskIndexedInfo([NotNull] MetaIndexedInfo metaIndexedInfo, [NotNull] string exceptionInfo, [CanBeNull] object taskData)
        {
            var typeName = metaIndexedInfo.Name;
            var data = (Data)map[typeName];
            if(data == null)
            {
                Type taskType;
                if(!taskDataRegistry.TryGetTaskType(typeName, out taskType))
                    return new TaskIndexedInfo<UnknownData>(metaIndexedInfo, exceptionInfo, null); //NOTE hack. Type can be unknown

                lock(lockObject)
                {
                    if((data = (Data)map[typeName]) == null)
                    {
                        var constructorFunc = EmitHelpers.EmitDynamicMethod<ConstructorDelegate>(string.Format("EmitConstruction_{0}_{1}", typeName, Guid.NewGuid()), GetType().Module, il => EmitCode(il, taskType));
                        data = new Data(constructorFunc);
                        map[typeName] = data;
                    }
                }
            }

            //NOTE TaskIndexedInfo<T> нужно для OmitNonIndexablePropertiesContractResolver. !!! не переделывать в object
            //BUG taskData can be null
            return data.constructorFunc(metaIndexedInfo, exceptionInfo, taskData);
            //return Activator.CreateInstance(typeof(TaskIndexedInfo<>).MakeGenericType(taskData.GetType()), new[] { metaIndexedInfo, taskData });
        }

        private static void EmitCode(GroboIL il, Type taskType)
        {
            var constructor = HackHelpers.GetObjectConstruction(() => new TaskIndexedInfo<int>(null, null, 0), taskType);
            il.Ldarg(0);
            il.Ldarg(1);
            il.Ldarg(2);
            il.Castclass(taskType);
            il.Newobj(constructor);
            il.Ret();
        }

        private delegate object ConstructorDelegate(MetaIndexedInfo info, string exceptionInfo, object data);

        private readonly ITaskDataRegistry taskDataRegistry;

        private readonly Hashtable map = new Hashtable();
        private readonly object lockObject = new object();

        // ReSharper disable ClassNeverInstantiated.Local

        public class UnknownData
        {
        }

        // ReSharper restore ClassNeverInstantiated.Local

        private class Data
        {
            public Data(ConstructorDelegate constructorFunc)
            {
                this.constructorFunc = constructorFunc;
            }

            public readonly ConstructorDelegate constructorFunc;
        }
    }
}