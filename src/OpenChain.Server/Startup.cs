﻿using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Cors;
using Microsoft.AspNet.Cors.Core;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.WebSockets.Server;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using OpenChain.Ledger;
using OpenChain.Server.Models;

namespace OpenChain.Server
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IHostingEnvironment env)
        {
            // Setup Configuration
            configuration = new ConfigurationBuilder()
                .AddJsonFile($"{env.WebRootPath}/config.json")
                .Build();
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(_ => this.configuration);

            // Setup ASP.NET MVC
            services
                .AddMvcCore()
                .AddCors()
                .AddJsonFormatters();

            // Logger
            services.AddTransient<ILogger>(ConfigurationParser.CreateLogger);

            // CORS Headers
            services.AddCors();
            CorsPolicy policy = new CorsPolicyBuilder().AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().Build();
            services.ConfigureCors(options => options.AddPolicy("Any", policy));

            // Ledger Store
            services.AddTransient<ITransactionStore>(ConfigurationParser.CreateLedgerStore);

            services.AddTransient<ILedgerQueries>(ConfigurationParser.CreateLedgerQueries);

            services.AddSingleton<IMutationValidator>(ConfigurationParser.CreateRulesValidator);

            services.AddSingleton<MasterProperties>(ConfigurationParser.CreateMasterProperties);

            services.AddTransient<TransactionValidator>(ConfigurationParser.CreateTransactionValidator);

            // Transaction Stream Subscriber
            services.AddSingleton<IStreamSubscriber>(ConfigurationParser.CreateStreamSubscriber);
        }

        // Configure is called after ConfigureServices is called.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory, IConfiguration configuration, ITransactionStore store)
        {
            loggerfactory.AddConsole();

            app.Map("/stream", managedWebSocketsApp =>
            {
                if (bool.Parse(configuration["enable_transaction_stream"]))
                {
                    managedWebSocketsApp.UseWebSockets(new WebSocketOptions() { ReplaceFeature = true });
                    managedWebSocketsApp.Use(next => new TransactionStreamMiddleware(next).Invoke);
                }
            });

            // Add MVC to the request pipeline.
            app.UseMvc();

            // Activate singletons
            app.ApplicationServices.GetService<IStreamSubscriber>();
            app.ApplicationServices.GetService<IMutationValidator>();

            await ConfigurationParser.InitializeLedgerStore(app.ApplicationServices);
        }
    }
}