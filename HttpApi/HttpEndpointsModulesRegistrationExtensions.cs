#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace HttpApi
{
    public static class HttpEndpointsModulesRegistrationExtensions
    {
        /// <summary>
        /// Will map all public instance endpoint methods that have an HttpMethodAttribute, which includes:
        ///
        /// - HttpGet
        /// - HttpPost
        /// - HttpPut
        /// - HttpDelete
        /// - HttpPatch
        ///
        /// Modules must be added to the services collection in ConfigureServices. Modules will be created
        /// using the DI features of ASP.NET Core.
        ///
        /// Endpoint methods are allowed to have additional parameters, which will be resolved via the
        /// services collection as a courtesy to developers.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <typeparam name="THttpEndpointsModule"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEndpointRouteBuilder Map<THttpEndpointsModule>(this IEndpointRouteBuilder endpoints)
        {
            var type = typeof(THttpEndpointsModule);

            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => new {method = m, attribute = m.GetCustomAttribute<HttpMethodAttribute>(true)})
                .Where(m => m.attribute is not null)
                .Select(m => new {
                    methodInfo = m.method, 
                    template = m.attribute?.Template, 
                    httpMethod = m.attribute?.HttpMethods
                })
                .ToList();
            
            foreach (var method in methods)
            {
                if (method.methodInfo.ReturnType != typeof(Task))
                    throw new Exception($"Endpoints must return a {nameof(Task)}.");
                
                if (method.methodInfo.GetParameters().FirstOrDefault()?.ParameterType != typeof(HttpContext))
                    throw new Exception($"{nameof(HttpContext)} must be the first parameter of any endpoint.");
                
                endpoints.MapMethods(method.template, method.httpMethod, context => {
                    var module = endpoints.ServiceProvider.GetService(type);

                    if (module is null) {
                        throw new Exception($"{type.Name} is not registered in services collection.");
                    }
                    
                    var parameters = method.methodInfo.GetParameters();
                    List<object?> arguments = new() { context };
                    
                    // skip httpContext
                    foreach (var parameter in parameters.Skip(1))
                    {
                        var arg = endpoints.ServiceProvider.GetService(parameter.ParameterType);
                        if (arg is null) {
                            throw new Exception($"{parameter.ParameterType} is not registered in services collection.");
                        }

                        arguments.Add(arg);
                    }
                    
                    var task = method.methodInfo.Invoke(module, arguments.ToArray()) as Task;
                    return task!;
                });
            }

            return endpoints;
        }
    }
}