using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public class TAMeshImportPlugin : EditorImportPlugin
{
    enum Presets { Default };

    public override string GetImporterName() => "tamesh";

    public override string GetVisibleName() => "Texture-Attribute Mesh";

    public override Godot.Collections.Array GetRecognizedExtensions() =>
        new Godot.Collections.Array(new List<string>(){ "tamesh" });

    public override string GetSaveExtension() => "res";

    public override string GetResourceType() => "Mesh";

    public override int GetPresetCount() => Enum.GetNames(typeof(Presets)).Length;

    public override string GetPresetName(int preset) => (Presets)preset switch {
        Presets.Default => "Default",
        _ => "Unknown"
    };

    public override Godot.Collections.Array GetImportOptions(int preset) => (Presets)preset switch {
        Presets.Default => new Godot.Collections.Array(),
        _ => new Godot.Collections.Array()
    };

    public override bool GetOptionVisibility(string option, Dictionary options) => true;

    public override int Import(string sourceFile, string savePath, Dictionary options, Godot.Collections.Array platformVariants, Godot.Collections.Array genFiles)
    {
        var file = new File();
        var err = file.Open(sourceFile, File.ModeFlags.Read);
        if (err != Error.Ok)
            return (int)err;

        var version = file.GetLine().Trim();
        if (version != "1.0.0") {
            file.Close();
            return (int)Error.FileCorrupt;
        }

        var mesh = new ArrayMesh();

        var numVertices = int.Parse(file.GetLine().Trim());
        var positions = new List<Vector3>();
        var uvs = new List<Vector2>();
        var colors = new List<Color>();
        for (int i = 0; i < numVertices; ++i) {
            var line = file.GetLine().Trim().Split().Select(s => float.Parse(s)).ToList();
            positions.Add(new Vector3(line[0], line[1], line[2]));
            uvs.Add(new Vector2(line[3], line[4]));
            colors.Add(Color.Color8((byte)(i >> 0 & 0xff), (byte)(i >> 8 & 0xff), (byte)(i >> 16 & 0xff), (byte)(i >> 24 & 0xff)));
        }

        var numIndices = int.Parse(file.GetLine().Trim());
        var indices = new List<int>();
        for (int i = 0; i < numIndices; ++i) {
            var line = file.GetLine().Trim().Split().Select(s => int.Parse(s)).ToList();
            // Deal with Godot clockwise order
            indices.Add(line[0]);
            indices.Add(line[2]);
            indices.Add(line[1]);
        }

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = positions.ToArray();
        arrays[(int)ArrayMesh.ArrayType.TexUv] = uvs.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Color] = colors.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Index] = indices.ToArray();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        file.Close();
        return (int)ResourceSaver.Save($"{savePath}.{GetSaveExtension()}", mesh);
    }
}