// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Types;
using Econolite.Ode.Persistence.Common.Entities;

namespace Econolite.Ode.Models.Entities;

public class EntitySync : GuidIndexedEntityBase
{
    public Guid ExternalSystemId = Guid.Empty;

    public IEnumerable<CorridorsSyncModel> Corridors { get; set; } = Array.Empty<CorridorsSyncModel>();
    
    public IEnumerable<SpatIntersectionModel> Intersections { get; set; } = Array.Empty<SpatIntersectionModel>();
}

public class CorridorsSyncModel
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
        
    public bool IsDeleted { get; set; }
        
    public long[] Intersections { get; set; } = Array.Empty<long>();
}

public class SpatIntersectionModel : GuidIndexedEntityBase
{
    public long? ClarityId { get; set; }
        
    public int? SpatId { get; set; }

    public string Name { get; set; } = string.Empty;
        
    public string? Description { get; set; }

    public string ControllerType { get; set; } = string.Empty;

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }
        
    public bool IsDeleted { get; set; }
}

public static class EntitySyncExtensions
{
    public static EntityNode ToNewNode(this CorridorsSyncModel model)
    {
        return new EntityNode
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            ExternalId = model.Id.ToString(),
            Type = new EntityTypeId() { Id=CorridorTypeId.Id, Name=CorridorTypeId.Name},
            IsLeaf = false
        };
    }
    public static EntityNode ToNewNode(this SpatIntersectionModel model)
    {
        var geometry = new GeoJsonGeometry();
        if (model is {Latitude: not null, Longitude: not null} && (model.Latitude != 0 && model.Longitude != 0))
        {
            geometry.Point = new GeoJsonPointFeature()
            {
                Coordinates = new[] {model.Longitude.Value, model.Latitude.Value}
            };
        }
        
        return new EntityNode
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description ?? string.Empty,
            ControllerType = "SPAT",
            ExternalId = model.ClarityId.ToString(),
            IdMapping = model.SpatId,
            Type = new EntityTypeId { Id=SignalTypeId.Id, Name=SignalTypeId.Name},
            Geometry = geometry
        };
    }
    
    public static EntityNode ToUpdateNode(this EntityNode node, CorridorsSyncModel model)
    {
        node.Name = model.Name;
        node.IsDeleted = model.IsDeleted;

        return node;
    }
    
    public static EntityNode[] ToUpdateNodes(this IEnumerable<EntityNode> intersections, CorridorsSyncModel model)
    {
        // To the intersections in the model, find the corresponding node in the intersections list.
        var intersectionNodes = intersections
            .Where(i => model.Intersections.Contains(int.Parse(i.ExternalId!)))
            .ToArray()
            .OrderBy(i => Array.IndexOf(model.Intersections, int.Parse(i.ExternalId!)));
        
        // Find all the intersections that are not already children of the corridor node.
        // var newIntersections = intersectionNodes.Where(i => !node.Children.Any(c => c.Id == i.Id));
        
        // Add the old intersections to node.Children in the same order as the model.
        //node.Children = intersectionNodes.OrderBy(i => Array.IndexOf(model.Intersections, int.Parse(i.ExternalId)));
        return intersectionNodes.ToArray();
    }

    public static EntityNode ToUpdateNode(this EntityNode node, SpatIntersectionModel model)
    {
        var geometry = new GeoJsonGeometry();
        if (model is {Latitude: not null, Longitude: not null})
        {
            geometry.Point = new GeoJsonPointFeature
            {
                Coordinates = new[] {model.Longitude.Value, model.Latitude.Value}
            };
        }
        
        node.Name = model.Name;
        node.Description = model.Description ?? string.Empty;
        node.IdMapping = model.SpatId;
        node.IsDeleted = model.IsDeleted;
        node.Geometry = geometry;

        return node;
    }
}