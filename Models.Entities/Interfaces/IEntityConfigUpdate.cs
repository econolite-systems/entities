// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Configuration;

namespace Econolite.Ode.Models.Entities.Interfaces;

public interface IEntityConfigUpdate
{
    Task Add(IEntityService service, EntityNode entity);
    Task Update(IEntityService service, EntityNode entity);
    Task Delete(IEntityService service, EntityNode entity);
}