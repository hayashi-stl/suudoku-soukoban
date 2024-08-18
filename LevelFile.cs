using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonSubTypes;
using System.IO;
using System.Text;
using Newtonsoft.Json.Converters;

// All coordinates are (horizontal, vertical, layer)
public class LevelFile
{
    public abstract class EntityCustomData {
        public static JsonConverter Converter() {
            return JsonSubtypesConverterBuilder
                .Of<EntityCustomData>("$Type")
                .RegisterSubtype<PlayerFile>("Player")
                .RegisterSubtype<BlockFile>("Block")
                .RegisterSubtype<MarkerFile>("Marker")
                .SerializeDiscriminatorProperty()
                .Build();
        }
    }

    public class PlayerFile : EntityCustomData {}

    public class BlockFile : EntityCustomData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Block.BlockType Type { get; set; }
    }

    public class MarkerFile : EntityCustomData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Marker.MarkerType Type { get; set; }
    }

    public class EntityFile
    {
        public Vector3I Position { get; set; }
        public Vector3I Direction { get; set; }
        public Vector3I Gravity { get; set; }
        public EntityCustomData CustomData { get; set; }
    }

    public string Name { get; set; }
    public string Controls { get; set; } = "";
    public Vector3I Base { get; set; } // minimum coordinates
    public Vector3I Size { get; set; }
    public List<string> SublevelPaths { get; set; } = new List<string>();
    public List<FileTile> Map { get; set; } // row-major then plane-major
    public List<EntityFile> Entities { get; set; }

    public enum FileTile {
        Invalid = 0,
        FirstFloor = 1,
        NumFloors = 8,
        Wall = 1,
    }

    public enum PlayTile {
        Invalid = -1,
        FirstFloor = 0,
        NumFloors = 8,
        Wall = FirstFloor + NumFloors,
    }

    public static (FileTile Tile, int Z) ToFileTile(PlayTile t) {
        return t switch {
            PlayTile.Invalid => (FileTile.Invalid, 0),
            PlayTile.Wall => (FileTile.Wall, 1),
            _ => ((FileTile)((int)t + (int)FileTile.FirstFloor - (int)PlayTile.FirstFloor), 0),
        };
    }

    public static PlayTile ToPlayTile(FileTile t, int z) {
        return (t, z) switch {
            (FileTile.Invalid, _) => PlayTile.Invalid,
            (FileTile.Wall, 1) => PlayTile.Wall,
            _ => (PlayTile)((int)t + (int)PlayTile.FirstFloor - (int)FileTile.FirstFloor),
        };
    }

    public static bool IsPlayTileFloor(PlayTile t) {
        return t >= PlayTile.FirstFloor && (int)t < (int)PlayTile.FirstFloor + (int)PlayTile.NumFloors;
    }


    public class Vector3IJsonConverter : JsonConverter<Vector3I>
    {
        public override Vector3I ReadJson(JsonReader reader, Type objectType, Vector3I existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var arr = serializer.Deserialize<int[]>(reader);
            return new Vector3I(arr[0], arr[1], arr[2]);
        }

        public override void WriteJson(JsonWriter writer, Vector3I value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"[{value.x}, {value.y}, {value.z}]");
        }
    }

    public class SerialMapJsonConverter : JsonConverter<List<FileTile>>
    {
        public Vector3I Size { private get; set; }

        public override List<FileTile> ReadJson(JsonReader reader, Type objectType, List<FileTile> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<List<FileTile>>(reader);
        }

        public override void WriteJson(JsonWriter writer, List<FileTile> value, JsonSerializer serializer)
        {
            var indent = string.Join("", Enumerable.Repeat("  ", 2));
            var str = string.Join($",\n\n{indent}  ", Enumerable.Range(0, Size.z).Select(z =>
                string.Join($",\n{indent}  ", Enumerable.Range(0, Size.y).Select(y =>
                    string.Join(", ", Enumerable.Range(0, Size.x).Select(x =>
                        ((int)value[(z * Size.y + y) * Size.x + x]).ToString()))))));
            writer.WriteRawValue($"[\n{indent}  {str}\n{indent}]");
        }
    }

    public string ToJson() {
        var serializer = new JsonSerializer { Formatting = Formatting.Indented };
        serializer.Converters.Add(new Vector3IJsonConverter());
        serializer.Converters.Add(new SerialMapJsonConverter{ Size = Size });
        serializer.Converters.Add(EntityCustomData.Converter());
        var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream);
        using var jsonWriter = new JsonTextWriter(streamWriter);
        serializer.Serialize(jsonWriter, this);
        jsonWriter.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static LevelFile FromJson(string json) {
        var serializer = new JsonSerializer { Formatting = Formatting.Indented };
        serializer.Converters.Add(new Vector3IJsonConverter());
        serializer.Converters.Add(EntityCustomData.Converter());
        var jsonReader = new JsonTextReader(new StringReader(json));
        return serializer.Deserialize<LevelFile>(jsonReader);
    }

    public static LevelFile FromJson(Resource json) => FromJson((string)json.Get("text"));

    public static LevelFile Read(string filename) {
        var file = new Godot.File();
        file.Open(filename, Godot.File.ModeFlags.Read);
        var json = file.GetAsText();
        file.Close();
        return FromJson(json);
    }

    public void Save(string filename) {
        var file = new Godot.File();
        file.Open(filename, Godot.File.ModeFlags.Write);
        var json = ToJson();
        file.StoreString(json);
        file.Close();
    }

    // Rebase the level so that the minimum coordinates are [0, 0, 0]
    public void Rebase() {
        var offset = -Base;
        Base = Vector3I.Zero;
        foreach (var entity in Entities) {
            entity.Position += offset;
        }
    }
}