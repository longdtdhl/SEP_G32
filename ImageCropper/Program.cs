using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

class Program
{
    static void Main()
    {
        var inputPath = @"C:\SEP_G32\SEP_G32\backend\OPCBS.Web\wwwroot\images\logo-and-icon.png";
        using var img = Image.Load(inputPath);
        
        // Main Logo (top left)
        var logoCrop = img.Clone(x => x.Crop(new Rectangle(60, 20, 680, 480)));
        logoCrop.Save(@"C:\SEP_G32\SEP_G32\backend\OPCBS.Web\wwwroot\images\logo-cropped.png");
        
        // Also let's extract the color palette for reference if needed
        Console.WriteLine("Cropped logo saved as logo-cropped.png.");
    }
}
