using Microsoft.IdentityModel.Claims;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading;

namespace Auth0_Wcf_Sample
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WcfService
    {
        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        [OperationContract]
        [WebGet]
        public string DoWork()
        {
            // Add your operation implementation here
            var userProfile = ((IClaimsIdentity)Thread.CurrentPrincipal.Identity).Claims;
            var result = "";

            foreach(var claim in userProfile)
                result += claim.ClaimType + ' ' + claim.Value + ", ";

            return result;
            /*
            Return all the claims that Identity have. You can use it like that:
            var email = userProfile.SingleOrDefault(c => c.ClaimType == "email");
            if (email != null)
            {
                return "Hello " + email;
            }
            else
            {
                return "Hello " + Thread.CurrentPrincipal.Identity.Name;
            } 
            */
        }

        // Add more operations here and mark them with [OperationContract]
    }
}
