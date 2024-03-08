// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;

namespace Econolite.Ode.Models.Entities.Types;

public static class StreetSegmentTypeId
{
    public static Guid Id => Guid.Parse("93963755-19fc-41fb-87af-e8ab2b1965aa");
    public static string Name => "Street Segment";
}

public class StreetSegmentEntityTypeProvider : EntityTypeProvider, IApproachTypeChildren
{
    public StreetSegmentEntityTypeProvider(IEnumerable<IStreetSegmentTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = StreetSegmentTypeId.Id,
            Name = StreetSegmentTypeId.Name,
            Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" enable-background=\"new 0 0 20 20\" height=\"18px\" viewBox=\"0 0 20 20\" width=\"18px\" fill=\"#000000\"><rect fill=\"none\" height=\"20\" width=\"20\"/><path d=\"M16.03,8.03l-3.23,3.23c-0.98,0.98-2.56,0.98-3.54,0l-0.7-0.7c-0.39-0.39-1.02-0.39-1.41,0l-4.09,4.09L2,13.59L6.08,9.5 c0.98-0.98,2.56-0.98,3.54,0l0.71,0.71c0.39,0.39,1.02,0.39,1.41,0l3.23-3.23L13,5h5v5L16.03,8.03z\"/></svg>",
            SystemType = true,
            Visible = true,
            SpatialType = GeoSpatialType.LineString,
            Copyable = false,
            Movable = false,
            Sections = new []
            {
                EntityTypeSections.Entity,
                EntityTypeSections.SpeedLimit
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
    }
    
    public override EntityNode ModifyEntityNode(EntityNode node, Guid? intersection)
    {
        if (node is not {Geometry: {LineString: not null}}) return node;
        node.GeoFence = node.Geometry.LineString.CreateBufferInFeet(36.7);
        if (node.Geometry.LineString.Properties != null)
        {
            node.Geometry.LineString.Properties.Intersection = intersection;
            node.Geometry.LineString.Properties.Destination = node.Parent;
            node.Geometry.LineString.Properties.TripPointLocations =
                node.Geometry.LineString.ToTripPointLocations(50).ToArray();
            var (bearing, error) = node.Geometry.LineString.ToLinestring().GetBearing(false);
            if (error == null)
                node.Geometry.LineString.Properties.Bearing = bearing;
        }

        return node;
    }
}

public interface IStreetSegmentTypeChildren : IEntityTypeChildren {}