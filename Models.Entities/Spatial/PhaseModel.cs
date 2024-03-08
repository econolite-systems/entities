// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities
{
  public class PhaseModel
  {
    public int? Number { get; set; } = 2;
    
    public string? Movement { get; set; } = "Thru";
    
    public int? Lanes { get; set; } = 1;
    
    public IEnumerable<DetectorModel>? Detectors { get; set; } = Array.Empty<DetectorModel>();
  }
}