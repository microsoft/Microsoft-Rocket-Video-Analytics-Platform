// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Utils
{
    public class Utils
    {
        public static void cleanFolder(string folder)
        {
            Directory.CreateDirectory(folder);
            DirectoryInfo di = new DirectoryInfo(folder);
            foreach (FileInfo file in di.GetFiles()) file.Delete();
        }

        public static byte[] ImageToByteBmp(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        public static byte[] ImageToByteJpeg(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public static float checkLineBboxOverlapRatio((int x1, int y1, int x2, int y2) line, int bbox_x, int bbox_y, int bbox_w, int bbox_h)
        {
            float overlapRatio = 0.0F;
            int insidePixels = 0;

            IEnumerable<Point> linePixels = EnumerateLineNoDiagonalSteps(line.x1, line.y1, line.x2, line.y2);
            
            foreach(Point pixel in linePixels)
            {
                if ((pixel.X >= bbox_x) && (pixel.X <= bbox_x + bbox_w) && (pixel.Y >= bbox_y) && (pixel.Y <= bbox_y + bbox_h))
                {
                    insidePixels++;
                }
            }

            overlapRatio = (float)insidePixels / linePixels.Count();
            return overlapRatio;
        }

        private static IEnumerable<Point> EnumerateLineNoDiagonalSteps(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                yield return new Point(x0, y0);

                if (x0 == x1 && y0 == y1) break;

                e2 = 2 * err;

                // EITHER horizontal OR vertical step (but not both!)
                if (e2 > dy)
                {
                    err += dy;
                    x0 += sx;
                }
                else if (e2 < dx)
                { // <--- this "else" makes the difference
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public static byte[] DrawImage(byte[] imageData, int x, int y, int w, int h, Brush color, string annotation = "")
        {
            using (var memoryStream = new MemoryStream(imageData))
            using (var image = Image.FromStream(memoryStream))
            using (var canvas = Graphics.FromImage(image))
            using (var pen = new Pen(color, 3))
            {
                canvas.DrawRectangle(pen, x, y, w, h);
                canvas.DrawString(annotation, new Font("Arial", 16), color, new PointF(x, y - 20));
                canvas.Flush();

                using (var memoryStream2 = new MemoryStream())
                {
                    image.Save(memoryStream2, ImageFormat.Bmp);
                    return memoryStream2.ToArray();
                }
            }
        }

        public static byte[] CropImage(byte[] imageData, int x, int y, int w, int h)
        {
            using (var memoryStream = new MemoryStream(imageData))
            using (var image = Image.FromStream(memoryStream))
            {
                Rectangle cropRect = new Rectangle(x, y, Math.Min(image.Width - x, w), Math.Min(image.Height - y, h));
                Bitmap bmpImage = new Bitmap(image);
                Image croppedImage = bmpImage.Clone(cropRect, bmpImage.PixelFormat);

                using (var memoryStream2 = new MemoryStream())
                {
                    croppedImage.Save(memoryStream2, ImageFormat.Bmp);
                    return memoryStream2.ToArray();
                }
            }
        }
    }
}
