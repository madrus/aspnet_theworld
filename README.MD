# Notes on the TheWorld solution

## Interface Tips
* Shortcut Alt+Shift+W wraps the selected piece of HTML code in DIV tags
* Shortcut Alt+Shift+???? compiles our LESS file to CSS

## HTML
**HTML Entity** is a shortcut to display certain kinds of characters.
Just type an ampersand '&' and watch the intellisense :)

## Index.cshtml
We need to use '~/' in front so that MVC knows 
how to find out CSS file. '~/' is the project 
root directory -->

## Startup.cs
```csharp
app.UseMvc(config =>
{
    config.MapRoute(
        name: "Default",
        // in our case we have AppController.Index()
        // but no 'id', so it will not be looking
        // for the parameter of the Index method
        template: "{controller}/{action}/{id?}",
        // basically we are telling here that // localhost:8000
        // should be same as localhost:8000/app/index
        defaults: new { controller = "App", action = "Index" }
    );
});
```
## Solution Structure
*  `Controllers` folder
   * `Web` folder
      * `[ControllerName]Controller.cs`, e.g. `AppController.cs`
*  `Services` folder
   *  `IMailService.cs` interface
*  `ViewModels` folder  
   ViewModels contain classes necessary for Views to display
   the model. They are like views of a model. 
   * `[ViewName]ViewModel.cs`  
     The model reference should be placed at the top 
     of the corresponding `[ViewName]` class  
     `@model` (lowercase) defines the model type (reference).  
     `@Model` (uppercase) would define the model itself to be used on the page.
*  `Views` folder
   *  `[ControllerName]`, e.g. `App`  
      Voor every IActionResult method in the controller there
      should be a .cshtml view file inside this folder
      * `About.cshtml`
      * `Contact.cshtml`
      * `Index.cshtml`
   *  `Shared` folder
      * `_Layout.cshtml` - base template for all views
   *  `_ViewImports.cshtml` - tag helpers
   *  `_ViewStart.cshtml` - layout template reference

## Tag Helpers
*  `asp-for="Name"`  
   This one should be put both on a label and an input.
   Then, if the user clicks on a label, the cursor will be 
   placed inside the input control and id and name will be 
   automagically created (visible in the Web Tools).
*  `asp-validation-summary="ValidationSummary.ModelOnly"`  
   Object validation errors (e.g. form) on the object level, 
   not individual property level
*  `asp-validation-for="Name"`  
   Property validation errors


## Annotations
On a ViewModel we can use annotations for input validation.  
>  `[Required]` - this field is required  
>  `[StringLength(255, MinimumLength = 5)]` - validation of a string input  
>  `[EmailAddress]` - this will validate the input against the email regex
>  `Http[Method]` where Method can be delete, get, post, and update.

We would probably need to add these two extensions to bower.json file:  
*  `jquery-validation`  
*  `jquery-validation-unobtrusive`

## POST method
When we submit the form, we want it write down the data that has been submitted.
This is done by using the POST method on the form:  
> `<form method="post">...</form>`

In the controller, we need to create a corresponding method:
```csharp
[HttpPost]
public IActionResult Contact(ContactViewModel model)
{
    return View();
}
```

The ViewModel class here should be the same as on the View page.

## Services
We need to create services on the server side to handle POST data.
Those are created based on the interface like `IMailService`.

We create a new service inside the Services folder and it should
implement a service interface. Next, we inject the service 
in the controller constructor.
```csharp
private IMailService _mailService;

public AppController(IMailService service)
{
    _mailService = service;
}
```

and add its reference in the Startup.cs (in DEBUG mode if necessary):
```csharp
#if DEBUG
    services.AddScoped<IMailService, DebugMailService>();
#else
    services.AddScoped<IMailService, RealMailService>();
#endif
``` 
>
Even better solution would be to use the new IHostingEnvironment interface
because with the Cloud Services it has become very important to understand
where our project is being hosted.
```csharp
public void ConfigureServices(IServiceCollection services, 
                              IHostingEnvironment env)
{
    ...

    if (env.IsDevelopment())
    {
        services.AddScoped<IMailService, DebugMailService>();
    }
    else
    {
        services.AddScoped<IMailService, MailService>();
    }
}
```

