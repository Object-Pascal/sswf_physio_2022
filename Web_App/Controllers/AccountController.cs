using Microsoft.AspNetCore.Mvc;
using Web_App.Models;
using Web_App.Session;

namespace Web_App.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        [Route("/Account/AccessDenied/{ReturnUrl?}")]
        public IActionResult AccessDenied(string ReturnUrl)
        {
            InfoModel infoModel = new InfoModel();
            infoModel.Title = "Access Denied";
            infoModel.Description = $"The access to this resource is denied.";
            infoModel.HasImportantData = false;

            infoModel.TableEntries.Add("URL", ReturnUrl ?? "null");

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }
    }
}
