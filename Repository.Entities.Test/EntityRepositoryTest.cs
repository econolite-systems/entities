// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Types;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Test.Helpers;
using Econolite.Ode.Persistence.Mongo.Test.Repository;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Econolite.Ode.Repository.Entities.Test;

[ExcludeFromCodeCoverage]
[Collection(nameof(MongoCollection))]
public class EntityRepositoryTest : IdRepositoryBaseTest<Guid, EntityRepository, EntityNode>,
    IClassFixture<MongoFixture>
{
    private readonly ILogger<EntityRepository> _entityLogger = Mock.Of<ILogger<EntityRepository>>();
    private readonly ILoggerFactory _loggerFactory = Mock.Of<ILoggerFactory>();
    private EntityTypeId SystemEntityTypeId { get; } = new(){ Id = SystemEntityTypeProvider.SystemTypeId.Id, Name = SystemEntityTypeProvider.SystemTypeId.Name };
    private EntityTypeId SignalEntityTypeId { get; } = new(){ Id = SignalTypeId.Id, Name = SignalTypeId.Name };
    
    public EntityRepositoryTest(MongoFixture fixture) : base(fixture)
    {
        Mock.Get(_loggerFactory)
            .Setup(x => x.CreateLogger(typeof(MongoContext).FullName!))
            .Returns(_entityLogger);
    }
    
    protected override Guid Id { get; } = Guid.NewGuid();

    protected override string ExpectedJsonIdFilter => "UUID(\"" + Id + "\")";

    [Fact]
    public async Task GivenEntityRepository_WhenGettingTypes_ThenReturnsDistinctType()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter = "{ }";
        var expected = new[] {"System", "Signal"};

        RepositorySetup.SetupDistinctQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);

        var actual = await target.GetTypesAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public async Task GivenEntityRepository_WhenGettingRootNodes_ThenReturnsAllRootNodes()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter = "System";
        var signal = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expected = new[] {CreateDocument(SignalEntityTypeId, new[] {signal.ToEntity()}), signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);

        var actual = await target.GetRootNodesAsync().ConfigureAwait(false);

        //Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public async Task GivenEntityRepository_WhenGetSearchNodes_ThenReturnsAllMatchingNodes()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter =
            "{ \"$text\" : { \"$search\" : \"test\" }, \"isDeleted\" : { \"$ne\" : true } }";
        var signal = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expected = new[] {CreateDocument(SignalEntityTypeId, new[] {signal.ToEntity()}), signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);

        var actual = await target.GetSearchNodesAsync("test").ConfigureAwait(false);

        Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }
    
    [Fact]
    public async Task GivenEntityRepository_WhenGetNodesByType_ThenReturnsAllMatchingNodes()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        
        const string expectedJsonFilter = "{ \"type.name\" : /^signal$/is, \"isDeleted\" : { \"$ne\" : true } }";
        var signal = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expected = new[] {CreateDocument(SignalEntityTypeId, Array.Empty<Entity>()), signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);

        var actual = await target.GetNodesByTypeAsync("Signal").ConfigureAwait(false);

        Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public async Task GivenEntityRepository_WhenGetDeletedNodes_ThenReturnsAllDeletedNodes()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter = "{ \"isDeleted\" : true }";
        var signal = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expected = new[] {CreateDocument(SignalEntityTypeId, new[] {signal.ToEntity()}), signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);


        var actual = await target.GetAllDeletedNodesAsync().ConfigureAwait(false);


        Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public async Task GivenEntityRepository_WhenGetAllNodes_ThenReturnsAllNodes()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter = "{ \"isDeleted\" : { \"$ne\" : true } }";
        var signal = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expected = new[] {CreateDocument(SignalEntityTypeId, new[] {signal.ToEntity()}), signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);


        var actual = await target.GetAllNodesAsync().ConfigureAwait(false);


        Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public async Task GivenEntityRepository_WhenGetExpandedNodes_ThenReturnsExpandedNodes()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter =
            "[UUID(\"4a3bfc33-7735-4976-942d-72e09c65d9ea\")]";
        var signal = CreateDocument(SignalEntityTypeId, Guid.Parse("4a3bfc33-7735-4976-942d-72e09c65d9ea"), Array.Empty<Entity>());
        var expected = new[] {signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);


        var actual = await target
            .GetExpandedNodesAsync(new[] {"1E8F7891-B144-4057-8B78-7B4FDA5A4FEA_4A3BFC33-7735-4976-942D-72E09C65D9EA"})
            .ConfigureAwait(false);


        //Assert.Equal(expected, actual);
        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public async Task GivenEntityRepository_WhenGetExpandedNodesWithNoInstanceIds_ThenReturnsEmptyList()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        const string expectedJsonFilter =
            "{ \"_id\" : { \"$in\" : [UUID(\"4a3bfc33-7735-4976-942d-72e09c65d9ea\")] } }";
        var signal = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expected = new[] {CreateDocument(SignalEntityTypeId, new[] {signal.ToEntity()}), signal};

        RepositorySetup.SetupQueryCollection(target.CollectionName, Context, testCollection, expected,
            expectedJsonFilter);


        var actual = await target.GetExpandedNodesAsync(new[] {""}).ConfigureAwait(false);

        Assert.Equal(Array.Empty<EntityNodeProjection>(), actual);
    }

    [Fact]
    public void GivenEntityRepository_WhenCreateNewSystemNode_ThenReturnsSuccess()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());

        RepositorySetup.SetupInsertOne(target.CollectionName, Context, testCollection, expected);

        target.Create(Guid.Empty, expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenCreateNewNode_ThenReturnsSuccess()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var parentId = Guid.NewGuid();
        var expectedUpdateJson =
            "UUID(\"" + expected.Id + "\")";
        var expectedFilterJson = "UUID(\"" + parentId + "\")";
        RepositorySetup.SetupInsertOneWithParent(target.CollectionName, Context, testCollection, expected,
            expectedFilterJson, expectedUpdateJson);

        target.Create(parentId, expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenCreateNewNode_ThenReturnsFailure()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var parentId = Guid.NewGuid();
        var expectedUpdateJson =
            "UUID(\"" + expected.Id + "\")";
        var expectedFilterJson = "UUID(\"" + parentId + "\")";
        RepositorySetup.SetupInsertOneWithParent(target.CollectionName, Context, testCollection, expected,
            expectedFilterJson, expectedUpdateJson, 0);

        target.Create(parentId, expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenEditNode_ThenReturnsSuccess()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expectedJsonIdFilter = "UUID(\"" + expected.Id + "\")";
        var expectedUpdateJson = "\"children.$.name\" : \"" + expected.Name + "\"";
        var expectedFilterJson = "UUID(\"" + expected.Id + "\")";
        RepositorySetup.SetupUpdateOneWithParent(target.CollectionName, Context, testCollection, expected,
            expectedJsonIdFilter, expectedFilterJson, expectedUpdateJson);

        target.Edit(expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenEditNode_ThenReturnsFailure()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expectedJsonIdFilter = "UUID(\"" + expected.Id + "\")";
        var expectedUpdateJson = "\"children.$.name\" : \"" + expected.Name + "\"";
        var expectedFilterJson = "UUID(\"" + expected.Id + "\")";
        RepositorySetup.SetupUpdateOneWithParent(target.CollectionName, Context, testCollection, expected,
            expectedJsonIdFilter, expectedFilterJson, expectedUpdateJson, 0);

        target.Edit(expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenDeleteNode_ThenReturnsSuccess()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var id = Guid.NewGuid();
        var expectedJsonIdFilter = "UUID(\"" + id + "\")";
        var expectedUpdateJson = "UUID(\"" + id + "\")";
        var expectedFilterJson = "UUID(\"" + id + "\")";
        RepositorySetup.SetupDeleteOneWithParent(target.CollectionName, Context, testCollection, expectedJsonIdFilter,
            expectedFilterJson, expectedUpdateJson);

        target.Delete(id);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenDeleteNode_ThenReturnsFailure()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var id = Guid.NewGuid();
        var expectedJsonIdFilter = "UUID(\"" + id + "\")";
        var expectedUpdateJson = "UUID(\"" + id + "\")";
        var expectedFilterJson = "UUID(\"" + id + "\")";
        RepositorySetup.SetupDeleteOneWithParent(target.CollectionName, Context, testCollection, expectedJsonIdFilter,
            expectedFilterJson, expectedUpdateJson, 0);

        target.Delete(id);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenSoftDeleteNode_ThenReturnsSuccess()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        expected.IsDeleted = true;
        var expectedJsonIdFilter = "UUID(\"" + expected.Id + "\")";
        var expectedUpdateJson = "\"children.$.isDeleted\" : true";
        var expectedFilterJson = "UUID(\"" + expected.Id + "\")";
        RepositorySetup.SetupUpdateOneWithParent(target.CollectionName, Context, testCollection, expected,
            expectedJsonIdFilter, expectedFilterJson, expectedUpdateJson);

        target.SoftDelete(expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    [Fact]
    public void GivenEntityRepository_WhenRestoreNode_ThenReturnsSuccess()
    {
        var target = CreateRepository();
        var testCollection = Mock.Of<IMongoCollection<EntityNode>>();
        var expected = CreateDocument(SignalEntityTypeId, Array.Empty<Entity>());
        var expectedJsonIdFilter = "UUID(\"" + expected.Id + "\")";
        var expectedUpdateJson = "\"children.$.isDeleted\" : false";
        var expectedFilterJson = "UUID(\"" + expected.Id + "\")";
        RepositorySetup.SetupUpdateOneWithParent(target.CollectionName, Context, testCollection, expected,
            expectedJsonIdFilter, expectedFilterJson, expectedUpdateJson);

        target.Restore(expected);

        Mock.Verify(Mock.Get(Context));
        Mock.Verify(Mock.Get(testCollection));
    }

    protected override EntityRepository CreateRepository()
    {
        return new EntityRepository(Context, _entityLogger);
    }

    protected override EntityNode CreateDocument()
    {
        return new EntityNode
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            Type = SignalEntityTypeId,
            Children = Array.Empty<Entity>()
        };
    }

    private EntityNode CreateDocument(EntityTypeId type, Entity[] children)
    {
        return new EntityNode
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            Type = type,
            Children = children
        };
    }

    private EntityNode CreateDocument(EntityTypeId type, Guid id, Entity[] children)
    {
        return new EntityNode
        {
            Id = id,
            Name = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            Type = type,
            Children = children
        };
    }
    
    // use this to test against real mongo
    //[Fact]
    /*public async Task AddNodeTest()
    {
        Mock.Get(_fixture.MongoOptions)
            .Setup(x => x.CurrentValue)
            .Returns(new MongoOptions()
            {
                DbConnection = "mongodb://root:rootpassword@localhost:27017",
                DbName = "test"
            });
        
        var client = new ClientProvider(Mock.Get(_fixture.MongoOptions).Object, Mock.Get(_fixture.LoggerFactory).Object);
        var context = new MongoContext(client, Mock.Get(_loggerFactory).Object);
        var repository = new EntityRepository(context, Mock.Get(_entityLogger).Object);

        var parentId = Guid.Parse("0b36c422-6b38-43b7-8f1b-9adb99737976");
        var toAdd = new EntityNode
        {
            Id = Guid.NewGuid(),
            Name = "Main St @ First Ave",
            Description = "",
            Type = "Signal"
        };

        //var nodes = await repository.GetAllNodes();
        //var nodes = await repository.GetAllDeletedNodes();
        //nodes.Should().NotBeEmpty();
        repository.Create(toAdd, parentId);

        //repository.Delete(Guid.Parse("5f798e0b-e3b0-4f70-89ba-15d0c8181f49"));
        //await repository.SoftDeleteAsync(Guid.Parse("c6e5b826-5bce-48dc-83c2-15d4abc6de8c"));
        //await repository.RestoreAsync(Guid.Parse("c6e5b826-5bce-48dc-83c2-15d4abc6de8c"));
        //var (success, errors) = await context.SaveChangesAsync();

        //success.Should().BeTrue();
        // var services = new ServiceCollection();
        // services.AddMongo(options: options =>
        // {
        //     options.DbConnection = "mongodb://root:rootpassword@192.168.1.139:27017";
        //     options.DbName = "test";
        // },);

        // var builder = services.BuildServiceProvider();
        // var logger = builder.GetService<ILogger>();
        // var context = builder.GetService<IMongoContext>();
        // var client = builder.GetService<IClientProvider>();
        // var id = Guid.NewGuid();
        // var node = new Model.VehiclePriority.Entities.EntityNode { Id = id };
        // _repository.Add(node);
        //var changes = await _context.SaveChanges();
    }*/
}