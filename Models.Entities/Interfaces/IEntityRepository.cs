// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Common.Repository;

namespace Econolite.Ode.Repository.Entities;

public interface IEntityRepository : IRepository
{
    string CollectionName { get; }
    Task<IEnumerable<string>> GetTypesAsync();
    Task<IEnumerable<EntityNode>> GetSearchNodesAsync(string search);
    Task<IEnumerable<EntityNode>> GetNodesByTypeAsync(string type);
    Task<IEnumerable<EntityNode>> GetAllDeletedNodesAsync();
    Task<IEnumerable<EntityNode>> GetAllNodesAsync();
    Task<IEnumerable<EntityNodeProjection>> GetRootNodesAsync();
    Task<IEnumerable<EntityNodeProjection>> GetExpandedNodesAsync(IEnumerable<string> instanceIds);
    Task<EntityNodeProjection?> GetNodeAsync(string instanceId);
    Task<IEnumerable<EntityNodeProjection>> GetNodesByTypesAsync(IEnumerable<string> types);
    Task<IEnumerable<EntityNode>> GetNodesByExternalIdAsync(IEnumerable<string> ids);
    Task<EntityNode?> GetIntersectionParentNodeAsync(EntityNode node);
    Task<IEnumerable<EntityNode>> QueryIntersectionsWithinRadiusDistanceInMilesAsync(GeoJsonPointFeature point, int miles);
    Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesAsync(GeoJsonPointFeature point);
    Task<IEnumerable<EntityNode>> GetNodesByIntersectionIdAsync(Guid id);
    Task<IEnumerable<EntityNode>> GetNodesByIntersectionIdMapAsync(int id);
    Task<IEnumerable<EntityNode>> QueryIntersectingByTypeAsync(string type, GeoJsonLineStringFeature route);
    IEnumerable<EntityNode> QueryIntersectingByType(string type, GeoJsonLineStringFeature route);
    Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesByTypeAsync(string type, GeoJsonPointFeature point);
    Task<(EntityNodeProjection? Node, string? error)> CopyAsync(EntityNodeProjection node, Guid parent);
    Task<(EntityNodeProjection? Node, string? error)> MoveAsync(EntityNodeProjection node, Guid newParent);
    Task<(EntityNodeProjection? Node, string? error)> MoveUpAsync(string instanceId);
    Task<(EntityNodeProjection? Node, string? error)> MoveDownAsync(string instanceId);
    void Create(Guid parent, EntityNode node);
    void Edit(EntityNode node);
    void Delete(Guid id);
    void SoftDelete(EntityNode node);
    void Restore(EntityNode node);
    void Add(EntityNode document);
    Task<EntityNode?> GetByIdAsync(Guid id);
    Task<IEnumerable<EntityNode>> GetByIdsAsync(IEnumerable<Guid> ids);
    EntityNode? GetById(Guid id);
    Task<IEnumerable<EntityNode>> GetAllAsync();
    IEnumerable<EntityNode> GetAll();
    void Update(EntityNode document);
    void Remove(Guid id);
    void Dispose();
    void SetChildrenOrder(Guid id, IEnumerable<Guid> childrenIds);
}