using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PR = Level.PushRelation;

[Tool]
public partial class Level : Node2D
{
    [Export]
    public string LevelName { get; set; }
    public Stage LevelStage { get; set; }
    bool _clear;
    bool _exiting;
    LevelFile _levelFile;
    Vector3I? _enterPosition;

    class Entry {
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

        public void Add(Entity ent) => entities[ent.Id] = ent;

        public void Remove(Entity ent) => entities.Remove(ent.Id);

        public bool HasFixedBlock() => entities.Values.Any(ent => ent.IsFixed());

        public bool HasBlock(Vector3I dir) => entities.Values.Any(ent => ent.IsBlock(dir));

        // Blocks that have a node. Returns Array[Entity]
        public IEnumerable<Entity> NodeBlocks(Vector3I dir) {
            return entities.Values.Where(ent => ent.EntityNode != null && ent.IsBlock(dir));
        }
            
        // Get entities that meet a certain condition of type Ent->bool
        public IEnumerable<Entity> Filter(Func<Entity, bool> condition) {
            return entities.Values.Where(condition);
        }
            
        public IEnumerable<Entity> WithType(Entity.EntityType type) {
            return entities.Values.Where(ent => ent.Type == type);
        }
    }
        
        
    public abstract class Action {
        public abstract Action Do(Level level);
    }
        
    // An entity was deleted; to undo it, add it back	
    public class AddAction : Action {
        Entity.EntityDef _def;
        
        public AddAction(Entity.EntityDef def) { _def = def; }
        
        public override Action Do(Level level) {
            Entity entity = _def.Spawn(level);
            return new DeleteAction(entity);
        }
    }
            
    // An entity was added; to undo it, delete it
    public class DeleteAction : Action {
        int _entityId;
        
        public DeleteAction(Entity entity) { _entityId = entity.Id; }
        
        public override Action Do(Level level) {
            return level.DeleteEntity(level._entitiesById[_entityId], false);
        }
    }
            
    // An entity moved.
    public class MoveAction : Action {
        int _entityId;
        Vector3I _dest;
        
        public MoveAction(Entity entity, Vector3I dest) {
            _entityId = entity.Id;
            _dest = dest;
        }
        
        public override Action Do(Level level) {
            return level.MoveEntity(level._entitiesById[_entityId], _dest, false);
        }
    }
            
    // An entity rotated. Much simpler than moving.
    public class RotateAction : Action {
        int _entityId;
        Vector3I _dest;
        
        public RotateAction(Entity entity, Vector3I dest) {
            _entityId = entity.Id;
            _dest = dest;
        }
        
        public override Action Do(Level level) {
            return level.RotateEntity(level._entitiesById[_entityId], _dest, false);
        }
    }
            
    class BatchAction : Action {
        public enum Tag {
            None,
            Restart,
        }
        public Tag Tag_ { get; private set; }
        Stack<Action> _actions = new Stack<Action>();
        
        public BatchAction(IEnumerable<Action> actions, Tag tag) {
            Tag_ = tag;
            _actions = new Stack<Action>(actions);
        }
        
        public override Action Do(Level level) {
            List<Action> reversed = new List<Action>();
            bool exists = Util.TryPop(_actions, out var action);
            while (exists) {
                reversed.Add(action.Do(level));
                exists = Util.TryPop(_actions, out action);
            }
            return new BatchAction(reversed, Tag_);
        }
    }
            
    class UndoStack {
        readonly List<Action> _actions = new List<Action>();
        readonly Stack<BatchAction> _batched = new Stack<BatchAction>();
        
        public void Add(Action action) {
            _actions.Add(action);
        }
            
        public void AddArray(IEnumerable<Action> action_arr) {
            _actions.AddRange(action_arr);
        }
            
        public void Batch(BatchAction.Tag tag = BatchAction.Tag.None) {
            if (_actions.Count > 0) {
                _batched.Push(new BatchAction(_actions, tag));
                _actions.Clear();
            }
        }
            
        public void Undo(Level level) {
            var exists = Util.TryPop(_batched, out var action);
            if (exists)
                action.Do(level);
        }

        // Gets the tag of the batch action on the top of the stack.
        // Returns null if there isn't a batch action at the top
        public BatchAction.Tag? TopBatchActionTag() {
            if (_actions.Count > 0 || _batched.Count == 0)
                return null;
            return _batched.Peek().Tag_;
        }
    }

    abstract class TweenEntry {
        // for lack of tagged unions, a string will be used
        public abstract string Key { get; }
        // acting entity, used for detecting conflicts. Null if none.
        public abstract Entity ActingEntity { get; }

        public abstract IEnumerable<SceneTreeTween> Execute(Level level, float delay);
    }

    class TweenEntityPositionEntry : TweenEntry {
        Entity _entity;
        Vector2 _position;
        float _scale;
        public override string Key => $"EntityPosition {_entity.Id}";
        public override Entity ActingEntity => _entity;

        public TweenEntityPositionEntry(Entity entity, Vector2 position, float scale) {
            _entity = entity;
            _position = position;
            _scale = scale;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenPosition(_position, _scale, delay);
        }
    }

    class TweenEntityOffsetPositionEntry : TweenEntry {
        Entity _entity;
        Vector2 _position;
        float _scale;
        public override string Key => $"EntityOffsetPosition {_entity.Id}";
        public override Entity ActingEntity => null; // This is a visual effect and can't cause conflicts

