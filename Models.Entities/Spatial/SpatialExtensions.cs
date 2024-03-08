// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Runtime.CompilerServices;
using GeoCoordinatePortable;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Econolite.Ode.Models.Entities.Spatial;

public static class SpatialExtensions
{
    private const double LonMin = -180.0;
    private const double LonMax = 180.0;
    private const double LatMin = -90.0;
    private const double LatMax = 90.0;
    private const double Radius = 6378136.98;

    public static Coordinate ToCoordinate(this (double x, double y) from, EpsgCode code = EpsgCode.Wgs84) =>
        new Coordinate(from.x, from.y);

    public static (double lon, double lat) ToTuple(this Coordinate from) => new(from.X, from.Y);

    public static Coordinate ToSphericalMercator(this (double lon, double lat) from)
    {
        if (from.lon is > LonMax or < LonMin || from.lat is > LatMax or < LatMin)
        {
            DefaultInterpolatedStringHandler
                interpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
            interpolatedStringHandler.AppendLiteral("Longitude: ");
            interpolatedStringHandler.AppendFormatted(from.lon);
            interpolatedStringHandler.AppendLiteral(", Latitude: ");
            interpolatedStringHandler.AppendFormatted(from.lat);
            interpolatedStringHandler.AppendLiteral(" is out of range.");
            throw new ArgumentOutOfRangeException(interpolatedStringHandler.ToStringAndClear());
        }

        return SpatialExtensions.LatLonToSphericalMercator(from.lon, from.lat).ToCoordinate();
    }

    public static LineString ToSphericalMercator(this LineString from) =>
        from.Coordinates
            .Select<Coordinate, Coordinate>(c => (c.X, c.Y).ToSphericalMercator())
            .ToLineString();

    public static LineString ToLatLon(this LineString from) =>
        from.Coordinates
            .Select<Coordinate, Coordinate>(c => (c.X, c.Y).ToLatLon())
            .ToLineString(4326);

    public static Polygon ToSphericalMercator(this Polygon from) =>
        from.Coordinates
            .Select<Coordinate, Coordinate>(c => (c.X, c.Y).ToSphericalMercator())
            .ToPolygon();

    public static Polygon ToLatLon(this Polygon from) => from.Coordinates
        .Select<Coordinate, Coordinate>(c => (c.X, c.Y).ToLatLon())
        .ToPolygon(4326);

    public static (double Latitude, double Longitude) ToLatLon(this Point from)
    {
        (double lon, double lat) lonLat = SpatialExtensions.SphericalMercatorToLonLat(from.X, from.Y);
        return (lonLat.lat, lonLat.lon);
    }

    public static Coordinate ToLatLon(this (double x, double y) from) =>
        SpatialExtensions.SphericalMercatorToLonLat(from.x, from.y).ToCoordinate();

    private static (double lon, double lat) SphericalMercatorToLonLat(double x, double y) => (
        SpatialExtensions.RadToDeg(x / Radius),
        SpatialExtensions.RadToDeg(Math.Atan(Math.Exp(y / 6378136.98)) * 2.0 - Math.PI / 2.0));

    private static (double x, double y) LatLonToSphericalMercator(double lon, double lat) => (
        SpatialExtensions.DegToRad(lon * Radius),
        Math.Log(Math.Tan(SpatialExtensions.DegToRad(lat) / 2.0 + Math.PI / 4.0)) * 6378136.98);

    private static double DegToRad(double a) => a / (180.0 / Math.PI);

    private static double RadToDeg(double a) => a * (180.0 / Math.PI);

    public static LineString ToLineStringFromDirection(
        this Coordinate from,
        string bearing)
    {
        Coordinate coordinate = new Coordinate();
        switch (bearing)
        {
            case "EB":
                coordinate = from.ToEastBound();
                break;
            case "NB":
                coordinate = from.ToNorthBound();
                break;
            case "NEB":
                coordinate = from.ToNortheastBound();
                break;
            case "NWB":
                coordinate = from.ToNorthwestBound();
                break;
            case "SB":
                coordinate = from.ToSouthBound();
                break;
            case "SEB":
                coordinate = from.ToSoutheastBound();
                break;
            case "SWB":
                coordinate = from.ToSouthwestBound();
                break;
            case "WB":
                coordinate = from.ToWestBound();
                break;
        }

        return new[]
        {
            from,
            coordinate
        }.ToLineString();
    }

