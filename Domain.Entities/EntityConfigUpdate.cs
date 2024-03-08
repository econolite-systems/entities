// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Domain.Entities;

public class EntityConfigUpdate : IEntityConfigUpdate
{
    private readonly ILogger<EntityConfigUpdate> _logger;

    public EntityConfigUpdate(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<EntityConfigUpdate>();
    }
    
    public Task Add(IEntityService service, EntityNode entity)
    {
        _logger.LogInformation("EntityConfigUpdate.Add {@Id} {@Name}", entity.Id.ToString(), entity.Name);
        return Task.CompletedTask;
    }

    public Task Update(IEntityService service, EntityNode entity)
    {
        _logger.LogInformation("EntityConfigUpdate.Update {@Id} {@Name}", entity.Id.ToString(), entity.Name);
        return Task.CompletedTask;
    }

    public Task Delete(IEntityService service, EntityNode entity)
    {
        _logger.LogInformation("EntityConfigUpdate.Delete {@Id} {@Name}", entity.Id.ToString(), entity.Name);
        return Task.CompletedTask;
    }
}