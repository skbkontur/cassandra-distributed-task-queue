using System;
using System.Web.Mvc;
using System.Web.Routing;

using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.CookiesManagement;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public class EmptyAuthorizationStrategy : IAuthorizationStrategy
    {
        public void OnControllerActionExecuting(string userId, ActionExecutingContext filterContext, Type controllerType)
        {
        }

        public bool CheckAccess(string userId, ResourseGroups accessLevel)
        {
            return true;
        }

        public void AuthorizeUser(RequestContext requestContext, ICookies cookies, out UserSession session, out User user)
        {
            user = null;
            session = null;
        }
    }
}