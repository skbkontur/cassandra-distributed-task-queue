using System;
using System.Collections;

using GrEmit;
using GrEmit.Utils;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class TaskDataService
    {
        public TaskDataService(ITaskDataTypeToNameMapper taskDataTypeToNameMapper)
        {
            this.taskDataTypeToNameMapper = taskDataTypeToNameMapper;
        }

        public object CreateTaskIndexedInfo([NotNull] MetaIndexedInfo metaIndexedInfo, [CanBeNull] object taskData)
        {
            var typeName = metaIndexedInfo.Name;
            var data = (Data)map[typeName];
            if(data == null)
            {
                Type taskType;
                if(!taskDataTypeToNameMapper.TryGetTaskType(typeName, out taskType))
                    return new TaskIndexedInfo<UnknownData>(metaIndexedInfo, null); //NOTE hack. Type can be unknown

                lock(lockObject)
                {
                    if((data = (Data)map[typeName]) == null)
                    {
                        var constructorFunc = EmitHelpers.EmitDynamicMethod<Func<MetaIndexedInfo, object, object>>(string.Format("EmitConstruction_{0}_{1}", typeName, Guid.NewGuid()), GetType().Module, il => EmitCode(il, taskType));
                        data = new Data(constructorFunc);
                        map[typeName] = data;
                    }
                }
            }

            //NOTE TaskIndexedInfo<T> нужно для OmitNonIndexablePropertiesContractResolver. !!! не переделывать в object
            //BUG taskData can be null
            return data.constructorFunc(metaIndexedInfo, taskData);
            //return Activator.CreateInstance(typeof(TaskIndexedInfo<>).MakeGenericType(taskData.GetType()), new[] { metaIndexedInfo, taskData });
        }

        private static void EmitCode(GroboIL il, Type taskType)
        {
            var constructor = HackHelpers.GetObjectConstruction(() => new TaskIndexedInfo<int>(null, 0), taskType);
            il.Ldarg(0);
            il.Ldarg(1);
            il.Castclass(taskType);
            il.Newobj(constructor);
            il.Ret();
        }

        private readonly Hashtable map = new Hashtable();
        private readonly object lockObject = new object();

        private readonly ITaskDataTypeToNameMapper taskDataTypeToNameMapper;

        // ReSharper disable ClassNeverInstantiated.Local

        public class UnknownData
        {
        }

        // ReSharper restore ClassNeverInstantiated.Local

        private class Data
        {
            public Data(Func<MetaIndexedInfo, object, object> constructorFunc)
            {
                this.constructorFunc = constructorFunc;
            }

            public readonly Func<MetaIndexedInfo, object, object> constructorFunc;
        }
    }
}