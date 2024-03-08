// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using NetTopologySuite.Geometries;

namespace Econolite.Ode.Models.Entities.Spatial;

public static class GeometryExtensions
{
  public static GeoJsonPolygonFeature? ToGeoJsonPolygon(this Polygon polygon)
  {
    if (polygon.Coordinates.Length == 0)
    {
      return null;
    }
    return new GeoJsonPolygonFeature
    {
      Coordinates = polygon.Coordinates.ToGeoJsonPolygonCoordinates()
    };
  }
    
  public static Polygon ToPolygon(this GeoJsonPolygonFeature polygon)
  {
    return Geometry.DefaultFactory.CreatePolygon(polygon.Coordinates!.ToCoordinates());
  }
    
  public static LineString ToLinestring(this GeoJsonLineStringFeature json, bool reverseCoordinates = false)
  {
    if (json.Type != GeoSpatialType.LineString)
    {
      throw new ArgumentException($"Invalid Type {json.Type} should be LineString");
    }

    var coordinates = reverseCoordinates
      ? json.Coordinates?.ToCoordinates().Reverse()
      : json.Coordinates?.ToCoordinates();
    var lineString = Geometry.DefaultFactory.CreateLineString(coordinates?.ToArray());
    if (lineString.IsValid)
    {
      return lineString;
    }
    return lineString;
  }
  
  public static Point ToPoint(this GeoJsonPointFeature json)
  {
    if (json.Type != GeoSpatialType.Point)
    {
      throw new ArgumentException($"Invalid Type {json.Type} should be Point");
    }

    return new Point(json.Coordinates?.ToCoordinate());
  }
  
  public static Point ToPoint(this GeoJsonPointFeature json, EpsgCode code)
  {
    if (json.Type != GeoSpatialType.Point)
    {
      throw new ArgumentException($"Invalid Type {json.Type} should be Point");
    }

    return new Point(json.Coordinates?.ToCoordinate());
  }
  
  public static GeoJsonPointFeature ToGeoJsonPoint(this Point point)
  {
    return new GeoJsonPointFeature
    {
      Coordinates = point.Coordinate.ToCoordinate()
    };
  }
  
  public static Polygon ToGeoFence(this GeoJsonPointFeature json, double radius)
  {
    return json.ToPoint().Buffer(radius) as Polygon ?? Polygon.Empty;
  }
  
  private static Coordinate[] ToCoordinates(this double[][][] coordinates)
  {
    return coordinates.Select(v => v.ToCoordinates()).SelectMany(c => c).ToArray();
  }
    
  private static Coordinate[] ToCoordinates(this double[][] coordinates)
  {
    return coordinates.Select(v => v.ToCoordinate()).ToArray();
  }

  private static Coordinate ToCoordinate(this double[] coordinate)
  {
    ArgumentNullException.ThrowIfNull(coordinate);
    if (coordinate.Length < 2)
    {
      throw new ArgumentException($"Wrong length for a coordinate {coordinate.Length} should be at least 2");
    }

    return new Coordinate(coordinate[0], coordinate[1]);
  }

  private static Coordinate ToCoordinate(this double[] coordinate, EpsgCode code)
  {
    ArgumentNullException.ThrowIfNull(coordinate);
    if (coordinate.Length < 2)
    {
      throw new ArgumentException($"Wrong length for a coordinate {coordinate.Length} should be at least 2");
    }

    return new Coordinate(coordinate[0], coordinate[1]);
  }
  
  private static double[] ToCoordinate(this Coordinate coordinate)
  {
    return new[] {coordinate.X, coordinate.Y};
  }
  
  private static double[][][] ToGeoJsonPolygonCoordinates(this Coordinate[] coordinates)
  {
    var result = coordinates.Select(c => new[] {c.X, c.Y}).ToArray();
    return new[] {result};
  }
}