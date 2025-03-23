using System;
using System.Diagnostics;
using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        string imagePath = "D:/Documents/APD/Proiect_APD/Images/image5.jpg";

        if (!System.IO.File.Exists(imagePath))
        {
            Console.WriteLine("Fisierul nu exista!");
            return;
        }

        Bitmap image = null;
        try
        {
            image = new Bitmap(imagePath);
            Console.WriteLine("\nImaginea a fost incarcata:" + image.Width + "x" + image.Height);
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nEroare la incarcarea imaginii:" + ex.Message);
            return;
        }

        Console.WriteLine("Alege un filtru:");
        Console.WriteLine("1. Outline");
        Console.WriteLine("2. Grayscale");
        Console.WriteLine("3. Sepia");
        Console.WriteLine("4. Negative");
        Console.Write("Introdu numarul filtrului: ");
        int filterChoice = int.Parse(Console.ReadLine());

        Bitmap filteredImage = null;

        Stopwatch stopwatch = Stopwatch.StartNew();

        switch (filterChoice)
        {
            case 1:
                filteredImage = ApplyOutlineFilter(image);
                Console.WriteLine("Filtrul Outline a fost aplicat.");
                break;
            case 2:
                filteredImage = ApplyGrayscaleFilter(image);
                Console.WriteLine("Filtrul Grayscale a fost aplicat.");
                break;
            case 3:
                filteredImage = ApplySepiaFilter(image);
                Console.WriteLine("Filtrul Sepia a fost aplicat.");
                break;
            case 4:
                filteredImage = ApplyNegativeFilter(image);
                Console.WriteLine("Filtrul Negative a fost aplicat.");
                break;
            default:
                Console.WriteLine("Optiune invalida!");
                return;
        }
        stopwatch.Stop();

        string defaultSaveFolder = "D:/Documents/APD/Proiect_APD/Images_filtered/";

        Console.Write("Introdu numele imaginii (fara extensie): ");
        string imageName = Console.ReadLine();

        string savePath = defaultSaveFolder + imageName + ".jpg";

        Console.WriteLine("\nSe salveaza imaginea ...");

        filteredImage.Save(savePath);
        Console.WriteLine("\nImaginea a fost salvata la:" + savePath);

        Console.WriteLine("\nTimpul de procesare:" + stopwatch.ElapsedMilliseconds + "ms");

        if (image != null)
        {
            image.Dispose();
        }
        if (filteredImage != null)
        {
            filteredImage.Dispose();
        }
    }

    // Filtru de Outline
    public static Bitmap ApplyOutlineFilter(Bitmap image)
    {
        Bitmap outlineImage = new Bitmap(image.Width, image.Height);
        int[,] kernel = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } }; 

        for (int x = 1; x < image.Width - 1; x++)
        {
            for (int y = 1; y < image.Height - 1; y++)
            {
                int r = 0, g = 0, b = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        Color pixel = image.GetPixel(x + i, y + j);
                        r += pixel.R * kernel[i + 1, j + 1];
                        g += pixel.G * kernel[i + 1, j + 1];
                        b += pixel.B * kernel[i + 1, j + 1];
                    }
                }
                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                b = Math.Min(255, Math.Max(0, b));
                outlineImage.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }

        return outlineImage;
    }
    // Filtru de Grayscale
    public static Bitmap ApplyGrayscaleFilter(Bitmap image)
    {
        Bitmap grayImage = new Bitmap(image.Width, image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                Color pixel = image.GetPixel(x, y);
                int gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                grayImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
            }
        }
        return grayImage;
    }

    // Filtru de Sepia
    public static Bitmap ApplySepiaFilter(Bitmap image)
    {
        Bitmap sepiaImage = new Bitmap(image.Width, image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                Color pixel = image.GetPixel(x, y);
                int r = (int)(pixel.R * 0.393 + pixel.G * 0.769 + pixel.B * 0.189);
                int g = (int)(pixel.R * 0.349 + pixel.G * 0.686 + pixel.B * 0.168);
                int b = (int)(pixel.R * 0.272 + pixel.G * 0.534 + pixel.B * 0.131);
                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                b = Math.Min(255, Math.Max(0, b));
                sepiaImage.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }
        return sepiaImage;
    }

    // Filtru de Negative
    public static Bitmap ApplyNegativeFilter(Bitmap image)
    {
        Bitmap negativeImage = new Bitmap(image.Width, image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                Color pixel = image.GetPixel(x, y);
                int r = 255 - pixel.R;
                int g = 255 - pixel.G;
                int b = 255 - pixel.B;
                negativeImage.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }
        return negativeImage;
    }
}