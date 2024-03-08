// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Econolite.Ode.Repository.Entities.Test;

[ExcludeFromCodeCoverage]
public class EntityRepositoryExtensionsTest
{
    [Fact]
    public void AddEntityRepo_Services_AddedEntityRepoToServices()
    {
        var services = new ServiceCollection();

        var result = services.AddEntityRepo();

        result.Should().NotBeNull();
    }
}