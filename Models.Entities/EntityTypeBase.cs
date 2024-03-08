// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities;

public class EntityTypeBase
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool SystemType { get; set; }
    public bool Visible { get; set; }
    public bool Copyable { get; set; }
    public bool Movable { get; set; }
    public GeoSpatialType? SpatialType { get; set; }
    public IEnumerable<EntityTypeSection> Sections { get; set; } = new EntityTypeSection[]
    {
        new ()
        {
            Id = Guid.Parse("6326ed66-30bf-401c-9301-9775144cf25e"),
            Name = "Active Days",
            Enabled = false,
        },
        new()
        {
            Id = Guid.Parse("2830f4dd-9dc4-492f-804a-070c44f49fac"),
            Name = "Snmp V3",
            Enabled = true,
        },
        new ()
        {
            Id = Guid.Parse("0d55dde1-cfe2-441b-9351-0d4405a69a25"),
            Name = "Communication",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("6c74ca50-69ca-4aa1-bfb8-1fbecbb53b88"),
            Name = "Controller",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("09e717e0-c78d-46ef-ace9-1710733bc32b"),
            Name = "Device Manager",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("e01326f7-5693-4c3c-964b-4ee57923be6d"),
            Name = "Entity",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("1b5e2856-06af-48e3-aaeb-88ec7019fc66"),
            Name = "FTP Credentials",
            Enabled = false,
            Sections = new EntityTypeSection[]
            {
                new ()
                {
                    Id = Guid.Parse("215f0bbe-8e17-4564-83cf-ad758b40bfeb"),
                    Name = "Username",
                    Enabled = false,
                },
                new ()
                {
                    Id = Guid.Parse("e5f340aa-f50c-4b71-a06f-ad0a3fcaf688"),
                    Name = "Password",
                    Enabled = false,
                }
            }
        },
        new ()
        {
            Id = Guid.Parse("a9af811d-8632-4797-9120-3aa447870edd"),
            Name = "Id Mapping",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("e1f5b5a5-6733-4f13-ab25-f7a2f290bd15"),
            Name = "Primary Secondary Street Names",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("446f023a-5d1b-4ed8-a926-b7fa16a1d519"),
            Name = "Bearing",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("37a322ec-ba02-4f67-b4b4-8823593900d3"),
            Name = "Speed Limit",
            Enabled = false,
        },
        new ()
        {
            Id = Guid.Parse("e0ea298f-d473-4cb4-b96d-33bf09191550"),
            Name = "Plans",
            Enabled = false,
        }
    };

    public IEnumerable<Guid> Children { get; set; }
        = Enumerable.Empty<Guid>();
}