    public static Point ToGeometry(this (double lon, double lat) from)
    {
        if (from.lon > 180.0 || from.lon < -180.0 || from.lat > 90.0 || from.lat < -90.0)
        {
            DefaultInterpolatedStringHandler
                interpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
            interpolatedStringHandler.AppendLiteral("Longitude: ");
            interpolatedStringHandler.AppendFormatted(from.lon);
            interpolatedStringHandler.AppendLiteral(", Latitude: ");
            interpolatedStringHandler.AppendFormatted(from.lat);
            interpolatedStringHandler.AppendLiteral(" is out of range.");
            throw new ArgumentOutOfRangeException(interpolatedStringHandler.ToStringAndClear());
        }

        return (from.lon, from.lat).ToSphericalMercator().ToPoint();
    }

    private static Point ToPoint(this Coordinate from)
    {
        Point point = Geometry.DefaultFactory.CreatePoint(from);
        point.SRID = 3857;
        return point;
    }

    public static LineString ToLineString(this IEnumerable<Coordinate> from, int srid = 3857) =>
        Geometry.DefaultFactory.WithSRID(srid).CreateLineString(from.ToArray());

    public static Polygon ToPolygon(this IEnumerable<Coordinate> from, int srid = 3857) =>
        Geometry.DefaultFactory.WithSRID(srid).CreatePolygon(from.ToArray());

    public static Coordinate ToNorthBound(this Coordinate from, double lengthInMeters = 100.0) =>
        new Coordinate(from.X, from.Y - lengthInMeters);

    public static Coordinate ToNortheastBound(this Coordinate from, double lengthInMeters = 100.0)
    {
        double radians = SpatialExtensions.DegreesToRadians(45.0);
        return new Coordinate(from.X - Math.Cos(radians) * lengthInMeters,
            from.Y - Math.Sin(radians) * lengthInMeters);
    }

    public static Coordinate ToNorthwestBound(this Coordinate from, double lengthInMeters = 100.0)
    {
        double radians = SpatialExtensions.DegreesToRadians(315.0);
        return new Coordinate(from.X + Math.Cos(radians) * lengthInMeters,
            from.Y + Math.Sin(radians) * lengthInMeters);
    }

    public static Coordinate ToSouthBound(this Coordinate from, double lengthInMeters = 100.0) =>
        new Coordinate(from.X, from.Y + lengthInMeters);

    public static Coordinate ToSoutheastBound(this Coordinate from, double lengthInMeters = 100.0)
    {
        double radians = SpatialExtensions.DegreesToRadians(135.0);
        return new Coordinate(from.X + Math.Cos(radians) * lengthInMeters,
            from.Y + Math.Sin(radians) * lengthInMeters);
    }

    public static Coordinate ToSouthwestBound(this Coordinate from, double lengthInMeters = 100.0)
    {
        double radians = SpatialExtensions.DegreesToRadians(225.0);
        return new Coordinate(from.X - Math.Cos(radians) * lengthInMeters,
            from.Y - Math.Sin(radians) * lengthInMeters);
    }

    public static Coordinate ToEastBound(this Coordinate from, double lengthInMeters = 100.0) =>
        new Coordinate(from.X - lengthInMeters, from.Y);

    public static Coordinate ToWestBound(this Coordinate from, double lengthInMeters = 100.0) =>
        new Coordinate(from.X + lengthInMeters, from.Y);

    public static string ToGeoJson(this Geometry geom) => new GeoJsonWriter().Write(geom);

    public static Geometry ToGeometry(this string geoJson) => string.IsNullOrEmpty(geoJson)
        ? LineString.Empty
        : new GeoJsonReader().Read<Geometry>(geoJson);

    public static (Bearing Bearing, string? Error) GetBearing(
        this Geometry geom,
        bool intersectionOnStartNode)
    {
        if (!geom.IsValid || geom.GeometryType != "LineString")
            return (Bearing.Unknown, "Invalid Geometry must be a LineString");
        if (geom.Coordinates != null)
        {
            Coordinate? end = geom.Coordinates.FirstOrDefault<Coordinate>();
            Coordinate? start = geom.Coordinates.LastOrDefault<Coordinate>();
            if (!intersectionOnStartNode)
            {
                end = geom.Coordinates.LastOrDefault<Coordinate>();
                start = geom.Coordinates.FirstOrDefault<Coordinate>();
            }

            if (end != null && start != null)
                return (GetBearing(GetAngle(end, start)), null);
        }

        return (Bearing.Unknown, null);
    }

