using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        string imagePath = "D:/Documents/APD/Proiect_APD/Images/image9.jpg";

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
                filteredImage = ApplyOutlineFilterPlinq(image);
                Console.WriteLine("Filtrul Outline a fost aplicat.");
                break;
            case 2:
                filteredImage = ApplyGrayscaleFilterPlinq(image);
                Console.WriteLine("Filtrul Grayscale a fost aplicat.");
                break;
            case 3:
                filteredImage = ApplySepiaFilterPlinq(image);
                Console.WriteLine("Filtrul Sepia a fost aplicat.");
                break;
            case 4:
                filteredImage = ApplyNegativeFilterPlinq(image);
                Console.WriteLine("Filtrul Negative a fost aplicat.");
                break;
            default:
                Console.WriteLine("Optiune invalida!");
                return;
        }
        stopwatch.Stop();
        Console.WriteLine("\nTimpul de procesare (paralel):" + stopwatch.ElapsedMilliseconds + "ms");

        string defaultSaveFolder = "D:/Documents/APD/Proiect_APD/Images_filtered/";

        Console.Write("Introdu numele imaginii (fara extensie): ");
        string imageName = Console.ReadLine();

        string savePath = defaultSaveFolder + imageName + ".jpg";

        Console.WriteLine("\nSe salveaza imaginea ...");

        filteredImage.Save(savePath);
        Console.WriteLine("\nImaginea a fost salvata la:" + savePath);


        if (image != null)
        {
            image.Dispose();
        }
        if (filteredImage != null)
        {
            filteredImage.Dispose();
        }
    }

    private static byte[] GetImageData(Bitmap image, out BitmapData bmpData)
    {
        Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
        bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        int bytes = Math.Abs(bmpData.Stride) * image.Height;
        byte[] rgbValues = new byte[bytes];

        Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

        return rgbValues;
    }

    private static void SetImageData(BitmapData bmpData, byte[] rgbValues)
    {
        int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
        Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
    }

    public static Bitmap ApplyOutlineFilterPlinq(Bitmap image)
    {
        Bitmap outlineImage = new Bitmap(image.Width, image.Height);

        int[,] kernel = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };

        BitmapData srcData = null;
        byte[] srcPixels = GetImageData(image, out srcData);

        BitmapData destData = null;
        Rectangle rect = new Rectangle(0, 0, outlineImage.Width, outlineImage.Height);
        destData = outlineImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        int destBytes = Math.Abs(destData.Stride) * outlineImage.Height;
        byte[] destPixels = new byte[destBytes];

        int width = image.Width;
        int height = image.Height;
        int stride = srcData.Stride;

        var pixelCoords = (from x in Enumerable.Range(1, width - 2)
                           from y in Enumerable.Range(1, height - 2)
                           select (x, y)).AsParallel();

        pixelCoords.ForAll(coord =>
        {
            int x = coord.x;
            int y = coord.y;

            int r = 0, g = 0, b = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int neighborX = x + i;
                    int neighborY = y + j;
                    int pixelPos = (neighborY * stride) + (neighborX * 4);

                    b += srcPixels[pixelPos] * kernel[i + 1, j + 1];
                    g += srcPixels[pixelPos + 1] * kernel[i + 1, j + 1];
                    r += srcPixels[pixelPos + 2] * kernel[i + 1, j + 1];
                }
            }

            b = Math.Min(255, Math.Max(0, b));
            g = Math.Min(255, Math.Max(0, g));
            r = Math.Min(255, Math.Max(0, r));

            int destPos = (y * destData.Stride) + (x * 4);
            destPixels[destPos] = (byte)b;
            destPixels[destPos + 1] = (byte)g;
            destPixels[destPos + 2] = (byte)r;
            destPixels[destPos + 3] = 255;
        });

        Marshal.Copy(destPixels, 0, destData.Scan0, destBytes);
        image.UnlockBits(srcData);
        outlineImage.UnlockBits(destData);

        return outlineImage;
    }


    public static Bitmap ApplyGrayscaleFilterPlinq(Bitmap image)
    {
        Bitmap grayImage = new Bitmap(image.Width, image.Height);

        BitmapData srcData = null;
        byte[] srcPixels = GetImageData(image, out srcData);

        BitmapData destData = null;
        Rectangle rect = new Rectangle(0, 0, grayImage.Width, grayImage.Height);
        destData = grayImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        int destBytes = Math.Abs(destData.Stride) * grayImage.Height;
        byte[] destPixels = new byte[destBytes];

        int width = image.Width;
        int height = image.Height;
        int stride = srcData.Stride;

        var pixels = Enumerable.Range(0, width * height).AsParallel();

        pixels.ForAll(index =>
        {
            int x = index % width;
            int y = index / width;

            int pos = (y * stride) + (x * 4);
            byte b = srcPixels[pos];
            byte g = srcPixels[pos + 1];
            byte r = srcPixels[pos + 2];

            byte gray = (byte)(r * 0.299 + g * 0.587 + b * 0.114);

            int destPos = (y * destData.Stride) + (x * 4);
            destPixels[destPos] = gray;
            destPixels[destPos + 1] = gray;
            destPixels[destPos + 2] = gray;
            destPixels[destPos + 3] = 255;
        });

        Marshal.Copy(destPixels, 0, destData.Scan0, destBytes);
        image.UnlockBits(srcData);
        grayImage.UnlockBits(destData);

        return grayImage;
    }

    public static Bitmap ApplySepiaFilterPlinq(Bitmap image)
    {
        Bitmap sepiaImage = new Bitmap(image.Width, image.Height);

        BitmapData srcData = null;
        byte[] srcPixels = GetImageData(image, out srcData);

        BitmapData destData = null;
        Rectangle rect = new Rectangle(0, 0, sepiaImage.Width, sepiaImage.Height);
        destData = sepiaImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        int destBytes = Math.Abs(destData.Stride) * sepiaImage.Height;
        byte[] destPixels = new byte[destBytes];

        int width = image.Width;
        int height = image.Height;
        int stride = srcData.Stride;

        var pixels = Enumerable.Range(0, width * height).AsParallel();

        pixels.ForAll(index =>
        {
            int x = index % width;
            int y = index / width;

            int pos = (y * stride) + (x * 4);
            byte b = srcPixels[pos];
            byte g = srcPixels[pos + 1];
            byte r = srcPixels[pos + 2];

            int newR = (int)(r * 0.393 + g * 0.769 + b * 0.189);
            int newG = (int)(r * 0.349 + g * 0.686 + b * 0.168);
            int newB = (int)(r * 0.272 + g * 0.534 + b * 0.131);

            newR = Math.Min(255, Math.Max(0, newR));
            newG = Math.Min(255, Math.Max(0, newG));
            newB = Math.Min(255, Math.Max(0, newB));

            int destPos = (y * destData.Stride) + (x * 4);
            destPixels[destPos] = (byte)newB;
            destPixels[destPos + 1] = (byte)newG;
            destPixels[destPos + 2] = (byte)newR;
            destPixels[destPos + 3] = 255;
        });

        Marshal.Copy(destPixels, 0, destData.Scan0, destBytes);
        image.UnlockBits(srcData);
        sepiaImage.UnlockBits(destData);

        return sepiaImage;
    }

    public static Bitmap ApplyNegativeFilterPlinq(Bitmap image)
    {
        Bitmap negativeImage = new Bitmap(image.Width, image.Height);

        BitmapData srcData = null;
        byte[] srcPixels = GetImageData(image, out srcData);

        BitmapData destData = null;
        Rectangle rect = new Rectangle(0, 0, negativeImage.Width, negativeImage.Height);
        destData = negativeImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        int destBytes = Math.Abs(destData.Stride) * negativeImage.Height;
        byte[] destPixels = new byte[destBytes];

        int width = image.Width;
        int height = image.Height;
        int stride = srcData.Stride;

        var pixels = Enumerable.Range(0, width * height).AsParallel();

        pixels.ForAll(index =>
        {
            int x = index % width;
            int y = index / width;

            int pos = (y * stride) + (x * 4);
            byte b = srcPixels[pos];
            byte g = srcPixels[pos + 1];
            byte r = srcPixels[pos + 2];

            byte newB = (byte)(255 - b);
            byte newG = (byte)(255 - g);
            byte newR = (byte)(255 - r);

            int destPos = (y * destData.Stride) + (x * 4);
            destPixels[destPos] = newB;
            destPixels[destPos + 1] = newG;
            destPixels[destPos + 2] = newR;
            destPixels[destPos + 3] = 255;
        });

        Marshal.Copy(destPixels, 0, destData.Scan0, destBytes);
        image.UnlockBits(srcData);
        negativeImage.UnlockBits(destData);

        return negativeImage;
    }

}