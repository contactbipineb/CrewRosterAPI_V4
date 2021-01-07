// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using Airlines.XAirlines.Dialogs;
using Airlines.XAirlines.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Web;
using XAirlinesUpdate.Bots;

namespace XAirlinesUpdate
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
            services.AddControllersWithViews()
                .AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<ConversationState>();
            services.AddSingleton<RootDialog>();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<RootDialog>>();
            InitializeApplicationSettings(Configuration);
        }

        public static void InitializeApplicationSettings(IConfiguration configuration)
        {
            ApplicationSettings.AppName = "Airlines";
            ApplicationSettings.BaseUrl = configuration["BaseUri"];
            ApplicationSettings.AppId = configuration["MicrosoftAppId"];
            ApplicationSettings.AppSecret = configuration["MicrosoftAppPassword"];
            Airlines.XAirlines.Common.Constants.PortalTabDeeplink = $"https://teams.microsoft.com/l/entity/{ApplicationSettings.AppId}/com.contoso.Airlines.portal?webUrl={HttpUtility.UrlEncode(ApplicationSettings.BaseUrl + "/portal")}&label=Portal";
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
