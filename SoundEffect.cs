using Godot;
using System;

public class SoundEffect : AudioStreamPlayer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private string Id => $"{Stream.GetInstanceId()}";

    private float Amplitude {
        get => Mathf.Pow(10, VolumeDb / 10);
        set => VolumeDb = Math.Max((float)Math.Log10(value) * 10, -80);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SoundEffectManager.Instance.Add(Id, this);
    }

    public void _on_AudioStreamPlayer_finished()
    {
        QueueFree();
    }

    public void FadeOut()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "Amplitude", 0f, 0.05f);
        tween.TweenCallback(this, "queue_free");
    }

    public override void _ExitTree()
    {
        SoundEffectManager.Instance.Remove(Id, this);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
