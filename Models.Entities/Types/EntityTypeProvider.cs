// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;

namespace Econolite.Ode.Models.Entities.Types;

public abstract class EntityTypeProvider : IEntityTypeProvider
{
    public EntityTypeId TypeId => new() { Id = Type.Id, Name = Type.Name };
    public EntityType Type { get; protected init; } = new();
    
    public virtual EntityNode ModifyEntityNode(EntityNode node, Guid? intersection)
    {
        return node;
    }
}

