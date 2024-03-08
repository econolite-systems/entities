// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;

namespace Econolite.Ode.Models.Entities.Types;

public static class ApproachTypeId
{
    public static Guid Id => Guid.Parse("8adb049d-2958-4210-af9c-412ec5c5726e");
    public static string Name => "Approach";
}

public class ApproachEntityTypeProvider : EntityTypeProvider, IIntersectionTypeChildren
{
    public ApproachEntityTypeProvider(IEnumerable<IApproachTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = ApproachTypeId.Id,
            Name = ApproachTypeId.Name,
            Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"18px\" viewBox=\"0 0 24 24\" width=\"18px\" fill=\"#000000\"><path d=\"M0 0h24v24H0V0z\" fill=\"none\"/><path d=\"M12 1C5.93 1 1 5.93 1 12s4.93 11 11 11 11-4.93 11-11S18.07 1 12 1zm3.57 16L12 15.42 8.43 17l-.37-.37L12 7l3.95 9.63-.38.37z\"/></svg>",
            SystemType = true,
            Visible = true,
            Copyable = false,
            Movable = false,
            SpatialType = GeoSpatialType.LineString,
            Sections = new []
            {
                EntityTypeSections.Entity,
                EntityTypeSections.Bearing,
                EntityTypeSections.Plans
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
    }
    
    public override EntityNode ModifyEntityNode(EntityNode node, Guid? intersection)
    {
        if (node is not {Geometry: {LineString: {Properties: not null}}}) return node;
        if (node is {Geometry: {LineString: not null}})
        {
            node.GeoFence = node.Geometry.LineString.CreateBufferInFeet(36.7);
            node.Geometry.LineString.Properties.Intersection = intersection;
        }
        
        return node;
    }
}

public interface IApproachTypeChildren : IEntityTypeChildren {}