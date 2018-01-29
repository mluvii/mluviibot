using System.Web.Mvc;
using System.Web.Routing;

namespace MluviiBot
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute("CheckOut", "Checkout", new { controller = "CheckOut", action = "Index" });
        }
    }
}
