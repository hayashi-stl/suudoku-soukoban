using Godot;
using System;

[Tool]
public partial class Player : EntityNode2D
{
    Sprite _activeVisual;

    public override Entity LevelEntity(int id) {
        return new Ent(id, this);
    }
        
	public override LevelFile.EntityCustomData LevelEntityCustomParams() {
        return new LevelFile.PlayerFile();
    }
        
    protected override void UpdateTexture() {
        //texture = (preload("res://Object/Player/PlayerF.png") if layer == Level.Layer.FLOOR else
        //    preload("res://Object/Player/Player.png"))
    }
            
            
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        PrepareCommon();
        _activeVisual = GetNode<Sprite>("%Player");
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        if (Engine.EditorHint)
            Rotation = Mathf.Round(Rotation / (Mathf.Tau / 4)) * (Mathf.Tau / 4);
        ProcessCommon(delta);

        //if (!Engine.EditorHint) {
        //    var player = GetNode<AnimationPlayer>("Animation");
        //    var anim = player.GetAnimation("Blink");
        //    GD.Print("First track: ", anim.TrackGetPath(0));
        //}
    }
        
    public static EntityNode2D SpawnNode(LevelFile.PlayerFile file) {
        return Global.Scene.Player.Instance<Player>();
    }


    public class Ent : Entity {
        public Ent(int id, Player node) : base(id, EntityType.Player) {
            EntityNode = node;
        }

        public override bool IsFixed() => false;

        public override bool IsBlock(Vector3I dir) => false;

        public override bool IsRigid(Vector3I dir) => false;

        public override bool IsPushable(Vector3I dir) => true;

        public override EntityDef Def
        {
            get =>
                new EntityDef(Id, this, new LevelFile.PlayerFile{
                    
                });
        }
    }
}
