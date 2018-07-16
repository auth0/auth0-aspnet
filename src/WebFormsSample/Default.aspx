<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormsSample.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>    
    <% if (!Request.IsAuthenticated) { %>
        <script src="https://cdn.auth0.com/js/lock/10.8.0/lock.min.js"></script>
        <script type="text/javascript">
            var lock = new Auth0Lock(
                "<%= System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"] %>",
                "<%= System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"] %>",
                {
                    auth: {
                        redirectUrl: window.location.origin + '/LoginCallback.ashx',
                        responseType: 'code',
                        params: {
                            scope: 'openid profile'
                        }
                    }
                }
              );
        </script>
        <button onclick="lock.show()">Login</button>
    <% } else { %>
        <form runat="server">
            Hi <%= User.Identity.Name %>!
            <asp:Button runat="server" OnClick="Logout_Click" Text="Logout" />
        </form>
    <% } %>
</body>
</html>
