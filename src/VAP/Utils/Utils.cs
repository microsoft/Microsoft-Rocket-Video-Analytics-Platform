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

        public static void cleanFolderAll()
        {
            cleanFolder(Config.OutputFolder.OutputFolderAll);
            cleanFolder(Config.OutputFolder.OutputFolderBGSLine);
            cleanFolder(Config.OutputFolder.OutputFolderLtDNN);
            cleanFolder(Config.OutputFolder.OutputFolderCcDNN);
            cleanFolder(Config.OutputFolder.OutputFolderAML);
            cleanFolder(Config.OutputFolder.OutputFolderFrameDNNDarknet);
            cleanFolder(Config.OutputFolder.OutputFolderFrameDNNTF);
            cleanFolder(Config.OutputFolder.OutputFolderFrameDNNONNX);
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

        public static float checkLineBboxOverlapRatio(int[] line, int bbox_x, int bbox_y, int bbox_w, int bbox_h)
        {
            (Point p1, Point p2) newLine = (new Point(line[0], line[1]), new Point(line[2], line[3]));
            return checkLineBboxOverlapRatio(newLine, bbox_x, bbox_y, bbox_w, bbox_h);
        }

        public static float checkLineBboxOverlapRatio((Point p1, Point p2) line, int bbox_x, int bbox_y, int bbox_w, int bbox_h)
        {
            float overlapRatio = 0.0F;
            int insidePixels = 0;

            IEnumerable<Point> linePixels = EnumerateLineNoDiagonalSteps(line.p1, line.p2);
            
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

        private static IEnumerable<Point> EnumerateLineNoDiagonalSteps(Point p0, Point p1)
        {
            int dx = Math.Abs(p1.X - p0.X), sx = p0.X < p1.X ? 1 : -1;
            int dy = -Math.Abs(p1.Y - p0.Y), sy = p0.Y < p1.Y ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                yield return p0;

                if (p0.X == p1.X && p0.Y == p1.Y) break;

                e2 = 2 * err;

                // EITHER horizontal OR vertical step (but not both!)
                if (e2 > dy)
                {
                    err += dy;
                    p0.X += sx;
                }
                else if (e2 < dx)
                { // <--- this "else" makes the difference
                    err += dx;
                    p0.Y += sy;
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

        public static List<Tuple<string, int[]>> ConvertLines(List<(string key, (Point p1, Point p2) coordinates)> lines)
        {
            List<Tuple<string, int[]>> newLines = new List<Tuple<string, int[]>>();
            foreach ((string key, (Point p1, Point p2) coordinates) line in lines)
            {
                int[] coor = new int[] { line.coordinates.p1.X, line.coordinates.p1.Y, line.coordinates.p2.X, line.coordinates.p2.Y };
                Tuple<string, int[]> newLine = new Tuple<string, int[]>(line.key, coor);
                newLines.Add(newLine);
            }
            return newLines;
        }

        public static Dictionary<string, int> CatHashSet2Dict(HashSet<string> cat)
        {
            Dictionary<string, int> catDict = new Dictionary<string, int>();
            foreach (string c in cat)
            {
                catDict.Add(c, 0);
            }
            return catDict;
        }
    }
}
