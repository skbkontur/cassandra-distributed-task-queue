using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    public interface ITypeNameAttribute
    {
        string Type { get; }
    }

    public class ContentTypeNameAttribute : Attribute, ITypeNameAttribute
    {
        public ContentTypeNameAttribute([NotNull] string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new InvalidOperationException("TypeName is empty");
            Type = typeName;
        }

        [NotNull]
        public string Type { get; }
    }

    public static class DerivedTypesSerialization<TBase, TTypeNameAttribute> where TTypeNameAttribute : Attribute, ITypeNameAttribute
    {
        [GroBufSizeCounter]
        public static SizeCounterDelegate GetSizeCounter(Func<Type, SizeCounterDelegate> sizeCountersFactory, SizeCounterDelegate baseSizeCounter)
        {
            return (o, writeEmpty, context) =>
                {
                    var type = o.GetType();
                    return sizeCountersFactory(typeof(string))(GetTypeNameByType(type), true, context) + sizeCountersFactory(type)(o, true, context);
                };
        }

        [GroBufWriter]
        public static WriterDelegate GetWriter(Func<Type, WriterDelegate> writersFactory, WriterDelegate baseWriter)
        {
            return (object o, bool writeEmpty, IntPtr result, ref int index, WriterContext context) =>
                {
                    var type = o.GetType();
                    writersFactory(typeof(string))(GetTypeNameByType(type), true, result, ref index, context);
                    writersFactory(type)(o, true, result, ref index, context);
                    if (index < 0)
                        index = 0;
                };
        }

        [GroBufReader]
        public static ReaderDelegate GetReader(Func<Type, ReaderDelegate> readersFactory, ReaderDelegate baseReader)
        {
            return (IntPtr data, ref int index, ref object result, ReaderContext context) =>
                {
                    object textType = null;
                    readersFactory(typeof(string))(data, ref index, ref textType, context);
                    var type = GetTypeByTypeNameType((string)textType) ?? typeof(string);
                    readersFactory(type)(data, ref index, ref result, context);
                };
        }

        private static Type GetTypeByTypeNameType(string typeName)
        {
            return DerivedTypesSerializationReflectionHelper<TBase, TTypeNameAttribute>.GetTypeByTypeNameType(typeName, attribute => attribute.Type);
        }

        private static string GetTypeNameByType(Type type)
        {
            return DerivedTypesSerializationReflectionHelper<TBase, TTypeNameAttribute>.GetTypeNameByType(type, attribute => attribute.Type);
        }
    }

    public static class DerivedTypesSerializationReflectionHelper<TBase, TAttribute> where TAttribute : Attribute
    {
        public static Type GetTypeByTypeNameType(string typeName, Func<TAttribute, string> attributeKeySelector)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            if (typeNameToTypeMap == null)
            {
                var types = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             where !assembly.IsDynamic && assembly.GetName().Name.StartsWith("SkbKontur.Cassandra.DistributedTaskQueue")
                             from type in assembly.GetTypes()
                             where typeof(TBase).IsAssignableFrom(type)
                             let attribute = (TAttribute)type.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault()
                             where attribute != null
                             select new {key = attributeKeySelector(attribute), type});
                var dict = new Dictionary<string, Type>();
                foreach (var type in types)
                {
                    if (dict.ContainsKey(type.key))
                        throw new InvalidOperationException($"Type attribute duplication for key '{type.key}', types: {dict[type.key].FullName}, {type.type.FullName}");
                    dict.Add(type.key, type.type);
                }
                typeNameToTypeMap = dict;
            }
            return typeNameToTypeMap.TryGetValue(typeName, out var result) ? result : null;
        }

        public static string GetTypeNameByType(Type type, Func<TAttribute, string> attributeKeySelector)
        {
            var typeName = typeToTypeNameMap.GetOrAdd(type,
                                                      _ =>
                                                          {
                                                              var attributes = type.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>().ToArray();
                                                              if (attributes.Length != 1)
                                                                  throw new InvalidOperationException($"Type attribute not found for type: {type}");
                                                              return attributeKeySelector(attributes[0]);
                                                          });
            return typeName;
        }

        // ReSharper disable StaticFieldInGenericType
        // NB !!! Здесь по существу используется тот факт, что на каждое инстанцирование Generic-класса заводится свой набор статических полей.
        private static Dictionary<string, Type> typeNameToTypeMap;

        private static readonly ConcurrentDictionary<Type, string> typeToTypeNameMap = new ConcurrentDictionary<Type, string>();
        // ReSharper restore StaticFieldInGenericType
    }
}