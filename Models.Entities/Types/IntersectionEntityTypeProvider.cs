// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;

namespace Econolite.Ode.Models.Entities.Types;

public static class IntersectionTypeId
{
    public static Guid Id => Guid.Parse("1231b98b-7320-41b6-a857-9c6097b20628");
    public static string Name => "Intersection";
}

public static class EntityTypeIdStatic
{
    public static Guid Id { get; set; }
    public static string Name { get; set; } = string.Empty;
}

public class IntersectionEntityTypeProvider : EntityTypeProvider, ISystemTypeChildren, ICorridorTypeChildren
{
    public IntersectionEntityTypeProvider(IEnumerable<IIntersectionTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = IntersectionTypeId.Id,
            Name = IntersectionTypeId.Name,
            Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-sign-intersection-fill\" viewBox=\"0 0 16 16\">\n  <path d=\"M9.05.435c-.58-.58-1.52-.58-2.1 0L.436 6.95c-.58.58-.58 1.519 0 2.098l6.516 6.516c.58.58 1.519.58 2.098 0l6.516-6.516c.58-.58.58-1.519 0-2.098L9.05.435ZM7.25 4h1.5v3.25H12v1.5H8.75V12h-1.5V8.75H4v-1.5h3.25V4Z\"/>\n</svg>",
            SystemType = true,
            Visible = true,
            SpatialType = GeoSpatialType.Polygon,
            Copyable = false,
            Movable = false,
            Sections = new []
            {
                EntityTypeSections.Entity,
                EntityTypeSections.PrimarySecondaryStreetNames,
                EntityTypeSections.IdMapping
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
    }
    
    public override EntityNode ModifyEntityNode(EntityNode node, Guid? intersection)
    {
        if (node is not {Geometry: {Polygon: not null}}) return node;
        
        node.Geometry.Point = node.Geometry.Polygon.ToPolygon().Centroid.ToGeoJsonPoint();
        if (node.Geometry.Point.Properties != null)
        {
            node.Geometry.Point.Properties.Intersection = node.Id;
        }
        else
        {
            node.Geometry.Point.Properties = new GeoJsonProperties()
            {
                Intersection = node.Id
            };
        }
        
        node.GeoFence = node.Geometry.Polygon;
        if (node.Geometry.Polygon.Properties != null) 
            node.Geometry.Polygon.Properties.Intersection = node.Id;
        
        return node;
    }
}

public interface IIntersectionTypeChildren : IEntityTypeChildren {}