using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer4___SignalR.IdentityServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
          new IdentityResource[]
          {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
          };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("api.ChatApp", "Live Chat API")
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("api.ChatApp", "Live Chat API")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "ChatApp",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,
                    AllowOfflineAccess = true,

                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                    // ở client này cho phép truy cập đến những cái này
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "api.ChatApp"
                    }
                 },
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "Swagger Client",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,

                    RedirectUris =           { "https://localhost:5009/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { "https://localhost:5009/swagger/oauth2-redirect.html" },
                    AllowedCorsOrigins =     { "https://localhost:5009" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api.ChatApp"
                    }
                },
            };
    }
}