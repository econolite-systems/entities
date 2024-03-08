// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Domain.Entities;

public class EntityUpdates : IEntityUpdates
{
    private readonly IEnumerable<IEntityConfigUpdate> _updates;

    public EntityUpdates(IServiceProvider serviceProvider)
    {
        _updates = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IEnumerable<IEntityConfigUpdate>>();
    }
    
    public async Task Add(IEntityService service, EntityNode entity)
    {
        foreach (var config in _updates.ToArray())
        {
            await config.Add(service, entity);
        }
    }
    
    public async Task Update(IEntityService service, EntityNode entity)
    {
        foreach (var config in _updates.ToArray())
        {
            await config.Update(service, entity);
        }
    }
    
    public async Task Delete(IEntityService service, EntityNode entity)
    {
        foreach (var config in _updates.ToArray())
        {
            await config.Delete(service, entity);
        }
    }
}