using Godot;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Marker : EntityNode2D
{
    public enum MarkerType {
        Target,
        Goal,
        Exit,
        ExitParallel,
    }

    public static bool IsTypeExit(MarkerType type) => type == MarkerType.Exit || type == MarkerType.ExitParallel;

    List<Sprite> _activeVisuals = new List<Sprite>();

            
    MarkerType _type = MarkerType.Target;
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
        _activeVisuals = new List<Sprite>(){ GetNode<Sprite>("%Target") };
        PrepareCommon();
        UpdateTexture();
    }

    protected override void UpdateTexture() {
        _activeVisuals.ForEach(m => m.Visible = false);
        var visualNames = Type switch {
            MarkerType.Target => new List<string>(){ "%Target" },
            MarkerType.Goal => new List<string>(){ "%Goal" },
            MarkerType.Exit => new List<string>(){ "%Exit Swirl" },
            MarkerType.ExitParallel => new List<string>(){ "%ExitParallel Flow" },
            _ => throw new InvalidEnumArgumentException()
        };
        _activeVisuals = visualNames.Select(m => GetNode<Sprite>(m)).ToList();
        _activeVisuals.ForEach(m => m.Visible = true);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        ProcessCommon(delta);
        if (Engine.EditorHint)
            Rotation = Mathf.Round(Rotation / (Mathf.Tau / 4)) * (Mathf.Tau / 4);
        var noRotating = GetNode<Node2D>("%NoRotating");
        noRotating.Rotation = -Rotation;
    }

    public override void OnSteppedOn() {
    }

    public override void OnEntered() {
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
