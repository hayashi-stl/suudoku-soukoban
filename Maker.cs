using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[Tool]
public class Maker : Node2D
{
    TileMap _map;

    bool _save = false;
    [Export]
    bool Save {
        get { return _save; }
        set {
            if (value) {
                var level = SaveLevel();
                level.Rebase();
                level.Save(_levelFile.ResourcePath);
                _levelFile.Set("text", level.ToJson());
            }
        }
    }

    [Export]
    string LevelName { get; set; }

    [Export]
    Godot.Collections.Array<Resource> Sublevels { get; set; }

    Resource _levelFile;
    [Export]
    Resource LevelTextFile {
        get { return _levelFile; }
        set {
            _levelFile = value;
            if (_levelFile != null) {
                var level = LevelFile.FromJson(_levelFile);
                LoadLevel(level);
            }
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _map = GetNode<TileMap>("%RegionMap");
    }

    void ClearLevel()
    {
        _map.Clear();
        Sublevels.Clear();
        foreach (var child in GetChildren().Cast<Node>().ToList())
            if (child is EntityNode2D)        
                child.QueueFree();
    }

    void LoadLevel(LevelFile level)
    {
        ClearLevel();
        LevelName = level.Name;

        Sublevels = new Godot.Collections.Array<Resource>(level.SublevelPaths.Select(p => GD.Load(p)));

        // Fill entries from the grid map
        for (int z = Level.MinZ; z < Level.MinZ + Level.SizeZ; ++z)
            for (int y = level.Base.y; y < level.Base.y + level.Size.y; ++y)
                for (int x = level.Base.x; x < level.Base.x + level.Size.x; ++x) {
                    var cellPosition = new Vector3I(x, y, z);
                    var mapPosition = cellPosition - level.Base;
                    var cell = level.Map[(mapPosition.z * level.Size.y + mapPosition.y) * level.Size.x + mapPosition.x];

                    if (cell != LevelFile.FileTile.Invalid) {
                        _map.SetCell(x, y, (int)LevelFile.ToPlayTile(cell, z));
                    }
                }

        foreach (var entData in level.Entities) {
            var def = new Entity.EntityDef(0, entData);
            var entityNode = def.SpawnInMaker(this);
        }
    }

    Rect2I Bounds()
    {
        var cells = _map.GetUsedCells().Cast<Vector2>()
            .Select(x => (Vector2I)x)
            .ToList();

        var minX = cells.Select(x => x.x).Min();
        var minY = cells.Select(x => x.y).Min();
        var maxX = cells.Select(x => x.x).Max() + 1;
        var maxY = cells.Select(x => x.y).Max() + 1;
        return new Rect2I(minX, minY, maxX - minX, maxY - minY);
    }

    LevelFile SaveLevel()
    {
        var bounds = Bounds();
        var entities = GetChildren().Cast<Node>().Where(n => n is EntityNode2D).Select(n => (EntityNode2D)n).ToList();

        return new LevelFile {
            Name = LevelName,
            Base = new Vector3I(bounds.Position.x, bounds.Position.y, Level.MinZ),
            Size = new Vector3I(bounds.Size.x, bounds.Size.y, Level.SizeZ),
            SublevelPaths = Sublevels.Select(s => s.ResourcePath).ToList(),
            Map = Enumerable.Range(Level.MinZ, Level.SizeZ).SelectMany(z =>
                Enumerable.Range(bounds.Position.y, bounds.Size.y).SelectMany(y =>
                    Enumerable.Range(bounds.Position.x, bounds.Size.x).Select(x => {
                            var (tile, tileZ) = LevelFile.ToFileTile((LevelFile.PlayTile)_map.GetCell(x, y));
                            return z == tileZ ? tile : LevelFile.FileTile.Invalid;
                        }
                ))).ToList(),
            Entities = entities.Select(e => e.LevelEntityFile()).ToList()
        };
    }

    void UpdateTileOutlines(TileMap map, TileMap outline)
    {
        var mapCells = map.GetUsedCells().Cast<Vector2>().Select(x => (Vector2I)x).ToHashSet();
        var outlineCells = outline.GetUsedCells().Cast<Vector2>().Select(x => (Vector2I)x).ToHashSet();
        var added = mapCells.Where(x => !outlineCells.Contains(x)).ToList();
        var removed = outlineCells.Where(x => !mapCells.Contains(x)).ToList();
        foreach (var cell in added)
            outline.SetCell(cell.x, cell.y, 0);
        foreach (var cell in removed)
            outline.SetCell(cell.x, cell.y, -1);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
    }
}
