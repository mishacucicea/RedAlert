using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RedAlert.API.Startup))]
namespace RedAlert.API
{

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}