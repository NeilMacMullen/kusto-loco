// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Server.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BabyKusto.Server
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddBabyKustoServer(this IServiceCollection services)
        {
            services.AddSingleton<IBabyKustoServerState, BabyKustoServerState>();
            services.AddSingleton<ManagementEndpointHelper>();
            services.AddSingleton<QueryEndpointHelper>();
            services.AddSingleton<QueryV2EndpointHelper>();

            return services;
        }
    }
}