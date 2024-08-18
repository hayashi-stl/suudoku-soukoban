using Godot;
using System;

public class ButtonStyle : Node
{
    Button _button;
    [Export]
    StyleBoxTexture HoverStyle { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _button = GetParent<Button>();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
