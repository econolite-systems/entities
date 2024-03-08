// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities.Spatial;

public class EntityModel
{
    public Guid Id { get; set; }
    public int? IdMapping { get; set; }
    public string EntityType { get; set; } = "Unknown";
    public GeoJsonPolygonFeature? GeoFence { get; set; }
    public GeoJsonGeometry Geometry { get; set; } = new GeoJsonGeometry();
}

public static class EntityModelExtensions
{
    public static EntityModel ToEntityModel(this EntityNode entityNode)
    {
        return new EntityModel
        {
            Id = entityNode.Id,
            IdMapping = entityNode.IdMapping,
            EntityType = entityNode.Type.Name,
            GeoFence = entityNode.GeoFence,
            Geometry = entityNode.Geometry
        };
    }
}