using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SteppingStone.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Enable Attribute Routing
            routes.LowercaseUrls = true;
            routes.MapMvcAttributeRoutes();

            // References
            //routes.MapRoute(name: "", url: "Admin/Candidates/{id}/Referees/{referenceId}/{action}", defaults: new { controller = "Referees", action = "Show" }, constraints: new { id = @"\d+", referenceId = @"\d+" });
            //routes.MapRoute(name: "", url: "Admin/Candidates/{id}/Referees/{action}", defaults: new { controller = "Referees", action = "Index" }, constraints: new { id = @"\d+" });

            routes.MapRoute(name: "", url: "Admin/{roleName}/{id}/{action}", defaults: new { controller = "Users", action = "Show" }, constraints: new { id = @"\d+", roleName = @"^(admin)s$" });

            //routes.MapRoute(name: "", url: "References/{id}/{action}", defaults: new { controller = "References" }, constraints: new { id = @"\d+" });
            routes.MapRoute(name: "", url: "Expenses/{ExpenseId}/{action}", defaults: new { controller = "Expenses" }, constraints: new { ExpenseId = @"\d+" });
                        
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "SteppingStone.WebUI.Controllers" }
            );
        }
    }
}
