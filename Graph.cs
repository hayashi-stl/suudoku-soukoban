using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Graph
{
    public enum EdgeType {
        Forcing, // Source moves and forces target to move.
        Rigid, // Source moves if and only if target has space to move.
    }

    public class Edge {
        public EdgeType Type { get; set; }
        public bool Backward { get; set; }
    }

    public class Target {
        public Entity Entity { get; set; }
        public Edge Edge { get; set; }
    }

    class Targets {
        public List<Target> TheTargets { get; set; } = new List<Target>();
        public bool CanMove { get; set; } = true;
    }

    readonly Dictionary<Entity, Targets> _nodes = new Dictionary<Entity, Targets>();

    public void AddEntity(Entity entity, bool can_move) {
        if (!_nodes.ContainsKey(entity))
            _nodes[entity] = new Targets();
        _nodes[entity].CanMove = can_move;
    }
        
    public bool HasEntity(Entity entity) {
        return _nodes.ContainsKey(entity);
    }
        
    public void AddTarget(Entity from, Entity to, EdgeType edge_type) {
        if (!_nodes.ContainsKey(from))
            _nodes[from] = new Targets();
        if (!_nodes.ContainsKey(to))
            _nodes[to] = new Targets();
        _nodes[from].TheTargets.Add(new Target{ Entity = to,   Edge = new Edge{ Type = edge_type, Backward = false }});
        _nodes[to]  .TheTargets.Add(new Target{ Entity = from, Edge = new Edge{ Type = edge_type, Backward = true }});
    }

    // Starts with entities that can't move, and uses rigid edges
    // to determine which other entities can't move
    public void MarkMovability() {
        Stack<Entity> stack = new Stack<Entity>(_nodes.Keys.Where((key) => !_nodes[key].CanMove));
        // Using actual entites instead of IDs because temporary
        HashSet<Entity> visited = new HashSet<Entity>();
        while (stack.Count > 0) {
            Entity ent = stack.Pop();
            if (visited.Contains(ent))
                continue;
            visited.Add(ent);
            
            _nodes[ent].CanMove = false;
            foreach (var target in _nodes[ent].TheTargets)
                if (target.Edge.Type == EdgeType.Rigid && target.Edge.Backward)
                    stack.Push(target.Entity);
        }
    }
        
    // Returns an array of moving entities, followed by an array of squished entities
    public (List<Entity> Moving, List<Entity> Squished) MovingSquishedEntities(Entity initial) {
        // Special case where the initial entity doesn't get squished
        if (!_nodes[initial].CanMove)
            return (new List<Entity>(), new List<Entity>());
            
        Stack<Entity> stack = new Stack<Entity>(new List<Entity>(){ initial });
        HashSet<Entity> visited = new HashSet<Entity>();
        List<Entity> moving = new List<Entity>();
        List<Entity> squished = new List<Entity>();
        while (stack.Count > 0) {
            Entity ent = stack.Pop();
            if (visited.Contains(ent))
                continue;
            visited.Add(ent);
            if (!_nodes[ent].CanMove) {
                squished.Add(ent);
                continue;
            }
            
            moving.Add(ent);
            foreach (var target in _nodes[ent].TheTargets) {
                if (!target.Edge.Backward &&
                        (target.Edge.Type == EdgeType.Forcing ||
                        (target.Edge.Type == EdgeType.Rigid && _nodes[target.Entity].CanMove)))
                    stack.Push(target.Entity);
            }
        }
                    
        return (moving, squished);
    }

    // Returns an array of entities that bump, assuming the original one can't move.
    // Entities that can't move naturally don't bump.    
    public List<Entity> BumpingEntities(Entity initial) {
        Stack<Entity> stack = new Stack<Entity>(new List<Entity>(){ initial });
        HashSet<Entity> visited = new HashSet<Entity>();
        List<Entity> bumping = new List<Entity>();
        while (stack.Count > 0) {
            Entity ent = stack.Pop();
            if (visited.Contains(ent))
                continue;
            visited.Add(ent);
            
            bumping.Add(ent);
            foreach (var target in _nodes[ent].TheTargets) {
                if (!target.Edge.Backward)
                    stack.Push(target.Entity);
            }
        }
                    
        return bumping;
    }
}
