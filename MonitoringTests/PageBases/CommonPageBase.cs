using System;
using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

using SKBKontur.Catalogue.WebTestCore.Pages;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public abstract class CommonPageBase : PageBase
    {
        //класс страниц - общий, используется также для страниц без меню пользователя
        protected static TPage RefreshUntil<TPage>(TPage page, Func<TPage, bool> conditionFunc, int timeout = 15000, int waitTimeout = 100)
            where TPage : PageBase, new()
        {
            var w = Stopwatch.StartNew();
            do
            {
                page = RefreshPage(page);
                if(conditionFunc(page))
                    return page;
                Thread.Sleep(waitTimeout);
            } while(w.ElapsedMilliseconds < timeout);
            Assert.Fail("Не смогли дождаться страницу за {0} мс", timeout);
            return default(TPage);
        }
    }
}