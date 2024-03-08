// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Domain.Entities;
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Spatial;
using Econolite.Ode.Models.Entities.Types;
using Econolite.Ode.Repository.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Domain.Configuration;

public class EntityService : IEntityService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IEntityTypeService _entityTypeService;
    private readonly IEntityUpdates _entityUpdates;
    private readonly ILogger<EntityService> _logger;

    public EntityService(IServiceProvider serviceProvider, IEntityTypeService entityTypeService, IEntityUpdates entityUpdates, ILogger<EntityService> logger)
    {
        _entityRepository = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IEntityRepository>(); ;
        _entityTypeService = entityTypeService;
        _entityUpdates = entityUpdates;
        _logger = logger;
    }

    public async Task<EntityNode?> Add(EntityNode node)
    {
        return await Add(node.Parent, node);
    }

    public async Task<EntityNode?> Add(Guid parent, EntityNode node)
    {
        if (parent == Guid.Empty && node.Type.Name != "System")
        {
            var existing = await _entityRepository.GetByIdAsync(node.Id);
            if (existing != null) return existing;
            var rootNodes = (await _entityRepository.GetRootNodesAsync()).ToArray();
            if (!rootNodes.Any())
            {
                parent = Guid.NewGuid();
                var systemEntity = new EntityNode
                {
                    Id = parent,
                    Name = "Default System",
                    Description = "",
                    Parent = Guid.Empty,
                    Type = new EntityTypeId()
                    {
                        Id = SystemEntityTypeProvider.SystemTypeId.Id,
                        Name = SystemEntityTypeProvider.SystemTypeId.Name
                    }
                };

                _entityRepository.Create(Guid.Empty, systemEntity);
                await _entityUpdates.Add(this, systemEntity);
            }

            parent = rootNodes.First().Id;
        }
        var intersection = await _entityRepository.GetIntersectionParentNodeAsync(node);
        node = _entityTypeService.ModifyByType(node, intersection?.Id);
        _entityRepository.Create(parent, node);

        var (success, _) = await _entityRepository.DbContext.SaveChangesAsync();
        if (!success) return null;

        await _entityUpdates.Add(this, node);
        var parentNodeUpdated = false;
        var parentNode = await _entityRepository.GetByIdAsync(parent);
        if (parentNode is { IsLeaf: true })
        {
            parentNode.IsLeaf = false;
            _entityRepository.Edit(parentNode);
            parentNodeUpdated = true;
        }

        var (success2, _) = await _entityRepository.DbContext.SaveChangesAsync();
        if (success2 && parentNodeUpdated && parentNode != null)
        {
            await _entityUpdates.Update(this, parentNode);
        }
        return !success2 ? null : node;
    }

    public async Task<bool> Update(EntityNode node)
    {
        var intersection = await _entityRepository.GetIntersectionParentNodeAsync(node);
        node = _entityTypeService.ModifyByType(node, intersection?.Id);
        _entityRepository.Edit(node);
        var (success, _) = await _entityRepository.DbContext.SaveChangesAsync();
        if (success)
        {
            await _entityUpdates.Update(this, node);
        }

        return success;
    }

    public async Task<bool> Delete(Guid id)
    {
        var node = await _entityRepository.GetByIdAsync(id);
        if (node == null) return true;
        
        if (node.Children.Any(c => c is {IsCopy: false, IsDeleted: false}))
        {
            foreach (var child in node.Children.Where(c => c is {IsCopy: false, IsDeleted: false}))
            {
                var result = await Delete(child.Id);
            }
            node = await _entityRepository.GetByIdAsync(id);
        }
        
        if (node == null) return true;
        _entityRepository.SoftDelete(node);

        var (success, _) = await _entityRepository.DbContext.SaveChangesAsync();

        switch (success)
        {
            case false:
                return false;
            case true:
                await _entityUpdates.Delete(this, node);
                break;
        }

        var parentNode = await _entityRepository.GetByIdAsync(node.Parent);
        if (parentNode != null && parentNode.Children.All(c => c.IsDeleted))
        {
            parentNode.IsLeaf = true;
            _entityRepository.Edit(parentNode);
        }

        var (success2, _) = await _entityRepository.DbContext.SaveChangesAsync();

        if (success2 && parentNode != null)
        {
            await _entityUpdates.Update(this, parentNode);
        }

        return success2;
    }

    public async Task<EntityNode?> GetByIdAsync(Guid id)
    {
        return await _entityRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<EntityNode>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _entityRepository.GetByIdsAsync(ids);
    }

    public async Task<IEnumerable<string>> GetTypesAsync()
    {
        return await _entityRepository.GetTypesAsync();
    }

    public async Task<IEnumerable<EntityNode>> GetNodesByTypeAsync(string type)
    {
        return await _entityRepository.GetNodesByTypeAsync(type);
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetPossibleParentNodesByTypeAsync(Guid typeId)
    {
        var parentTypes = await _entityTypeService.GetParentTypesByTypeIdAsync(typeId);
        return await _entityRepository.GetNodesByTypesAsync(parentTypes.Select(t => t.Name));
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetNodesByTypesAsync(IEnumerable<string> types)
    {
        return await _entityRepository.GetNodesByTypesAsync(types);
    }

    public async Task<IEnumerable<EntityNode>> GetSearchNodesAsync(string search)
    {
        return await _entityRepository.GetSearchNodesAsync(search);
    }

    public async Task<IEnumerable<EntityNode>> GetAllDeletedNodesAsync()
    {
        return await _entityRepository.GetAllDeletedNodesAsync();
    }

    public async Task<IEnumerable<EntityNode>> GetAllNodesAsync()
    {
        return await _entityRepository.GetAllNodesAsync();
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetRootNodesAsync()
    {        
        var result = await _entityRepository.GetRootNodesAsync();
        if (!result.Any())
        {
            _logger.LogInformation("No entities found in the database. Adding default system.");
            var parent = Guid.NewGuid();
            var systemEntity = new EntityNode
            {
                Id = parent,
                Name = "System",
                Description = "",
                Parent = Guid.Empty,
                Type = new EntityTypeId()
                {
                    Id = SystemEntityTypeProvider.SystemTypeId.Id,
                    Name = SystemEntityTypeProvider.SystemTypeId.Name
                }
            };

            _entityRepository.Create(Guid.Empty, systemEntity);
            await _entityRepository.DbContext.SaveChangesAsync();
            await _entityUpdates.Add(this, systemEntity);
            result = await _entityRepository.GetRootNodesAsync();
        }
        
        return result;
    }

    public async Task<IEnumerable<EntityNodeProjection>> GetExpandedNodesAsync(IEnumerable<string> instanceIds)
    {
        return await _entityRepository.GetExpandedNodesAsync(instanceIds);
    }

    public async Task<EntityNodeProjection?> GetNodeAsync(string instanceId)
    {
        return await _entityRepository.GetNodeAsync(instanceId);
    }

    public async Task<IEnumerable<EntityNode>> GetByIntersectionIdAsync(Guid intersectionId)
    {
        return await _entityRepository.GetNodesByIntersectionIdAsync(intersectionId);
    }

    public async Task<IEnumerable<EntityNode>> GetByIntersectionIdMapAsync(int id)
    {
        return await _entityRepository.GetNodesByIntersectionIdMapAsync(id);
    }
    
    public async Task<IEnumerable<EntityNode>> QueryIntersectionsWithinRadiusDistanceInMilesAsync(
        GeoJsonPointFeature point, int miles)
    {
        return await _entityRepository.QueryIntersectionsWithinRadiusDistanceInMilesAsync(point, miles);
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesAsync(GeoJsonPointFeature point)
    {
        return await _entityRepository.QueryIntersectingGeoFencesAsync(point);
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingByTypeAsync(GeoJsonLineStringFeature route, string type)
    {
        return await _entityRepository.QueryIntersectingByTypeAsync(type, route);
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesByTypeAsync(GeoJsonPointFeature point, string type)
    {
        return await _entityRepository.QueryIntersectingGeoFencesByTypeAsync(type, point);
    }

    public async Task<IEnumerable<EntityNode>> GetDownstreamIntersectionsAsync(GeoJsonPointFeature point, int signals)
    {
        return await GetUpstreamDownstreamIntersectionsAsync(point, signals, true).ConfigureAwait(false);
    }

    public async Task<IEnumerable<EntityNode>> GetUpstreamIntersectionsAsync(GeoJsonPointFeature point, int signals)
    {
        return await GetUpstreamDownstreamIntersectionsAsync(point, signals).ConfigureAwait(false);
    }

    private async Task<IEnumerable<EntityNode>> GetUpstreamDownstreamIntersectionsAsync(GeoJsonPointFeature point, int signals, bool isDownstream = false)
    {
        var intersectingEntities = await _entityRepository.QueryIntersectingGeoFencesAsync(point).ConfigureAwait(false);
        var entityNodes = intersectingEntities as EntityNode[] ?? intersectingEntities.ToArray();
        var intersectingIntersections = entityNodes.Where(e => e.Type.Id == IntersectionTypeId.Id).ToList();

        var bearing = Bearing.Unknown;
        EntityNode? intersection = null;

        if (!intersectingIntersections.Any())
        {
            var streetSegment = entityNodes.FirstOrDefault(e => e.Type.Name == StreetSegmentTypeId.Name);
            if (streetSegment != null)
            {
                var properties = streetSegment.Geometry.LineString!.Properties!;
                if (properties.Intersection == null) return Enumerable.Empty<EntityNode>();
                intersection = await _entityRepository.GetByIdAsync(properties.Intersection.Value).ConfigureAwait(false);
                // Bearing is the only difference between upstream and downstream
                bearing = isDownstream
                    ? properties.Bearing ?? Bearing.Unknown
                    : properties.Bearing?.ToOppositeBearing() ?? Bearing.Unknown;
            }
            else
            {
                var entities = await _entityRepository.QueryIntersectionsWithinRadiusDistanceInMilesAsync(point, 1).ConfigureAwait(false);
                var enumerable = entities as EntityNode[] ?? entities.ToArray();
                var intersections = enumerable.Where(e => e.Type.Id == IntersectionTypeId.Id).ToList();
                if (!intersections.Any())
                    return Enumerable.Empty<EntityNode>();
                intersection = intersections.FirstOrDefault();
                if (intersection == null) return Enumerable.Empty<EntityNode>();
                intersectingIntersections.Clear();
                var coordinates = intersection.Geometry.Point?.Coordinates ?? intersection.Geometry.Polygon?.ToPolygon().Centroid.ToGeoJsonPoint().Coordinates;
                if (coordinates == null) return Enumerable.Empty<EntityNode>();
                // Bearing is the only difference between upstream and downstream
                bearing = isDownstream
                    ? point.Coordinates!.ToBearing(coordinates)
                    : point.Coordinates!.ToBearing(coordinates).ToOppositeBearing();
            }
        }
        else if (intersectingIntersections.Any())
        {
            intersection = intersectingIntersections.FirstOrDefault();
            if (intersection == null) return Enumerable.Empty<EntityNode>();
            intersectingIntersections.Clear();
            var coordinates = intersection.Geometry.Point?.Coordinates ?? intersection.Geometry.Polygon?.ToPolygon().Centroid.ToGeoJsonPoint().Coordinates;
            if (coordinates == null) return Enumerable.Empty<EntityNode>();
            // Bearing is the only difference between upstream and downstream
            bearing = isDownstream
                ? point.Coordinates!.ToPerpendicularBearing(coordinates)
                : point.Coordinates!.ToPerpendicularBearing(coordinates).ToOppositeBearing();
        }

        return await UpstreamDownstreamIntersectionsAsync(signals, intersection, bearing, intersectingIntersections, isDownstream).ConfigureAwait(false);
    }

    private async Task<IEnumerable<EntityNode>> UpstreamDownstreamIntersectionsAsync(int signals, EntityNode? intersection, Bearing bearing,
        List<EntityNode> intersectingIntersections, bool isDownstream = false)
    {
        if (intersection == null) return Enumerable.Empty<EntityNode>();
        var numSignals = signals;
        var currentIntersection = intersection;
        while (numSignals > 0)
        {
            if (isDownstream)
            {
                intersectingIntersections.Add(currentIntersection);
            }

            var upstreamIntersection =
                await GetIntersectionNextFromBearingAsync(currentIntersection, bearing).ConfigureAwait(false);
            if (upstreamIntersection == null) break;
            currentIntersection = upstreamIntersection;

            if (!isDownstream)
            {
                intersectingIntersections.Add(currentIntersection);
            }

            numSignals--;
        }

        return intersectingIntersections;
    }

    private async Task<EntityNode?> GetIntersectionNextFromBearingAsync(EntityNode intersection, Bearing bearing)
    {
        var intersectionPoint = intersection.Geometry.Point ?? intersection.Geometry.Polygon?.ToPolygon().Centroid.ToGeoJsonPoint();
        if (intersectionPoint == null) return null;
        var intersections = await _entityRepository.QueryIntersectionsWithinRadiusDistanceInMilesAsync(intersectionPoint, 2).ConfigureAwait(false);
        var allIntersections = intersections.Where(i => i.Id != intersection.Id && i.Type.Id == IntersectionTypeId.Id).ToList();
        if (allIntersections.Count == 0) return null;
        return allIntersections.FirstOrDefault(i => intersection.Geometry.Point!.Coordinates!.IsMatchingBearings(i.Geometry.Point!.Coordinates!, bearing));
    }

    public async Task<(EntityNodeProjection? Node, string? error)> CopyAsync(EntityNodeProjection node, Guid parent)
    {
        return await _entityRepository.CopyAsync(node, parent);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> MoveAsync(EntityNodeProjection node, Guid newParent)
    {
        return await _entityRepository.MoveAsync(node, newParent);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> MoveUpAsync(string instanceId)
    {
        return await _entityRepository.MoveUpAsync(instanceId);
    }

    public async Task<(EntityNodeProjection? Node, string? error)> MoveDownAsync(string instanceId)
    {
        return await _entityRepository.MoveDownAsync(instanceId);
    }

    public async Task<bool> SyncAsync(EntitySync sync)
    {
        if (sync.Corridors.Any())
        {
            var ids = sync.Corridors.Select(c => c.Id.ToString());
            var intersectionIds = sync.Corridors.SelectMany(c => c.Intersections).Select(i => i.ToString()).Distinct().ToArray();
            var corridors = (await _entityRepository.GetNodesByExternalIdAsync(ids)).ToArray();
            var intersections = (await _entityRepository.GetNodesByExternalIdAsync(intersectionIds)).ToArray();
            foreach (var corridor in sync.Corridors.ToArray())
            {
                var node = corridors.SingleOrDefault(c => c.ExternalId == corridor.Id.ToString());
                if (node == null)
                {
                    var orderedIntersections = intersections.ToUpdateNodes(corridor);

                    if (!orderedIntersections.Any())
                    {
                        continue;
                    }

                    var toAdd = corridor.ToNewNode();
                    var result = await Add(toAdd);
                    // Add intersections to the corridor
                    foreach (var intersection in orderedIntersections)
                    {
                        if (result != null)
                            await _entityRepository.CopyAsync(intersection.ToProjection(result.Id), result.Id);
                    }
                }
                else
                {
                    if (corridor.IsDeleted)
                    {
                        _ = await Delete(node.Id);
                    }
                    else
                    {
                        var orderedIntersections = intersections.ToUpdateNodes(corridor);

                        var toUpdate = node.ToUpdateNode(corridor);
                        var success = await Update(toUpdate);
                        if (!success) continue;
                        var newIntersections = orderedIntersections.Where(i => node.Children.All(c => c.Id != i.Id));
                        foreach (var intersection in newIntersections.ToArray())
                        {
                            await _entityRepository.CopyAsync(intersection.ToProjection(node.Id), node.Id);
                        }

                        _entityRepository.SetChildrenOrder(node.Id, orderedIntersections.Select(i => i.Id));
                        await _entityRepository.DbContext.SaveChangesAsync();
                    }
                }
            }
        }

        if (sync.Intersections.Any())
        {
            var ids = sync.Intersections.Select(c => c.ClarityId.ToString() ?? string.Empty).Where(id => id != string.Empty);
            var intersections = (await _entityRepository.GetNodesByExternalIdAsync(ids)).ToArray();
            foreach (var item in sync.Intersections.ToArray())
            {
                var node = intersections.SingleOrDefault(i => i.ExternalId == item.ClarityId.ToString());
                if (node == null)
                {
                    _ = await Add(item.ToNewNode());
                }
                else
                {
                    if (item.IsDeleted)
                    {
                        _ = await Delete(node.Id);
                    }
                    else
                    {
                        _ = await Update(node.ToUpdateNode(item));
                    }
                }
            }

            return true;
        }

        return false;
    }
    
    public async Task<IEnumerable<EntityNode>> RemoveInvisibleTypesAsync(IEnumerable<EntityNode> entities)
    {
        var result = new List<EntityNode>();
        var types = (await GetInvisibleTypes()).ToArray();
        foreach (var entity in entities)
        {
            if(types.Contains(entity.Type.Id)) continue;
            entity.Children = entity.Children.Where(c => !types.Contains(c.Type.Id)).ToArray();
            result.Add(entity);
        }
        return result;
    }
    
    public async Task<IEnumerable<EntityNodeProjection>> RemoveInvisibleTypesAsync(IEnumerable<EntityNodeProjection> entities)
    {
        var result = new List<EntityNodeProjection>();
        var types = (await GetInvisibleTypes()).ToArray();
        foreach (var entity in entities)
        {
            if(types.Contains(entity.Type.Id)) continue;
            entity.Children = entity.Children.Where(c => !types.Contains(c.Type.Id)).ToArray();
            result.Add(entity);
        }
        return result;
    }
    
    public async Task<EntityNode> RemoveInvisibleTypesAsync(EntityNode entity)
    {
        var types = (await GetInvisibleTypes()).ToArray();
        entity.Children = entity.Children.Where(c => !types.Contains(c.Type.Id)).ToArray();
        return entity;
    }
    
    public async Task<EntityNodeProjection> RemoveInvisibleTypesAsync(EntityNodeProjection entity)
    {
        var types = (await GetInvisibleTypes()).ToArray();
        entity.Children = entity.Children.Where(c => !types.Contains(c.Type.Id)).ToArray();
        return entity;
    }
    
    private async Task<IEnumerable<Guid>> GetInvisibleTypes()
    {
        return (await _entityTypeService.GetAllAsync()).ToArray().Where(t => !t.Visible).Select(t => t.Id);
    }
}