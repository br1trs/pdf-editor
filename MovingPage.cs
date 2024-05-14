using Godot;
using System;

public partial class MovingPage : TextureRect
{
    public override void _Process(double delta)
    {
        var mousePos = GetGlobalMousePosition();
        Position = mousePos;
    }
}
