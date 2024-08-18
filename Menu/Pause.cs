using Godot;
using System;

public class Pause : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public void Init()
    {

    }

    public override void _Input(InputEvent @event)
    {
        if (GetTree().Paused && @event.IsActionPressed("pause")) {
            Visible = false;
            GetTree().Paused = false;
            GetTree().SetInputAsHandled();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!GetTree().Paused)
            return;
    }

    public void Action_Continue() {
        Visible = false;
        GetTree().Paused = false;
    }

    public void Action_ExitCourse() {
        var level = Util.AncestorOfType<Stage>(this).Level;
        GetTree().Paused = false;
        level.ReturnLevel();
    }

    public void Action_TitleMenu() {
        var level = Util.AncestorOfType<Stage>(this).Level;
        GetTree().Paused = false;
        level.ReturnToTitle();
    }
}
