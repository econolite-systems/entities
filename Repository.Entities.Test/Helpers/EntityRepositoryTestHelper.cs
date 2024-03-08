// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Moq;

namespace Econolite.Ode.Repository.Entities.Test.Helpers;

public class EntityRepositoryTestHelper
{
    public IEntityRepository Repository { get; } = Mock.Of<IEntityRepository>();
}