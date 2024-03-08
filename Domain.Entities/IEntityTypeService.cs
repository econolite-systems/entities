// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;

namespace Econolite.Ode.Domain.Entities;

public interface IEntityTypeService
{
    Task<IEnumerable<EntityTypeSection>> GetConfigSections();
    Task<IEnumerable<EntityType>> GetAllAsync();
    Task<EntityType?> GetByIdAsync(Guid id);
    Task<IEnumerable<EntityType>> GetParentTypesByTypeIdAsync(Guid typeId);
    Task<EntityType?> Add(EntityTypeAdd add);
    Task<EntityType?> Update(EntityType update);
    Task<bool> Delete(Guid id);
    EntityNode ModifyByType(EntityNode node, Guid? intersection);
}