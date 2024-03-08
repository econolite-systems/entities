// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Persistence.Common.Entities;

namespace Econolite.Ode.Models.Entities;

public class EntityBase : GuidIndexedEntityBase, IEntity
{
    public string Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public int Version { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
}