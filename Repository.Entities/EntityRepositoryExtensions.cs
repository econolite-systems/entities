// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.Entities;

public static class EntityRepositoryExtensions
{
    public static IServiceCollection AddEntityRepo(this IServiceCollection services)
    {
        services.AddScoped<IEntityRepository, EntityRepository>();
        services.AddScoped<IEntityTypeRepository, EntityTypeRepository>();
        return services;
    }
}