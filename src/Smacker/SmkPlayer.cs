using Godot;
using System;

public class SmkPlayer : Sprite
{
    [Export]
    public string path;

    SmackerFile file;
    SmackerDecoder decoder;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Texture = new ImageTexture();
        file = SmackerFile.OpenFromStream(new System.IO.FileStream(path, System.IO.FileMode.Open)); ;
        decoder = file.Decoder;
        decoder.ReadNextFrame();
        Image i = new Image();
        i.CreateFromData((int)file.Header.Width, (int)file.Header.Height, false, Image.Format.Rgb8, decoder.RGBData);
        var t = new ImageTexture();

        t.CreateFromImage(i);
        Texture = t;

    }
    public byte[] ARGBData(byte[] data, byte[] palette)
    {
        byte[] result = new byte[data.Length * 4];
        int j = 0;
        for (int i = 0; i < data.Length; i++)
        {
            j = i * 4;
            result[j + 0] = (byte)(palette[data[i]] * 3 + 2);
            result[j + 1] = (byte)(palette[data[i]] * 3 + 1);
            result[j + 2] = (byte)(palette[data[i]] * 3 + 0);

            if (result[j + 1] != 0 || result[j + 2] != 0 || result[j + 3] != 0)
                result[j + 3] = 255;
        }
        return result;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        decoder.ReadNextFrame();
        Image i = new Image();
        i.CreateFromData((int)file.Header.Width, (int)file.Header.Height, false, Image.Format.Rgb8, decoder.RGBData);
        var t = new ImageTexture();

        t.CreateFromImage(i);
        Texture = t;
    }
}
