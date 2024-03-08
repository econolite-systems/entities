// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.Entities.Interfaces;

namespace Econolite.Ode.Models.Entities.Types;

public static class CorridorTypeId
{
    public static Guid Id => Guid.Parse("4fcb475e-72a3-4b71-a66d-fa6629129137");
    public static string Name => "Corridor";
}

public class CorridorEntityTypeProvider : EntityTypeProvider, ISystemTypeChildren
{
    public CorridorEntityTypeProvider(IEnumerable<ICorridorTypeChildren>? typeChildren)
    {
        Type = new EntityType()
        {
            Id = CorridorTypeId.Id,
            Name = CorridorTypeId.Name,
            Icon = "<svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 85.09\"><defs><style>.cls-1-section-color{fill:#3f4040;}.cls-2-section-color{fill:#bfbebe;}.cls-3-section-color{fill:#323031;}.cls-4-section-color{fill:#bf5555;}.cls-5-section-color{fill:#c0b454;}.cls-6-section-color{fill:#57bc82;}</style></defs><path class=\"cls-1-section-color\" d=\"M66.52,72c0,.33-.88.61-2,.61H35.85c-1.09,0-2-.28-2-.61V52.74c0-.34.89-.61,2-.61H64.54c1.1,0,2,.27,2,.61Z\" transform=\"translate(0 -0.05)\"/><path class=\"cls-1-section-color\" d=\"M66.52,32.6c0,.34-.88.61-2,.61H35.85c-1.09,0-2-.27-2-.61V13.3c0-.33.89-.61,2-.61H64.54c1.1,0,2,.28,2,.61Z\" transform=\"translate(0 -0.05)\"/><rect class=\"cls-2\" x=\"36.88\" y=\"60.12\" width=\"7.39\" height=\"4.44\"/><rect class=\"cls-2-section-color\" x=\"46.5\" y=\"60.12\" width=\"7.4\" height=\"4.44\"/><rect class=\"cls-2-section-color\" x=\"56.11\" y=\"60.12\" width=\"7.39\" height=\"4.44\"/><rect class=\"cls-2-section-color\" x=\"36.88\" y=\"20.69\" width=\"7.39\" height=\"4.44\"/><rect class=\"cls-2-section-color\" x=\"46.5\" y=\"20.69\" width=\"7.4\" height=\"4.44\"/><rect class=\"cls-2-section-color\" x=\"56.11\" y=\"20.69\" width=\"7.39\" height=\"4.44\"/><path class=\"cls-3-section-color\" d=\"M39,82.6a2.45,2.45,0,0,1-2.36,2.54H2.37A2.46,2.46,0,0,1,0,82.6v-80A2.45,2.45,0,0,1,2.37.05H36.66A2.45,2.45,0,0,1,39,2.59Z\" transform=\"translate(0 -0.05)\"/><circle class=\"cls-4-section-color\" cx=\"19.51\" cy=\"18.39\" r=\"10.32\"/><path class=\"cls-5-section-color\" d=\"M29.83,42.59A10.32,10.32,0,1,1,19.51,32.27,10.32,10.32,0,0,1,29.83,42.59Z\" transform=\"translate(0 -0.05)\"/><path class=\"cls-6-section-color\" d=\"M29.83,67.59A10.32,10.32,0,1,1,19.51,57.27,10.32,10.32,0,0,1,29.83,67.59Z\" transform=\"translate(0 -0.05)\"/><path class=\"cls-3-section-color\" d=\"M100,82.6a2.46,2.46,0,0,1-2.37,2.54H63.34A2.45,2.45,0,0,1,61,82.6v-80A2.45,2.45,0,0,1,63.34.05H97.63A2.45,2.45,0,0,1,100,2.59Z\" transform=\"translate(0 -0.05)\"/><circle class=\"cls-4-section-color\" cx=\"80.49\" cy=\"18.39\" r=\"10.32\"/><circle class=\"cls-5-section-color\" cx=\"80.49\" cy=\"42.54\" r=\"10.32\"/><circle class=\"cls-6-section-color\" cx=\"80.49\" cy=\"67.54\" r=\"10.32\"/></svg>",
            SystemType = true,
            Visible = true,
            SpatialType = GeoSpatialType.None,
            Copyable = false,
            Movable = false,
            Sections = new[]
            {
                EntityTypeSections.Entity
            },
            Children = typeChildren?.Select(t => t.TypeId.Id).ToArray() ?? Array.Empty<Guid>()
        };
    }
}

public interface ICorridorTypeChildren : IEntityTypeChildren {}