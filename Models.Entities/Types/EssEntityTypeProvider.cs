// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;
using Econolite.Ode.Models.Entities.Spatial;

namespace Econolite.Ode.Models.Entities.Types;

public static class EssTypeId
{
    public static Guid Id => Guid.Parse("37855e7c-0750-4a4f-80bc-3f00c90b15ce");
    public static string Name => "Environmental Sensor";
}

public class EssEntityTypeProvider : EntityTypeProvider, ISystemTypeChildren, ICorridorTypeChildren, IIntersectionTypeChildren
{
    public EssEntityTypeProvider(IEnumerable<IEssTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = EssTypeId.Id,
            Name = EssTypeId.Name,
            Icon = "<svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 43.01 32.88\"><defs><style>.cls-1-evo{fill:#333132;}</style></defs><rect class=\"cls-1-evo\" x=\"3.81\" width=\"35.4\" height=\"15.17\"/><rect class=\"cls-1-evo\" x=\"18.99\" y=\"25.29\" width=\"5.06\" height=\"7.59\"/><polygon class=\"cls-1-evo\" points=\"34.89 22.03 39.42 26.58 43.01 22.99 38.46 18.46 34.89 22.03\"/><rect class=\"cls-1-evo\" x=\"29.3\" y=\"62.38\" width=\"6.42\" height=\"5.06\" transform=\"translate(-64.74 -0.67) rotate(-44.8)\"/></svg>",
            SystemType = true,
            Visible = true,
            SpatialType = GeoSpatialType.Point,
            Copyable = true,
            Movable = true,
            Sections = new []
            {
                EntityTypeSections.Entity,
                EntityTypeSections.Communication,
                EntityTypeSections.Controller,
                EntityTypeSections.DeviceManager,
                EntityTypeSections.FtpCredentials
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

public interface IEssTypeChildren : IEntityTypeChildren {}