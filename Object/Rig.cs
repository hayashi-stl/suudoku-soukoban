using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public class Rig : Skeleton2D
{

    List<MeshInstance2D> _riggedMeshes = new List<MeshInstance2D>();

    Godot.Collections.Array<string> _riggedMeshPaths = new Godot.Collections.Array<string>();
    [Export]
    Godot.Collections.Array<string> RiggedMeshPaths {
        get => _riggedMeshPaths;
        set {
            _riggedMeshPaths = value;
            _riggedMeshes = _riggedMeshPaths.Select(p => GetNodeOrNull<MeshInstance2D>(p)).Where(n => n != null).ToList();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        RiggedMeshPaths = _riggedMeshPaths;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Stack<Bone2D> bones = new Stack<Bone2D>(
            new Godot.Collections.Array<Node>(GetChildren()).Where(n => n is Bone2D).Select(n => (Bone2D)n).Reverse());
        int index = 0;
        while (Util.TryPop(bones, out var bone)) {
            foreach (var mesh in _riggedMeshes) {
                var transform = mesh.GlobalTransform.AffineInverse() * ((Node2D)bone.GetParent()).GlobalTransform * bone.Transform * bone.Rest.AffineInverse();
                //GD.Print(index, " ", bone.Name, " ", mesh.Name, " ", $"rig_transform_{index:D2} ", transform);
                Util.SetInstanceShaderParameter2D(mesh, $"rig_transform_{index:D2}", transform, Engine.EditorHint);
            }
            foreach (var child in new Godot.Collections.Array<Node>(bone.GetChildren()).Where(n => n is Bone2D).Select(n => (Bone2D)n).Reverse()) 
                bones.Push(child);
            ++index;
        }
    }
}
