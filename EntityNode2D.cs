using Godot;
using System;

public interface IEntityNode {
    // Rotates only what should be rotated
	public float BaseRotation { get; set; }
    // Scales only what should be scaled
    public float BaseScale { get; set;}

	public Entity LevelEntity(int id);

    // Intended for saving a level in the ad-hoc level maker
	public LevelFile.EntityCustomData LevelEntityCustomParams();
}

public abstract partial class EntityNode2D : Node2D, IEntityNode {
    Vector2 _basePosition;
    public Vector2 BasePosition {
        get => _basePosition;
        set {
            _basePosition = value;
            CalcPositionAndScale();
        }
    }
    Vector2 _offsetPosition = Vector2.Zero;
    public Vector2 OffsetPosition {
        get => _offsetPosition;
        set {
            _offsetPosition = value;
            CalcPositionAndScale();
        }
    }

    Vector2 _enterLevelOffsetPosition = Vector2.Zero;
    public Vector2 EnterLevelOffsetPosition {
        get => _enterLevelOffsetPosition;
        set {
            _enterLevelOffsetPosition = value;
            CalcPositionAndScale();
        }
    }

    Vector2 _bumpOffsetPosition = Vector2.Zero;
    public Vector2 BumpOffsetPosition {
        get => _bumpOffsetPosition;
        set {
            _bumpOffsetPosition = value;
            CalcPositionAndScale();
        }
    }

    Vector2 _squishDirection;
    public Vector2 SquishDirection {
        get => _squishDirection;
        set {
            _squishDirection = value;
            CalcPositionAndScale();
        }
    }

    public float BaseRotation {
        get => Rotation;
        set {
            Rotation = value;
            GetNode<Node2D>("%NoRotating").Rotation = -value;
        }
    }

    float _baseScale = 1.0f;
    public float BaseScale {
        get => _baseScale;
        set {
            _baseScale = value;
            CalcPositionAndScale();
        }
    }

    float _offsetScale = 1.0f;
    public float OffsetScale {
        get => _offsetScale;
        set {
            _offsetScale = value;
            CalcPositionAndScale();
        }
    }

    float _squishScale = 1.0f;
    public float SquishScale {
        get => _squishScale;
        set {
            _squishScale = value;
            CalcPositionAndScale();
        }
    }

    float _pitFallScale = 1.0f;
    public float PitFallScale {
        get => _pitFallScale;
        set {
            _pitFallScale = value;
            CalcPositionAndScale();
        }
    }
    
    protected bool _ready = false;

    void CalcPositionAndScale() {
        Position = BasePosition + OffsetPosition + BumpOffsetPosition + EnterLevelOffsetPosition;
        Scale = BaseScale * OffsetScale * PitFallScale * Vector2.One;
        float squishAngle = Vector2.Down.AngleTo(SquishDirection);

        Vector2 translation = BasePosition + SquishDirection * (Util.TileSize / 2);
        Transform = Util.Translation(translation)
            * Util.Rotation(squishAngle)
            * Util.Scale(new Vector2(1.0f, SquishScale))
            * Util.Rotation(-squishAngle)
            * Util.Translation(-translation)
            * Transform;
        GetNode<Node2D>("%NoRotating").Scale = Vector2.One / BaseScale;
        GetNode<Node2D>("%NoRotating").Rotation = -Rotation;
    }

    void SetModulateRgbRecursive(Node node, Color value) {
        foreach (var child in node.GetChildren()) {
            if (child is CanvasItem item)
                item.SelfModulate = new Color(value, item.SelfModulate.a);
            SetModulateRgbRecursive((Node)child, value);
        }
    }

    public Color BaseModulateRgb {
        set {
            SelfModulate = new Color(value, SelfModulate.a);
            SetModulateRgbRecursive(this, value);
        }
    }

    // Scale to use when the actor is in the wall layer.
    // Useful for differentiation.
    public virtual float WallLayerScale => 1.0f;

	public abstract Entity LevelEntity(int id);

	public abstract LevelFile.EntityCustomData LevelEntityCustomParams();

    public LevelFile.EntityFile LevelEntityFile() {
        var rotated = (Vector2I)Vector2.Down.Rotated(Rotation).Round();
        var entityFile = new LevelFile.EntityFile {
            Position = Util.ToTileSpace(Position, ZIndex),
            Direction = new Vector3I(rotated.x, rotated.y, 0),
            Gravity = Vector3I.Forward,
            CustomData = LevelEntityCustomParams()
        };
        return entityFile;
    }

    protected abstract void UpdateTexture();

    // Call this *after* initializing node variables
    public void PrepareCommon() {
        _basePosition = Position;
        _ready = true;
    }

    public void ProcessCommon(float delta) {
    }

    public virtual void OnSteppedOn() {}
    public virtual void OnTargetStateChanged(bool active) {}
    public virtual void OnEntered() {}
    public virtual void OnMoveStart(bool gravity) {}
    public virtual void OnLevelClearChanged(bool levelClear) {
    }
}