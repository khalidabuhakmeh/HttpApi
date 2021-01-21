using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using HttpApi.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HttpApi.Modules
{
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
}