using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(GymNet.Startup))]

namespace GymNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}