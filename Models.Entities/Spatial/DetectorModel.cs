// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities;

public class DetectorModel
{
  public int Number { get; set; }

  public string DetectorName { get; set; } = String.Empty;

  public Bearing Bearing { get; set; }

  public Movement Movement { get; set; }

  public int Length { get; set; }

  public int SetBack { get; set; }

  public double DistanceToNextSignal { get; set; }

  public int Lanes { get; set; }

  public int LaneNumber { get; set; }

  public IEnumerable<int> Phases { get; set; } = Array.Empty<int>();

  public int ProtectedPhase { get; set; }

  public int PermittedPhase { get; set; }

  public DetectorType Type { get; set; }

  public bool Advanced { get; set; }

  public bool PhaseData { get; set; }

  public bool ExitDetection { get; set; }

  public bool TurningMovementCount { get; set; }

  public bool SplitFailure { get; set; }

  public bool RedLightMonitor { get; set; }

  public bool Speed { get; set; }
}