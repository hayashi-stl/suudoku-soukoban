using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Stage : Node
{
    public class ExitParameters {
        public string NewLevelPath { get; set; }
        public Vector3I ExitPosition { get; set; }
        public Vector3I ExitDirection { get; set; }
    }

    public class EnterParameters {
        public string LevelPath { get; set; }
        public string OldLevelPath { get; set; }
        public Vector3I ExitDirection { get; set; }
    }

    public string LevelPath { get; private set; }
    LevelFile _levelFile;
    Vector3I? _enterPosition;
    ExitParameters _exitParameters;
    EnterParameters _enterParameters;
    public Level Level { get; private set; }
    Stack<(LevelFile Level, Vector3I ReturnPos)> _levelStack = new Stack<(LevelFile Level, Vector3I ReturnPos)>();

    const int FullSizeMaxHeight = 14;
    const int FullSizeMaxWidth = 20;

    public const float EnterLevelTweenTime = 0.75f;
    public const float EnterLevelFadeTime = 0.25f;
    public const float EnterLevelBlackTime = 0.25f;
    const float EnterLevelZoom = 8.0f;

    float _normalLevelScale;
    Vector2 _normalLevelCenter;

    Vector2 LevelScale { get => Level.Scale; set => Level.Scale = value; }

    Vector2 LevelCenter {
        get {
            var windowSize = Global.WindowSize(this);
            return Level.Transform.AffineInverse() * (windowSize / 2);
        }
        set {
            var windowSize = Global.WindowSize(this);
            Level.BasePosition = windowSize / 2 - Level.Transform.BasisXform(value);
        }
    }

    Control _pause;

    public static Stage Instantiate(EnterParameters enterParams) {
        var stage = Global.Scene.Stage.Instance<Stage>();
        stage._enterParameters = enterParams;
        stage.LevelPath = enterParams.LevelPath;
        stage._levelFile = LevelFile.Read(enterParams.LevelPath);

        // Calculate entrance position
        if (Global.LevelGraph.NodesByFilename.TryGetValue(enterParams.LevelPath, out var levelNode)) {
            var targetMarkerType = Marker.MarkerType.Exit;
            if (enterParams.ExitDirection.z == 0) {
                levelNode = levelNode.Parent;
                targetMarkerType = Marker.MarkerType.ExitParallel;
            }

            foreach (var ent in stage._levelFile.Entities)
                if (ent.CustomData is LevelFile.MarkerFile marker) {
                    //if (marker.Type == targetMarkerType)
                    //    if (levelNode.Children[$"{ent.CounterValue}"].Filename == enterParams.OldLevelPath)
                    //        stage._enterPosition = ent.Position + Vector3I.Back;
                }
        }

        return stage;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _pause = GetNode<Control>("%Pause");
        GetNode<Label>("%Title").Text = _levelFile.Name;
        GetNode<Label>("%Controls").Text = _levelFile.Controls;

        var windowSize = Global.WindowSize(this);
        Level = Level.Instantiate(_levelFile, _enterPosition);
        Level.LevelStage = this;
        AddChild(Level);
        MoveChild(Level, 0);
        var levelRect = Level.LevelRect();
        var scale = Mathf.Min(
            Mathf.Min(1.0f, (float)FullSizeMaxHeight / levelRect.Size.y), (float)FullSizeMaxWidth / levelRect.Size.x
        );
        _normalLevelScale = scale;
        _normalLevelCenter = (Vector2)levelRect.Size * Util.TileSize / 2;

        if (_enterParameters.ExitDirection.z > 0) {
            LevelScale = EnterLevelZoom * Vector2.One;
            LevelCenter = Util.FromTileSpace((Vector3I)_enterPosition).XY;
            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(this, "LevelScale", _normalLevelScale * Vector2.One, EnterLevelTweenTime).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(this, "LevelCenter", _normalLevelCenter, EnterLevelTweenTime).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        } else if (_enterParameters.ExitDirection.z == 0) {
            LevelScale = _normalLevelScale * Vector2.One;
            LevelCenter = _normalLevelCenter;
            Level.BasePosition += windowSize * (Vector2)_enterParameters.ExitDirection.XY;
            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(this, "LevelCenter", _normalLevelCenter, EnterLevelTweenTime).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        } else {
            LevelScale = _normalLevelScale * Vector2.One;
            LevelCenter = _normalLevelCenter;
        }
    }

    public void SetLevelClear(bool is_clear) {
        var levelClears = new Node2D[]{ GetNode<Node2D>("%LevelClear1"), GetNode<Node2D>("%LevelClear2") };
        var height = Global.WindowSize(this).y;
        foreach (var node in levelClears)
            node.Visible = is_clear;
        var offsetX = Math.Max(_normalLevelScale * Level.LevelRect().Size.x / 2 * Util.TileSize, 6 * Util.TileSize);
        levelClears[0].Position = new Vector2(levelClears[0].Position.x - offsetX, height *  3 / 2);
        levelClears[1].Position = new Vector2(levelClears[1].Position.x + offsetX, height * -1 / 2);
        var tween = CreateTween();
        tween.TweenProperty(levelClears[0], "position", new Vector2(levelClears[0].Position.x, height / 2), 0.5f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(levelClears[1], "position", new Vector2(levelClears[1].Position.x, height / 2), 0.5f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        GetNode<Timer>("%Timer").Start();
    }

    public void LoadNewLevel() {
        QueueFree();
        var enterParams = new EnterParameters {
            LevelPath = _exitParameters.NewLevelPath,
            OldLevelPath = LevelPath,
            ExitDirection = _exitParameters.ExitDirection
        };
        Util.Root(this).AddChild(Instantiate(enterParams));
    }

    void SpawnFader() {
        Util.Root(this).AddChild(Global.Scene.Fader.Instance());
    }

    public void SetNewLevel(ExitParameters exitParams) {
        _exitParameters = exitParams;

        var tween = CreateTween();
        tween.SetParallel();
        if (exitParams.ExitDirection.z == 0) {
            var newPosition = Level.BasePosition - Global.WindowSize(this) * (Vector2)exitParams.ExitDirection.XY;
            tween.TweenProperty(Level, "BasePosition", newPosition, EnterLevelTweenTime)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
            tween.TweenCallback(this, "LoadNewLevel").SetDelay(EnterLevelTweenTime);
        } else {
            tween.TweenProperty(this, "LevelScale", EnterLevelZoom * Vector2.One, EnterLevelTweenTime)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "LevelCenter", Util.FromTileSpace(exitParams.ExitPosition).XY, EnterLevelTweenTime)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenCallback(this, "SpawnFader").SetDelay(EnterLevelTweenTime - EnterLevelFadeTime - EnterLevelBlackTime / 2);
            tween.TweenCallback(this, "LoadNewLevel").SetDelay(EnterLevelTweenTime);
        }
    }

    public void ReturnLevel() {
        if (Global.LevelGraph.NodesByFilename.TryGetValue(LevelPath, out var thisNode) && thisNode.Parent is LevelGraph.Node levelNode) {
            _exitParameters = new ExitParameters {
                NewLevelPath = levelNode.Filename,
                ExitDirection = Vector3I.Back,
            };

            SpawnFader();
            var tween = CreateTween();
            tween.TweenCallback(this, "LoadNewLevel").SetDelay(EnterLevelFadeTime);
            tween.TweenCallback(this, "queue_free");
            //LoadNewLevel(true);
        } else {
            ReturnToTitle();
        }
    }

    public void ReturnToTitle() {
        Util.Root(this).AddChild(Global.Scene.LevelSelect.Instance());
        QueueFree();
    }

    public void Pause() {
        _pause.Visible = true;
        GetTree().Paused = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
    }


    public void _on_Timer_timeout() {
        QueueFree();
        Util.Root(this).AddChild(Global.Scene.LevelSelect.Instance());
    }
}
