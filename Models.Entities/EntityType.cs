// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Persistence.Common.Interfaces;

namespace Econolite.Ode.Models.Entities;

public class EntityType : EntityTypeBase, IIndexedEntity<Guid>
{
    public Guid Id { get; set; }

    public static EntityType CreateNewInstance(EntityTypeAdd add)
    {
        
        return new EntityType() { Id = Guid.NewGuid(), Icon = add.Icon, Sections = add.Sections, Children = add.Children };
    }
}