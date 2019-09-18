using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleImage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(100, 50);
            Console.SetBufferSize(100, 50);
            Console.CursorVisible = false;

            var invertColor = false;

            var frames = new List<Bitmap>();
            foreach (var filename in Directory.GetFiles("llama_frame"))
            {
                frames.Add((Bitmap)Bitmap.FromFile(filename));
            }

            while (true)
            {
                for (int i = 0; i < frames.Count; i += 4)
                {
                    Console.Clear();
                    DrawToWindow(frames[i], Console.WindowWidth, Console.WindowHeight, invertColor);
                }

                Thread.Sleep(50);
            }

        }

        static void DrawToWindow(Bitmap bitmap, int width, int height, bool invertColors)
        {
            using (var smallerImage = (Bitmap)bitmap.GetThumbnailImage(width - 1, height - 1, null, IntPtr.Zero))
            {
                var lockRect = new Rectangle(0, 0, smallerImage.Width, smallerImage.Height);
                var bitmapData = smallerImage.LockBits(lockRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < smallerImage.Height; ++y)
                    {
                        for (int x = 0; x < smallerImage.Width; ++x)
                        {
                            var pixel = Marshal.ReadInt32(bitmapData.Scan0, y * bitmapData.Stride + x * 4);
                            Console.SetCursorPosition(x, y);

                            if (true)
                            {
                                Console.ForegroundColor= MapPixelToConsoleColor(pixel, invertColors);
                                Console.Write("*");
                            }
                            else
                            {
                                Console.BackgroundColor = MapPixelToConsoleColor(pixel, invertColors);
                                Console.Write(" ");
                            }
                        }
                    }
                }
                finally
                {
                    smallerImage.UnlockBits(bitmapData);
                }
            }
        }

        static int GetPixelComponent(int pixel, PixelComponent component)
        {
            switch (component)
            {
                case PixelComponent.Red: return (pixel >> 16) & 0xff;
                case PixelComponent.Green: return (pixel >> 8) & 0xff;
                case PixelComponent.Blue: return pixel & 0xff;
            }

            throw new Exception("Invalid component");
        }

        static ConsoleColor MapPixelToConsoleColor(int pixel, bool invertColors)
        {
            var colors = new List<KeyValuePair<int, ConsoleColor>>();
            colors.Add(new KeyValuePair<int, ConsoleColor>(0xff0000, ConsoleColor.Red));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0x00ff00, ConsoleColor.Green));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0x0000ff, ConsoleColor.Blue));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0xffff00, ConsoleColor.Yellow));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0xff00ff, ConsoleColor.Magenta));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0x00ffff, ConsoleColor.Cyan));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0xffffff, ConsoleColor.White));
            colors.Add(new KeyValuePair<int, ConsoleColor>(0x000000, ConsoleColor.Black));

            colors.Sort((a, b) =>
            {
                var px_r = GetPixelComponent(pixel, PixelComponent.Red);
                var px_g = GetPixelComponent(pixel, PixelComponent.Green);
                var px_b = GetPixelComponent(pixel, PixelComponent.Blue);

                var a_r = GetPixelComponent(a.Key, PixelComponent.Red);
                var a_g = GetPixelComponent(a.Key, PixelComponent.Green);
                var a_b = GetPixelComponent(a.Key, PixelComponent.Blue);

                var b_r = GetPixelComponent(b.Key, PixelComponent.Red);
                var b_g = GetPixelComponent(b.Key, PixelComponent.Green);
                var b_b = GetPixelComponent(b.Key, PixelComponent.Blue);

#if true
                int a_dist = (int)Math.Sqrt(Math.Pow(px_r - a_r, 2) + Math.Pow(px_g - a_g, 2) + Math.Pow(px_b - a_b, 2));
                int b_dist = (int)Math.Sqrt(Math.Pow(px_r - b_r, 2) + Math.Pow(px_g - b_g, 2) + Math.Pow(px_b - b_b, 2));

                if (invertColors)
                {
                    if (a_dist < b_dist)
                        return 1;

                    if (a_dist > b_dist)
                        return -1;
                }
                else
                {
                    if (a_dist < b_dist)
                        return -1;

                    if (a_dist > b_dist)
                        return 1;
                }

                return 0;
#else
                int cmp_r = (a_r - px_r) + (a_b - px_g) + (a_b - px_b);
                int cmp_g = (a_g - px_g) + (a_b - px_g) + (a_b - px_b);
                int cmp_b = (a_b - px_b) + (a_b - px_g) + (a_b - px_b);

                return (cmp_r + cmp_g + cmp_b) / 3;
#endif
            });

            return colors[0].Value;

            //var red = ((pixel >> 16) & 0xff) / 127;
            //var green = ((pixel >> 8) & 0xff) / 127;
            //var blue = (pixel & 0xff) / 127;
        }
    }

    enum PixelComponent
    {
        Red,
        Green,
        Blue
    }
}
