// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.Entities.Spatial;

public static class EpsgCodeExtension
{
  public static int ToInt(this EpsgCode code) => (int) code;

  public static EpsgCode ToEpsgCode(this int code) => (EpsgCode) Enum.ToObject(typeof (EpsgCode), code);
}