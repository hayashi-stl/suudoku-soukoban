using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class YuuWrong : Node2D
{
    public enum Way {
        Row,
        Column,
        Region,
    }

    TileMap _map;

    public void Init(Way way, List<List<Vector2I>> regions, TileMap map)
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
                foreach (var region in regions) {
                    var min = region.Min();
                    var max = region.Max();
                    _map.SetCell(cell.x, cell.y, 0);
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

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
