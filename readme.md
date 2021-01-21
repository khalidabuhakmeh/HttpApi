# 🦸 Super Basic HTTP API

This is a project with a very simple HTTP API written using ASP.NET Core. Inspired by my work with [Ktor](https://ktor.io), I was curious what kind of minimalism I could achieve when building an HTTP API.

I added some naive form model binding that also supports collections.

## Startup

Endpoints are grouped by `Module` and are a collection of endpoints.

```c#
app.UseEndpoints(endpoints =>
{
    endpoints.Map<HelloWorldModule>();
});
```

Modules **must be registered** in the services collection, as they are created using the dependency injection of ASP.NET Core. Modules can be registered as **Singleton** or as **Transient**, but it's recommended to register them as **Transient** first.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(new Person {Name = "Khalid"});
    services.AddTransient<HelloWorldModule>();
}
```

## Modules

Modules are created using the DI features of ASP.NET Core. That means modules can use constructor dependency injection. In addition to constructor DI, any additional parameters to a method will be resolved from the ServicesCollection.

Endpoints are defined by the presence of an `HttpMethodAttribute`, which comes from ASP.NET MVC. These attributes hold metadata regarding the HTTP methods and the path template.

```c#
public class HelloWorldModule
{
    private readonly Person _person;

    public HelloWorldModule(Person person)
    {
        _person = person;
    }
    
    [HttpGet("/")]
    public async Task Index(HttpContext context, Person person)
    {
        await context.Response.WriteAsync($"Hello {person.Name}!");
    }

    [HttpGet("/bye")]
    public Task Get(HttpContext context)
    {
        return context.Response.WriteAsync($"Goodbye, {_person.Name}!");
    }

    [HttpGet("/person")]
    public async Task GetPerson(HttpContext context)
    {
        var reader = File.OpenText("./Views/Form.html");
        context.Response.ContentType = "text/html";
        var html = await reader.ReadToEndAsync();
        await context.Response.WriteAsync(html);
    }

    [HttpPost("/person")]
    public async Task Post(HttpContext context)
    {
        var person = await context.Request.BindFromFormAsync<PersonRequest>();
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync($"<h1>&#129321; Happy Birthday, {person.Name} ({person.Birthday:d})!</h1>");
        await context.Response.WriteAsync($"<h2>Counting {string.Join(",",person.Count)}...</h2>");
    }
    
}
```

## License

Copyright 2021 Khalid Abuhakmeh

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.





