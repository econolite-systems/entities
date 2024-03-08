// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Repository.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Domain.Entities;

public class EntityTypeService : IEntityTypeService
{
    private readonly IEnumerable<IEntityTypeProvider> _entityTypeProviders;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly ILogger<EntityTypeService> _logger;

    public EntityTypeService(IServiceProvider serviceProvider, ILogger<EntityTypeService> logger)
    {
        _entityTypeProviders = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IEnumerable<IEntityTypeProvider>>();
        _entityTypeRepository = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IEntityTypeRepository>();
        _logger = logger;
        CheckSystemTypes().ConfigureAwait(true);
    }

    private async Task CheckSystemTypes()
    {
        //var add = false;
        foreach (var entityType in _entityTypeProviders.ToArray())
        {
            var result = await GetByIdAsync(entityType.TypeId.Id);
            if (result == null)
            {
                _entityTypeRepository.Add(entityType.Type);
                //add = true;
            }
            else
            {
                _entityTypeRepository.Update(entityType.Type);
            }
        }
        
        await _entityTypeRepository.DbContext.SaveChangesAsync();
    }

    public Task<IEnumerable<EntityTypeSection>> GetConfigSections()
    {
        return Task.FromResult(EntityTypeSections.AllEntityTypeSections);
    }

    public async Task<IEnumerable<EntityType>> GetAllAsync()
    {
        return await Task.FromResult(_entityTypeProviders.Select(p => p.Type).ToArray());
    }

    public async Task<EntityType?> GetByIdAsync(Guid id)
    {
        var entityTypeProvider = _entityTypeProviders.FirstOrDefault(p => p.Type.Id == id);
        return await Task.FromResult(entityTypeProvider?.Type);
    }

    public async Task<IEnumerable<EntityType>> GetParentTypesByTypeIdAsync(Guid typeId)
    {
        var parents = _entityTypeProviders.Where(p => p.Type.Children.Any(c => c == typeId)).Select(p => p.Type).ToArray();
        return await Task.FromResult(parents);
    }

    public async Task<EntityType?> Add(EntityTypeAdd add)
    {
        var entityType = EntityType.CreateNewInstance(add);
        _entityTypeRepository.Add(entityType);
        var (success, error) = await _entityTypeRepository.DbContext.SaveChangesAsync();
        if (!success)
        {
            if (error != null) _logger.LogError(error);
            return null;
        }

        return entityType;
    }

    public async Task<EntityType?> Update(EntityType update)
    {
        _entityTypeRepository.Update(update);
        var (success, error) = await _entityTypeRepository.DbContext.SaveChangesAsync();
        if (!success)
        {
            if (error != null) _logger.LogError(error);
            return null;
        }

        return update;
    }

    public async Task<bool> Delete(Guid id)
    {
        _entityTypeRepository.Remove(id);
        var (success, error) = await _entityTypeRepository.DbContext.SaveChangesAsync();

        if (error != null) _logger.LogError(error);

        return success;
    }
    
    public EntityNode ModifyByType(EntityNode node, Guid? intersection)
    {
        var provider = _entityTypeProviders.FirstOrDefault(x => x.TypeId.Id == node.Type.Id);
        if (provider != null)
        {
            return provider.ModifyEntityNode(node, intersection);
        }

        return node;
    }
}