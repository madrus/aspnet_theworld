using Microsoft.AspNet.Mvc;
using System;
using System.Linq;
using Microsoft.AspNet.Authorization;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld.Controllers.Web
{
    public class AppController : Controller
    {
        private readonly IMailService _mailService;
        // after creating the repository, we can use its interface instead of the context
        //private readonly WorldContext _context;
        private readonly IWorldRepository _repository;

        // inject our DbContext into the controller
        public AppController(IMailService service, IWorldRepository repository)
        {
            _mailService = service;
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Trips()
        {
            var trips = _repository.GetAllTrips();
            // for the @model to work in the view, we give trips back as a param
            return View(trips);
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
