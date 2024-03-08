// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Persistence.Common.Interfaces;

namespace Econolite.Ode.Models.Entities.Interfaces;

public interface IEntity : IIndexedEntity<Guid>
{
    string Name { get; set; }
}