    public static double ToRadians(this double degrees)
    {
        return (Math.PI / 180) * degrees;
    }

    public static double ToDegrees(this double radians)
    {
        return (180 / Math.PI) * radians;
    }

    public static double DistanceTo(this Coordinate origin, Coordinate destination)
    {
        if (double.IsNaN(origin.Y) || double.IsNaN(origin.X) || double.IsNaN(destination.Y) ||
            double.IsNaN(destination.X))
            throw new ArgumentException("Argument latitude or longitude is not a number");
        double d1 = origin.Y.ToRadians();
        double num1 = origin.X.ToRadians();
        double d2 = destination.Y.ToRadians();
        double num2 = destination.X.ToRadians() - num1;
        double d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                    Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        return EarthRadius * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }

    public static Coordinate ToCoordinate(this double[] from, EpsgCode code = EpsgCode.Wgs84) =>
        new(from[0], from[1]);

    public static TripPointLocation NearestTo(this Coordinate origin, IEnumerable<TripPointLocation> locations)
    {
        return locations.Select(l => (l.Point.ToCoordinate().DistanceTo(origin), l)).OrderBy(c => c.Item1)
            .FirstOrDefault().l;
    }

    public static Bearing ToOppositeBearing(this Bearing bearing)
    {
        return bearing switch
        {
            Bearing.NB => Bearing.SB,
            Bearing.SB => Bearing.NB,
            Bearing.EB => Bearing.WB,
            Bearing.WB => Bearing.EB,
            Bearing.NEB => Bearing.SWB,
            Bearing.SWB => Bearing.NEB,
            Bearing.NWB => Bearing.SEB,
            Bearing.SEB => Bearing.NWB,
            _ => Bearing.Unknown
        };
    }

    public static Bearing ToBearing(this double[] start, double[] end)
    {
        if (start.Length != 2 || end.Length != 2)
            return Bearing.Unknown;
        return GetBearing(start.ToCoordinate().HeadingTo(end.ToCoordinate()));
    }

    public static Bearing ToPerpendicularBearing(this double[] start, double[] end)
    {
        if (start.Length != 2 || end.Length != 2)
            return Bearing.Unknown;
        var angle = start.ToCoordinate().HeadingTo(end.ToCoordinate());
        var resultingAngle = angle <= 270 ? (angle + 90) : (angle - 270);
        return GetBearing(resultingAngle);
    }

    public static bool IsMatchingBearings(this double[] start, double[] end, Bearing toMatch)
    {
        if (toMatch == Bearing.Unknown)
            return true;
        if (start.Length != 2 || end.Length != 2)
            return false;
        var angle = start.ToCoordinate().HeadingTo(end.ToCoordinate());
        var bearing1 = GetBearing(angle);
        var bearing2 = GetBearing(angle >= 45 ? angle - 45 : angle + 315);
        var bearing3 = GetBearing(angle >= 315 ? angle + 45 : angle - 315);
        return bearing1 == toMatch || bearing2 == toMatch || bearing3 == toMatch;
    }

    public static double HeadingTo(this Coordinate start, Coordinate end)
    {
        return start.BearingTo(end).ToDegrees();
    }

    public static double BearingTo(this Coordinate start, Coordinate end)
    {
        double rdl = DegreesToRadians(end.X - start.X);
        double rsy = DegreesToRadians(start.Y);
        double rey = DegreesToRadians(end.Y);
        double x = Math.Cos(rsy) * Math.Sin(rey) - Math.Sin(rsy) * Math.Cos(rey) * Math.Cos(rdl);
        double y = Math.Sin(rdl) * Math.Cos(rey);

        return (Math.Atan2(y, x) + Math.PI * 2) % (Math.PI * 2);
    }

    private static int GetAngle(Coordinate end, Coordinate start)
    {
        double x = end.X - start.X;
        return (int) ((90.0 - Math.Atan2(end.Y - start.Y, x) * (180.0 / Math.PI) + 360.0) % 360.0);
    }

