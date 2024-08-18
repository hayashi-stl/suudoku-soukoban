using Godot;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Block : EntityNode2D
{
    public enum BlockType {
        Push,
    }

    Sprite _activeOutline;
    List<MeshInstance2D> _activeMeshes = new List<MeshInstance2D>();

            
    BlockType _type = BlockType.Push;
    [Export]
    public BlockType Type {
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

    public override Entity LevelEntity(int id) {
        return new Ent(id, this);
    }
        
	public override LevelFile.EntityCustomData LevelEntityCustomParams() {
        return new LevelFile.BlockFile {
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
            BlockType.Push => ("%PushOutline", new List<string>(){ "%Push" }),
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

    public static EntityNode2D SpawnNode(LevelFile.BlockFile file) {
        var scene = Global.Scene.Block.Instance<Block>();
        scene.Type = file.Type;
        return scene;
    }
            
            
    public class Ent : Entity {
        Block ThisNode => (Block)EntityNode;

        public Ent(int id, Block node) : base(id, EntityType.Block) {
            EntityNode = node;
        }

        public BlockType BlockType_ => ThisNode.Type;

        public override Vector3I Gravity => base.Gravity;

        public override bool IsFixed() => false;
            
        public override bool IsBlock(Vector3I dir) => true;
        
        public override bool IsRigid(Vector3I dir) => true;
            
        public override bool IsPushable(Vector3I dir) => !IsFixed();
            
        public override EntityDef Def
        {
            get => 
                new EntityDef(Id, this, new LevelFile.BlockFile{
                    Type = ThisNode.Type
                });
        }
    }
}
