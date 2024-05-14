using Godot;
using System;
using Godot.Collections;
using PDFEditor;

public partial class PageContainer : Panel
{
    [Export] private TextureRect textureRect;
    [Export] private TextureButton deletePageButton;
    [Export] private RichTextLabel pageNumberLabel;
    [Export] private TextureButton moveToLeftButton;
    [Export] private TextureButton moveToRightButton;
    private PagePreview preview;
    private int _pageNumber;
    private PageOrientation PageOrientation = PageOrientation.Portrait;
    private Array<Node> childrenList;
    private bool changePageOrder = true;
    public void SetImage(Image img)
    {
        var sceneTexture = new ImageTexture();
        sceneTexture = ImageTexture.CreateFromImage(img);
        textureRect.Texture = sceneTexture;
        
    }

    public void SetPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        PageOrientation = PdfManager.CheckPageOrienation(pageNumber);
        pageNumberLabel.Text = "[center]page " + pageNumber;
    }
    public int GetPageNumber()
    {
        return _pageNumber;
    }
    public void OnDeletePageToggled(bool selected)
    {
        if (selected)
        {
            PdfManager.AddPageNumberToCache(_pageNumber);
        }
        else
        {
            PdfManager.RemovePageNumberFromCache(_pageNumber);
        }
    }

    public override void _Ready()
    {
        childrenList = deletePageButton.GetChildren();
        PackedScene movingPageScene = GD.Load<PackedScene>("res://MovingPage.tscn");
    }

    public override void _Process(double delta)
    {
        
        if (deletePageButton.IsHovered())
        {
            PdfManager.SetPreviewTexture(textureRect.Texture);
            OnHoveredUpdate(deletePageButton.IsHovered());
        }
        else
        {
            OnHoveredUpdate(deletePageButton.IsHovered());
        }
        switch (PdfManager.getcurrentMode())
        {
            case EditorMode.DeletePage:
                deletePageButton.Visible = true;
                break;
            case EditorMode.MovePage:
                deletePageButton.Visible = false;
                moveToLeftButton.Visible = true;
                moveToRightButton.Visible = true;
                break;
            default:
                deletePageButton.Visible = false;
                moveToLeftButton.Visible = false;
                moveToRightButton.Visible = false;
                break;
        }
    }

    private void OnHoveredUpdate(bool hovered)
    {
        foreach (Node child in childrenList)
        {
            if (child is PanelContainer)
            {
                var panel = (PanelContainer)child;
                panel.Visible = hovered;
            }
        }
    }

    public void OnMoveToRightPressed()
    {
        var newIndex = GetIndex()+1;
        if (newIndex < GetParent().GetChildCount())
        {
            GetParent().MoveChild(this, newIndex);
        }
    }
    public void OnMoveToLeftPressed()
    {
        var newIndex = GetIndex()-1;
        if (newIndex >= 0)
        {
            GetParent().MoveChild(this, newIndex);
        }
    }
}
