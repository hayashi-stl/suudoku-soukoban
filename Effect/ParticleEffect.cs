using Godot;
using System;
using System.Collections.Generic;

public class ParticleEffect : Node2D
{
    Particles2D _particles;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _particles = GetNode<Particles2D>("%Particles");
    }

    public IEnumerable<SceneTreeTween> TweenEffect(Vector3I tilePosition, Vector3I direction, float delay)
    {
        var (XY, Z) = Util.FromTileSpace(tilePosition);
        Position = XY;
        ZIndex += Z;
        Rotation = Vector2.Down.AngleTo((Vector2)direction.XY);

        var tween = CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(_particles, "emitting", true, 0.0f);
        if (_particles.OneShot) {
            tween.TweenInterval(_particles.Lifetime * 2);
            tween.TweenCallback(this, "queue_free");
        }
        return new List<SceneTreeTween>() { tween };
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
