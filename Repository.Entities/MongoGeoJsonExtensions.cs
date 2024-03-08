// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using MongoDB.Driver.GeoJsonObjectModel;

namespace Econolite.Ode.Repository.Entities;

public static class MongoGeoJsonExtensions
{
    public static GeoJson2DCoordinates[] ToMongoCoordinates(this double[][] coordinates)
    {
        return coordinates.Select(c => c.ToMongoCoordinates()).ToArray();
    }

    public static GeoJson2DCoordinates ToMongoCoordinates(this double[] coordinate)
    {
        return GeoJson.Position(coordinate[0], coordinate[1]);
    }
}