// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Collections.Generic;
using System.Text.Json;
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Spatial;
using FluentAssertions;
using Xunit;

namespace Models.Entities.Test.Spatial;

public class CreatingGeoBoundsTest
{
    [Fact]
    public void CreateGeoBounds_WithValidCoordinates_ReturnsGeoBounds()
    {
        // Arrange
        var lat = 42.52123686553108;
        var lon = -83.04755588046585;
        var pointFeature = new GeoJsonPointFeature() {Coordinates = new double[] {lon, lat}};
        // Act
        var result = pointFeature.CreateBufferInMiles(5);
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateGeoBounds_WithMultiplePolygons_ReturnsGeoBounds()
    {
        // Arrange
        var lat1 = 42.52123686553108;
        var lon1 = -83.04755588046585;
        var pointFeature1 = new GeoJsonPointFeature() {Coordinates = new double[] {lon1, lat1}};
        var lat2 = 42.52097488207045;
        var lon2 = -83.04754517775392;
        var pointFeature2 = new GeoJsonPointFeature() {Coordinates = new double[] {lon2, lat2}};
        var result1 = pointFeature1.CreateBufferInFeet(80);
        var result2 = pointFeature2.CreateBufferInFeet(80);
        // Act
        var result = new List<GeoJsonPolygonFeature> {result1, result2}.Union();
        // Assert
        result.Should().NotBeNull();
    }
}