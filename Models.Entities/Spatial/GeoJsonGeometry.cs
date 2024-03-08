// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;

namespace Econolite.Ode.Models.Entities;

public class GeoJsonProperties
{
    public Guid? Intersection { get; set; }
    
    public Bearing? Bearing { get; set; }
    
    public Guid? Origin { get; set; }
    
    public Guid? Destination { get; set; }
    
    public double? SpeedLimit { get; set; }
    
    public IEnumerable<PhaseModel>? Phases { get; set; }
    
    public IEnumerable<TripPointLocation>? TripPointLocations { get; set; }

    public IEnumerable<Guid>? Intersections { get; set; }
}

public class GeoJsonGeometry : IGeoJsonGeometry
{
    public GeoSpatialType Type { get; set; } = GeoSpatialType.Point;
    public double? Radius { get; set; }
    public GeoJsonPointFeature? Point { get; set; }
    public GeoJsonLineStringFeature? LineString { get; set; }
    public GeoJsonPolygonFeature? Polygon { get; set; }
}

public class GeoJsonPointFeature
{
    public GeoSpatialType Type { get; set; } = GeoSpatialType.Point;
    public double[]? Coordinates { get; set; }
    public GeoJsonProperties? Properties { get; set; } = new GeoJsonProperties();
}

public class GeoJsonLineStringFeature
{
    public GeoSpatialType Type { get; set; } = GeoSpatialType.LineString;
    public double[][]? Coordinates { get; set; }
    public GeoJsonProperties? Properties { get; set; } = new GeoJsonProperties();
}

public class GeoJsonPolygonFeature
{
    public GeoSpatialType Type { get; set; } = GeoSpatialType.Polygon;
    public double[][][]? Coordinates { get; set; }
    public GeoJsonProperties? Properties { get; set; } = new GeoJsonProperties();
}

public static class GeoJsonExtensions
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
