using Microsoft.AspNet.Mvc;
using System;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld.Controllers.Web
{
    public class AppController : Controller
    {
        private IMailService _mailService;

        public AppController(IMailService service)
        {
            _mailService = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            // server side validation
            if (ModelState.IsValid)
            {
                // we are using colon (':') as a separator
                // because the Configuration object will hold all sorts
                // of configuration properties, not only in json format
                var email = Startup.Configuration["AppSettings:SiteEmailAddress"];

                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError("", "Could not send email, configuration problem");
                }

                if (_mailService.SendMail(
                        email, // to
                        email, // from
                        $"Contact Page from {model.Name} ({model.Email})", // subject
                        model.Message)) // body
                {
                    // clean the state and also the form
                    ModelState.Clear();
                    ViewBag.Message = "Mail sent. Thanks!";
                }
            }

            return View();
        }
    }
}