    private static double DegreesToRadians(double degrees) => Math.PI * degrees / 180.0;

    private static Bearing GetBearing(double angle) => angle < 337.5 && angle >= 22.5
        ? (angle < 22.5 || angle >= 67.5
            ? (angle < 67.5 || angle >= 112.5
                ? (angle < 112.5 || angle >= 157.5
                    ? (angle < 157.5 || angle >= 202.5
                        ? (angle < 202.5 || angle >= 247.5
                            ? (angle < 247.5 || angle >= 292.5 ? Bearing.NWB : Bearing.WB)
                            : Bearing.SWB)
                        : Bearing.SB)
                    : Bearing.SEB)
                : Bearing.EB)
            : Bearing.NEB)
        : Bearing.NB;

    public static GeoCoordinate ToGeoCoordinate(this (double x, double y) from)
    {
        return new GeoCoordinate(from.y, from.x);
    }

    public static double ToLength(this LineString line)
    {
        var coordinates = line.Coordinates.Length - 1;
        return line.Coordinates.Select((c, i) => i < coordinates ? c.DistanceTo(line.Coordinates[i + 1]) : 0.0)
            .Sum();
    }

    public static IEnumerable<(string Intersection, PhaseModel Plan)> ToOrderedIntersectionPhase(
        this IEnumerable<EntityNode> approaches, GeoJsonLineStringFeature route)
    {
        var tripPoints = route.ToTripPointLocations(25, false);
        var results = approaches
            .Select(i => (
                i.Geometry?.LineString
                    ?.ToTripPointLocation(tripPoints), i))
            .Where(t => t.Item1 != null)
            .Cast<(TripPointLocation, EntityNode)>()
            .OrderBy(t => t.Item1.Distance)
            .GroupBy(c => c.Item2.Geometry.LineString?.Properties?.Intersection.ToString() ?? String.Empty)
            .Where(g => g.Key != String.Empty)
            .Select(g => g.ToPlanOrPhase())
            .Where(oa => oa.Item2 != null)
            .Cast<(string, PhaseModel)>();
        return results;
    }

    public static (string Key, PhaseModel? phase) ToPlanOrPhase(
        this IGrouping<string, ValueTuple<TripPointLocation, EntityNode>> group)
    {
        var phase = group.ToArray().ToPlanOrPhase();
        return (group.Key, phase);
    }

    public static PhaseModel? ToPlanOrPhase(this IEnumerable<ValueTuple<TripPointLocation, EntityNode>> values)
    {
        PhaseModel? result = null;
        var valueTuples = values as (TripPointLocation, EntityNode)[] ?? values.ToArray();
        if (!valueTuples.Any())
        {
            return null;
        }

        var approaches = valueTuples.Select(v => (v.Item1, v.Item2.Geometry.LineString?.Properties));

        var enumerable = approaches as (TripPointLocation, GeoJsonProperties?)[] ?? approaches.ToArray();
        if (enumerable.Count() == 1)
        {
            var approachPropertiesModel = enumerable.FirstOrDefault().Item2;
            if (approachPropertiesModel is {Phases: { }})
            {
                result = approachPropertiesModel.Phases.ToPlanOrPhasePriority();
            }
        }
        else
        {
            var ingress = enumerable.First();
            var egress = enumerable.Last();

            if (ingress.Item2 != null && egress.Item2 != null)
            {
                result = ingress.Item2.ToPlanOrPhaseModel(egress.Item2);
            }
        }

        return result;
    }

    public static PhaseModel ToPlanOrPhase(this IEnumerable<PhaseModel> models)
    {
        var phaseModels = models as PhaseModel[] ?? models.ToArray();
        var hasThru = phaseModels.Any(m => m.Movement == "Thru");
        if (hasThru)
        {
            return phaseModels.First(m => m.Movement == "Thru");
        }

        var hasLeft = phaseModels.Any(m => m.Movement == "Left");
        if (hasLeft)
        {
            return phaseModels.First(m => m.Movement == "Left");
        }

        return phaseModels.First();
    }

