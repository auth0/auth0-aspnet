<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormsSample.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>    
    <% if (!Request.IsAuthenticated) { %>
        <script src="https://cdn.auth0.com/w2/auth0-widget-5.1.min.js"></script>
        <script type="text/javascript">
            var widget = new Auth0Widget({
                domain:     '<%= System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"] %>',
                clientID:   '<%= System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"] %>',
                callbackURL: window.location.origin + '/LoginCallback.ashx'
            });
        </script>
        <button onclick="widget.signin()">Login</button>
    <% } else { %>
        <form runat="server">
            Hi <%= System.Security.Claims.ClaimsPrincipal.Current.FindFirst("name").Value %>!
            <asp:Button runat="server" OnClick="Logout_Click" Text="Logout" />
        </form>
    <% } %>
</body>
</html>
