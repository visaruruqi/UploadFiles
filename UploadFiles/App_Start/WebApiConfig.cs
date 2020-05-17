using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace UploadFiles
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute("DefaultApiWithAction", "api/{controller}/{action}");

            //Configuration.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter());
            //config.Formatters.Add(new FormMultipa)
            //config.Formatters.XmlFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data"));
        }
    }
}
