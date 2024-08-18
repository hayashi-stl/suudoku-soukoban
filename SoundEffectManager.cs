using Godot;
using System;
using System.Collections.Generic;

public class SoundEffectManager {
    readonly Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();

    public void Add(string name, SoundEffect effect) {
        if (_soundEffects.ContainsKey(name))
            _soundEffects[name].FadeOut();
        _soundEffects[name] = effect;
    }

    public void Remove(string name, SoundEffect effect) {
        if (_soundEffects[name] == effect)
            _soundEffects.Remove(name);
    }

    public static SoundEffectManager Instance = new SoundEffectManager();
}