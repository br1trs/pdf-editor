using Godot;
using System;
using PDFEditor;

public partial class PagePreview : Panel
{
    private TextureRect textureRect;
    public override void _Ready()
    {
        textureRect = GetChild(0) as TextureRect;
    }
    public override void _Process(double delta)
    {
        if (PdfManager.GetPreviewTexture() != null && PdfManager.GetPreviewTexture() != textureRect.Texture)
        {
            GeneratePreview(PdfManager.GetPreviewTexture());
        }
    }

    public void GeneratePreview (Texture2D texture)
    {
        GD.Print("Generating preview");
        GD.Print(textureRect);
        textureRect.Texture = texture;
    }
}
