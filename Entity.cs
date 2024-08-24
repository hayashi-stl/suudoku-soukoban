using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Entity
{
    public enum EntityType {
        Fixed,
        Player,
        Marker,
        Block,
    }

	public static readonly int NumTypes = Enum.GetNames(typeof(EntityType)).Length;

    public const int CoinZIndex = 10;
    public const int BaddyZIndex = 11;
    public const int PlayerZIndex = 12;
    public const int BlockZIndex = 13;

    // Information needed to spawn an entity node.
    public class EntityDef {
        public int Id { get; set; }
        public LevelFile.EntityFile File { get; set; }

        public EntityDef(int id, LevelFile.EntityFile file) {
            Id = id;
            File = file;
        }
        
        public EntityDef(int id, Entity ent, LevelFile.EntityCustomData customData) {
            Id = id;
            File = new LevelFile.EntityFile{
                Position = ent.Position,
                Direction = ent.Direction,
                Gravity = ent.Gravity,
                CustomData = customData
            };
        }

        EntityNode2D SpawnNode() {
            return File.CustomData switch
            {
                LevelFile.PlayerFile file => Player.SpawnNode(file),
                LevelFile.BlockFile file => Block.SpawnNode(file),
                LevelFile.MarkerFile file => Marker.SpawnNode(file),
                _ => throw new ArgumentException($"{File.CustomData} is invalid")
            };
        }

        void SetEntityParams(Entity ent) {
            ent.SetPosition(File.Position, false);
            ent.SetDirection(File.Direction, false);
        }

        // Spawns an entity. If the id is negative, this gives
        // the entity an unused id.
        public Entity Spawn(Level level) {
            var node = SpawnNode();
            level.AddChild(node);
            Entity ent = node.LevelEntity(Id >= 0 ? Id : Global.Instance(level).NextEntityID());
            SetEntityParams(ent);
            level.AddEntity(ent, File.Position);
            return ent;
        }

        public EntityNode2D SpawnInMaker(Maker maker) {
            var node = SpawnNode();
            maker.AddChild(node);
            node.Owner = node.GetTree().EditedSceneRoot;

            var (XY, ZIndex) = Util.FromTileSpace(File.Position);
            node.BasePosition = XY;
            node.ZIndex = ZIndex;

            var newRot = Vector2.Down.AngleTo(new Vector2(File.Direction.x, File.Direction.y));
            node.BaseRotation = newRot;

            return node;
        }
    }
            

    public const float TweenTime = 0.1f;
        
    // Unique ID that doesn't give the same entity in the past
    // two different IDs
    public int Id { get; set; }
    public EntityNode2D EntityNode { get; set; }
    public EntityType Type { get; set; } // determines movement pattern and priority
    Vector3I _pos; // true position of the entity, as opposed to visual position
    Vector3I _dir; // direction teh entity is facing. Must be a cardinal direction.
    int? _counter; // value of the entity's counter
    Vector2 _counterPos;
    float _counterScale;
    Vector2 _offsetPos;
    float _offsetScale;
	bool _alive = true;
	public bool Alive { get => _alive; }

    public Entity(int id, EntityType type) {
        Id = id;
        Type = type;
    }

    // Whether the entity is a fixed block. It can't even be moved by gravity.
    public virtual bool IsFixed() => false;
        
    // Whether the entity is a block on the relevant side; i.e. it can be stepped on
    public virtual bool IsBlock(Vector3I dir) => false;
        
    // Whether the entity is rigid on the relevant side; i.e. it can't be squished.
    // is_rigid(dir) and not is_block(dir) is invalid for now.
    public virtual bool IsRigid(Vector3I dir) => false;
        
    // Whether the entity can be pushed (on the relevant side) by an active force or block
    // by its very nature; disregarding surroundings.
    // Note that if this returns false, that doesn't mean the entity is fixed.
    // It can still move by gravity, for example.
    public virtual bool IsPushable(Vector3I dir) => false;

    public virtual IEnumerable<Yuu> Yuus => new List<Yuu>();

    public Yuu YuuAt(int index) => Yuus.ToList()[index];
        
    public Vector3I Position => _pos;

    // The direction the entity is facing.
    public Vector3I Direction => _dir;

    public String Debug() => $"{Type}: at {Position}, dir {Direction}";

    // Priority of movement.
    // Not based on type for now.
    // Movement direction priority: up, down, left, right
    // Entities in front move before entities behind
    public int[] MovePriority() {
        var first = _dir.y < 0 ? 0 :
            _dir.y > 0 ? 1 :
            _dir.x < 0 ? 2 : 3;
        var second = -(int)((Vector3)_pos).Dot((Vector3)_dir);
        return new int[]{first, second};
    }

    // The direction of gravity for the entity.
    public virtual Vector3I Gravity => Vector3I.Forward;

    // The parameters needed to spawn this entity
    public virtual EntityDef Def => null;

    // Returns the value to use for tweening
    public virtual (Vector2 Pos, float Scale) SetPosition(Vector3I pos, bool tween) {
        _pos = pos;
        var newPos = Util.FromTileSpace(pos);
        var scale = pos.z > 0 ? EntityNode.WallLayerScale : 1.0f;
        if (!tween) {
            EntityNode.BasePosition = newPos.XY;
            EntityNode.BaseScale = scale;
        }
        EntityNode.ZIndex = newPos.ZIndex;
        return (newPos.XY, scale);
    }

    public (bool Changed, Vector2 Pos, float Scale) SetOffsetPosition(Vector2 pos, float scale, bool tween) {
        var newPos = pos * Util.TileSize;
        bool changed = pos != _offsetPos || scale != _offsetScale;
        _offsetPos = pos;
        _offsetScale = scale;
        if (!tween) {
            EntityNode.OffsetPosition = newPos;
            EntityNode.OffsetScale = scale;
        }
        return (changed, newPos, scale);
    }

    // Returns the value to use for tweening
    public virtual float SetDirection(Vector3I dir, bool tween) {
        _dir = dir;
        float oldRot = EntityNode.BaseRotation;
        var newRot = Vector2.Down.AngleTo(new Vector2(dir.x, dir.y));
        var toRound = (newRot - oldRot) / Mathf.Tau;
        newRot = (toRound - Mathf.Round(toRound)) * Mathf.Tau + oldRot;
        if (!tween)
            EntityNode.BaseRotation = newRot;
        return newRot;
    }

    public virtual void OnSteppedOn() => EntityNode.OnSteppedOn();
    public virtual void OnTargetStateChanged(bool active) => EntityNode.OnTargetStateChanged(active);
    public virtual void OnEntered() => EntityNode.OnEntered();
    public virtual void OnMoveStart(bool gravity) => EntityNode.OnMoveStart(gravity);
    public virtual void OnLevelClearChanged(bool levelClear) => EntityNode.OnLevelClearChanged(levelClear);

    public Vector2 Squish(Vector2I dir, bool tween) {
        Kill(tween);
        return (Vector2)dir;
    }

	public void Kill(bool tween) {
        _alive = false;
        if (!tween)
            EntityNode.QueueFree();
    }

    // Just a killing method. Actual level entering is handled elsewhere
    public Vector2 EnterLevel(Vector3I exitDir, bool tween) {
        Kill(tween);
        return (Vector2)exitDir.XY * 0.45f * Util.TileSize;
    }


    public IEnumerable<SceneTreeTween> TweenPosition(Vector2 pos, float scale, float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(EntityNode, "BasePosition", pos, TweenTime);
        tween.Parallel().TweenProperty(EntityNode, "BaseScale", scale, TweenTime);
        return new List<SceneTreeTween>(){ tween };
    }

    public IEnumerable<SceneTreeTween> TweenOffsetPosition(Vector2 pos, float scale, float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(EntityNode, "OffsetPosition", pos, TweenTime);
        tween.Parallel().TweenProperty(EntityNode, "OffsetScale", scale, TweenTime);
        return new List<SceneTreeTween>(){ tween };
    }

    public IEnumerable<SceneTreeTween> TweenBumpOffsetPosition(Vector2 dir, float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(EntityNode, "BumpOffsetPosition", dir * 0.15f, TweenTime * 0.25f);
        tween.TweenProperty(EntityNode, "BumpOffsetPosition", Vector2.Zero, TweenTime * 0.75f);
        return new List<SceneTreeTween>(){ tween };
    }

    public IEnumerable<SceneTreeTween> TweenDirection(float angle, float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(EntityNode, "BaseRotation", angle, TweenTime);
        return new List<SceneTreeTween>(){ tween };
    }

    public IEnumerable<SceneTreeTween> TweenExistence(bool exists, float delay) {
        if (!exists) {
            var tween = EntityNode.GetTree().CreateTween();
            tween.TweenInterval(delay);
            tween.TweenCallback(EntityNode, "queue_free");
            return new List<SceneTreeTween>(){ tween };
        }
        return new List<SceneTreeTween>(){};
    }
        
    public IEnumerable<SceneTreeTween> TweenSquish(Vector2 dir, float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(EntityNode, "SquishDirection", dir, 0.0f);
        tween.TweenProperty(EntityNode, "SquishScale", 0.0f, TweenTime);
        tween.TweenCallback(EntityNode, "queue_free");
        return new List<SceneTreeTween>(){ tween };
    }
        
    public IEnumerable<SceneTreeTween> TweenFall(float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.TweenProperty(EntityNode, "PitFallScale", 0.0f, TweenTime);
        tween.TweenCallback(EntityNode, "queue_free");
        return new List<SceneTreeTween>(){ tween };
    }
        
    public IEnumerable<SceneTreeTween> TweenEnterLevel(Vector2 offsetPos, float delay) {
        var tween = EntityNode.GetTree().CreateTween();
        tween.TweenInterval(delay);
        tween.SetParallel();
        tween.TweenProperty(EntityNode, "PitFallScale", 0.0f, Stage.EnterLevelTweenTime);
        tween.TweenProperty(EntityNode, "EnterLevelOffsetPosition", offsetPos, Stage.EnterLevelTweenTime).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
        tween.TweenCallback(EntityNode, "queue_free").SetDelay(Stage.EnterLevelTweenTime);
        return new List<SceneTreeTween>(){ tween };
    }
        
    // Called when squished. Returns whether the entity should be deleted.
    public virtual bool HandleSquished() {
        return true;
    }

    public partial class Fixed : Entity {
        public Fixed(int id, Vector3I position) : base(id, EntityType.Fixed) {
            _pos = position;
            _dir = Vector3I.Zero;
            _counter = null;
        }
            
        public override bool IsFixed() => true;
        
        public override bool IsBlock(Vector3I dir) => true;
                
        public override bool IsRigid(Vector3I dir) => true;
            
        public override bool IsPushable(Vector3I dir) => false;

        public override void OnSteppedOn() {}
        public override void OnTargetStateChanged(bool active) {}
        public override void OnEntered() {}
        public override void OnMoveStart(bool gravity) {}
        public override void OnLevelClearChanged(bool levelClear) {}
    }

}
