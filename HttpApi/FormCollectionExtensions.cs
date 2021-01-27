using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HttpApi
{
    public static class FormCollectionExtensions
    {
        /// <summary>
        /// A naive model binding that takes the values found in the IFormCollection
        /// </summary>
        /// <param name="formCollection"></param>
        /// <param name="model"></param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public static TModel Bind<TModel>(this IFormCollection formCollection, TModel model = default)
            where TModel : new()
        {
            model ??= new TModel();
            
            var properties = typeof(TModel)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in properties)
            {
                if (formCollection.TryGetValue(property.Name, out var values) ||
                    formCollection.TryGetValue($"{property.Name}[]", out values)) 
                {
                    // support generic collections
                    if (property.PropertyType.IsAssignableTo(typeof(IEnumerable)) &&
                        property.PropertyType != typeof(string) &&
                        property.PropertyType.IsGenericType)
                    {
                        var collectionType = property.PropertyType.GenericTypeArguments[0];
                        
                        var mi = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
                        var cast = mi?.MakeGenericMethod(collectionType);

                        var items = values.Select<string, object?>(v => ConvertToType(v, collectionType));
                        var collection = cast?.Invoke(null, new object?[]{ items });
                        property.SetValue(model, collection);
                    }
                    else
                    {
                        // last in wins
                        var result = ConvertToType(values[^1], property.PropertyType);
                        property.SetValue(model, result);    
                    }
                }
            }

            return model;
        }

        public static async Task<TModel> BindFromFormAsync<TModel>(this HttpRequest request, TModel model = default)
            where TModel : new()
        {
            var form = await request.ReadFormAsync();
            return form.Bind(model);
        }

        private static object? ConvertToType(string value, Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (value.Length > 0)
            {
                if (type == typeof(DateTimeOffset) || underlyingType == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
                }
                
                if (type == typeof(DateTime) || underlyingType == typeof(DateTime))
                {
                    return DateTime.Parse(value, CultureInfo.InvariantCulture);
                }

                if (type == typeof(Guid) || underlyingType == typeof(Guid))
                {
                    return new Guid(value);
                }

                if (type == typeof(Uri) || underlyingType == typeof(Uri))
                {
                    if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        return uri;
                    }

                    return null;
                }
            }
            else
            {
                if (type == typeof(Guid))
                {
                    return default(Guid);
                }

                if (underlyingType != null)
                {
                    return null;
                }
            }

            if (underlyingType is not null)
            {
                return Convert.ChangeType(value, underlyingType);
            }

            return Convert.ChangeType(value, type);
        }
    }
}