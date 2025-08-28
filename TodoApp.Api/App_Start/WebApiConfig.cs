using System.Web.Http;

namespace WebApiDemo1
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Add OWASP security headers globally
            config.MessageHandlers.Add(new Security.SecurityHeadersHandler());
            // Force JSON responses by default
            var jsonFormatter = config.Formatters.JsonFormatter;
            config.Formatters.Remove(config.Formatters.XmlFormatter);



            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
