using Godot;
using System;

[Tool]
public class taimage_import : EditorPlugin
{
    TAImageImportPlugin _importPlugin;

    public override void _Ready()
    {
        
    }

    public override void _EnterTree()
    {
        _importPlugin = new TAImageImportPlugin();
        AddImportPlugin(_importPlugin);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(_importPlugin);
        _importPlugin = null;
    }
}
