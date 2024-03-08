// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.


using Econolite.Ode.Config.Devices;

namespace Econolite.Ode.Models.Entities;

public class EntityNode : Entity
{
    public Guid Parent { get; set; }

    public GeoJsonGeometry Geometry { get; set; } = new GeoJsonGeometry();
    
    public GeoJsonPolygonFeature? GeoFence { get; set; }
    
    public string? ExternalId { get; set; }
    
    public string? Primary { get; set; }
    
    public string? Secondary { get; set; }
    
    public int? ActiveDays { get; set; }
    
    public int? IdMapping { get; set; }
    
    public string? ControllerType { get; set; }
    
    public Guid? DeviceManager { get; set; }

    public Guid? Channel { get; set; }

    public CommMode? CommMode { get; set; }

    public string IPAddress { get; set; } = "0.0.0.0";

    public int? Port { get; set; }

    public int? SSHPort { get; set; }

    public string? SSHHostKey { get; set; }

    public byte? FilteredCommBad { get; set; }

    public byte? FilteredCommMarginal { get; set; }

    public byte? FilteredCommWeightingFactor { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public string PrivacyPhrase { get; set; } = string.Empty;
    
    public int? Retries { get; set; }
    
    public int? Timeout { get; set; }
    
    public int? PollRate { get; set; }
    
    public string? Authentication { get; set; }
    
    public string? Privacy { get; set; }
    
    public bool? RequireStandbyOnSet { get; set; }

    public IEnumerable<Guid> Parents { get; set; } = Array.Empty<Guid>();

    public IEnumerable<Entity> Children { get; set; } = Array.Empty<Entity>();
}

public static partial class EntityExtensions
{
    public static IEnumerable<(Guid parent, Guid node)> ToParentNodeGuids(this IEnumerable<string> items)
    {
        return items.Select(i => i.ToParentNodeGuids());
    }

    public static (Guid parent, Guid node) ToParentNodeGuids(this string instanceId)
    {
        var split = instanceId.Split("_");
        if (split.Length == 2)
        {
            if (Guid.TryParse(split.FirstOrDefault(), out var first) &&
                Guid.TryParse(split.LastOrDefault(), out var last)) return (first, last);
        }
        else if (Guid.TryParse(split.FirstOrDefault(), out var first))
        {
            return (Guid.Empty, first);
        }

        return (Guid.Empty, Guid.Empty);
    }

    public static IEnumerable<EntityNodeProjection> ToProjection(this IEnumerable<EntityNode> nodes,
        IEnumerable<(Guid parent, Guid node)>? instanceIds = null)
    {
        var results = new List<EntityNodeProjection>();
        if (!nodes.Any()) return results;

        var rootNodeIds = nodes.FindRootNodeIds();
        if (!rootNodeIds.Any())
            foreach (var node in nodes)
            {
                var projection = node.ToProjection(node.Parent);
                if (projection != null) results.Add(projection);
            }
        else
            foreach (var rootId in rootNodeIds)
            {
                var projection = GetEntityNodeProjection(rootId, Guid.Empty, nodes, instanceIds);
                if (projection != null) results.Add(projection);
            }

        return results;
    }

    private static EntityNodeProjection? GetEntityNodeProjection(Guid root, Guid parent, IEnumerable<EntityNode> nodes,
        IEnumerable<(Guid parent, Guid node)>? instanceIds = null)
    {
        var rootNode = nodes.FirstOrDefault(n => n.Id == root);
        if (rootNode == null) return null;
        var result = rootNode.ToProjection(parent);
        var children = new List<EntityNodeProjection>();
        var expanded = instanceIds != null
            ? instanceIds.Where(i => i.parent == rootNode.Id)
            : Array.Empty<(Guid parent, Guid node)>();

        foreach (var child in rootNode.Children)
            if (expanded.Any(e => e.node == child.Id))
            {
                var node = GetEntityNodeProjection(child.Id, rootNode.Id, nodes,
                    instanceIds ?? Array.Empty<(Guid parent, Guid node)>());
                if (node is {IsDeleted: false})
                    children.Add(node);
            }
            else
            {
                if (!child.IsDeleted)
                    children.Add(child.ToProjection(rootNode.Id));
            }

        result.Children = children;
        return result;
    }

    public static (Guid parent, Guid id)[] ToInstanceIds(this IEnumerable<EntityNode> nodes)
    {
        return nodes.Where(n => !n.IsLeaf).Select(n => (n.Parent, n.Id)).ToArray();
    }

