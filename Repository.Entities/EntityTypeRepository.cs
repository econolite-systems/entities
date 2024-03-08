// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Econolite.Ode.Repository.Entities;

public class EntityTypeRepository: GuidDocumentRepositoryBase<EntityType>, IEntityTypeRepository
{
    public EntityTypeRepository(IMongoContext context, ILogger<EntityTypeRepository> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<EntityType>> GetParentTypesByTypeIdAsync(Guid typeId)
    {
        var filter = Builders<EntityType>.Filter.Where(x => x.Children.Contains(typeId));
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await results.ToListAsync();
    }
}