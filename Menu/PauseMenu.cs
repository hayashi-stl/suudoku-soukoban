using Godot;
using System;

public class PauseMenu : Control
{
    [Export]
    NodePath InitFocus { get; set; }
    bool _prevVisible;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public override void _Process(float delta)
    {
        bool visible = IsVisibleInTree();
        if (visible && !_prevVisible) {
            GetNode<Control>(InitFocus).GrabFocus();
        }
        _prevVisible = visible;
    }
}
