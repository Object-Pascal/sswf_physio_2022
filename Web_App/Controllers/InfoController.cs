using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Web_App.Models;
using Web_App.Session;

namespace Web_App.Controllers
{
    public class InfoController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Index(InfoModel redirectModel)
        {
            redirectModel.TableEntries = HttpContext.Session.GetObject<Dictionary<string, string>>("InfoEntries");
            return View(redirectModel);
        }
    }
}