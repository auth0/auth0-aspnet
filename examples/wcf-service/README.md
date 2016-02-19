# Auth0 + WCF WebAPI Sample
This is the sample project you need to use if you're going to create an WCF WebAPI application.

#Running the example
In order to run the example you need to have Visual Studio 2013 installed.

Install package `Auth0-WCF-Service-JWT` via Package Manager or running the following command:

```Powershell
Install-Package Auth0-WCF-Service-JWT
```

You also need to set the ClientSecret and ClientId of your Auth0 app. To do that find the following lines and modify accordingly in `web.config`:

```CSharp
<add key="jwt:AllowedAudience" value="{CLIENT_ID}" />
<add key="jwt:SymmetricKey" value="{CLIENT_SECRET}" />
```

Also, modify line 31 in `index.html`
```js
var lock = new Auth0Lock('{CLIENT_ID}','{CLIENT_DOMAIN}');
```

After that just press **F5** to run the application. It will start running in port **49732** and open [http://localhost:49732/](http://localhost:49732/) in your web browser.