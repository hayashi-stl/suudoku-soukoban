using Godot;
using System;

public class Fader : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    Node2D wipe;
    float _wipeFactor;
    float WipeFactor {
        get => _wipeFactor;
        set {
            _wipeFactor = value;
            Util.SetInstanceShaderParameter2D(wipe, "radius", value);
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        wipe = GetNode<Node2D>("%Wipe");
        _wipeFactor = 1.0f;

        var tween = CreateTween();
        tween.TweenProperty(this, "WipeFactor", 0.0f, Stage.EnterLevelFadeTime);
        tween.TweenInterval(Stage.EnterLevelBlackTime);
        tween.TweenProperty(this, "WipeFactor", 1.0f, Stage.EnterLevelFadeTime);
        tween.TweenCallback(this, "queue_free");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
