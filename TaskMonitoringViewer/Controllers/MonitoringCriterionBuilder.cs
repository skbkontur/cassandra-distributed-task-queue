using System;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.Expressions.Visitors;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public class MonitoringSearchRequestCriterionBuilder : IMonitoringSearchRequestCriterionBuilder
    {
        public Expression<Func<MonitoringTaskMetadata, bool>> BuildCriterion(MonitoringSearchRequest searchRequest)
        {
            Expression<Func<MonitoringTaskMetadata, bool>> criterion = x => true;
            if(searchRequest.States != null && searchRequest.States.Length > 0)
            {
                Expression<Func<MonitoringTaskMetadata, bool>> cr = x => false;
                foreach(var state in searchRequest.States)
                {
                    var state1 = state;
                    cr = Or(cr, x => x.State == state1);
                }
                criterion = And(criterion, cr);
            }
            if(!string.IsNullOrEmpty(searchRequest.Name))
                criterion = And(criterion, x => x.Name == searchRequest.Name);
            if(!string.IsNullOrEmpty(searchRequest.TaskId))
                criterion = And(criterion, x => x.TaskId == searchRequest.TaskId);
            if(!string.IsNullOrEmpty(searchRequest.ParentTaskId))
                criterion = And(criterion, x => x.ParentTaskId == searchRequest.ParentTaskId);

            AddDataTimeRangeCriterion(ref criterion, x => x.Ticks, searchRequest.Ticks);
            AddDataTimeRangeCriterion(ref criterion, x => x.MinimalStartTicks, searchRequest.MinimalStartTicks);
            AddDataTimeRangeCriterion(ref criterion, x => x.StartExecutingTicks, searchRequest.StartExecutingTicks);

            return criterion;
        }

        public Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var pr = new ParameterReplacer(a.Parameters[0], b.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(pr.Visit(a.Body), b.Body), b.Parameters[0]);
        }

        public Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var pr = new ParameterReplacer(a.Parameters[0], b.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(pr.Visit(a.Body), b.Body), b.Parameters[0]);
        }

        private void AddDataTimeRangeCriterion(ref Expression<Func<MonitoringTaskMetadata, bool>> criterion, Expression<Func<MonitoringTaskMetadata, DateTime?>> pathToTicks, DateTimeRange dateTimeRange)
        {
            if(dateTimeRange.From != null)
            {
                criterion = And(criterion, pathToTicks.Merge(time => time >= dateTimeRange.From));
            }
            if(dateTimeRange.To != null)
            {
                criterion = And(criterion, pathToTicks.Merge(time => time <= dateTimeRange.To));
            }
        }
    }
}