        public TweenEntityOffsetPositionEntry(Entity entity, Vector2 position, float scale) {
            _entity = entity;
            _position = position;
            _scale = scale;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenOffsetPosition(_position, _scale, delay);
        }
    }

    class TweenEntityBumpPositionEntry : TweenEntry {
        Entity _entity;
        Vector2 _direction;
        public override string Key => $"BumpOffsetPosition {_entity.Id}";
        public override Entity ActingEntity => null; // This is a visual effect and can't cause conflicts

        public TweenEntityBumpPositionEntry(Entity entity, Vector2 direction) {
            _entity = entity;
            _direction = direction;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenBumpOffsetPosition(_direction, delay);
        }
    }

    class TweenEntityDirectionEntry : TweenEntry {
        Entity _entity;
        float _angle;
        public override string Key => $"EntityDirection {_entity.Id}";
        public override Entity ActingEntity => _entity;

        public TweenEntityDirectionEntry(Entity entity, float angle) {
            _entity = entity;
            _angle = angle;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenDirection(_angle, delay);
        }
    }

    class TweenEntityExistenceEntry : TweenEntry {
        Entity _entity;
        bool _exists;
        float _delay;
        public override string Key => $"EntityExists {_entity.Id}";
        public override Entity ActingEntity => _entity;

        public TweenEntityExistenceEntry(Entity entity, bool exists, float delay) {
            _entity = entity;
            _exists = exists;
            _delay = delay;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenExistence(_exists, delay + _delay);
        }
    }

    class TweenEntitySquishEntry : TweenEntry {
        Entity _entity;
        Vector2 _direction;
        public override string Key => $"EntitySquish {_entity.Id}";
        public override Entity ActingEntity => _entity;

        public TweenEntitySquishEntry(Entity entity, Vector2 direction) {
            _entity = entity;
            _direction = direction;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenSquish(_direction, delay);
        }
    }

    class TweenEntityPitFallEntry : TweenEntry {
        Entity _entity;
        public override string Key => $"EntityPitFall {_entity.Id}";
        public override Entity ActingEntity => _entity;

        public TweenEntityPitFallEntry(Entity entity) {
            _entity = entity;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenFall(delay);
        }
    }

    class TweenEntityEnterLevelEntry : TweenEntry {
        Entity _entity;
        Vector2 _offset;
        public override string Key => $"EntityEnterLevel {_entity.Id}";
        public override Entity ActingEntity => _entity;

        public TweenEntityEnterLevelEntry(Entity entity, Vector2 offset) {
            _entity = entity;
            _offset = offset;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _entity.TweenEnterLevel( _offset, delay);
        }
    }

    class TweenParticleEffectEntry : TweenEntry {
        ParticleEffect _effect;
        Vector3I _position;
        Vector3I _direction;
        float _delay;
        ulong _id;
        static ulong nextID;
        public override string Key => $"ParticleEffect {_id}";
        public override Entity ActingEntity => null;

        public TweenParticleEffectEntry(ParticleEffect effect, Vector3I position, Vector3I direction, float delay) {
            _effect = effect;
            _position = position;
            _direction = direction;
            _delay = delay;
            _id = nextID;
            ++nextID;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return _effect.TweenEffect(_position, _direction, delay + _delay);
        }
    }

    class TweenSoundEffectEntry : TweenEntry {
        PackedScene _sfx;
        float _delay;
        public override string Key => $"SoundEffect {_sfx.ResourcePath}";
        public override Entity ActingEntity => null;

        public TweenSoundEffectEntry(PackedScene sfx, float delay) {
            _sfx = sfx;
            _delay = delay;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            var tween = level.GetTree().CreateTween();
            tween.TweenInterval(delay + _delay);
            tween.TweenCallback(Global.Instance(level), "PlaySound", new Godot.Collections.Array(_sfx));
            return new List<SceneTreeTween>(){ tween };
        }
    }

    class TweenEntityEffectEntry : TweenEntry {
        Entity _entity;
        EntityEffect _effect;
        float _delay;
        ulong _id;
        static ulong nextID;
        public override string Key => $"EntityEffect {_id}";
        public override Entity ActingEntity => null;

        public TweenEntityEffectEntry(Entity entity, EntityEffect effect, float delay) {
            _entity = entity;
            _effect = effect;
            _delay = delay;
            _id = nextID;
            ++nextID;
        }

