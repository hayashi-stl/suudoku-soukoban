using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class Yuu : Sprite
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public IEnumerable<SceneTreeTween> TweenWrong(float delay)
    {
        var tween = CreateTween();
        return new List<SceneTreeTween>(){ tween };
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
