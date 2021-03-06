﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using fIT.WebApi.Manager;
using fIT.WebApi.Repository;
using Newtonsoft.Json.Serialization;
using Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin;
using fIT.WebApi.Provider;
using System.Configuration;
using System.IO;
using System.Web.Http.Cors;
using fIT.WebApi.Repository.Interfaces;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Swashbuckle.Application;

namespace fIT.WebApi
{
  public class Startup
  {
    #region Methods
    /// <summary>
    /// Get's fired when the applications is started by the host
    /// </summary>
    /// <param name="app"></param>
    public void Configuration(IAppBuilder app)
    {
      HttpConfiguration httpConfig = new HttpConfiguration();
      ConfigureOAuthTokenGeneration(app);
      ConfigureOAuthTokenConsumption(app);
      ConfigureWebApi(httpConfig);
      app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
      InitialiseSwagger(httpConfig);

      var cors = new EnableCorsAttribute("*", "*", "*");
      httpConfig.EnableCors(cors);

      app.UseWebApi(httpConfig);
    }

    /// <summary>
    /// Initializes Swagger Documentation
    /// </summary>
    /// <param name="httpConfig"></param>
    private static void InitialiseSwagger(HttpConfiguration httpConfig)
    {
      try
      {
        httpConfig
        .EnableSwagger(c =>
        {
          c.SingleApiVersion("v1", "fIT Api");
          c.IncludeXmlComments(GetXmlPath());
          c.IgnoreObsoleteActions();
          c.DescribeAllEnumsAsStrings();
          c.OAuth2("oauth2")
          .Description("OAuth2 Password Grant")
          .Flow("password")
            //.AuthorizationUrl("http://petstore.swagger.wordnik.com/api/oauth/dialog")
          .TokenUrl("/Accounts/Login")
          .Scopes(scopes =>
          {
            scopes.Add("user", "Read access to protected resources");
            scopes.Add("admin", "Write access to protected resources");
          });
        })
        .EnableSwaggerUi(c =>
        {
          c.InjectJavaScript(typeof(Startup).Assembly, "fIT.WebApi/js/onComplete.js");
        });
      }
      catch (Exception e)
      {
        throw new Exception("Error on creating Swagger: " + e.Message + Environment.NewLine + e.StackTrace);
      }

    }

    private static string GetXmlPath()
    {
      return Path.Combine(System.Web.HttpRuntime.AppDomainAppPath, "fIT.WebApi.XML");

    }

    /// <summary>
    /// Configure the db context and user manager to use a single instance per request
    /// </summary>
    /// <param name="app"></param>
    private void ConfigureOAuthTokenGeneration(IAppBuilder app)
    {
      app.CreatePerOwinContext(ApplicationDbContext.Create);
      app.CreatePerOwinContext<IRepository>(ApplicationDbContext.CreateRepository);
      app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
      app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

      OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
      {
#warning For Dev enviroment only (on production should be AllowInsecureHttp = false)
        AllowInsecureHttp = true,
        TokenEndpointPath = new PathString("/api/accounts/login"),
        AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
        Provider = new CustomOAuthProvider(), // specify, how to validate the Resource Owner
        AccessTokenFormat = new CustomJwtFormat(ConfigurationManager.AppSettings["as:Issuer"]), //Specifies the implementation, how to generate the access token
        RefreshTokenProvider = new CustomRefreshTokenProvider()
      };

      // OAuth 2.0 Bearer Access Token Generation
      app.UseOAuthAuthorizationServer(OAuthServerOptions);
    }

    /// <summary>
    /// Defines Web Api Routing and Json-Response-Resolver 
    /// </summary>
    /// <param name="config"></param>
    private void ConfigureWebApi(HttpConfiguration config)
    {
      config.MapHttpAttributeRoutes();

      var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
      jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    }

    /// <summary>
    /// Configures how the web api should handle authorization.
    /// The Api will now only trust issues by our Authorization Server and if Authorization Server = Resource Server
    /// </summary>
    /// <param name="app"></param>
    private void ConfigureOAuthTokenConsumption(IAppBuilder app)
    {
      var issuer = ConfigurationManager.AppSettings["as:Issuer"];
      string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
      byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

      // Api controllers with an [Authorize] attribute will be validated with JWT
      app.UseJwtBearerAuthentication(
          new JwtBearerAuthenticationOptions
          {
            AuthenticationMode = AuthenticationMode.Active,
            AllowedAudiences = new[] { audienceId },
            IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
          });
    }

    #endregion
  }
}
