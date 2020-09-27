using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NYFInter.Startup))]
namespace NYFInter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
