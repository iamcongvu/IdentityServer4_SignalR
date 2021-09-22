using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4___SignalR.IdentityServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using IdentityServer4SignalR.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

namespace IdentityServer4___SignalR
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //1. Setup entity framework
            services.AddDbContext<ManageAppDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            //2. Setup idetntity
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ManageAppDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryApiResources(Config.Apis)
            .AddInMemoryClients(Configuration.GetSection("IdentityServer:Clients"))
            .AddInMemoryIdentityResources(Config.Ids)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddAspNetIdentity<User>()
            .AddDeveloperSigningCredential();

            services.AddTransient<IEmailSender, EmailSenderService>();

            services.AddControllersWithViews();
            services.AddRazorPages(options =>
            {
                options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model =>
                {
                    foreach (var selector in model.Selectors)
                    {
                        var attributeRouteModel = selector.AttributeRouteModel;
                        attributeRouteModel.Order = -1;
                        attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length);
                    }
                });
            });
            // add policy for [Authorize("...")] 1:00p b20
            services.AddAuthentication()
               .AddLocalApi("Bearer", option =>
               {
                   option.ExpectedScope = "api.ChatApp";
               });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Bearer", policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web Chat Space Api", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://localhost:5000/connect/authorize"),
                                Scopes = new Dictionary<string, string> { { "api.ChatApp", "Live Chat API" } }
                            },
                        },
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            new List<string>{ "api.ChatApp" }
                        }
                    });
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId("swagger"); // trỏ đúng clientId trong Config
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApp Space Api V1");
            });
        }
    }
}