// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;
using Econolite.Ode.Repository.Entities;

namespace Econolite.Ode.Models.Entities.Types;

public static class SpeedSegmentTypeId
{
    public static Guid Id => Guid.Parse("1313f542-670f-46dd-94ca-179b56240a6e");
    public static string Name => "Speed Segment";
}

public class SpeedSegmentEntityTypeProvider : EntityTypeProvider, ISystemTypeChildren, ICorridorTypeChildren
{

    private readonly IEntityRepository _entityRepository;
    public SpeedSegmentEntityTypeProvider(IEnumerable<IIntersectionTypeChildren>? typeChildren, IEntityRepository entityRepository)
    {
        Type = new EntityType()
        {
            Id = SpeedSegmentTypeId.Id,
            Name = SpeedSegmentTypeId.Name,
            Icon = "",
            SystemType = true,
            Visible = false,
            SpatialType = GeoSpatialType.LineString,
            Copyable = false,
            Movable = false,
            Sections = new[]
                    {
                EntityTypeSections.Entity,
                EntityTypeSections.SpeedLimit,
                EntityTypeSections.SpeedSegment
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
        _entityRepository = entityRepository;
    }

    public override EntityNode ModifyEntityNode(EntityNode node, Guid? intersection)
    {
        if (node is not { Geometry: { LineString: not null } }) return node;
        node.GeoFence = node.Geometry.LineString.CreateBufferInFeet(36.7);
        if (node.Geometry.LineString.Properties != null)
        {
            node.Geometry.LineString.Properties.Origin = node.Parents.First();
            node.Geometry.LineString.Properties.Destination = node.Parents.Last();
            var res = _entityRepository.QueryIntersectingByType(SpeedSegmentTypeId.Name, node.Geometry.LineString);
            node.Geometry.LineString.Properties.Intersections = res.Select(x => x.Id).ToList();
        }

        return node;
    }
}

public interface ISpeedSegmentTypeChildren : IEntityTypeChildren { }