﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Armadillo.Shared;
using Armadillo.Siebel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;

namespace Armadillo.Agent
{
    public class Application
    {
        ILoggerFactory loggerFactory_;
        ILogger logger_; 
        
        public Application(ILoggerFactory loggerFactory)
        {
            loggerFactory_ = loggerFactory;
            logger_ = loggerFactory_.CreateLogger("Application");
        }

        public async Task Run()
        {
            try
            {
                logger_.LogInformation($"This is a console application");

                var builder = new ConfigurationBuilder().AddUserSecrets("28e6f711-a4c4-4cef-9e37-50ebfee35f91");
                var configuration = builder.Build();
                var endpointUri = configuration["CosmosDB:EndpointUri"];
                var primaryKey = configuration["CosmosDB:PrimaryKey"];

                logger_.LogInformation("Database endpoint {0}, key {1}", endpointUri, primaryKey);
                var documentClient = new DocumentClient(new Uri(endpointUri), primaryKey);

                var dataProvider  = new RandomDataProvider();
                var logger = loggerFactory_.CreateLogger("Uploader");
                var uploader = new Uploader(dataProvider, documentClient, logger);
                await uploader.UpdateAsync();

                var cosmosDataProvider = new CosmosDataProvider(documentClient);
                var subcases = await cosmosDataProvider.GetSubcasesAsync("ignored");
                foreach(var subcase in subcases)
                {
                    logger_.LogInformation($"Subcase {subcase.Id} {subcase.Title}");
                }
            }
            catch (Exception ex)
            {
                logger_.LogError(ex.ToString());
            }
        }
    }
}
