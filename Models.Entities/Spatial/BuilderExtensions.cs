// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Union;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace Econolite.Ode.Models.Entities.Spatial;

public static class BuilderExtensions
{
    public static Polygon CreateBuffer(this LineString lineString, double distance) =>
        BufferOp.Buffer((Geometry) lineString, distance) as Polygon ?? Polygon.Empty;

    public static Polygon CreateBuffer(this Point point, double distance) =>
        BufferOp.Buffer((Geometry) point, distance) as Polygon ?? Polygon.Empty;

    public static GeoJsonPolygonFeature CreateBufferInMiles(this GeoJsonLineStringFeature lineString, double distance) =>
        lineString.ToLinestring().CreateBufferInMeters(SpatialExtensions.ConvertMilesToMeters(distance)).ToGeoJsonPolygon();
    
    public static GeoJsonPolygonFeature CreateBufferInMiles(this GeoJsonPointFeature point, double distance) =>
        point.ToPoint().CreateBufferInMeters(SpatialExtensions.ConvertMilesToMeters(distance)).ToGeoJsonPolygon();
    
    public static GeoJsonPolygonFeature CreateBufferInFeet(this GeoJsonLineStringFeature lineString, double distance) =>
        lineString.ToLinestring().CreateBufferInMeters(SpatialExtensions.ConvertFeetToMeters(distance)).ToGeoJsonPolygon();
    
    public static GeoJsonPolygonFeature CreateBufferInFeet(this GeoJsonPointFeature point, double distance) =>
        point.ToPoint().CreateBufferInMeters(SpatialExtensions.ConvertFeetToMeters(distance)).ToGeoJsonPolygon();
    
    public static GeoJsonPolygonFeature CreateBufferInFeet(this GeoJsonPolygonFeature polygon, double distance) => 
        polygon.ToPolygon().CreateBufferInMeters(SpatialExtensions.ConvertFeetToMeters(distance)).ToGeoJsonPolygon();
    
    public static GeoJsonPolygonFeature CreateBufferInFeet(this IEnumerable<GeoJsonPolygonFeature> polygons, double distance) =>
        polygons.ToGeometry().CreateBufferInMeters(SpatialExtensions.ConvertFeetToMeters(distance)).ToGeoJsonPolygon();
    
    public static GeoJsonPolygonFeature? Union(this IEnumerable<GeoJsonPolygonFeature> polygons)
    {
        var polygonList = polygons.Select(p => p.ToPolygon()).ToArray();
        var geometry = UnaryUnionOp.Union(polygonList) as Polygon;
        return geometry?.ToGeoJsonPolygon();
    }
    
    public static Geometry ToGeometry(this IEnumerable<GeoJsonPolygonFeature> polygons)
    {
        var polygonList = polygons.Select(p => p.ToPolygon()).ToArray();
        var geometry = UnaryUnionOp.Union(polygonList);
        return geometry;
    }

    public static Polygon CreateBufferInMeters(this Geometry geometry, double distance)
    {
        var valid = geometry.IsValid;
        var projectGeometry = ProjectGeometry(geometry, WKT4326, WKT3857);
        var result = BufferOp.Buffer(projectGeometry, distance) as Polygon;
        var buffer = ProjectGeometry(result, WKT3857, WKT4326) as Polygon;
        return buffer ?? Polygon.Empty;
    }
    
    public static Polygon CreateBufferInMeters(this LineString lineString, double distance)
    {
        var projectGeometry = ProjectGeometry(lineString, WKT4326, WKT3857);
        var buffer = ProjectGeometry(BufferOp.Buffer(projectGeometry, distance), WKT3857, WKT4326) as Polygon;
        if (buffer != null && buffer.Coordinates[0].X != buffer.Coordinates[buffer.Coordinates.Length - 1].X &&
            buffer.Coordinates[0].Y != buffer.Coordinates[buffer.Coordinates.Length - 1].Y)
        {
            return Polygon.Empty;
        }
        return buffer ?? Polygon.Empty;
    }

    public static Polygon CreateBufferInMeters(this Point point, double distance)
    {
        var projectGeometry = ProjectGeometry(point, WKT4326, WKT3857);
        var buffer = ProjectGeometry(BufferOp.Buffer(projectGeometry, distance), WKT3857, WKT4326) as Polygon;
        return buffer ?? Polygon.Empty;
    }

    public static Geometry ProjectGeometry(Geometry geom,
        string fromWKT, string toWKT)
    {
        var sourceCoordSystem = new CoordinateSystemFactory().CreateFromWkt(fromWKT);
        var targetCoordSystem = new CoordinateSystemFactory().CreateFromWkt(toWKT);

        var trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(sourceCoordSystem,
            targetCoordSystem);

        var projGeom = Transform(geom, trans.MathTransform);

        return projGeom;
    }

    static Geometry Transform(Geometry geom,
        MathTransform transform)
    {
        geom = geom.Copy();
        geom.Apply(new MTF(transform));
        return geom;
    }

    sealed class MTF : ICoordinateSequenceFilter
    {
        private readonly MathTransform _mathTransform;

        public MTF(MathTransform mathTransform) => _mathTransform = mathTransform;

        public bool Done => false;
        public bool GeometryChanged => true;

        public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i)
        {
            double x = seq.GetX(i);
            double y = seq.GetY(i);
            double z = seq.GetZ(i);
            _mathTransform.Transform(ref x, ref y, ref z);
            seq.SetX(i, x);
            seq.SetY(i, y);
            seq.SetZ(i, z);
        }
    }

    private static string WKT3857 =
        "PROJCS[\"WGS 84 / Pseudo-Mercator\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION[\"Mercator_1SP\"],PARAMETER[\"central_meridian\",0],PARAMETER[\"scale_factor\",1],PARAMETER[\"false_easting\",0],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH],EXTENSION[\"PROJ4\",\"+proj=merc +a=6378137 +b=6378137 +lat_ts=0 +lon_0=0 +x_0=0 +y_0=0 +k=1 +units=m +nadgrids=@null +wktext +no_defs\"],AUTHORITY[\"EPSG\",\"3857\"]]";

    private static string WKT4326 =
        "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";
}