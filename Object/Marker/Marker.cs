using Godot;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Marker : EntityNode2D
{
    public enum MarkerType {
        Rotate,
        Exit,
        ExitParallel,
    }

    public static bool IsTypeExit(MarkerType type) => type == MarkerType.Exit || type == MarkerType.ExitParallel;

    Sprite _activeOutline;
    List<MeshInstance2D> _activeMeshes = new List<MeshInstance2D>();

            
    MarkerType _type = MarkerType.Rotate;
    [Export]
    public MarkerType Type {
        get => _type;
        set {
            _type = value;
            if (_ready)
                UpdateTexture();
        }
    }

    float _flashFactor = 0.0f;
    float FlashFactor {
        get => _flashFactor;
        set {
            _flashFactor = value;
            if (_ready)
                UpdateTexture();
        }
    }

    float _exitAnimValue = 0.0f;
    const float ExitSwirlSpeed = 1.0f;
    const float ExitFlowSpeed = 0.5f;

    public override float WallLayerScale => 0.75f;

    public bool IsExit => IsTypeExit(Type);
                
    public override Entity LevelEntity(int id) {
        return new Ent(id, this);
    }
        
	public override LevelFile.EntityCustomData LevelEntityCustomParams() {
        return new LevelFile.MarkerFile {
            Type = Type
        };
    }
        
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _activeOutline = GetNode<Sprite>("%FixedOutline");
        _activeMeshes = new List<MeshInstance2D>(){ GetNode<MeshInstance2D>("%Fixed") };
        PrepareCommon();
        UpdateTexture();
    }

    protected override void UpdateTexture() {
        _activeOutline.Visible = false;
        _activeMeshes.ForEach(m => m.Visible = false);
        var (outlineName, meshNames) = Type switch {
            MarkerType.Rotate => ("%FixedOutline", new List<string>(){ "%Exit Frame", "%Exit Swirl" }),
            MarkerType.Exit => ("%FixedOutline", new List<string>(){ "%Exit Frame", "%Exit Swirl" }),
            MarkerType.ExitParallel => ("%FixedOutline", new List<string>(){ "%ExitParallel Frame", "%ExitParallel Flow" }),
            _ => throw new InvalidEnumArgumentException()
        };
        _activeOutline = GetNode<Sprite>(outlineName);
        _activeMeshes = meshNames.Select(m => GetNode<MeshInstance2D>(m)).ToList();
        _activeOutline.Visible = true;
        _activeMeshes.ForEach(m => m.Visible = true);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        ProcessCommon(delta);
        if (Engine.EditorHint)
            Rotation = Mathf.Round(Rotation / (Mathf.Tau / 4)) * (Mathf.Tau / 4);
        var noRotating = GetNode<Node2D>("%NoRotating");
        noRotating.Rotation = -Rotation;

        if (!Engine.EditorHint) {
            if (Type == MarkerType.Exit) {
                _exitAnimValue += ExitSwirlSpeed * delta;
                _exitAnimValue %= Mathf.Tau;
                var swirlTransform =
                    Util.Translation(new Vector2(0.5f, 0.5f)) *
                    Util.Rotation(_exitAnimValue) *
                    Util.Translation(new Vector2(-0.5f, -0.5f));
                Util.SetInstanceShaderParameter2D(_activeMeshes[1], "uv_transform", swirlTransform);
            } else if (Type == MarkerType.ExitParallel) {
                _exitAnimValue += ExitFlowSpeed * delta;
                _exitAnimValue %= 1.0f;
                var flowTransform = Util.Translation(new Vector2(0.0f, -_exitAnimValue));
                Util.SetInstanceShaderParameter2D(_activeMeshes[1], "uv_transform", flowTransform);
            }
        }
    }

    public override void OnSteppedOn() {
        FlashFactor = 1.0f;
        var tween = CreateTween();
        tween.TweenProperty(this, "FlashFactor", 0.0f, 0.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }

    public override void OnEntered()
    {
        FlashFactor = 0.5f;
        var tween = CreateTween();
        tween.TweenProperty(this, "FlashFactor", 0.0f, Stage.EnterLevelTweenTime).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }

    public static EntityNode2D SpawnNode(LevelFile.MarkerFile file) {
        var scene = Global.Scene.Marker.Instance<Marker>();
        scene.Type = file.Type;
        return scene;
    }
            
            
    public class Ent : Entity {
        Marker ThisNode => (Marker)EntityNode;

        public Ent(int id, Marker node) : base(id, EntityType.Marker) {
            EntityNode = node;
        }

        public MarkerType MarkerType_ => ThisNode.Type;

        public bool IsExit => ThisNode.IsExit;

        public override Vector3I Gravity => Vector3I.Zero;

        public override bool IsFixed() => true;
            
        public override bool IsBlock(Vector3I dir) => false;
        
        public override bool IsRigid(Vector3I dir) => false;
            
        public override bool IsPushable(Vector3I dir) => false;
            
        public override EntityDef Def
        {
            get => 
                new EntityDef(Id, this, new LevelFile.MarkerFile{
                    Type = ThisNode.Type
                });
        }
    }
}
