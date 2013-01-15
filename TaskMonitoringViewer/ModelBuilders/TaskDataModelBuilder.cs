using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ValueHandlerRegistry;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class TaskDataModelBuilder : ITaskDataModelBuilder
    {
        public TaskDataModelBuilder(IValueHandlerRegistryCollection valueHandlerRegistryCollection, IFileDataStorage fileDataStorage)
        {
            this.valueHandlerRegistryCollection = valueHandlerRegistryCollection;
            this.fileDataStorage = fileDataStorage;
        }

        public ITaskDataValue Build(string taskId, ITaskData taskData)
        {
            return (ITaskDataValue)MakeBuildMethod(taskData.GetType()).Invoke(this, new object[] {taskId, taskData});
        }

        private MethodInfo MakeBuildMethod(Type type)
        {
            Expression<Func<string, object, ITaskDataValue>> expression = (id, data) => BuildInternal<ITaskData>(id, data);
            return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition().MakeGenericMethod(type);
        }

        private ITaskDataValue BuildInternal<T>(string taskId, object taskData) where T : ITaskData
        {
            return BuildTaskDataValue(taskId, valueHandlerRegistryCollection.Get<T>(), "", typeof(T), taskData);
        }

        private ITaskDataValue BuildTaskDataValue<T>(string taskId, IValueHandlerRegistry<T> valueHandlerRegistry, string path, Type objectType, object objectValue) where T : ITaskData
        {
            if(objectValue == null)
                return new EmptyTaskDataValue();
            if(objectType == typeof(byte[]))
            {
                var bytes = (byte[])objectValue;
                return new ByteArrayTaskDataValue
                    {
                        TaskId = taskId,
                        Path = path,
                        Size = bytes.Length
                    };
            }
            if (objectType == typeof(string))
            {
                string fileName;
                var fileId = (string)objectValue;
                if (!string.IsNullOrEmpty(fileId) && fileDataStorage.TryReadFilename(fileId, out fileName))
                {
                    return new FileDataTaskDataValue
                    {
                        Filename = fileName,
                        FileSize = fileDataStorage.GetFileSize(fileId),
                        // ReSharper disable Mvc.ControllerNotResolved
// ReSharper disable Asp.NotResolved
                        GetUrl = url => url.Action("Run", "FileData", new { fileId })
// ReSharper restore Asp.NotResolved
                        // ReSharper restore Mvc.ControllerNotResolved
                    };
                }
            }
            if(IsPrimitive(objectType))
            {
                return new StringTaskDataValue
                    {
                        Value = objectValue.ToString()
                    };
            }
            if(objectType.IsArray)
            {
                var array = (Array)objectValue;
                var elementType = objectType.GetElementType();
                return new ObjectTaskDataModel
                    {
                        Properties = array.Cast<object>().Select((element, i) => new TaskDataProperty
                            {
                                Name = i.ToString(),
                                Value = BuildTaskDataValue(taskId, valueHandlerRegistry, ConcatPaths(path, i.ToString()), elementType, element),
                                Hidden = true
                            }).ToArray()
                    };
            }
            return BuildObjectTaskDataModel(taskId, valueHandlerRegistry, path, objectType, objectValue);
        }

        private ObjectTaskDataModel BuildObjectTaskDataModel<T>(string taskId, IValueHandlerRegistry<T> valueHandlerRegistry, string path, Type type, object obj) where T : ITaskData
        {
            return new ObjectTaskDataModel
                {
                    Properties = GetProperties(type).Select(propertyInfo => BuildTaskDataProperty(taskId, valueHandlerRegistry, path, obj, propertyInfo)).ToArray()
                };
        }

        private TaskDataProperty BuildTaskDataProperty<T>(string taskId, IValueHandlerRegistry<T> valueHandlerRegistry, string path, object obj, PropertyInfo propertyInfo) where T : ITaskData
        {
            var newPath = ConcatPaths(path, propertyInfo.Name);
            var propertyType = propertyInfo.PropertyType;
            var propertyValue = propertyInfo.GetGetMethod().Invoke(obj, new object[0]);
            var valueHandler = valueHandlerRegistry.GetValueHanlder(newPath);
            ITaskDataValue value;
            bool hidden;
            if(valueHandler == null)
            {
                value = BuildTaskDataValue(taskId, valueHandlerRegistry, newPath, propertyType, propertyValue);
                hidden = false;
            }
            else
            {
                var handledValue = valueHandler(propertyValue);
                value = BuildTaskDataValue(taskId, valueHandlerRegistry, newPath, handledValue.GetType(), handledValue);
                hidden = true;
            }
            return new TaskDataProperty
                {
                    Name = propertyInfo.Name,
                    Value = value,
                    Hidden = hidden
                };
        }

        private static string ConcatPaths(string path, string name)
        {
            if(string.IsNullOrEmpty(path))
                return name;
            return path + "." + name;
        }

        private static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type.IsEnum ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitive(type.GetGenericArguments()[0]));
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return propertiesByType.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        }

        private readonly IValueHandlerRegistryCollection valueHandlerRegistryCollection;
        private readonly IFileDataStorage fileDataStorage;

        private readonly ConcurrentDictionary<Type, PropertyInfo[]> propertiesByType = new ConcurrentDictionary<Type, PropertyInfo[]>();
    }
}