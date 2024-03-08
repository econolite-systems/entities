// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Domain.Entities.Extensions;

public static class Defined
{
    public static IServiceCollection AddSystemEntityTypes(this IServiceCollection services)
    {
        services.AddScoped<ISystemTypeChildren, CorridorEntityTypeProvider>();
        services.AddScoped<ISystemTypeChildren, EssEntityTypeProvider>();
        services.AddScoped<ISystemTypeChildren, IntersectionEntityTypeProvider>();
        services.AddScoped<ISystemTypeChildren, SignalEntityTypeProvider>();
        services.AddScoped<ISystemTypeChildren, SpeedSegmentEntityTypeProvider>();

        services.AddScoped<ICorridorTypeChildren, EssEntityTypeProvider>();
        services.AddScoped<ICorridorTypeChildren, IntersectionEntityTypeProvider>();
        services.AddScoped<ICorridorTypeChildren, SignalEntityTypeProvider>();
        services.AddScoped<ICorridorTypeChildren, SpeedSegmentEntityTypeProvider>();

        services.AddScoped<IIntersectionTypeChildren, ApproachEntityTypeProvider>();
        services.AddScoped<IIntersectionTypeChildren, EssEntityTypeProvider>();
        services.AddScoped<IIntersectionTypeChildren, SignalEntityTypeProvider>();
        services.AddScoped<IIntersectionTypeChildren, RsuEntityTypeProvider>();

        services.AddScoped<IApproachTypeChildren, StreetSegmentEntityTypeProvider>();
        
        services.AddScoped<IStreetSegmentTypeChildren, DetectorEntityTypeProvider>();

        services.AddScoped<IEntityTypeProvider, DetectorEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, EssEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, StreetSegmentEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, ApproachEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, RsuEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, SignalEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, IntersectionEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, CorridorEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, SystemEntityTypeProvider>();
        services.AddScoped<IEntityTypeProvider, SpeedSegmentEntityTypeProvider>();

        return services;
    }
    
    public static IServiceCollection AddEntityService(this IServiceCollection services)
    {
        services.AddScoped<IEntityConfigUpdate, EntityConfigUpdate>();
        services.AddSingleton<IEntityUpdates, EntityUpdates>();
        services.AddEntityTypeService();
        services.AddSingleton<IEntityService, EntityService>();

        return services;
    }
    
    public static IServiceCollection AddEntityTypeService(this IServiceCollection services)
    {
        services.AddSystemEntityTypes();
        services.AddSingleton<IEntityTypeService, EntityTypeService>();

        return services;
    }
}