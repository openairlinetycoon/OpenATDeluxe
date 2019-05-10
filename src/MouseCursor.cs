using Godot;
using System;

public class MouseCursor : Sprite
{
    GFXLibrary lib;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    	lib = new GFXLibrary(GFXLibrary.pathToAirlineTycoonD + "/gli/glbasis.gli");
        lib.GetFilesInLibrary();


        if (lib.files.Count > 0) {
            SetTexture (lib.files[0].GetTexture());
        }
        Input.SetMouseMode(Input.MouseMode.Hidden);
    }

    int currentTexture = 0;

    public override void _Input(InputEvent e){
        if(e is InputEventMouseButton){
            InputEventMouseButton mouse = e as InputEventMouseButton;

            if (mouse.IsPressed ()) {
                if (currentTexture < lib.filesInLibrary) {
                    
                    SetTexture (lib.files[currentTexture++].GetTexture());
                } else {
                    currentTexture = 0;
                }
            }
        }
    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        SetPosition(GetViewport().GetMousePosition());
    }
}
