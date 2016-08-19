using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

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

            
            app.UseMicrosoftAccountAuthentication(
             clientId: "9175cab7-24b8-4e2e-b89e-08fde0fcd270",
             clientSecret: "ZPjkMpkcoyvSWP4fDtrkvkv");

            //app.UseTwitterAuthentication(
            // consumerKey: "",
            // consumerSecret: "");

            app.UseFacebookAuthentication(
             appId: "888396751264300",
             appSecret: "1c71fa8e65465b9a90fad55cf91d4343");

            app.UseGoogleAuthentication("320583182272-6nkt1lq6g1p8p6lmdeeroutspkdk2ojq.apps.googleusercontent.com", "Lsxc-Zh9kLzXtlWdkR3kvHcV");
        }
    }
}