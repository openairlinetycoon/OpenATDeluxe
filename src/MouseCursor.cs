using System;
using Godot;

public class MouseCursor : Node {
    [Export]

    Sprite s;
    GFXLibrary lib;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready () {
        //lib = new GFXLibrary(System.IO.Path.Combine(pathToAirlineTycoonD, "gli", "glbasis.gli"));
        //System.Console.WriteLine (System.IO.Path.Combine ("/home/xerxes/Downloads/ATD", "gli", "glbasis.gli"));
        lib = new GFXLibrary ("/home/xerxes/Downloads/ATD/fr/gli/glbasis.gli");
        lib.read ();
        lib.save ();

        s = new Sprite ();
        this.AddChild (s);

        if (lib.files.Count > 0) {
            s.SetTexture (lib.CreateTextureFromFile (lib.files[0]));
        }
    }

    int currentTexture = 0;

    public override void _Input (InputEvent e) {
        if (e is InputEventMouseButton) {
            InputEventMouseButton mouse = e as InputEventMouseButton;

            if (mouse.IsPressed ()) {
                if (currentTexture < lib.filesInLibrary) {
                    s.SetTexture (lib.CreateTextureFromFile (lib.files[currentTexture++]));
                } else {
                    currentTexture = 0;
                }
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process (float delta) {
        s.SetPosition (GetViewport ().GetMousePosition ());
    }
}