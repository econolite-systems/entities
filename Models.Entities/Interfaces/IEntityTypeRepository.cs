// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Common.Repository;

namespace Econolite.Ode.Repository.Entities;

public interface IEntityTypeRepository : IRepository<EntityType, Guid>
{
    Task<IEnumerable<EntityType>> GetParentTypesByTypeIdAsync(Guid typeId);
}