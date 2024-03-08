// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities;

public class EntityNodeProjection : Entity
{
    public string InstanceId { get; set; } = string.Empty;
    public Guid Parent { get; set; }
    public IEnumerable<EntityNodeProjection> Children { get; set; } = Array.Empty<EntityNodeProjection>();
}