// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;

namespace Econolite.Ode.Models.Entities.Types;

public static class RsuTypeId
{
    public static Guid Id => Guid.Parse("d5788aa1-95f7-4eb4-9954-6200635dda59");
    public static string Name => "Rsu";
}

public class RsuEntityTypeProvider : EntityTypeProvider, IIntersectionTypeChildren, ISignalTypeChildren
{
    public RsuEntityTypeProvider(IEnumerable<IRsuTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = RsuTypeId.Id,
            Name = RsuTypeId.Name,
            Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" enable-background=\"new 0 0 20 20\" height=\"18px\" viewBox=\"0 0 20 20\" width=\"18px\" fill=\"#000000\"><g><rect fill=\"none\" height=\"20\" width=\"20\"/></g><g><g><path d=\"M6.5,11.5l1.06-1.06C6.94,9.81,6.55,8.95,6.55,8s0.39-1.81,1.01-2.44L6.5,4.5C5.6,5.4,5.05,6.63,5.05,8S5.6,10.6,6.5,11.5 z\"/><path d=\"M5.41,3.41L4.34,2.34C2.9,3.79,2,5.79,2,8s0.9,4.21,2.34,5.66l1.06-1.06C4.23,11.42,3.5,9.79,3.5,8S4.23,4.58,5.41,3.41z\"/><path d=\"M13.5,4.5l-1.06,1.06c0.62,0.62,1.01,1.49,1.01,2.44s-0.39,1.81-1.01,2.44l1.06,1.06c0.9-0.9,1.45-2.13,1.45-3.5 S14.4,5.4,13.5,4.5z\"/><path d=\"M15.66,2.34l-1.06,1.06C15.77,4.58,16.5,6.21,16.5,8s-0.73,3.42-1.91,4.59l1.06,1.06C17.1,12.21,18,10.21,18,8 S17.1,3.79,15.66,2.34z\"/><path d=\"M12,8c0-1.1-0.9-2-2-2S8,6.9,8,8c0,0.63,0.29,1.18,0.75,1.55L6,18h1.5l0.49-1.5h3.6L12,18h1.5l-2.31-8.4 C11.68,9.23,12,8.66,12,8z M8.47,15l1.46-4.5l1.24,4.5H8.47z\"/></g></g></svg>",
            SystemType = true,
            Visible = true,
            SpatialType = GeoSpatialType.Point,
            Copyable = false,
            Movable = true,
            Sections = new []
            {
                EntityTypeSections.Entity,
                EntityTypeSections.SnmpV3
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
    }
    
    public override EntityNode ModifyEntityNode(EntityNode node, Guid? intersection)
    {
        if (node is not {Geometry: {Point: {Properties: not null}}}) return node;
        if (node is {Geometry: {Point: not null}})
        {
            node.GeoFence = node.Geometry.Point.CreateBufferInFeet(100);
            node.Geometry.Point.Properties.Intersection = intersection;
        }
        
        return node;
    }
}

public interface IRsuTypeChildren : IEntityTypeChildren {}