Finally, we will use the new service in our Http method:
```csharp
[HttpPost]
public IActionResult Contact(ContactViewModel model)
{
    _mailService.SendMail(
        "", // to
        "", // from
        $"Contact Page from {model.Name} ({model.Email})", // subject
        model.Message); // body

    return View();
}
```

Actions Taken
-------------
* Add Bootstrap 
  * dep to bower.json
  * min.css to _Layout.cshtml (before site.css)
  * min.js to _Layout.cshtml (after jQuery, befor site.js)
* CSS: body, label, input in comment (to use BS)
* _Layout
  * button: btn + btn-primary
  * p: text-primary
  * i: glyphicon glyphicon-chevron-left
* Add Font-Awesome
  * dep to bower.json
  * min.css to _Layout.cshtml (before site.css)
* _Layout
  * i: fa fa-angle-left
* site.js
  * addClass/removeClass fa-angle-left/fa-angle-right
* choose a [bootswatch.com](bootswatch.com) theme
  * dep in bower.json
  * min.css to _Layout.cshtml (after bootstrap.css)
* CSS: main and footer in comment
  * i: fa fa-angle-left

Entity Framework
----------------
* Add DbContext file (`WorldContex.cs`) to the Models map
* Add SqlServer
* Add Migrations
  * add a new dependency `"EntityFramework.Commands": "7.0.0-beta8"`
    to `project.json`
  * add a new command `"ef": "EntityFramework.Commands"` to `project.json`
  * add connection string to `config.json`
    ```csharp
        "Data": {
            "WorldContextConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TheWorldDB;Trusted_Connection=true;MultipleActiveResultSets=true;"
        }
    ```
  * run `dnx ef`
  * run `dnx ef migrations add InitialDatabase` in the project folder
  * now we get two new classes in our Migrations folder.
    We now have both the schema and the database generation scripts.
  * add `Database.EnsureCreated()` to the WorldContext constructor

Creating the API
----------------
* In the project properties take the checkmark off "Launch URL". 
  So that we could launch our web server instead.
* We are going to use [Postman](https://www.getpostman.com).
  Choose the App version of it.
* Create Api subfolder in Controllers
* Create a `TripController` inside it and derive it from `Controller`
* Create a `Get` method with an `[HttpGet("api/trips")]` attribute
* We can now return a test Json object and see it in Postman
* Next we can add our repository in the ctor and get the real results.
  Now we can see them in the Postman but they are not camel-cased.
  And that could be a bit of a problem in a JavaScript environment.
* In the `Startup.cs`, we can add the special camel-case `ContractResolver`
  to our Mvc service:
```cs
    services.AddMvc()
        .AddJsonOptions(opt =>
        {
            opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });
```
* We add a `[Route("api/trips")]` attribute to the class. This gives the root route
  for all our methods inside this controller. So, `[HttpGet("api/trips")]`
  becomes `[HttpGet("")]`, etc.
* Now, we can change GET to PUT in the Postman and create a test trip in the Body tab.
  For that we choose raw/JSON(application/json) type data.
* Create a POST method in TripController. This could be a nice test implementation
  that allows us to see the incoming data in the debugger:
```cs
    [HttpPost("")]
    public JsonResult Post([FromBody]Trip newTrip)
    {
        return Json(true);
    }
```
* Next we have to look at validation. And we have done that already in our `AppController`. 
  So, it could be something like this:
```cs
    [HttpPost("")]
    public JsonResult Post([FromBody]Trip newTrip)
    {
        if (ModelState.IsValid)
        {
            Response.StatusCode = (int)HttpStatusCode.Created;
            return Json(true);
        }

        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json("Failed");
    }
```
* The problem remains that in this way we are exposing all the properties
  of the underlying entity (`Trip`) and we don't want that. Therefore, we
  will replace it with a TripViewModel class in order to hide it.
* 



