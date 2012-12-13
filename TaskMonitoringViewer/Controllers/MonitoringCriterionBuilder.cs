using System;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Expressions.Visitors;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public class MonitoringSearchRequestCriterionBuilder : IMonitoringSearchRequestCriterionBuilder
    {
        public Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> BuildCriterion(MonitoringSearchRequest searchRequest)
        {
            Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion = x => true;
            if(searchRequest.States != null && searchRequest.States.Length > 0)
            {
                Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> cr = x => false;
                foreach(var state in searchRequest.States)
                {
                    var state1 = state;
                    cr = Or(cr, x => x.Info.State == state1);
                }
                criterion = And(criterion, cr);
            }
            if(!string.IsNullOrEmpty(searchRequest.Name))
                criterion = And(criterion, x => x.Info.Name == searchRequest.Name);
            if(!string.IsNullOrEmpty(searchRequest.Id))
                criterion = And(criterion, x => x.Id == searchRequest.Id);
            if(!string.IsNullOrEmpty(searchRequest.ParentTaskId))
                criterion = And(criterion, x => x.Info.ParentTaskId == searchRequest.ParentTaskId);

            AddDataTimeRangeCriterion(ref criterion, x => x.Info.Ticks, searchRequest.Ticks);
            AddDataTimeRangeCriterion(ref criterion, x => x.Info.MinimalStartTicks, searchRequest.MinimalStartTicks);
            AddDataTimeRangeCriterion(ref criterion, x => x.Info.StartExecutingTicks, searchRequest.StartExecutingTicks);

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

        private void AddDataTimeRangeCriterion(ref Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion, Func<TaskMetaInformationBusinessObjectWrap, long?> pathToTicks, DateTimeRange dateTimeRange)
        {
            if(dateTimeRange.From != null)
            {
                Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> cr = x => pathToTicks(x).HasValue ? new DateTime(pathToTicks(x).Value) >= dateTimeRange.From : false;
                criterion = And(criterion, cr);
            }
            if(dateTimeRange.To != null)
            {
                Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> cr = x => pathToTicks(x).HasValue ? new DateTime(pathToTicks(x).Value) <= dateTimeRange.To : false;
                criterion = And(criterion, cr);
            }
        }
    }
}