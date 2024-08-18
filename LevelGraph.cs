using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class LevelGraph {
    public class Node {
        public string Name { get; set; }
        public string Filename { get; set; }
        public Node Parent { get; set; }
        public Dictionary<string, Node> Children { get; set; } = new Dictionary<string, Node>(); // with labels
    }

    public Dictionary<string, Node> NodesByFilename { get; private set; }

    public static LevelGraph FromRoot(string rootPath) {
        // First, collect the nodes
        var nodes = new Dictionary<string, Node>();
        var pathsToProcess = new Stack<string>(new List<string>(){ rootPath });
        while (Util.TryPop(pathsToProcess, out var path)) {
            var level = LevelFile.Read(path);
            nodes.Add(path, new Node { Name = level.Name, Filename = path });
            level.SublevelPaths.ForEach(p => pathsToProcess.Push(p));
        }

        // Now build relationships
        var nodesToProcess = new Stack<(string Label, Node Node)>(new List<(string, Node)>(){ ("", nodes[rootPath]) });
        while (Util.TryPop(nodesToProcess, out var pair)) {
            var level = LevelFile.Read(pair.Node.Filename);

            // Child-parent
            for (int i = 0; i < level.SublevelPaths.Count; ++i) {
                var child = nodes[level.SublevelPaths[i]];
                child.Parent = pair.Node;
                pair.Node.Children.Add($"{i + 1}", child);
                nodesToProcess.Push(($"{i + 1}", child));
            }
        }

        return new LevelGraph() { NodesByFilename = nodes };
    }
}