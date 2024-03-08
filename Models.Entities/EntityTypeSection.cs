// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities;

public class EntityTypeSection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public bool Enabled { get; set; } = false;
    public IEnumerable<EntityTypeSection> Sections { get; set; } = Enumerable.Empty<EntityTypeSection>();
}