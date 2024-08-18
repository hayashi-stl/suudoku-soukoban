using Godot;
using System;

public class PauseActionButton : Button
{
    public enum Action {
        Continue,
        ExitCourse,
        TitleMenu,
    }

    [Export]
    public Action ButtonAction { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var pause = Util.AncestorOfType<Pause>(this);

        switch (ButtonAction) {
            case Action.Continue:
                Connect("pressed", pause, "Action_Continue");
                break;

            case Action.ExitCourse:
                Connect("pressed", pause, "Action_ExitCourse");
                break;

            case Action.TitleMenu:
                Connect("pressed", pause, "Action_TitleMenu");
                break;
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
