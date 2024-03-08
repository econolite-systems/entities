// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;

namespace Econolite.Ode.Models.Entities.Types;

public static class SignalTypeId
{
    public static Guid Id => Guid.Parse("4cbc4cfa-5b47-4f42-acad-dd6f1d6bef2d");
    public static string Name => "Signal";
}

public class SignalEntityTypeProvider : EntityTypeProvider, ISystemTypeChildren, ICorridorTypeChildren, IIntersectionTypeChildren
{
    public SignalEntityTypeProvider(IEnumerable<ISignalTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = SignalTypeId.Id,
            Name = SignalTypeId.Name,
            Icon = "<svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 45.86 100\"><defs><style>.cls-1-signal-color{fill:#323031;}.cls-2-signal-color{fill:#bf5555;}.cls-3-signal-color{fill:#c0b454;}.cls-4-signal-color{fill:#57bc82;}</style></defs><path class=\"cls-1-signal-color\" d=\"M45.81,97A2.89,2.89,0,0,1,43,100H2.73A2.89,2.89,0,0,1,0,97V3A2.89,2.89,0,0,1,2.73,0H43a2.89,2.89,0,0,1,2.78,3Z\" transform=\"translate(0.05 0)\"/><circle class=\"cls-2-signal-color\" cx=\"22.93\" cy=\"21.61\" r=\"12.13\"/><circle class=\"cls-3-signal-color\" cx=\"22.93\" cy=\"50\" r=\"12.13\"/><circle class=\"cls-4-signal-color\" cx=\"22.93\" cy=\"79.38\" r=\"12.13\"/></svg>",
            SystemType = true,
            Visible = true,
            SpatialType = GeoSpatialType.Point,
            Copyable = true,
            Movable = true,
            Sections = new []
            {
                EntityTypeSections.Entity,
                EntityTypeSections.IdMapping
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

public interface ISignalTypeChildren : IEntityTypeChildren {}