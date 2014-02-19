using System;
using System.Configuration;

namespace WebFormsSample
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Logout_Click(object sender, EventArgs e)
        {
            var returnUrl = this.Request.Url;

            System.IdentityModel.Services.FederatedAuthentication.SessionAuthenticationModule.SignOut();
            
            Response.Redirect(
                string.Format("https://{0}/logout?returnTo={1}",
                    ConfigurationManager.AppSettings["auth0:Domain"],
                    returnUrl));
        }
    }
}