// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;

namespace Econolite.Ode.Models.Entities.Types;

public class SystemEntityTypeProvider : EntityTypeProvider
{
    public static class SystemTypeId
    {
        public static Guid Id => Guid.Parse("a7432a6e-1569-42f9-bc7e-10397f65f6b7");
        public static string Name => "System";
    }
    
    public SystemEntityTypeProvider(IEnumerable<ISystemTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = SystemTypeId.Id,
            Name = SystemTypeId.Name,
            Icon = "<svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 99.78 96.4\"><defs><style>.cls-1-system-color{fill:#7e7e7e;}.cls-2-system-color{fill:#3f4040;}</style></defs><polygon class=\"cls-1-system-color\" points=\"99.78 96.4 99.78 37.2 78.11 37.2 78.11 84.93 71.2 84.93 71.2 17.07 52.36 17.07 52.36 0 33.52 0 33.52 84.93 26.62 84.93 26.62 50.95 0 70.74 0 96.4 99.78 96.4\"/><rect class=\"cls-2-system-color\" x=\"56.05\" y=\"24.68\" width=\"8.94\" height=\"8.94\" transform=\"translate(120.81 58.25) rotate(-180)\"/><rect class=\"cls-2-system-color\" x=\"40.1\" y=\"24.68\" width=\"8.94\" height=\"8.94\" transform=\"translate(88.92 58.25) rotate(-180)\"/><rect class=\"cls-2-system-color\" x=\"40.1\" y=\"40.63\" width=\"8.94\" height=\"8.94\" transform=\"translate(88.92 90.15) rotate(-180)\"/><rect class=\"cls-2-system-color\" x=\"84.6\" y=\"43.29\" width=\"8.94\" height=\"8.94\" transform=\"translate(177.91 95.47) rotate(-180)\"/></svg>",
            SystemType = true,
            SpatialType = GeoSpatialType.None,
            Visible = true,
            Copyable = false,
            Movable = false,
            Sections = new EntityTypeSection[]
            {
                EntityTypeSections.Entity
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
    }
}

public interface ISystemTypeChildren : IEntityTypeChildren {}