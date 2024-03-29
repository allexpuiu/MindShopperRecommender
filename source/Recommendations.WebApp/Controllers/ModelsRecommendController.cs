﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Recommendations.Common;
using Recommendations.Common.Api;
using Recommendations.Core;
using Recommendations.Core.Recommend;
using Recommendations.WebApp.Models;
using Swashbuckle.Swagger.Annotations;

namespace Recommendations.WebApp.Controllers
{
    /// <summary>
    /// A controller for getting recommendations APIs
    /// </summary>
    [ApiKeyAuthorizationFilter(AuthorizationAppSettingsKeys.AdminPrimaryKey, AuthorizationAppSettingsKeys.AdminSecondaryKey,
         AuthorizationAppSettingsKeys.RecommendPrimaryKey, AuthorizationAppSettingsKeys.RecommendSecondaryKey)]
    public class ModelsRecommendController : ApiController
    {
        /// <summary>
        /// Get recommendations using the default model
        /// </summary>
        /// <param name="cancellationToken">The cancellation token assigned for the operation.</param>
        /// <param name="itemId">Item id to get recommendations for</param>
        /// <param name="recommendationCount">The number of requested recommendations</param>
        [Route("api/models/default/recommend", Name = "GetItemRecommendationsFromDefaultModel")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Item>)), SwaggerResponseRemoveDefaults]
        public Task<IHttpActionResult> GetItemRecommendationsFromDefaultModel(CancellationToken cancellationToken,
            string itemId, int recommendationCount = DefaultRecommendationCount)
        {
            // get recommendations for a single item
            UsageEvent[] usageEvents = {new UsageEvent {ItemId = itemId}};
            return GetRecommendationsAsync(null, usageEvents, null, recommendationCount, cancellationToken);
        }


        /// <summary>
        /// Get recommendations using the default model
        /// </summary>
        /// <param name="cancellationToken">The cancellation token assigned for the operation.</param>
        /// <param name="categoryCode">Item id to get recommendations for</param>
        /// <param name="recommendationCount">The number of requested recommendations</param>
        [Route("api/topsellers", Name = "GetTopSellers")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Item>)), SwaggerResponseRemoveDefaults]
        public Task<IHttpActionResult> GetTopSellers(CancellationToken cancellationToken,
            string categoryCode, int recommendationCount = DefaultRecommendationCount)
        {
            // get recommendations for a single item
            
            return GetTopSellersAsync(categoryCode, cancellationToken);
        }

        /// <summary>
        /// Get recommendations using the default model
        /// </summary>
        /// <param name="cancellationToken">The cancellation token assigned for the operation.</param>
        /// <param name="itemId">Item id to get recommendations for</param>
        /// <param name="recommendationCount">The number of requested recommendations</param>
        [Route("api/item", Name = "GetItemAsync")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Item>)), SwaggerResponseRemoveDefaults]
        public Task<IHttpActionResult> GetItemAsync(CancellationToken cancellationToken,
            string itemId, int recommendationCount = DefaultRecommendationCount)
        {
            // get recommendations for a single item

            return GetItemAsync(itemId, cancellationToken);
        }

        /// <summary>
        /// Get recommendations using the requested model
        /// </summary>
        /// <param name="cancellationToken">The cancellation token assigned for the operation.</param>
        /// <param name="modelId">The model id to use when scoring</param>
        /// <param name="itemId">Item id to get recommendations for</param>
        /// <param name="recommendationCount">The number of requested recommendations</param>
        [Route("api/models/{modelId}/recommend", Name = nameof(GetItemRecommendations))]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof (IEnumerable<Item>)), SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> GetItemRecommendations(CancellationToken cancellationToken,
            [FromUri] Guid? modelId, string itemId, int recommendationCount = DefaultRecommendationCount)
        {
            if (!modelId.HasValue)
            {
                var message = $"{nameof(modelId)} is not valid";
                Trace.TraceVerbose(message);
                return BadRequest(message);
            }

            // get recommendations for a single item
            UsageEvent[] usageEvents = {new UsageEvent {ItemId = itemId}};
            return await GetRecommendationsAsync(modelId, usageEvents, null, recommendationCount, cancellationToken);
        }

        /// <summary>
        /// Get recommendations using the default model
        /// </summary>
        /// <param name="cancellationToken">The cancellation token assigned for the operation.</param>
        /// <param name="usageEvents">The usage events to get recommendations for</param>
        /// <param name="userId">An optional id of the user to provide recommendations for. Any stored usage events associated with this user will be considered when getting recommendations</param>
        /// <param name="recommendationCount">The number of requested recommendations</param>
        [Route("api/models/default/recommend", Name = nameof(GetPersonalizedRecommendationsFromDefaultModel))]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Item>)), SwaggerResponseRemoveDefaults]
        public Task<IHttpActionResult> GetPersonalizedRecommendationsFromDefaultModel(CancellationToken cancellationToken,
            [FromBody]IList<UsageEvent> usageEvents, string userId = null, int recommendationCount = DefaultRecommendationCount)
        {
            // get recommendations for the usage events
            return GetRecommendationsAsync(null, usageEvents, userId, recommendationCount, cancellationToken);
        }

        /// <summary>
        /// Get recommendations using the requested model
        /// </summary>
        /// <param name="cancellationToken">The cancellation token assigned for the operation.</param>
        /// <param name="modelId">The model id to use when scoring</param>
        /// <param name="usageEvents">The usage events to get recommendations for</param>
        /// <param name="userId">An optional id of the user to provide recommendations for. Any stored usage events associated with this user will be considered when getting recommendations</param>
        /// <param name="recommendationCount">The number of requested recommendations</param>
        [Route("api/models/{modelId}/recommend", Name = nameof(GetPersonalizedRecommendations))]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Item>)), SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> GetPersonalizedRecommendations(
            CancellationToken cancellationToken,
            [FromUri] Guid? modelId, 
            [FromBody] IList<UsageEvent> usageEvents,
            string userId = null,
            int recommendationCount = DefaultRecommendationCount)
        {
            if (!modelId.HasValue)
            {
                var message = $"{nameof(modelId)} is not valid";
                Trace.TraceVerbose(message);
                return BadRequest(message);
            }

            // get recommendations for events
            return await GetRecommendationsAsync(modelId, usageEvents, userId, recommendationCount, cancellationToken);
        }

        private async Task<IHttpActionResult> GetRecommendationsAsync(Guid? modelId, IList<UsageEvent> usageEvents,
            string userId, int recommendationCount, CancellationToken cancellationToken)
        {
            // set the model id to context
            ContextManager.ModelId = modelId?.ToString() ?? "default";

            // validate recommendations count
            if (recommendationCount < MinRecommendationCount || recommendationCount > MaxRecommendationCount)
            {
                string message =
                    $"{nameof(recommendationCount)} must be between {MinRecommendationCount} and {MaxRecommendationCount}";
                Trace.TraceInformation(message);
                return BadRequest(message);
            }

            if (!modelId.HasValue)
            {
                // get the default model id 
                Trace.TraceVerbose("Getting the default model id");
                modelId = await WebAppContext.ModelsRegistry.GetDefaultModelIdAsync(cancellationToken);
                if (!modelId.HasValue)
                {
                    Trace.TraceWarning("Default model is not defined");
                    return NotFound();
                }

                // update the the model id in the context
                ContextManager.ModelId = modelId.Value;
            }

            Trace.TraceVerbose($"Getting model '{modelId}' status from the registry");
            ModelStatus? modelStatus =
                await WebAppContext.ModelsRegistry.GetModelStatusAsync(modelId.Value, cancellationToken);
            if (!modelStatus.HasValue)
            {
                Trace.TraceWarning($"Model with id '{modelId}' does not exists");
                return NotFound();
            }

            // validate that the model training was completed successfully
            if (modelStatus.Value != ModelStatus.Completed)
            {
                var message = $"Model must be in the '{ModelStatus.Completed}' status before getting recommendation";
                Trace.TraceInformation(message);
                return BadRequest(message);
            }

            try
            {
                Trace.TraceVerbose($"Getting {recommendationCount} recommendations for model '{modelId}' using {usageEvents?.Count} usage event(s) and user id '{userId}'");
                IList<Recommendation> recommendations = await WebAppContext.ModelsProvider.ScoreAsync(
                    modelId.Value, usageEvents, userId, recommendationCount, cancellationToken);

                Trace.TraceVerbose($"Got {recommendations.Count} recommendations for model '{modelId}'");
                
                // convert the result and return
                return Ok(recommendations.Select(r => {
                        Item item = RetrieveItemByItemId(r.RecommendedItemId);
                        item.RecommendationScore = r.Score;
                        return item;
                    }));
            }
            catch (ModelNotFoundException exception)
            {
                Trace.TraceWarning($"{nameof(ModelNotFoundException)} while getting recommendations: {exception}");
                return NotFound();
            }
        }

        private async Task<IHttpActionResult> GetTopSellersAsync(string categoryCode, CancellationToken cancellationToken)
        {
            try
            {
                // convert the result and return
                return Ok(RetrieveTopSellersByCategoryCode(categoryCode));
            }
            catch (ModelNotFoundException exception)
            {
                Trace.TraceWarning($"{nameof(ModelNotFoundException)} while getting recommendations: {exception}");
                return NotFound();
            }
        }

        private async Task<IHttpActionResult> GetItemAsync(string itemId, CancellationToken cancellationToken)
        {
            try
            {
                // convert the result and return
                return Ok(RetrieveItemByItemId(itemId));
            }
            catch (ModelNotFoundException exception)
            {
                Trace.TraceWarning($"{nameof(ModelNotFoundException)} while getting recommendations: {exception}");
                return NotFound();
            }
        }

        private Item RetrieveItemByItemId(string itemId)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "mindshopper.database.windows.net";
            builder.UserID = "mindshopper";
            builder.Password = "8799LipYAA9oksRLG6ia";
            builder.InitialCatalog = "recommender";
            Item item = null; ;


            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT ItemId, ItemName, CategoryCode, Category, SalesValue, ItemRank ");
                    sb.Append("FROM [dim].[Item]");
                    sb.Append($"WHERE ItemId = {itemId}");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                item = new Item(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDecimal(4), reader.GetInt32(5));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return item;
        }


        private IList<Item> RetrieveTopSellersByCategoryCode(string categoryCode)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "mindshopper.database.windows.net";
            builder.UserID = "mindshopper";
            builder.Password = "8799LipYAA9oksRLG6ia";
            builder.InitialCatalog = "recommender";
            IList<Item> items = new List<Item>();


            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT TOP 10  ItemId, ItemName, CategoryCode, Category, SalesValue, ItemRank ");
                    sb.Append("FROM [dim].[Item]");
                    sb.Append($"WHERE CategoryCode = {categoryCode}");
                    sb.Append("ORDER BY ItemRank ASC");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new Item(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDecimal(4), reader.GetInt32(5)));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return items;
        }

        private const int MinRecommendationCount = 1;
        private const int DefaultRecommendationCount = 10;
        private const int MaxRecommendationCount = 100;
        private static readonly ITracer Trace = new Tracer(nameof(ModelsRecommendController));
    }
}
