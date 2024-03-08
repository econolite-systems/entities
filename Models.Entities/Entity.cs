// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities;

public class Entity : EntityBase
{
    public EntityTypeId Type { get; set; } = new EntityTypeId();

    public Jurisdiction? Jurisdiction { get; set; }

    public bool IsCopy { get; set; } = false;

    public bool IsLeaf { get; set; } = true;
}