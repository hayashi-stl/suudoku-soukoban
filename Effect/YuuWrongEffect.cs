using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class YuuWrongEffect : Node2D
{
    public enum Way {
        Row,
        Column,
        Region,
    }

    TileMap _map;

    public void Init(Way way, IEnumerable<IEnumerable<Vector2I>> regions, TileMap map)
    {
        var mapName = way switch {
            Way.Row => "Row",
            Way.Column => "Column",
            Way.Region => "Region",
            _ => throw new InvalidEnumArgumentException()
        };
        _map = GetNode<TileMap>(mapName);

        switch (way) {
            case Way.Row:
            case Way.Column: {
                var bounds = (Rect2I)map.GetUsedRect();
                var extBounds = bounds.Grow(1);
                var extDir = way == Way.Row ? Vector2I.Right : Vector2I.Down;
                GD.Print($"");
                foreach (var region in regions) {
                    var min = region.Min();
                    var max = region.Max();
                    while (min.Dot(extDir) > extBounds.Position.Dot(extDir) && map.GetCell((min - extDir).x, (min - extDir).y) != (int)LevelFile.PlayTile.Wall)
                        min -= extDir;
                    while (max.Dot(extDir) < (extBounds.End - Vector2I.One).Dot(extDir) && map.GetCell((max + extDir).x, (max + extDir).y) != (int)LevelFile.PlayTile.Wall)
                        max += extDir;

                    bool fadeStart = map.GetCell((min - extDir).x, (min - extDir).y) != (int)LevelFile.PlayTile.Wall;
                    bool fadeEnd   = map.GetCell((max + extDir).x, (max + extDir).y) != (int)LevelFile.PlayTile.Wall;
                    for (Vector2I v = min; v <= max; v += extDir) {
                        _map.SetCell(v.x, v.y, v == min && fadeStart ? 0 : v == max && fadeEnd ? 2 : 1);
                    }
                }
            } break;

            case Way.Region: {
                foreach (var cell in regions.SelectMany(r => r))
                    _map.SetCell(cell.x, cell.y, 0);
            } break;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public IEnumerable<SceneTreeTween> TweenEffect(float delay)
    {
        var tween = CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(_map, "visible", true, 0);
        tween.TweenProperty(_map, "modulate", _map.Modulate * new Color(1, 1, 1, 0), Entity.TweenTime);
        tween.TweenCallback(this, "queue_free");
        return new List<SceneTreeTween>(){ tween };
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
