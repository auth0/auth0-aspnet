Add the following to your App_Start\WebApiConfig.cs file under the Register method:

config.MessageHandlers.Add(new WebApi.App_Start.JsonWebTokenValidationHandler
{
    Audience = "..your-client-id..",
    SymmetricKey = "....your-client-secret...."
});