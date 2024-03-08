// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Models.Entities;

namespace Econolite.Ode.Domain.Entities;

public interface IEntityUpdates
{
    Task Add(IEntityService service, EntityNode entity);
    Task Update(IEntityService service, EntityNode entity);
    Task Delete(IEntityService service, EntityNode entity);
}