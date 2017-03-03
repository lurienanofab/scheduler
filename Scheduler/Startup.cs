using Owin;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using LNF.Impl;
using LNF.WebApi;

namespace Scheduler
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseDataAccess();

            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}