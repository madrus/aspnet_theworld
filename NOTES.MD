# Notes on the TheWorld solution

## Interface Tips
* Shortcut Alt+Shift+W wraps the selected piece of HTML code in DIV tags

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

#Actions Taken
* Add Bootstrap 
  * dep to bower.json
  * min.css to _Layout.cshtml (before site.css)
  * min.js to _Layout.cshtml (after jQuery, befor site.js)