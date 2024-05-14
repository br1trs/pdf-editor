using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageMagick;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using PDFEditor;
using Environment = System.Environment;
using FileAccess = Godot.FileAccess;
using Image = Godot.Image;

public partial class Main : Control
{
    List<Image> images = new ();
    private PackedScene PageContainerScene;
    [Export] private GridContainer pdfGrid;
    private PackedScene loadingScreen;
    [Export] private Panel preview;
    [Export] private FileDialog saveDialog;
    public override void _Ready()
    {
        string ghostscriptPath = Path.Combine(Environment.CurrentDirectory, "Dependencies" + Path.DirectorySeparatorChar + "GhostScript"); 
        MagickNET.SetGhostscriptDirectory(ghostscriptPath);
        PageContainerScene = GD.Load<PackedScene>("res://page_container.tscn");
        loadingScreen = GD.Load<PackedScene>("res://LoadingScreen.tscn");
        clearTempFolder();
    }
    

    public override void _ExitTree()
    {
        clearTempFolder();
        base._ExitTree();

    }
    
    public async void OnFileSelected(string path)
    {
        if (FileAccess.FileExists(path))
        {
            LoadingScreen loadingScreenInstance = (LoadingScreen)loadingScreen.Instantiate();
            AddChild(loadingScreenInstance);
            var progressBar = loadingScreenInstance.GetProgressBar();
            var progress = new Progress<float>(value =>
            {
                // Update the progress bar value
                progressBar.Value = value * 100;
            });
            await Task.Run((() => PdfManager.LoadPDFToImage(path, progress)));
            var paths = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Temp"));
            foreach (var p in paths)
            {
                var pageContainer = (PageContainer)PageContainerScene.Instantiate();
                pageContainer.SetPageNumber(ExtractPageNumber(p));
                string replacedPath = p.Replace("\\", "/");
                var img=Image.LoadFromFile(replacedPath);
                pageContainer.SetImage(img);
                pdfGrid.AddChild(pageContainer);
            }
            loadingScreenInstance.QueueFree();
        }
    }
    public int ExtractPageNumber(string path)
    {
        var match = Regex.Match(path, @"\d+");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        else
        {
            throw new ArgumentException("No number found in the path");
        }
    }
    public void OnDeletePageModePressed(bool pressed)
    {
        PdfManager.SetCurrentMode(pressed ? EditorMode.DeletePage : EditorMode.None);
    }
    public void OnOrganizePagesPressed(bool pressed)
    {
        PdfManager.SetCurrentMode(pressed ? EditorMode.MovePage : EditorMode.None);
    }

    public override void _Process(double delta)
    {
        foreach (var VARIABLE in PdfManager.GetPageNumbersCache())
        {
            GD.Print(VARIABLE + " is in cache");
        }
    }
    private void clearTempFolder()
    {
        string tempPath = Path.Combine(Environment.CurrentDirectory, "Temp");

        if (!Directory.Exists(tempPath)) return;
        if (!Directory.EnumerateFileSystemEntries(tempPath).Any())return;
        var files = Directory.GetFiles(tempPath);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }
    public void OnSaveButtonPressed()
    {
        
        saveDialog.Visible = true;
    }
    public void OnSaveDialogFileSelected(string path)
    {
        switch (PdfManager.getcurrentMode())
        {
            case EditorMode.DeletePage:
                PdfManager.DeletePages(path);
                break;
            case EditorMode.MovePage:
                PdfManager.RearrangePages(path, GetPagesOrder());
                break;
        }
    }

    public List<int> GetPagesOrder()
    {
        List<int> pagesOrder = new();
        foreach (var node in pdfGrid.GetChildren())
        {
           var page = (PageContainer)node;
           pagesOrder.Add(page.GetPageNumber());
        }
        return pagesOrder;
    }
}
