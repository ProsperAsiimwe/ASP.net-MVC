using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SteppingStone.WebUI.Startup))]
namespace SteppingStone.WebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
