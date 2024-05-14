using Godot;
using System;

public partial class LoadingScreen : Control
{
    [Export] private ProgressBar progressBar;
    public ProgressBar GetProgressBar()
    {
        return progressBar;
    }
}