    public static PhaseModel ToPlanOrPhasePriority(this IEnumerable<PhaseModel> models)
    {
        var phaseModels = models as PhaseModel[] ?? models.ToArray();
        var hasThru = phaseModels.Any(m => m.Movement == "Thru");
        if (hasThru)
        {
            return phaseModels.First(m => m.Movement == "Thru");
        }

        var hasLeft = phaseModels.Any(m => m.Movement == "Left");
        if (hasLeft)
        {
            return phaseModels.First(m => m.Movement == "Left");
        }

        return phaseModels.First();
    }

    public static PhaseModel ToPlanOrPhaseModel(this IEnumerable<PhaseModel> models, string direction)
    {
        var phaseModels = models as PhaseModel[] ?? models.ToArray();
        PhaseModel? result = phaseModels.FirstOrDefault(m => m.Movement == direction);
        if (result != null)
        {
            return result;
        }

        var hasThru = phaseModels.Any(m => m.Movement == "Thru");
        if (hasThru)
        {
            return phaseModels.First(m => m.Movement == "Thru");
        }

        var hasLeft = phaseModels.Any(m => m.Movement == "Left");
        if (hasLeft)
        {
            return phaseModels.First(m => m.Movement == "Left");
        }

        return phaseModels.First();
    }

    public static PhaseModel ToPlanOrPhaseModel(this GeoJsonProperties ingress,
        GeoJsonProperties egress)
    {
        var direction = "Thru";
        switch (ingress.Bearing)
        {
            case Bearing.NB:
                switch (egress.Bearing)
                {
                    case Bearing.WB:
                        direction = "Right";
                        break;
                    case Bearing.EB:
                        direction = "Left";
                        break;
                }

                break;
            case Bearing.SB:
                switch (egress.Bearing)
                {
                    case Bearing.WB:
                        direction = "Left";
                        break;
                    case Bearing.EB:
                        direction = "Right";
                        break;
                }

                break;
            case Bearing.WB:
                switch (egress.Bearing)
                {
                    case Bearing.NB:
                        direction = "Right";
                        break;
                    case Bearing.SB:
                        direction = "Left";
                        break;
                }

                break;
            case Bearing.EB:
                switch (egress.Bearing)
                {
                    case Bearing.NB:
                        direction = "Left";
                        break;
                    case Bearing.SB:
                        direction = "Right";
                        break;
                }

                break;
        }

        if (ingress.Phases != null) return ingress.Phases.ToPlanOrPhaseModel(direction);
        return new PhaseModel();
    }

    public static TripPointLocation ToTripPointLocation(this GeoJsonLineStringFeature json,
        IEnumerable<TripPointLocation> locations)
    {
        var coordinate = json.Coordinates![0].ToCoordinate();
        var orderByDistance = locations.Select(l => (l.Point.ToCoordinate().DistanceTo(coordinate), l))
            .OrderBy(l => l.Item1);
        return orderByDistance.FirstOrDefault().l;
    }

    public static TripPointLocation ToTripPointLocation(this GeoJsonPointFeature json,
        IEnumerable<TripPointLocation> locations)
    {
        var coordinate = json.Coordinates!.ToCoordinate();
        var orderByDistance = locations.Select(l => (l.Point.ToCoordinate().DistanceTo(coordinate), l))
            .OrderBy(l => l.Item1);
        return orderByDistance.FirstOrDefault().l;
    }

    public static IEnumerable<TripPointLocation> ToTripPointLocations(this GeoJsonLineStringFeature json,
        double distanceInFeet, bool reverseCoordinates = true)
    {
        return json.ToLinestring(reverseCoordinates).ToTripPointLocations(distanceInFeet);
    }

    public static IEnumerable<TripPointLocation> ToTripPointLocations(this LineString line, double distanceInFeet)
    {
        var distanceInMeters = ConvertFeetToMeters(distanceInFeet);
        var lineLength = ConvertMetersToFeet(line.ToLength());
        var totalLocations = (int) (lineLength / distanceInFeet) + 1;
        var segments = line.ToSplitDistanceSegments().ToList();
        var results = Enumerable.Range(0, totalLocations).Select(d =>
        {
            var distance = d * distanceInMeters;
            var id = (int) (d * distanceInFeet);
            var coordinate = d == 0 ? line.Coordinates.First() : segments.ToCoordinate(distance);
            var location = new TripPointLocation(id, new[] {coordinate?.X ?? 0, coordinate?.Y ?? 0});

            return location;
        });

        return results;
    }

