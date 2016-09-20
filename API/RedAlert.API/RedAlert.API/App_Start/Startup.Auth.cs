using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using RedAlert.API.BL;

namespace RedAlert.API
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            
            //app.UseMicrosoftAccountAuthentication(
            // clientId: ConfigManager.MicrosoftClientId,
            // clientSecret: ConfigManager.MicrosoftSecretKey);

            //app.UseTwitterAuthentication(
            // consumerKey: "",
            // consumerSecret: "");

            app.UseFacebookAuthentication(
             appId: ConfigManager.FacebookClientId,
             appSecret: ConfigManager.FacebookSecretKey);

            app.UseGoogleAuthentication(
                ConfigManager.GoogleClientId,
                ConfigManager.GoogleSecretKey);
        }
    }
}