// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Econolite.Ode.Repository.Entities;

public class EntityRepository : GuidDocumentRepositoryBase<EntityNode>, IEntityRepository
{
    public EntityRepository(IMongoContext context, ILogger<EntityRepository> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<string>> GetTypesAsync()
    {
        var results = await ExecuteDbSetFuncAsync(collection =>
            collection.DistinctAsync<string>("type", Builders<EntityNode>.Filter.Empty));
        return results?.ToList() ?? new List<string>();
    }

    public async Task<IEnumerable<EntityNode>> GetSearchNodesAsync(string search)
    {
        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Text(search),
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted));

        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results?.ToList() ?? new List<EntityNode>();
    }

    public async Task<EntityNode?> GetIntersectionParentNodeAsync(EntityNode node)
    {
        if (node.Type.Name is "Intersection")
            return node;
        var parent = node.Parent;
        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.In("_id", node.Parents),
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted));
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        var nodesList = results.ToList();
        if (!nodesList.Any())
            return null;

        var intersection = nodesList.FirstOrDefault(n => n.Type.Name is "Intersection");
        if (intersection == null)
        {
            foreach (var entityNode in nodesList)
            {
                intersection = await GetIntersectionParentNodeAsync(entityNode);
                if (intersection != null)
                    return intersection;
            }
        }

        return intersection;
    }

    public async Task<IEnumerable<EntityNode>> GetNodesByTypeAsync(string type)
    {
        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Where(x => x.Type.Name.ToLower() == type.ToLower()),
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted));
        var json = RenderToJson(filter);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results?.ToList() ?? new List<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> GetNodesWithChildrenByIdAsync(string type)
    {
        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Where(x => x.Type.Name.ToLower() == type.ToLower()),
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted));
        var json = RenderToJson(filter);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results?.ToList() ?? new List<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> GetAllDeletedNodesAsync()
    {
        var results = await ExecuteDbSetFuncAsync(collection => collection
            .FindAsync(Builders<EntityNode>.Filter.Where(x => x.IsDeleted)));
        return results?.ToList() ?? new List<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> GetAllNodesAsync()
    {
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(i => !i.IsDeleted));
        return results?.ToList() ?? new List<EntityNode>();
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetRootNodesAsync()
    {
        var filter = Builders<EntityNode>.Filter.Eq("type.name", "System");
        var json = RenderToJson(filter);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(i => i.Type.Name == "System"));
        return results?.ToList().ToProjection() ?? new List<EntityNodeProjection>();
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetExpandedNodesAsync(IEnumerable<string> instanceIds)
    {
        var nodes = instanceIds.ToParentNodeGuids().Select(pn => pn.node).ToArray();
        if (nodes.Any(p => p == Guid.Empty))
            return Array.Empty<EntityNodeProjection>();

        var filter = Builders<EntityNode>.Filter.In("_id", nodes);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        if (results == null)
            return Array.Empty<EntityNodeProjection>();
        return results.ToList().ToProjection(instanceIds.ToParentNodeGuids());
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetNodesByTypesAsync(IEnumerable<string> types)
    {
        var filter = Builders<EntityNode>.Filter.In("type.name", types);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results.ToList().ToProjection();
    }

    public async Task<IEnumerable<EntityNode>> GetNodesByExternalIdAsync(IEnumerable<string> ids)
    {
        var filter = Builders<EntityNode>.Filter.In("externalId", ids);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results.ToList();
    }

    public async Task<IEnumerable<EntityNode>> GetNodesByIntersectionIdAsync(Guid id)
    {
        var intersection = Builders<EntityNode>.Filter.Or(
            Builders<EntityNode>.Filter.Eq("geometry.point.properties.intersection", id),
                Builders<EntityNode>.Filter.Eq("geometry.lineString.properties.intersection", id),
                Builders<EntityNode>.Filter.Eq("geometry.polygon.properties.intersection", id)
            );
        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted),
            intersection);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results.ToList();
    }
    
    public async Task<IEnumerable<EntityNode>> GetNodesByIntersectionIdMapAsync(int id)
    {
        var mappingId = Builders<EntityNode>.Filter.Eq("idMapping", id);
        var typeFilter = Builders<EntityNode>.Filter.Or(
            Builders<EntityNode>.Filter.Eq("type.name", "Intersection"),
            Builders<EntityNode>.Filter.Eq("type.name", "Signal")
        );
        var filterIntersection = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted),
            mappingId);
        var mappingIdResults = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filterIntersection));
        var node = mappingIdResults.FirstOrDefault();
        if (node == null)
            return Array.Empty<EntityNode>();
        var guidId = node.Id;
        
        if (node.Type.Name == "Signal")
        {
            if (node.Geometry.Point?.Properties?.Intersection != null)
            {
                guidId = node.Geometry.Point.Properties.Intersection.Value;
            }
            else
            {
                return Array.Empty<EntityNode>();
            }
        }
        
        var intersection = Builders<EntityNode>.Filter.Or(
            Builders<EntityNode>.Filter.Eq("geometry.point.properties.intersection", guidId),
            Builders<EntityNode>.Filter.Eq("geometry.lineString.properties.intersection", guidId),
            Builders<EntityNode>.Filter.Eq("geometry.polygon.properties.intersection", guidId)
        );
        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Where(x => !x.IsDeleted),
            intersection);
        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return results.ToList();
    }
    
    public async Task<IEnumerable<EntityNode>> QueryIntersectionsWithinRadiusDistanceInMilesAsync(GeoJsonPointFeature point, int miles)
    {
        if (point.Coordinates == null)
        {
            return Enumerable.Empty<EntityNode>();
        }

        var coordinates = point.Coordinates.ToMongoCoordinates();
        var center = GeoJson.Point(coordinates);

        var filter = Builders<EntityNode>.Filter.And(
            Builders<EntityNode>.Filter.Where(x => x.IsDeleted == false),
            Builders<EntityNode>.Filter.NearSphere("geometry.point", center, miles * SpatialConsts.METERS_PER_MILE));

        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityNode>(filter));
        return results?.ToList() ?? Enumerable.Empty<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesAsync(GeoJsonPointFeature point)
    {
        if (point.Coordinates == null)
        {
            return Enumerable.Empty<EntityNode>();
        }

        var coordinates = point.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.Point(coordinates);
        var filter = MongoDB.Driver.Builders<EntityNode>.Filter.GeoIntersects("geoFence", intersect);

        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityNode>(filter));
        return results?.ToList() ?? Enumerable.Empty<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesByTypeAsync(string type, GeoJsonPointFeature point)
    {
        if (point.Coordinates == null)
        {
            return Enumerable.Empty<EntityNode>();
        }

        var coordinates = point.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.Point(coordinates);
        var filter = MongoDB.Driver.Builders<EntityNode>.Filter.And(
            MongoDB.Driver.Builders<EntityNode>.Filter.Where(x => x.Type.Name == type),
            MongoDB.Driver.Builders<EntityNode>.Filter.GeoIntersects("geoFence", intersect));

        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityNode>(filter));
        return results?.ToList() ?? Enumerable.Empty<EntityNode>();
    }
    public IEnumerable<EntityNode> QueryIntersectingByType(string type, GeoJsonLineStringFeature route)
    {
        if (route.Coordinates == null)
        {
            return Enumerable.Empty<EntityNode>();
        }

        var coordinates = route.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.LineString(coordinates);
        var filter = MongoDB.Driver.Builders<EntityNode>.Filter.And(
            MongoDB.Driver.Builders<EntityNode>.Filter.Where(x => x.Type.Name == type),
            MongoDB.Driver.Builders<EntityNode>.Filter.GeoIntersects("geoFence", intersect));

        var results = ExecuteDbSetFunc(collection => collection.Find<EntityNode>(filter));
        return results?.ToList() ?? Enumerable.Empty<EntityNode>(); ;
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingByTypeAsync(string type, GeoJsonLineStringFeature route)
    {
        if (route.Coordinates == null)
        {
            return Enumerable.Empty<EntityNode>();
        }

        var coordinates = route.Coordinates.ToMongoCoordinates();
        var intersect = GeoJson.LineString(coordinates);
        var filter = MongoDB.Driver.Builders<EntityNode>.Filter.And(
            MongoDB.Driver.Builders<EntityNode>.Filter.Where(x => x.Type.Name == type),
            MongoDB.Driver.Builders<EntityNode>.Filter.GeoIntersects("geoFence", intersect));

        var results = await ExecuteDbSetFuncAsync(collection => collection.FindAsync<EntityNode>(filter));
        return results?.ToList() ?? Enumerable.Empty<EntityNode>(); ;
    }

    public async Task<EntityNodeProjection?> GetNodeAsync(string instanceId)
    {
        var node = instanceId.ToParentNodeGuids();
        if (node.node == Guid.Empty)
            return null;

        var result = await GetByIdAsync(node.node);
        return result?.ToProjection(node.parent);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> CopyAsync(EntityNodeProjection entity, Guid parent)
    {
        var id = entity.InstanceId.ToParentNodeGuids();
        var node = await GetByIdAsync(id.node);
        node!.Parents.Append(parent);
        var toParent = node.ToEntity();
        toParent.IsCopy = true;
        AddToParent(parent, toParent);
        AddParentToChild(node, parent);

        var (success, error) = await DbContext.SaveChangesAsync();

        if (!success)
        {
            return (null, error);
        }

        return (node.ToProjection(parent), null);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> MoveAsync(EntityNodeProjection entity, Guid newParent)
    {
        var id = entity.InstanceId.ToParentNodeGuids();
        var node = await GetByIdAsync(id.node);

        if (node == null)
        {
            return (null, $"Entity {entity.Name} couldn't be moved because the entity doesn't exist");
        }

        AddToParent(newParent, node);
        RemoveFromParent(node, node.Parent);
        MoveChildToParent(node, newParent);

        var (success, error) = await DbContext.SaveChangesAsync();

        if (!success)
        {
            return (null, error);
        }

        return (node.ToProjection(newParent), null);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> MoveUpAsync(string instanceId)
    {
        MoveChild(instanceId, true);

        var (success, error) = await DbContext.SaveChangesAsync();

        if (!success)
        {
            return (null, error);
        }

        var node = await GetNode(instanceId);
        return (node, null);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> MoveDownAsync(string instanceId)
    {
        MoveChild(instanceId);

        var (success, error) = await DbContext.SaveChangesAsync();

        if (!success)
        {
            return (null, error);
        }

        var node = await GetNode(instanceId);
        return (node, null);
    }

    public void Create(Guid parent, EntityNode node)
    {
        node.Parent = parent;
        node.Version = 1;

        if (!node.Parents.Contains(parent)) node.Parents = node.Parents.Append(parent);

        Add(node);

        var entity = node.ToEntity();

        entity.IsCopy = false;
        if (parent == Guid.Empty) return;

        AddToParent(parent, entity);
    }

    public void Edit(EntityNode node)
    {
        Update(node);
        EditInParents(node);
    }


    public void Delete(Guid id)
    {
        RemoveFromAllParents(id);
        Remove(id);
    }

    public void SoftDelete(EntityNode node)
    {
        node.IsDeleted = true;
        Edit(node);
    }

    public void Restore(EntityNode node)
    {
        node.IsDeleted = false;
        Edit(node);
    }

    private void AddToParent(Guid parent, Entity entity)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var update = Builders<EntityNode>.Update.AddToSet(f => f.Children, entity);
                var filter = Builders<EntityNode>.Filter.Eq("_id", parent);
                var result = await collection.UpdateOneAsync(filter, update, default, cancellationToken);
                if (result is { ModifiedCount: < 1 })
                    throw new Exception($"Parent {parent} wasn't updated.");
            }
        );
    }

    private void AddParentToChild(EntityNode node, Guid parent)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var filter = Builders<EntityNode>.Filter.And(Builders<EntityNode>.Filter.Eq(x => x.Id, node.Id), Builders<EntityNode>.Filter.Where(x => !x.Parents.Contains(parent)));
                var update = Builders<EntityNode>.Update.Push(n => n.Parents, parent);
                var result = await collection.UpdateOneAsync(filter, update, default, cancellationToken);
                if (result is { ModifiedCount: < 1 })
                    throw new Exception($"Parent {parent} wasn't added to child {node.Name}.");
            }
        );
    }

    public void MoveChildToParent(EntityNode node, Guid parent)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var filter = Builders<EntityNode>.Filter.Eq(x => x.Id, node.Id);
                UpdateDefinition<EntityNode> update;
                if (!node.Parents.Contains(parent))
                {
                    update = Builders<EntityNode>.Update.Push(n => n.Parents, parent).Set(n => n.Parent, parent);
                }
                else
                {
                    update = Builders<EntityNode>.Update.Set(n => n.Parent, parent);
                }

                var result = await collection.UpdateOneAsync(filter, update, default, cancellationToken);
                if (result is { ModifiedCount: < 1 })
                    throw new Exception($"child {node.Name} wasn't moved to parent {parent}.");
            }
        );
    }

    private void MoveChild(string instanceId, bool moveUp = false)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var parentNodePairs = instanceId.ToParentNodeGuids();
                var entity = await collection.Find(e => e.Id == parentNodePairs.parent)
                    .SingleOrDefaultAsync(cancellationToken);

                var index = entity.Children.ToList().FindIndex(a => a.Id == parentNodePairs.node);
                var indexToMove = moveUp ? index - 1 : index + 1;
                var children = entity.Children.ToList();
                if (moveUp && index == 0 || !moveUp && index == entity.Children.Count() - 1) return;

                if (moveUp)
                {
                    for (var i = (index - 1); i > 0; i--)
                    {
                        if (children[i].IsDeleted) continue;
                        indexToMove = i;
                        break;
                    }
                }

                if (!moveUp)
                {
                    for (var i = (index + 1); i < children.Count - 1; i++)
                    {
                        if (children[i].IsDeleted) continue;
                        indexToMove = i;
                        break;
                    }
                }

                var childToMove = entity.Children.FirstOrDefault(a => a.Id == parentNodePairs.node);

                var position = indexToMove;
                if (position >= 0 && position < entity.Children.Count())
                {

                    var filter = Builders<EntityNode>.Filter.Eq(x => x.Id, parentNodePairs.parent);
                    var pullUpdate = Builders<EntityNode>.Update.PullFilter(n => n.Children,
                        Builders<Entity>.Filter.Eq(e => e.Id, parentNodePairs.node));
                    await collection.UpdateOneAsync(filter, pullUpdate, default, cancellationToken);
                    var pushUpdate =
                        Builders<EntityNode>.Update.PushEach(n => n.Children, new[] { childToMove }, position: position);
                    var options = new FindOneAndUpdateOptions<EntityNode>
                    {
                        ReturnDocument = ReturnDocument.After
                    };

                    await collection.FindOneAndUpdateAsync(filter, pushUpdate, options, cancellationToken);
                }
                else
                {
                    throw new Exception($"Can't move {childToMove?.Name} outside bound of the children.");
                }
            }
        );
    }

    private void MoveChildToIndex(string instanceId, int newIndex)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var parentNodePairs = instanceId.ToParentNodeGuids();
                var entity = await collection.Find(e => e.Id == parentNodePairs.parent)
                    .SingleOrDefaultAsync(cancellationToken);

                var index = entity.Children.ToList().FindIndex(a => a.Id == parentNodePairs.node);
                var childToMove = entity.Children.FirstOrDefault(a => a.Id == parentNodePairs.node);

                var position = newIndex >= entity.Children.Count() ? entity.Children.Count() - 1 : newIndex;
                if (position >= 0 && position < entity.Children.Count())
                {

                    var filter = Builders<EntityNode>.Filter.Eq(x => x.Id, parentNodePairs.parent);
                    var pullUpdate = Builders<EntityNode>.Update.PullFilter(n => n.Children,
                        Builders<Entity>.Filter.Eq(e => e.Id, parentNodePairs.node));
                    await collection.UpdateOneAsync(filter, pullUpdate, default, cancellationToken);
                    var pushUpdate =
                        Builders<EntityNode>.Update.PushEach(n => n.Children, new[] { childToMove }, position: position);
                    var options = new FindOneAndUpdateOptions<EntityNode>
                    {
                        ReturnDocument = ReturnDocument.After
                    };

                    await collection.FindOneAndUpdateAsync(filter, pushUpdate, options, cancellationToken);
                }
                else
                {
                    throw new Exception($"Can't move {childToMove?.Name} outside bound of the children.");
                }
            }
        );
    }

    public void SetChildrenOrder(Guid id, IEnumerable<Guid> childrenIds)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var entity = await collection.Find(e => e.Id == id)
                    .SingleOrDefaultAsync(cancellationToken);
                // Change the order of the children to match the order of the childrenIds
                entity.Children = entity.Children.OrderBy(c => childrenIds.ToList().IndexOf(c.Id)).ToList();
                var filter = Builders<EntityNode>.Filter.Eq(x => x.Id, id);
                await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions(), cancellationToken);
            }
        );
    }

    public void RemoveFromParent(EntityNode node, Guid parent)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var filter = Builders<EntityNode>.Filter.And(
                    Builders<EntityNode>.Filter.ElemMatch(x => x.Children, c => c.Id == node.Id),
                    Builders<EntityNode>.Filter.Eq(x => x.Id, parent));
                var update = Builders<EntityNode>.Update.PullFilter(n => n.Children,
                    Builders<Entity>.Filter.Eq(e => e.Id, node.Id));
                var result = await collection.UpdateOneAsync(filter, update, default, cancellationToken);
                if (result is { ModifiedCount: < 1 })
                    throw new Exception($"child {node.Name} wasn't removed from parent {parent}.");
            }
        );
    }

    private void RemoveFromAllParents(Guid id)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var filter = Builders<EntityNode>.Filter.And(
                    Builders<EntityNode>.Filter
                        .ElemMatch(x => x.Children, c => c.Id == id));
                var update = Builders<EntityNode>.Update
                    .PullFilter(n => n.Children,
                        Builders<Entity>.Filter
                            .Eq(e => e.Id, id));

                var editResult = await collection.UpdateOneAsync(filter, update, default, cancellationToken);

                if (editResult is { IsModifiedCountAvailable: true } &&
                    editResult.MatchedCount != editResult.ModifiedCount)
                    throw new Exception(
                        $"Didn't remove {id} from {editResult.MatchedCount - editResult.ModifiedCount} of {editResult.ModifiedCount} parents");
            }
        );
    }

    private void EditInParents(EntityNode node)
    {
        AddCommandFunc(collection =>
            async (cancellationToken) =>
            {
                var filter = Builders<EntityNode>.Filter.ElemMatch(x => x.Children, c => c.Id == node.Id);
                var update = Builders<EntityNode>.Update
                    .Set("children.$.name", node.Name)
                    .Set("children.$.description", node.Description)
                    .Set("children.$.isLeaf", node.IsLeaf)
                    .Set("children.$.jurisdiction", node.Jurisdiction)
                    .Set("children.$.isDeleted", node.IsDeleted)
                    .Set("children.$.version", node.Version);

                //var updateJson = RenderToJson(update);
                //var filterJson = RenderToJson(filter);
                var editResult = await collection.UpdateManyAsync(filter, update, default, cancellationToken).ConfigureAwait(false);
                if (editResult is { IsAcknowledged: false })
                    throw new Exception($"Entity {node.Name} parents didn't get updated.");
            }
        );
    }

    public static string RenderToJson<TDocument>(FilterDefinition<TDocument> filter)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<TDocument>();
        return filter.Render(documentSerializer, serializerRegistry).ToJson();
    }

    public static string RenderToJson<TDocument>(UpdateDefinition<TDocument> filter)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<TDocument>();
        return filter.Render(documentSerializer, serializerRegistry).ToJson();
    }

    public async Task<IEnumerable<Guid>> GetAllNodeIds()
    {
        var results = await GetAllNodesAsync();
        return results.Select(en => en.Id);
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetAllRootNodes()
    {
        var results = (await GetAllNodesAsync()).ToList();
        var instanceIds = results.Where(n => !n.IsLeaf).Select(n => (n.Parent, n.Id)).ToArray();
        return results.ToProjection(instanceIds);
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetRootNodes()
    {
        var results = await ExecuteDbSetFuncAsync(collection =>
            collection.Find(i => i.Type.Name.ToLower() == "system").ToListAsync());
        return results.ToProjection();
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetExpandedNodes(IEnumerable<string> instanceIds)
    {
        var parentNodePairs = (instanceIds.ToParentNodeGuids()).ToList();
        var nodes = parentNodePairs.Select(pn => pn.node);
        var results = await ExecuteDbSetFuncAsync(collection =>
            collection.Find(i => nodes.Contains(i.Id)).ToListAsync());
        return results.ToProjection(parentNodePairs);
    }

    public async Task<EntityNodeProjection?> GetNode(string instanceId)
    {
        var id = instanceId.ToParentNodeGuids();
        var result = await GetByIdAsync(id.node);
        return result?.ToProjection(id.parent);
    }
}