using System;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string path = @"C:\SEP_G32\SEP_G32\media\logo-and-icon.png";
        
        try
        {
            using (var bmp = SKBitmap.Decode(path))
            {
                Dictionary<string, int> colors = new Dictionary<string, int>();
                
                for (int x = 0; x < bmp.Width; x += 3)
                {
                    for (int y = 0; y < bmp.Height; y += 3)
                    {
                        var c = bmp.GetPixel(x, y);
                        if (c.Alpha > 200 && !(c.Red > 240 && c.Green > 240 && c.Blue > 240))
                        {
                            string hex = $"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}";
                            if (colors.ContainsKey(hex))
                                colors[hex]++;
                            else
                                colors[hex] = 1;
                        }
                    }
                }
                
                var topColors = colors.OrderByDescending(kv => kv.Value).Take(15);
                foreach (var kv in topColors)
                {
                    Console.WriteLine($"{kv.Key}: {kv.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
