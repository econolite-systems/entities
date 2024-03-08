// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities.Interfaces;

public interface IEntityTypeProvider
{
    public EntityTypeId TypeId { get; }
    public EntityType Type { get; }
    public EntityNode ModifyEntityNode(EntityNode node, Guid? intersection); 
}