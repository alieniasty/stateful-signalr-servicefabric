using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.ServiceFabric.Data;
using Owin;
using SignalR.Hubs;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace SignalR
{
    public static class Startup 
    {
        public static void ConfigureApp(IAppBuilder appBuilder, IReliableStateManager stateManager)
        {
            var config = new HttpConfiguration();
            var container = new Container();

            config.MapHttpAttributeRoutes();
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            container.RegisterSingleton(() => stateManager);
            container.RegisterSingleton<TestNotificationHub>();

            appBuilder.UseWebApi(config);
            appBuilder.ConfigureSignalR();
        }

        public static void ConfigureSignalR(this IAppBuilder app)
        {
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true
                };

                map.RunSignalR(hubConfiguration);
            });
        }
    }
}
