using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class Yuu : Sprite
{
    Sprite _wrong;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _wrong = GetNode<Sprite>("%Wrong");
    }

    public IEnumerable<SceneTreeTween> TweenWrong(float delay)
    {
        var tween = CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(_wrong, "visible", true, 0);
        tween.TweenProperty(_wrong, "modulate", _wrong.Modulate * new Color(1, 1, 1, 0), Entity.TweenTime);
        tween.TweenProperty(_wrong, "visible", false, 0);
        tween.TweenProperty(_wrong, "modulate", _wrong.Modulate, 0);
        return new List<SceneTreeTween>(){ tween };
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