    public static double FeetInMeters = 3.28084;

    public static double ConvertMilesToMeters(double miles)
    {
        return miles * 1609.34;
    }
    
    public static double ConvertMilesToFeet(double miles)
    {
        return miles * 5280;
    }
    
    public static double ConvertMetersToFeet(double meters)
    {
        return meters * FeetInMeters;
    }

    public static double ConvertFeetToMeters(double feet)
    {
        return feet / FeetInMeters;
    }

    public static Coordinate ToCoordinate(this LineString line, double distanceMeters)
    {
        return line.ToSplitDistanceSegments().ToList().ToCoordinate(distanceMeters);
    }

    private static IEnumerable<(int index, double total, double heading, Coordinate origin)>
        ToSplitDistanceSegments(this LineString line)
    {
        var coordinates = line.Coordinates.Length - 1;
        var runningTotal = 0.0;
        return line.Coordinates.Select<Coordinate, (int index, double total, double heading, Coordinate origin)>(
                (c, i) =>
                {
                    (int index, double total, double heading, Coordinate origin) result = (0, 0, 0, null)!;
                    if (i < coordinates)
                    {
                        runningTotal += c.DistanceTo(line.Coordinates[i + 1]);
                        result = (i, runningTotal,
                            c.HeadingTo(line.Coordinates[i + 1]), c);
                    }

                    return result;
                })
            .Where(v => v != (0, 0, 0, null)).ToArray();
    }

    private static Coordinate ToCoordinate(
        this List<(int index, double total, double heading, Coordinate origin)> pairs, double distanceMeters)
    {
        var index = pairs.First(p => p.total > distanceMeters).index;

        if (index == 0)
        {
            return pairs[index].origin.ToCoordinate(distanceMeters, pairs[0].heading);
        }

        return pairs[index].origin.ToCoordinate(distanceMeters - pairs[index - 1].total, pairs[index].heading);
    }

    public static readonly double EarthRadius = 6378160; //#Radius of the Earth meters

    public static Coordinate ToCoordinate(this Coordinate from, double distanceMeters, double heading)
    {
        double bearingR = heading.ToRadians();

        double latR = from.Y.ToRadians();
        double lonR = from.X.ToRadians();

        double distanceToRadius = distanceMeters / EarthRadius;

        double newLatR = Math.Asin(Math.Sin(latR) * Math.Cos(distanceToRadius)
                                   + Math.Cos(latR) * Math.Sin(distanceToRadius) * Math.Cos(bearingR));

        double newLonR = lonR + Math.Atan2(
            Math.Sin(bearingR) * Math.Sin(distanceToRadius) * Math.Cos(latR),
            Math.Cos(distanceToRadius) - Math.Sin(latR) * Math.Sin(newLatR)
        );

        return new Coordinate(newLonR.ToDegrees(), newLatR.ToDegrees());
    }

    // public static bool Intersects(this Polygon geofence, Coordinate coordinate)
    // {
    //     return geofence.Intersects(coordinate);
    // }

    // public static Coordinate ToCoordinate(this Coordinate from, double easternOffsetDecimeters, double northernOffsetDecimeters, double heading)
    // {
    //     double bearingR = heading.ToRadians();
    //
    //     double latR = from.Y.ToRadians();
    //     double lonR = from.X.ToRadians();
    //
    //     double distanceToRadiusLat = DecimetersToMeters(northernOffsetDecimeters) / EarthRadius;
    //     double distanceToRadiusLon = DecimetersToMeters(easternOffsetDecimeters) / EarthRadius;
    //
    //     double newLatR = Math.Asin(Math.Sin(latR) * Math.Cos(distanceToRadiusLat)
    //                                + Math.Cos(latR) * Math.Sin(distanceToRadiusLat) * Math.Cos(bearingR));
    //
    //     double newLonR = lonR + Math.Atan2(
    //         Math.Sin(bearingR) * Math.Sin(distanceToRadius) * Math.Cos(latR),
    //         Math.Cos(distanceToRadius) - Math.Sin(latR) * Math.Sin(newLatR)
    //     );
    //
    //     return new Coordinate(newLonR.ToDegrees(), newLatR.ToDegrees());
    // }

    public static double DecimetersToMeters(double decimeters)
    {
        return decimeters / 10;
    }
}