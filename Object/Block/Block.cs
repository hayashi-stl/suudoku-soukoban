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

    List<Sprite> _activeVisuals = new List<Sprite>();
    Yuu _yuu;

            
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

    bool _hasYuu;
    [Export]
    public bool HasYuu {
        get => _hasYuu;
        set {
            _hasYuu = value;
            if (_ready)
                UpdateTexture();
        }
    }
    

    public override Entity LevelEntity(int id) {
        return new Ent(id, this);
    }
        
	public override LevelFile.EntityCustomData LevelEntityCustomParams() {
        return new LevelFile.BlockFile {
            Type = Type,
            HasYuu = HasYuu,
        };
    }
        
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _activeVisuals = new List<Sprite>(){ GetNode<Sprite>("%Push") };
        _yuu = GetNode<Yuu>("%Yuu");
        PrepareCommon();
        UpdateTexture();
    }

    protected override void UpdateTexture() {
        _activeVisuals.ForEach(m => m.Visible = false);
        var visualNames = Type switch {
            BlockType.Push => new List<string>(){ "%Push" },
            _ => throw new InvalidEnumArgumentException()
        };
        _activeVisuals = visualNames.Select(m => GetNode<Sprite>(m)).ToList();
        _activeVisuals.ForEach(m => m.Visible = true);
        _yuu.Visible = HasYuu;
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

    public static EntityNode2D SpawnNode(LevelFile.BlockFile file) {
        var scene = Global.Scene.Block.Instance<Block>();
        scene.Type = file.Type;
        scene.HasYuu = file.HasYuu;
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
            
        public override IEnumerable<Yuu> Yuus => Enumerable.Repeat(ThisNode._yuu, ThisNode.HasYuu ? 1 : 0);
        public override EntityDef Def
        {
            get => 
                new EntityDef(Id, this, new LevelFile.BlockFile{
                    Type = ThisNode.Type,
                    HasYuu = ThisNode.HasYuu,
                });
        }
    }
}
