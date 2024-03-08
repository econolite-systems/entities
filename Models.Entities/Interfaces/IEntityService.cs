// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;

namespace Econolite.Ode.Domain.Configuration;

public interface IEntityService
{
    Task<EntityNode?> Add(EntityNode node);
    Task<EntityNode?> Add(Guid parent, EntityNode node);
    Task<bool> Update(EntityNode node);
    Task<bool> Delete(Guid id);
    Task<IEnumerable<string>> GetTypesAsync();
    Task<EntityNode?> GetByIdAsync(Guid id);
    Task<IEnumerable<EntityNode>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<IEnumerable<EntityNode>> GetNodesByTypeAsync(string type);
    Task<IEnumerable<EntityNodeProjection>> GetPossibleParentNodesByTypeAsync(Guid typeId);
    Task<IEnumerable<EntityNodeProjection>> GetNodesByTypesAsync(IEnumerable<string> types);
    Task<IEnumerable<EntityNode>> GetSearchNodesAsync(string search);
    Task<IEnumerable<EntityNode>> GetAllDeletedNodesAsync();
    Task<IEnumerable<EntityNode>> GetAllNodesAsync();
    Task<IEnumerable<EntityNodeProjection>> GetRootNodesAsync();
    Task<IEnumerable<EntityNodeProjection>> GetExpandedNodesAsync(IEnumerable<string> instanceIds);
    Task<EntityNodeProjection?> GetNodeAsync(string instanceId);
    Task<IEnumerable<EntityNode>> GetByIntersectionIdAsync(Guid intersectionId);
    Task<IEnumerable<EntityNode>> GetByIntersectionIdMapAsync(int id);
    Task<IEnumerable<EntityNode>> QueryIntersectionsWithinRadiusDistanceInMilesAsync(GeoJsonPointFeature point, int miles);
    Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesAsync(GeoJsonPointFeature point);
    Task<IEnumerable<EntityNode>> QueryIntersectingByTypeAsync(GeoJsonLineStringFeature route, string type);
    Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesByTypeAsync(GeoJsonPointFeature point, string type);
    Task<IEnumerable<EntityNode>> GetDownstreamIntersectionsAsync(GeoJsonPointFeature point, int signals);
    Task<IEnumerable<EntityNode>> GetUpstreamIntersectionsAsync(GeoJsonPointFeature point, int signals);
    Task<(EntityNodeProjection? Node, string? error)> CopyAsync(EntityNodeProjection node, Guid parent);
    Task<(EntityNodeProjection? Node, string? error)> MoveAsync(EntityNodeProjection node, Guid newParent);
    Task<(EntityNodeProjection? Node, string? error)> MoveUpAsync(string instanceId);
    Task<(EntityNodeProjection? Node, string? error)> MoveDownAsync(string instanceId);
    Task<bool> SyncAsync(EntitySync sync);
    Task<IEnumerable<EntityNode>> RemoveInvisibleTypesAsync(IEnumerable<EntityNode> entities);
    Task<IEnumerable<EntityNodeProjection>> RemoveInvisibleTypesAsync(IEnumerable<EntityNodeProjection> entities);
    Task<EntityNode> RemoveInvisibleTypesAsync(EntityNode entity);
    Task<EntityNodeProjection> RemoveInvisibleTypesAsync(EntityNodeProjection entities);
}