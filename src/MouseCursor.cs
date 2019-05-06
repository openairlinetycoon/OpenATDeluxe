using Godot;
using System;

public class MouseCursor : Node
{
    [Export]
	public string pathToAirlineTycoonD = "";

    Sprite s; 
    GFXLibrary lib;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    	lib = new GFXLibrary(System.IO.Path.Combine(pathToAirlineTycoonD, "gli", "glbasis.gli"));
        lib.GetFilesInLibrary();

        s = new Sprite();
        this.AddChild(s);

        
    
        s.SetTexture(lib.CreateTextureFromFile(lib.files[0]));
    }

    int currentTexture = 0;

    public override void _Input(InputEvent e){
        if(e is InputEventMouseButton){
            InputEventMouseButton mouse = e as InputEventMouseButton;

            if(mouse.IsPressed()){ 
                currentTexture++;
                if(currentTexture >= lib.filesInLibrary)
                    currentTexture = 0;

                s.SetTexture(lib.CreateTextureFromFile(lib.files[currentTexture]));
            }
        }
    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        s.SetPosition(GetViewport().GetMousePosition());
    }
}
