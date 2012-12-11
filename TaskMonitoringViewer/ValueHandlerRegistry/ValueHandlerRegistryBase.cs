using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ValueHandlerRegistry
{
    public abstract class ValueHandlerRegistryBase<T> : IValueHandlerRegistry<T>
        where T : ITaskData
    {
        public Func<object, object> GetValueHanlder(string path)
        {
            Func<object, object> result;
            if(!dictionary.TryGetValue(path, out result))
                result = null;
            return result;
        }

        protected void Register<TValue>(Expression<Func<T, TValue>> path, Func<object, object> handler)
        {
            dictionary.Add(ExpressionTextExtractor.GetExpressionText(path), handler);
        }

        private readonly Dictionary<string, Func<object, object>> dictionary = new Dictionary<string, Func<object, object>>();
    }
}