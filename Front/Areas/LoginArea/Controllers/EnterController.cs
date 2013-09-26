using System.Web.Mvc;

using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton;
using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton.Post;
using SKBKontur.Catalogue.Core.Web.Blocks.KeyboardNavigation;
using SKBKontur.Catalogue.Core.Web.Blocks.PostUrl;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.CookiesManagement;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Models.Enter;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Controllers
{
    public class EnterController : ControllerBase
    {
        public EnterController(ControllerBaseParameters parameters, IAuthenticatorFactory authenticatorFactory, ISimpleHtmlModelsCreator<EnterModelData> htmlModelsCreator)
            : base(parameters)
        {
            this.authenticatorFactory = authenticatorFactory;
            this.htmlModelsCreator = htmlModelsCreator;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string backUrl, string message, string authenticatorType)
        {
            if(!string.IsNullOrEmpty(Cookies.KonturPortalTokenString.Value))
            {
                var userSession = authenticatorFactory.GetAuthenticator(authenticatorType)
                    .AuthenticateByPortalToken(Cookies.KonturPortalTokenString.Value, ApplicationSettings.GetKonturPortalCrypt());
                if(userSession != null)
                {
                    Cookies.SessionString = new Cookie<string>(UserSession.ToSessionString(userSession), null);
                    return Redirect(backUrl ?? successUrl);
                }
            }
            if(!string.IsNullOrEmpty(Cookies.SessionString.Value) && !string.IsNullOrEmpty(Cookies.KonturPortalTokenString.Value))
                return Redirect(backUrl ?? successUrl);

            var enterPageModel = new EnterPageModel(PageModelBaseParameters);
            enterPageModel.Data.Login = Cookies.Login.Value;
            if(message == userNotFound)
            {
                enterPageModel.UserNotFoundClass = "login_field__error login_field__userNotFound";
                enterPageModel.Data.ErrorMessage = enterPageModel.EnterPageText.UserNotFound;
            }
            if(message == invalidPassword)
            {
                enterPageModel.InvalidPasswordClass = "login_field__error login_field__invalidPassword";
                enterPageModel.Data.ErrorMessage = enterPageModel.EnterPageText.InvalidPassword;
            }
            enterPageModel.BackUrl = backUrl;
            enterPageModel.AuthenticatorType = authenticatorType;
            enterPageModel.LoginButton = htmlModelsCreator.PostActionButtonFor(
                new PostUrl<EnterModelData>(url => url.Action("Login", "Enter")).AddParameter("backUrl", enterPageModel.BackUrl).AddParameter("authenticatorType", enterPageModel.AuthenticatorType),
                new PostActionButtonOptions
                {
                    Id = "loginSubmit",
                    Style = ActionButtonStyle.Enter,
                    Title = enterPageModel.EnterPageText.DoLogin,
                    KeyCombination = KeyCombination.Enter
                });
            return View("EnterView", enterPageModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(string backUrl, string authenticatorType, EnterModelData data)
        {
            Cookies.Login = new Cookie<string>(data.Login, Expiration.never);
            try
            {
                var userSession = authenticatorFactory.GetAuthenticator(authenticatorType)
                    .Authenticate(data.Login, data.Password);
                Cookies.SetSession(userSession);
            }
            catch(UserNotFoundException)
            {
                return Json(new SuccessOperationResult
                {
                    NeedRedirect = true,
                    RedirectTo = Url.Action("Run", new {backUrl, message = userNotFound, authenticatorType})
                });
            }
            catch(InvalidPasswordException)
            {
                return Json(new SuccessOperationResult
                {
                    NeedRedirect = true,
                    RedirectTo = Url.Action("Run", new {backUrl, message = invalidPassword, authenticatorType})
                });
            }
            return Json(new SuccessOperationResult
            {
                NeedRedirect = true,
                RedirectTo = string.IsNullOrEmpty(backUrl) ? successUrl : backUrl
            });
        }

        private readonly IAuthenticatorFactory authenticatorFactory;
        private readonly ISimpleHtmlModelsCreator<EnterModelData> htmlModelsCreator;
        private const string userNotFound = "UserNotFound";
        private const string invalidPassword = "InvalidPassword";
        private const string successUrl = "/AdminTools/RemoteTaskQueue";
    }
}