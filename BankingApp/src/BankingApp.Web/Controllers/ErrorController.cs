namespace BankingApp.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("Error/401")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult UnauthorizedError()
    {
        return View("401");
    }

    [Route("Error/403")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ForbiddenError()
    {
        return View("403");
    }

    [Route("Error/404")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFoundError()
    {
        return View("404");
    }

    [Route("Error/500")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ServerError()
    {
        return View("500");
    }
}
