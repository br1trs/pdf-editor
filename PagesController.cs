using Godot;
using System;
using PDFEditor;

public partial class PagesController : GridContainer
{
    public override void _Process(double delta)
    {
        if (PdfManager.IsPdfLandscape())
        {
            Columns = 2;
        }
        else
        {
            Columns = 3;
        }
    }
}
