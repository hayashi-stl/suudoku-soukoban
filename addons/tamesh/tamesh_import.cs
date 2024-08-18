using Godot;
using System;

[Tool]
public class tamesh_import : EditorPlugin
{
    TAMeshImportPlugin _importPlugin;

    public override void _Ready()
    {
        
    }

    public override void _EnterTree()
    {
        _importPlugin = new TAMeshImportPlugin();
        AddImportPlugin(_importPlugin);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(_importPlugin);
        _importPlugin = null;
    }
}
