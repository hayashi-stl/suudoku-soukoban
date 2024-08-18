using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public class TAImageImportPlugin : EditorImportPlugin
{
    enum Presets { Default };

    public override string GetImporterName() => "taimage";

    public override string GetVisibleName() => "Texture-Attribute Image";

    public override Godot.Collections.Array GetRecognizedExtensions() =>
        new Godot.Collections.Array(new List<string>(){ "taimage" });

    public override string GetSaveExtension() => "res";

    public override string GetResourceType() => "ImageTexture";

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

        var headerLine = file.GetLine().Trim().Split();
        var numVertices = int.Parse(headerLine[0]);
        var numAttributes = int.Parse(headerLine[1]);

        var image = new Image();
        image.Create(numVertices, (numAttributes + 3) / 4, false, Image.Format.Rgbaf);
        image.Lock();
        for (int v = 0; v < numVertices; ++v) {
            var attributes = file.GetLine().Trim().Split().Select(s => float.Parse(s)).ToList();
            attributes.AddRange(Enumerable.Repeat(0.0f, (4 - (attributes.Count % 4)) % 4));
            for (int i = 0; i < attributes.Count / 4; ++i)
                image.SetPixel(v, i, new Color(attributes[4 * i + 0], attributes[4 * i + 1], attributes[4 * i + 2], attributes[4 * i + 3]));
        }
        image.Unlock();

        file.Close();
        var texture = new ImageTexture();
        texture.CreateFromImage(image, 0);
        return (int)ResourceSaver.Save($"{savePath}.{GetSaveExtension()}", texture);
    }
}