        public override IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            var tween = level.GetTree().CreateTween();
            tween.TweenInterval(delay + _delay);
            tween.TweenCallback(level, "PlayEntityEffect", new Godot.Collections.Array(_entity.Id, _effect));
            return new List<SceneTreeTween>(){ tween };
        }
    }

    class EntityEffect : Reference
    {
        public class OnSteppedOn : EntityEffect {}
        public class OnPunched : EntityEffect {}
        public class OnEntered : EntityEffect {}
        public class OnMoveStart : EntityEffect { public bool Gravity { get; set; }}
        public class OnLevelClearChanged : EntityEffect { public bool LevelClear { get; set; } }
    }


    void PlayEntityEffect(int entityID, EntityEffect effect) {
        if (_entitiesById.TryGetValue(entityID, out var entity))
            switch (effect) {
                case EntityEffect.OnSteppedOn e: entity.OnSteppedOn(); break;
                case EntityEffect.OnPunched e: entity.OnPunched(); break;
                case EntityEffect.OnEntered e: entity.OnEntered(); break;
                case EntityEffect.OnMoveStart e: entity.OnMoveStart(e.Gravity); break;
                case EntityEffect.OnLevelClearChanged e: entity.OnLevelClearChanged(e.LevelClear); break;
            }
    }


    // Every tween in a group happens at the same time.
    class TweenGroup {
        public Dictionary<string, TweenEntry> Entries {get; private set;} = new Dictionary<string, TweenEntry>();

        public bool Any => Entries.Any();

        public void Add(TweenEntry entry) {
            Entries[entry.Key] = entry;
        }

        public void Clear() {
            Entries.Clear();
        }

        public IEnumerable<SceneTreeTween> Execute(Level level, float delay) {
            return Entries.Values.SelectMany(e => e.Execute(level, delay));
        }
    }

    // Tweens in different groups happen sequentially,
    // with overlap if there's no interference.
    class TweenGrouping {
        List<TweenGroup> _groups = new List<TweenGroup>();
        TweenGroup _currGroup = new TweenGroup();
        float _totalTweenTime;
        List<SceneTreeTween> _tweens = new List<SceneTreeTween>();

        public void AddTween(TweenEntry entry) {
            _currGroup.Add(entry);
        }

        public void BatchTweens() {
            if (_currGroup.Any) {
                _groups.Add(_currGroup);
                _currGroup = new TweenGroup();
            }
        }

        public void FlushAndClear() {
            foreach (var tween in _tweens)
                tween.CustomStep(_totalTweenTime);
            _groups.Clear();
            _currGroup.Clear();
            _tweens.Clear();
        }

        public void Execute(Level level) {
            // Calculate conflicts.
            // A group conflicts with another if they share an acting entity.
            var entitySets = _groups.Select(
                g => g.Entries.Values.Select(e => e.ActingEntity).Where(e => e != null).ToHashSet()
            ).ToList();

            const float NoConflictDelay = Entity.TweenTime * 0.125f;
            const int MaxConflictDistance = (int)(Entity.TweenTime / NoConflictDelay);
            var delays = new List<float>(){ 0.0f };
            int lastConflict = 0;
            for (int i = 1; i < entitySets.Count; ++i) {
                bool conflicted = false;
                for (int j = i - 1; j >= Math.Max(lastConflict, i - MaxConflictDistance); --j) {
                    if (entitySets[i].Intersect(entitySets[j]).Any()) {
                        lastConflict = i;
                        delays.Add(delays[j] + Entity.TweenTime);
                        conflicted = true;
                        break;
                    }
                }
                if (!conflicted)
                    delays.Add(delays.Last() + NoConflictDelay);
            }

            foreach (var (group, delay) in _groups.Zip(delays, (a, b) => (a, b))) {
                _tweens.AddRange(group.Execute(level, delay));
            }

            if (delays.Count > 0)
                _totalTweenTime = delays.Last() + Entity.TweenTime + 1.0f;
        }
    }

    class GameButton {
        public enum Action {
            Up,
            Down,
            Left,
            Right,
            Wait,
            Undo,
            Enter,
            Exit,
        }

        public static int NumActions => Enum.GetNames(typeof(Action)).Length;

        string _name;
        bool _repeatEvents;
        public bool JustPressed { get; private set; }
        public bool Held { get; private set; }
        double _heldTime;

        public GameButton(string name, bool repeatEvents) {
            _name = name;
            _repeatEvents = repeatEvents;
        }

        public int RepeatedEventFloorDiv(double time) {
            const double INIT_DELAY = 0.25;
            const double MIN_DELAY = 0.05;
            const double INIT_DERIV = 1.0 / INIT_DELAY;
            const double MAX_DERIV = 1.0 / MIN_DELAY;
            if (time > Math.Log(MAX_DERIV / INIT_DERIV)) {
                double value = MAX_DERIV - INIT_DERIV; // many cancellations happened here
                value += (time - Math.Log(MAX_DERIV / INIT_DERIV)) * MAX_DERIV;
                return (int)Math.Floor(value);
            }
            return (int)Math.Floor(INIT_DERIV * Math.Exp(time) - INIT_DERIV);
        }

        public void Update(double delta) {
            bool held = Input.IsActionPressed(_name);
            if (held) {
                _heldTime = !Held ? 0.0 : _heldTime + delta;
                JustPressed = false;
                if (held && !Held)
                    JustPressed = true;
                else if (_repeatEvents && RepeatedEventFloorDiv(_heldTime - delta) != RepeatedEventFloorDiv(_heldTime))
                    JustPressed = true;
                Held = true;
            } else {
                JustPressed = false;
                Held = false;
            }
        }
    }

    class DefeatParams {
        public class Normal : DefeatParams {}
        public class Punch : DefeatParams {
            public const float KillDelay = 0.025f;
        }

        public class Squish : DefeatParams {
            public Vector2I Direction { get; set; }
        }
        public class Fall : DefeatParams {}
        public class EnterLevel : DefeatParams {
            public Vector3I ExitDirection { get; set; }
        }
    }

    List<GameButton> _buttons = new List<GameButton>();
        
    UndoStack _undoStack = new UndoStack();
        
    int _minX;
    int _minY;
    int _sizeX;
    int _sizeY;
    public const int MinZ = 0;
    public const int SizeZ = 2; // floor layer and wall layer
    enum Layer { Floor, Wall }

    readonly List<Entry> _entries = new List<Entry>();
    readonly List<Entry> _entriesByType = new List<Entry>(); // Keys are Entity.Type
    readonly Dictionary<int, Entity> _entitiesById = new Dictionary<int, Entity>();
    readonly List<PuzzleInput> _inputs = new List<PuzzleInput>();
    Entry _pitEntry;
    readonly HashSet<Entity> _movedInStep = new HashSet<Entity>();
    readonly TweenGrouping _tweenGrouping = new TweenGrouping();
    float _shakeMagnitude = 0.0f;

    TileMap _regionMap;

    Vector2 _basePosition;
    public Vector2 BasePosition {
        get { return _basePosition; }
        set {
            _basePosition = value;
            Position = _basePosition;
        }
    }

    Entry DefaultEntry(Vector3I pos) {
        var entry = new Entry();
        entry.Add(new Entity.Fixed(-1, pos));
        return entry;
    }
        
    Entry EntryAt(Vector3I pos) {
        return 
            _minX <= pos.x && pos.x < _minX + _sizeX &&
            _minY <= pos.y && pos.y < _minY + _sizeY &&
            MinZ <= pos.z && pos.z < MinZ + SizeZ
             ? _entries[((pos.z - MinZ) * _sizeY + (pos.y - _minY)) * _sizeX + (pos.x - _minX)]
             : pos.z < 0 ? _pitEntry : DefaultEntry(pos);
    }
        
    public DeleteAction AddEntity(Entity ent, Vector3I pos, bool tween = true) {
        EntryAt(pos).Add(ent);
        _entriesByType[(int)ent.Type].Add(ent);
        _entitiesById[ent.Id] = ent;
        return new DeleteAction(ent);
    }

    void AddEntityUndoable(Entity ent, Vector3I pos) => _undoStack.Add(AddEntity(ent, pos));

    AddAction DeleteEntity(Entity ent, bool tween = true) =>
        DeleteEntity(ent, new DefeatParams.Normal(), tween);

    AddAction DeleteEntity(Entity ent, DefeatParams param, bool tween = true) {
        var def = ent.Def;
        EntryAt(ent.Position).Remove(ent);
        _entriesByType[(int)ent.Type].Remove(ent);
        _entitiesById.Remove(ent.Id);

        switch (param) {
            case DefeatParams.Normal p:
                ent.Kill(tween); // Invalidate in case there's references left
                if (tween)
                    _tweenGrouping.AddTween(new TweenEntityExistenceEntry(ent, false, Entity.TweenTime));
                break;
            case DefeatParams.Punch p:
                ent.Kill(tween);
                if (tween) {
                    _tweenGrouping.AddTween(new TweenEntityExistenceEntry(ent, false, DefeatParams.Punch.KillDelay));
                    SpawnParticleEffect(Global.ParticleEffect.BaddyPoof, ent.Position, Vector3I.Up, DefeatParams.Punch.KillDelay);
                    _tweenGrouping.AddTween(new TweenSoundEffectEntry(Global.SFX.Poof, DefeatParams.Punch.KillDelay));
                }
                break;
            case DefeatParams.Squish p:
                var dir = ent.Squish(p.Direction, tween);
                if (tween) {
                    _tweenGrouping.AddTween(new TweenEntitySquishEntry(ent, dir));
                    _tweenGrouping.AddTween(new TweenSoundEffectEntry(Global.SFX.Squish, 0));
                }
                break;
            case DefeatParams.Fall p:
                ent.Kill(tween);
                if (tween) {
                    _tweenGrouping.AddTween(new TweenEntityPitFallEntry(ent));
                }
                break;
            case DefeatParams.EnterLevel p:
                dir = ent.EnterLevel(p.ExitDirection, tween);
                if (tween) {
                    _tweenGrouping.AddTween(new TweenEntityEnterLevelEntry(ent, dir));
                }
                break;
        };
        return new AddAction(def);
    }

    void DeleteEntityUndoable(Entity ent, bool tween = true) =>
        _undoStack.Add(DeleteEntity(ent, tween));

    void DeleteEntityUndoable(Entity ent, DefeatParams param, bool tween = true) =>
        _undoStack.Add(DeleteEntity(ent, param, tween));

    // Not intended for fixed blocks
    MoveAction MoveEntity(Entity ent, Vector3I new_pos, bool tween = true) {
        var old_pos = ent.Position;
        EntryAt(ent.Position).Remove(ent);
        EntryAt(new_pos).Add(ent);
        var (XY, Scale) = ent.SetPosition(new_pos, tween);
        if (tween)
            _tweenGrouping.AddTween(new TweenEntityPositionEntry(ent, XY, Scale));
        _movedInStep.Add(ent);
        return new MoveAction(ent, old_pos);
    }

    void MoveEntityUndoable(Entity ent, Vector3I new_pos) => _undoStack.Add(MoveEntity(ent, new_pos));

    RotateAction RotateEntity(Entity ent, Vector3I new_dir, bool tween = true) {
        var old_dir = ent.Direction;
        var angle = ent.SetDirection(new_dir, tween);
        if (tween)
            _tweenGrouping.AddTween(new TweenEntityDirectionEntry(ent, angle));
        return new RotateAction(ent, old_dir);
    }

    void RotateEntityUndoable(Entity ent, Vector3I new_dir) => _undoStack.Add(RotateEntity(ent, new_dir));

    void SpawnParticleEffect(PackedScene effectScene, Vector3I position, Vector3I direction, float delay) {
        var effect = effectScene.Instance<ParticleEffect>();
        AddChild(effect);
        _tweenGrouping.AddTween(new TweenParticleEffectEntry(effect, position, direction, delay));
    }

    public Rect2I LevelRect() {
        return new Rect2I(_levelFile.Base.x, _levelFile.Base.y, _levelFile.Size.x, _levelFile.Size.y);
    }

    public static void DoBorders(TileMap regionMap, TileMap horzBorders, TileMap vertBorders) {
        horzBorders.Clear();
        vertBorders.Clear();

        var map = regionMap.GetUsedCells().Cast<Vector2>().ToDictionary(v => (Vector2I)v, v => (LevelFile.PlayTile)regionMap.GetCell((int)v.x, (int)v.y));
        var mapOffsetHorz = map.ToDictionary(pair => pair.Key + Vector2I.Right, pair => pair.Value);
        var mapOffsetVert = map.ToDictionary(pair => pair.Key + Vector2I.Down, pair => pair.Value);

        for (int i = 0; i < 2; ++i) {
            var mapOffset = i == 0 ? mapOffsetHorz : mapOffsetVert;
            var borders = i == 0 ? vertBorders : horzBorders;
            var vecScale = i == 0 ? new Vector2I(2, 1) : new Vector2I(1, 2);
            var vecOffset = i == 0 ? new Vector2I(-1, 0) : new Vector2I(0, -1);
            var cells = map.Keys.Concat(mapOffset.Keys).ToHashSet();

            foreach (var cell in cells) {
                var region = Util.GetOr(map, cell, LevelFile.PlayTile.Invalid);
                var region2 = Util.GetOr(mapOffset, cell, LevelFile.PlayTile.Invalid);
                var value = TileMap.InvalidCell;

                if (region != region2) {
                    value = 0;
                } else if (LevelFile.IsPlayTileFloor(region)) {
                    value = 2;
                }

                if (value != TileMap.InvalidCell) {
                    var borderCell = cell * vecScale + vecOffset;
                    borders.SetCell(borderCell.x, borderCell.y, value);
                    borderCell = cell * vecScale;
                    borders.SetCell(borderCell.x, borderCell.y, value + 1);
                }
            }
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _buttons = Enumerable.Range(0, GameButton.NumActions).Select(i => null as GameButton).ToList();
        _buttons[(int)GameButton.Action.Up]    = new GameButton("up",    true);
        _buttons[(int)GameButton.Action.Down]  = new GameButton("down",  true);
        _buttons[(int)GameButton.Action.Left]  = new GameButton("left",  true);
        _buttons[(int)GameButton.Action.Right] = new GameButton("right", true);
        _buttons[(int)GameButton.Action.Wait]  = new GameButton("wait",  true);
        _buttons[(int)GameButton.Action.Undo]  = new GameButton("undo",  true);
        _buttons[(int)GameButton.Action.Enter] = new GameButton("enter", false);
        _buttons[(int)GameButton.Action.Exit]  = new GameButton("back",  false);

        if (Engine.EditorHint)
            return;

        _pitEntry = new Entry();
        
        var rect = LevelRect();
        _minX = rect.Position.x;
        _minY = rect.Position.y;
        _sizeX = rect.Size.x;
        _sizeY = rect.Size.y;
        for (int i = 0; i < _sizeX * _sizeY * SizeZ; ++i)
            _entries.Add(new Entry());
            
        for (int i = 0; i < Entity.NumTypes; ++i)
            _entriesByType.Add(new Entry());

        _regionMap = GetNode<TileMap>("%RegionMap");
            
        // Fill entries from the grid map
        for (int z = MinZ; z < MinZ + SizeZ; ++z)
            for (int y = _minY; y < _minY + _sizeY; ++y)
                for (int x = _minX; x < _minX + _sizeX; ++x) {
                    var cellPosition = new Vector3I(x, y, z);
                    var mapPosition = cellPosition - _levelFile.Base;
                    var cellIndex = (mapPosition.z * _levelFile.Size.y + mapPosition.y) * _levelFile.Size.x + mapPosition.x;
                    var lowerCellIndex = ((mapPosition.z - 1) * _levelFile.Size.y + mapPosition.y) * _levelFile.Size.x + mapPosition.x;
                    var cell = _levelFile.Map[cellIndex];
                    if (cell != LevelFile.FileTile.Invalid) {
                        _regionMap.SetCell(x, y, (int)LevelFile.ToPlayTile(cell, z));

                        var ent = new Entity.Fixed(Global.Instance(this).NextEntityID(), cellPosition);
                        AddEntity(ent, cellPosition);
                    }

                    // Invalid is out-of-bounds
                    if (z == 1 && cell == LevelFile.FileTile.Invalid && _levelFile.Map[lowerCellIndex] == LevelFile.FileTile.Invalid) {
                        var ent = new Entity.Fixed(Global.Instance(this).NextEntityID(), cellPosition);
                        AddEntity(ent, cellPosition);
                    }
                }

        DoBorders(_regionMap, GetNode<TileMap>("LineHorzMap"), GetNode<TileMap>("LineVertMap"));

        LoadInEntities(false);
    }

    void LoadInEntities(bool undoable) {
        List<Entity> addedEntities = new List<Entity>();
        foreach (var entData in _levelFile.Entities) {
            var def = new Entity.EntityDef(Global.Instance(this).NextEntityID(), entData);
            if (_enterPosition is Vector3I v && def.File.CustomData is LevelFile.PlayerFile)
                def.File.Position = v;

            var entity = def.Spawn(this);
            var action = AddEntity(entity, entity.Position);
            if (undoable)
                _undoStack.Add(action);    
            addedEntities.Add(entity);
        }
        
        AdjustVisualEffects(false);
    }
                
    void Move(Entity ent, Vector3I dir, bool gravity) {
        var new_pos = ent.Position + dir;
        if (new_pos.z < 0) {
            DeleteEntityUndoable(ent, new DefeatParams.Fall());
            return;
        }
            
        MoveEntityUndoable(ent, new_pos);
    }
        
    public enum PushRelation {
        Blocking,
        Phasing,
        Forcing,
        Rigid
    }

    readonly List<List<List<PR>>> PUSH_TABLE = new List<List<List<PR>>>(){
    //                                                            entity in front
    //                                             non-block                                 block
    //                                        non-push  push                          non-push   push
        new List<List<PR>>(){new List<PR>(){PR.Phasing, PR.Phasing}, new List<PR>(){PR.Blocking, PR.Blocking}}, // inactive  c  e i
        new List<List<PR>>(){new List<PR>(){PR.Phasing, PR.Phasing}, new List<PR>(){PR.Blocking, PR.Rigid   }}, // pushing   u  n t
        new List<List<PR>>(){new List<PR>(){PR.Phasing, PR.Forcing}, new List<PR>(){PR.Blocking, PR.Rigid   }}, // block     r  t y
    };

    PR GetPushRelation(Entity ent, Entity front, Vector3I dir, bool pushing) {
        var first = Math.Min((pushing ? 1 : 0) + (ent.IsBlock(dir) ? 2 : 0), 2);
        var second = front.IsBlock(-dir) ? 1 : 0;
        var third = front.IsPushable(-dir) ? 1 : 0;
        var result = PUSH_TABLE[first][second][third];
        return result == PR.Rigid && !front.IsRigid(-dir) ? PR.Forcing : result;
    }

    // Not to be called with fixed blocks. The array returned is
    // a list of entities that moved with it.
    List<Entity> AttemptMove(Entity ent, Vector3I dir, bool gravity, bool can_push, bool doBumpEffect) {
        // Build dependency graph of entities
        var visited = new HashSet<Entity>();
        var stack = new Stack<Entity>(new List<Entity>{ ent });
        var graph = new Graph();
        
        while (Util.TryPop(stack, out var e)) {
            // Using entities as keys is okay because the graph is fleeting
            if (visited.Contains(e))
                continue;
            visited.Add(e);
            graph.AddEntity(e, true);

            var frontEnts = EntryAt(e.Position + dir).entities.Values;
            var pushRels = frontEnts.Select(e2 =>
                (e2, GetPushRelation(e, e2, dir, can_push && e == ent))
            );
                
            // Blocking => blocked
            if (pushRels.Any((tup) => tup.Item2 == PushRelation.Blocking)) {
                graph.AddEntity(e, false);
                continue;
            }
                
            // Otherwise, add forcing and rigid edges
            foreach (var tup in pushRels)
                if (tup.Item2 != PushRelation.Phasing) {
                    var type = tup.Item2 == PR.Forcing ? Graph.EdgeType.Forcing : Graph.EdgeType.Rigid;
                    graph.AddTarget(e, tup.e2, type);
                    stack.Push(tup.e2);
                }
        }

        graph.MarkMovability();
        var (Moving, Squished) = graph.MovingSquishedEntities(ent);
        if (Moving.Contains(ent) && !gravity)
            SpawnParticleEffect(Global.ParticleEffect.Dash, ent.Position, ent.Direction, 0);
        if (!gravity) {
            if (Moving.Any())
                _tweenGrouping.AddTween(new TweenSoundEffectEntry(Global.SFX.Move, 0));
            else if (doBumpEffect) {
                foreach (var entity in graph.BumpingEntities(ent))
                    _tweenGrouping.AddTween(new TweenEntityBumpPositionEntry(entity, (Vector2)dir.XY * Util.TileSize));
                _tweenGrouping.AddTween(new TweenSoundEffectEntry(Global.SFX.Bump, 0));
            }
        }
        foreach (var e in Moving)
            Move(e, dir, gravity);
        
        foreach (var e in Squished)
            if (e.Alive)
                if (e.HandleSquished())
                    DeleteEntityUndoable(e, new DefeatParams.Squish(){ Direction = dir.XY });
        
        HandleHazardousSurface(Moving);
        return Moving;
    }


    // Objects fall inward, up, down, left, or right.
    void HandleGravity() {
        var moved = true;
        
        while (moved) {
            moved = false;
            for (int i = 0; i < Entity.NumTypes; ++i)
                if (i != (int)Entity.EntityType.Fixed)
                    foreach (var id in _entriesByType[i].entities.Keys.ToList())
                        if (_entitiesById.ContainsKey(id) && _entitiesById[id].Gravity != Vector3I.Zero) {
                            var ent = _entitiesById[id];
                            var result = AttemptMove(ent, ent.Gravity, true, false, false);
                            if (result.Count > 0)
                                moved = true;
                        }
        }

        // For now, gravity is 1 step
        BatchTweens();
    }
    
    // null if not on top of an exit
    Stage.ExitParameters AttemptEnterLevel(Player.Ent ent) {
        var belowEnts = EntryAt(ent.Position + ent.Gravity).entities.Values.ToList();
        foreach (var e in belowEnts)
            if (e is Marker.Ent b && b.IsExit) {
                _tweenGrouping.AddTween(new TweenEntityEffectEntry(e, new EntityEffect.OnEntered(), 0.0f));

                var levelNode = Global.LevelGraph.NodesByFilename[LevelStage.LevelPath];
                if (b.MarkerType_ == Marker.MarkerType.ExitParallel)
                    levelNode = levelNode.Parent;
                return new Stage.ExitParameters() {
                    NewLevelPath = levelNode.Children["1" /*$"{(int)b.CounterValue}"*/].Filename,
                    ExitPosition = e.Position,
                    ExitDirection = b.MarkerType_ == Marker.MarkerType.ExitParallel ? e.Direction : Vector3I.Forward
                };
            }
        return null;
    }
            
    class PuzzleInput {
        public enum Action {
            Move,
            Undo,
            Enter,
            Exit,
        }
        public Action Action_ { get; set; }
        public Vector3I Dir { get; set; }
    }
        
    // Sorts an array of entities in place by priority.
    void PrioritySort<T>(List<T> entities) where T: Entity {
        entities.Sort((a, b) => Util.SequenceCompare(a.MovePriority(), b.MovePriority()));
    }

    // Returns the level the player entered, if any
    Stage.ExitParameters ProcessPlayerEnterLevel() {
        var players = _entriesByType[(int)Entity.EntityType.Player].entities.Values.Select(e => (Player.Ent)e).ToList();
        PrioritySort(players);
        foreach (var player in players)
            if (player.Alive)
                if (AttemptEnterLevel(player) is Stage.ExitParameters p) {
                    var playerPos = player.Position;
                    DeleteEntityUndoable(player, new DefeatParams.EnterLevel(){ ExitDirection = p.ExitDirection });
                    return p;
                }
        return null;
    }

    void MovePlayers(PuzzleInput input) {
        var players = _entriesByType[(int)Entity.EntityType.Player].entities.Values.ToList();
        PrioritySort(players);
        foreach (var player in players)
            if (player.Alive) {
                var dir = input.Dir;
                if (input.Action_ == Level.PuzzleInput.Action.Move) {
                    if (dir != Vector3I.Zero) {
                        var move_dir = dir;
                        RotateEntityUndoable(player, move_dir);
                        AttemptMove(player, move_dir, false, true, true);
                        BatchTweens();
                    }
                }
            }
    }

    void HandleHazardousSurface(IEnumerable<Entity> moved) {
        //foreach (var ent in moved) {
        //    if (!ent.Alive || ent.Type != Entity.EntityType.Player)
        //        continue;
        //    var below = EntryAt(ent.Position + ent.Gravity);
        //    foreach (var block in below.NodeBlocks(-ent.Gravity))
        //        if (block is Block.Ent ent1 && ent1.BlockType_ == Block.BlockType.Spikes) {
        //            DeleteEntityUndoable(ent);
        //            SpawnParticleEffect(Global.ParticleEffect.PlayerPoof, ent.Position, Vector3I.Up, Entity.TweenTime);
        //            _tweenGrouping.AddTween(new TweenSoundEffectEntry(Global.SFX.Poof, Entity.TweenTime));
        //            break;
        //        }
        //}
    }
        
    readonly List<(float, List<Vector2>)> _overlappingEntityOffsets = new List<(float, List<(double, double)>)>() {
        (1.0f,  new List<(double, double)>(){ (0.0, 0.0) }),
        (0.75f, new List<(double, double)>(){ (-0.125, 0.125), (0.125, -0.125) }),
        (0.65f, new List<(double, double)>(){ (0.175, 0.175), (-0.175, 0.175), (0.0, -0.175) }),
        (0.6f,  new List<(double, double)>(){ (0.2, 0.2), (-0.2, 0.2), (0.2, -0.2), (-0.2, -0.2) }),
        (0.5f,  new List<(double, double)>(){ (0.25, 0.25), (-0.25, 0.25), (0.0, 0.0), (0.25, -0.25), (-0.25, -0.25) }),
        (0.5f,  new List<(double, double)>(){ (0.25, 0.25), (0.0, 0.11), (-0.25, 0.25), (0.25, -0.11), (0.0, -0.25), (-0.25, -0.11) }),
    }
        .Select(x => (x.Item1, x.Item2.Select(v => new Vector2((float)v.Item1, (float)v.Item2)).ToList())).ToList();

    (float Scale, List<Vector2> Offsets) OverlappingEntityOffsets(int numEntities) {
        if (numEntities <= _overlappingEntityOffsets.Count)
            return _overlappingEntityOffsets[numEntities - 1];

        const float Overlap = 0.4f;
        int itemsPerSide = (int)Math.Ceiling(Math.Sqrt(numEntities));
        float divisor = itemsPerSide - (itemsPerSide - 1) * Overlap;
        float spacing = (1.0f - Overlap) / divisor;
        float initialOffset = 0.5f - 0.5f / divisor;
        return (1.0f / divisor, Enumerable.Range(0, numEntities).Select(i => {
            var x = initialOffset - i % itemsPerSide * spacing;
            var y = initialOffset - i / itemsPerSide * spacing;
            return new Vector2(x, y);
        }).ToList());
    }

    void AdjustLevelClearEffects(bool tween) {
        foreach (var ent in _entitiesById.Values) {
            var effect = new EntityEffect.OnLevelClearChanged(){ LevelClear = _clear };
            if (tween)
                _tweenGrouping.AddTween(new TweenEntityEffectEntry(ent, effect, 0));
            else
                PlayEntityEffect(ent.Id, effect);
        }
    }

    void AdjustVisualEffects(bool tween) {
    }

    void BatchTweens() {
        AdjustVisualEffects(true);
        _tweenGrouping.BatchTweens();
    }

    // Returns whether the clear state changed
    bool CheckLevelClear() {
        bool newClear = false;

        // TODO: Clear condition

        bool changed = newClear != _clear;
        _clear = newClear;
        return changed;
    }

    public void ReturnLevel() {
        _exiting = true;
        LevelStage.ReturnLevel();
    }

    public void ReturnToTitle() {
        _exiting = true;
        LevelStage.ReturnToTitle();
    }

    void Step(PuzzleInput input) {
        _tweenGrouping.FlushAndClear();

        if (input.Action_ == PuzzleInput.Action.Undo) {
            _undoStack.Undo(this);
            AdjustVisualEffects(false);
            return;
        }

        _movedInStep.Clear();

        if (input.Action_ == PuzzleInput.Action.Enter) {
            var levelFile = ProcessPlayerEnterLevel();
            BatchTweens();
            if (levelFile is Stage.ExitParameters p) {
                _exiting = true;
                LevelStage.SetNewLevel(p);
            }
        } else if (input.Action_ == PuzzleInput.Action.Exit) {
            ReturnLevel();
        } {
            MovePlayers(input);
            HandleGravity();
        }
                    
        // This is just for display
        bool levelClearChanged = CheckLevelClear();
        if (levelClearChanged) {
            AdjustLevelClearEffects(true);
            BatchTweens();
        }
        _tweenGrouping.Execute(this);
            
        _undoStack.Batch();
    }

    //LevelFile ToFile() {
    //    var tileMapFloor = GetNode<TileMap>("%TileMapFloor");
    //    var tileMapWall = GetNode<TileMap>("%TileMapWall");

    //    return new LevelFile {
    //        Name = LevelName,
    //        Base = new Vector3I(_minX, _minY, MinZ),
    //        Size = new Vector3I(_sizeX, _sizeY, SizeZ),
    //        Map = Enumerable.Range(MinZ, SizeZ).SelectMany(z =>
    //            Enumerable.Range(_minY, _sizeY).SelectMany(y =>
    //                Enumerable.Range(_minX, _sizeX).Select(x =>
    //                    z > 0
    //                        ? tileMapWall.GetCell(x, y) != TileMap.InvalidCell ? 1 : 0
    //                        : tileMapWall.GetCell(x, y) != TileMap.InvalidCell ||
    //                            tileMapFloor.GetCell(x, y) != TileMap.InvalidCell ? 1 : 0 
    //            ))).ToList(),
    //        Entities = _entitiesById.Values
    //            .Where(ent => ent.Type != Entity.EntityType.Fixed)
    //            .Select(ent => ent.Def.File).ToList()
    //    };
    //}

    public static Level Instantiate(LevelFile levelFile, Vector3I? enterPosition) {
        var level = Global.Scene.Level.Instance<Level>();
        level._levelFile = levelFile;
        level._enterPosition = enterPosition;
        return level;
    }

        
    void HandleInput() {
        var action = PuzzleInput.Action.Move;
        var dir = Vector3I.Zero;
        
        if (_buttons[(int)GameButton.Action.Up].JustPressed)
            dir = Util.DirVec(Util.Direction.Up);
        else if (_buttons[(int)GameButton.Action.Down].JustPressed)
            dir = Util.DirVec(Util.Direction.Down);
        else if (_buttons[(int)GameButton.Action.Left].JustPressed)
            dir = Util.DirVec(Util.Direction.Left);
        else if (_buttons[(int)GameButton.Action.Right].JustPressed)
            dir = Util.DirVec(Util.Direction.Right);
        else if (_buttons[(int)GameButton.Action.Wait].JustPressed)
            {}
        else if (_buttons[(int)GameButton.Action.Undo].JustPressed)
            action = PuzzleInput.Action.Undo;
        else if (_buttons[(int)GameButton.Action.Enter].JustPressed)
            action = PuzzleInput.Action.Enter;
        else if (_buttons[(int)GameButton.Action.Exit].JustPressed)
            action = PuzzleInput.Action.Exit;
        else
            return;
            
        _inputs.Add(new PuzzleInput{ Action_ = action, Dir = dir });
    }

    void HandleShake(float delta) {
        var rng = new Random();
        var angle = (float)rng.NextDouble() * Mathf.Tau;
        _shakeMagnitude = Mathf.MoveToward(_shakeMagnitude, 0.0f, delta * Util.TileSize / 4);
        Position = BasePosition + Vector2.Down.Rotated(angle) * _shakeMagnitude;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        if (Engine.EditorHint)
            return;

        HandleShake(delta);

        foreach (var button in _buttons)
            button.Update(delta); 

        HandleInput();

        foreach (var input in _inputs)
            if (!_clear && !_exiting) {
                Step(input);
                if (_clear) {
                    LevelStage.SetLevelClear(true);
                    break;
                }
            }
                
        if (Input.IsActionJustPressed("restart") && _undoStack.TopBatchActionTag() != BatchAction.Tag.Restart) {
            _tweenGrouping.FlushAndClear();
            foreach (var ent in Enumerable.Range(0, Entity.NumTypes)
                .Where(i => i != (int)Entity.EntityType.Fixed)
                .SelectMany(i => _entriesByType[i].entities.Values).ToList())
            {
                DeleteEntityUndoable(ent, false);
            }
            LoadInEntities(true);

            _undoStack.Batch(BatchAction.Tag.Restart);
        }

        _inputs.Clear();

    }

    public override void _Input(InputEvent @event)
    {
        if (!_clear && !_exiting && !GetTree().Paused && @event.IsActionPressed("pause")) {
            LevelStage.Pause();
            GetTree().SetInputAsHandled();
        }
    }

}
