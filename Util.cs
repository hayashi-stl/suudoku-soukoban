using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Util : Node
{
    public const int TileSize = 64;
    public const int ZIndexGap = 50;

    public static (Vector2 XY, int ZIndex) FromTileSpace(Vector3I pos) {
        return ((new Vector2(pos.x, pos.y) + Vector2.One / 2) * TileSize, pos.z * ZIndexGap);
    }

    public static Vector3I ToTileSpace(Vector2 XY, int ZIndex) {
        var posXY = (Vector2I)(XY / TileSize);
        return new Vector3I(posXY.x, posXY.y, ZIndex / ZIndexGap);
    }

    public enum Direction {
        Right,
        Up,
        Left,
        Down
    }

    public static Vector3I DirVec(Direction dir) {
        return dir switch {
            Direction.Right => Vector3I.Right,
            Direction.Up => Vector3I.Down,
            Direction.Left => Vector3I.Left,
            Direction.Down => Vector3I.Up,
            _ => throw new ArgumentOutOfRangeException($"{dir} is not a direction."),
        };
    }

    public static Viewport Root(Node node) {
        return node.GetNode<Viewport>("/root");
    }

    public static T Ancestor<T>(Node node, Func<Node, bool> condition) where T: Node {
        while (node != null) {
            node = node.GetParent();
            if (node != null && condition(node))
                return (T)node;
        }
        return null;
    }

    public static T AncestorOfType<T>(Node node) where T: Node {
        return Ancestor<T>(node, (n) => n is T);
    }

    public static void SetInstanceShaderParameter2D(Node2D node, String name, object value, bool affectAllInstances = false) {
        // Duplicate; as instance shader parameters aren't supported on the web
        var mat = (ShaderMaterial)node.Material;
        if (!affectAllInstances && !mat.HasMeta("hack_unique")) {
            mat = (ShaderMaterial)mat.Duplicate();
            mat.SetMeta("hack_unique", true);
            node.Material = mat;
        }
        mat.SetShaderParam(name, value);
    }

    public static int SequenceCompare<T>(IEnumerable<T> aSeq, IEnumerable<T> bSeq) where T: IComparable {
        foreach (var (a, b) in aSeq.Zip(bSeq, (a, b) => (a, b))) {
            if (a.CompareTo(b) is int result && result != 0)
                return result;
        }
        return aSeq.Count().CompareTo(bSeq.Count());
    }

    public static Vector3 SwapYZ(Vector3 v) => new Vector3(v.x, v.z, v.y);
    public static Vector3I SwapYZ(Vector3I v) => new Vector3I(v.x, v.z, v.y);

    public static int[] Vec3IToIntArray(Vector3I v) => new int[]{ v.x, v.y, v.z };
    public static Vector3I IntArrayToVec3I(int[] v) => new Vector3I(v[0], v[1], v[2]);

    public static bool TryPop<T>(Stack<T> stack, out T value) {
        bool hasValue = stack.Count > 0;
        value = hasValue ? stack.Pop() : default;
        return hasValue;
    }

    public static List<T> ToList<T>(Godot.Collections.Array array) {
        List<T> list = new List<T>();
        foreach (var value in array)
            list.Append((T)value);
        return list;
    }

    public static Transform2D Scale(Vector2 scale) {
        return Transform2D.Identity.Scaled(scale);
    }

    public static Transform2D Rotation(float rotation) {
        return Transform2D.Identity.Rotated(rotation);
    }

    public static Transform2D Translation(Vector2 translation) {
        return Transform2D.Identity.Translated(translation);
    }
}
