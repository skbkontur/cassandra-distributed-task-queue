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
            if(searchRequest.TaskStates != null && searchRequest.TaskStates.Length > 0)
            {
                Expression<Func<MonitoringTaskMetadata, bool>> cr = x => false;
                foreach(var state in searchRequest.TaskStates)
                {
                    var state1 = state;
                    cr = Or(cr, x => x.State == state1);
                }
                criterion = And(criterion, cr);
            }
            if (searchRequest.TaskNames != null && searchRequest.TaskNames.Length > 0)
            {
                Expression<Func<MonitoringTaskMetadata, bool>> cr = x => false;
                foreach (var taskName in searchRequest.TaskNames)
                {
                    var taskName1 = taskName;
                    cr = Or(cr, x => x.Name == taskName1);
                }
                criterion = And(criterion, cr);
            }
            if(!string.IsNullOrEmpty(searchRequest.TaskId))
                criterion = And(criterion, x => x.TaskId == searchRequest.TaskId);
            if(!string.IsNullOrEmpty(searchRequest.ParentTaskId))
                criterion = And(criterion, x => x.ParentTaskId == searchRequest.ParentTaskId);

            AddDataTimeRangeCriterion(ref criterion, x => x.Ticks, searchRequest.Ticks);
            AddDataTimeRangeCriterion(ref criterion, x => x.MinimalStartTicks, searchRequest.MinimalStartTicks);
            AddDataTimeRangeCriterion(ref criterion, x => x.StartExecutingTicks, searchRequest.StartExecutingTicks);
            AddDataTimeRangeCriterion(ref criterion, x => x.FinishExecutingTicks, searchRequest.FinishExecutingTicks);

            return criterion;
        }

        private static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var pr = new ParameterReplacer(a.Parameters[0], b.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(pr.Visit(a.Body), b.Body), b.Parameters[0]);
        }

        private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var pr = new ParameterReplacer(a.Parameters[0], b.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(pr.Visit(a.Body), b.Body), b.Parameters[0]);
        }

        private static void AddDataTimeRangeCriterion(ref Expression<Func<MonitoringTaskMetadata, bool>> criterion, Expression<Func<MonitoringTaskMetadata, DateTime?>> pathToTicks, DateTimeRange dateTimeRange)
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