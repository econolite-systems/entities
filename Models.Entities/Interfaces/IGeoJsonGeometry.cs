// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities;

public interface IGeoJsonType
{
    string GeoType { get; set; }
}

public interface IGeoJsonGeometry
{
    GeoSpatialType Type { get; set; }
    GeoJsonPointFeature? Point { get; set; }
    GeoJsonLineStringFeature? LineString { get; set; }
    GeoJsonPolygonFeature? Polygon { get; set; }
}

public interface IGeoJsonGeoFence
{
    string Type { get; set; }
    
    double[][][] Coordinates { get; set; }
}