    public static EntityNodeProjection ToProjection(this EntityNode node, Guid parent)
    {
        return new EntityNodeProjection
        {
            Id = node.Id,
            InstanceId = $"{parent}_{node.Id}",
            Name = node.Name,
            Description = node.Description,
            Type = node.Type,
            Jurisdiction = node.Jurisdiction,
            IsCopy = node.IsCopy,
            IsLeaf = node.IsLeaf,
            IsDeleted = node.IsDeleted,
            Version = node.Version,
            Parent = node.Parent,
            Children = node.Children.ToProjection(node.Id).ToArray()
        };
    }

    public static IEnumerable<EntityNodeProjection> ToProjection(this IEnumerable<Entity> list, Guid parent)
    {
        return list.Where(e => !e.IsDeleted).Select(e => e.ToProjection(parent));
    }

    public static EntityNode ToEntityNode(this Entity node, Guid parent, Guid? id = null)
    {
        node.Id = id ?? node.Id;
        return new EntityNode
        {
            Id = node.Id,
            Name = node.Name,
            Description = node.Description,
            Type = node.Type,
            Jurisdiction = node.Jurisdiction,
            IsCopy = node.IsCopy,
            IsLeaf = node.IsLeaf,
            IsDeleted = node.IsDeleted,
            Version = node.Version,
            Parent = parent
        };
    }

    public static EntityNodeProjection ToProjection(this Entity node, Guid parent)
    {
        return new EntityNodeProjection
        {
            Id = node.Id,
            InstanceId = $"{parent}_{node.Id}",
            Name = node.Name,
            Description = node.Description,
            Type = node.Type,
            Jurisdiction = node.Jurisdiction,
            IsCopy = node.IsCopy,
            IsLeaf = node.IsLeaf,
            IsDeleted = node.IsDeleted,
            Version = node.Version,
            Parent = parent
        };
    }

    public static IEnumerable<Guid> FindRootNodeIds(this IEnumerable<EntityNode> nodes)
    {
        return nodes.Where(n => n.Parent == Guid.Empty).Select(r => r.Id);
    }

    public static Entity ToEntity(this EntityNode node)
    {
        return new Entity
        {
            Id = node.Id,
            Name = node.Name,
            Description = node.Description,
            Type = node.Type,
            Jurisdiction = node.Jurisdiction,
            IsCopy = node.IsCopy,
            IsLeaf = !node.IsLeaf ? node.IsLeaf : !node.Children.Any(),
            IsDeleted = node.IsDeleted,
            Version = node.Version
        };
    }

    // public static EntityNode ToEntityNode(this IntersectionModel node, Guid tenantId, Guid parent, bool isLeaf)
    // {
    //     return new EntityNode()
    //     {
    //         TenantId = tenantId,
    //         Id = node.Id,
    //         ClarityId = node.ClarityId,
    //         Name = node.Name,
    //         Description = node.Description,
    //         Type = "Signal",
    //         SubType = (byte?)node.ControllerType,
    //         IsLeaf = isLeaf,
    //         Parent = parent,
    //         Parents = new Guid[] { parent },
    //     };
    // }

    // public static EntityNode ToEntityNode(this EntityModel node, Guid tenantId, Guid parent, bool isLeaf)
    // {
    //     return new EntityNode()
    //     {
    //         Id = node.Id,
    //         Name = node.Name,
    //         Description = node.Description,
    //         Type = node.Type,
    //         IsLeaf = isLeaf,
    //         Parent = parent,
    //         Parents = new Guid[] { parent },
    //     };
    // }

    // public static EntityNode ToSystemEntityNode(this RequestContext context)
    // {
    //     return new EntityNode()
    //     {
    //         Id = Guid.NewGuid(),
    //         Name = "System",
    //         Description = "Default System",
    //         Type = "System",
    //         IsLeaf = false,
    //         Parent = Guid.Empty,
    //         Parents = new Guid[0],
    //     };
    // }

    public static bool IsDifferent(this EntityNode model, EntityNode node)
    {
        return
            model.Name != node.Name ||
            model.Description != node.Description ||
            model.Type != node.Type;
    }

    public static EntityNode Update(this EntityNode node, EntityNode model)
    {
        node.Name = model.Name;
        node.Description = model.Description;
        return node;
    }
}