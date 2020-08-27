using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute("RemindersConfig", typeof(Glorious.Reminders.Startup))]
namespace Glorious.Reminders
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
