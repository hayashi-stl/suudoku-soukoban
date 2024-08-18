using Godot;
using System;
using System.Collections.Generic;

public partial class LevelSelect : Node
{
    string _levelPath;
        
    public void PlayLevel(string levelPath) {
        _levelPath = levelPath;
        var stage = Stage.Instantiate(new Stage.EnterParameters() {
            LevelPath = levelPath,
            ExitDirection = Vector3I.Forward,
        });
        Util.Root(this).AddChild(stage);
        QueueFree();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // TODO: Delete
        //for (int i = 1; i <= 24; ++i) {
        //    string filename = $"res://Level/{i:d3}.json";
        //    GD.Print(filename);
        //    var lf = LevelFile.Read(filename);
        //    lf.Save(filename);
        //}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
    }
}
