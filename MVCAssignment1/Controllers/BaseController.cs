using System.Web;
using System.Web.Mvc;

public class BaseController : Controller
{
    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (!User.Identity.IsAuthenticated)
        {
            // Store the return URL to redirect after login
            string returnUrl = HttpUtility.UrlEncode(filterContext.HttpContext.Request.RawUrl);
            filterContext.Result = new RedirectResult(Url.Action("Login", "Home", new { returnUrl }));
        }
        base.OnActionExecuting(filterContext);
    }
}
