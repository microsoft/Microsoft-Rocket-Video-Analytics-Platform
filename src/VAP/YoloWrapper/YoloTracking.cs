// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Wrapper.Yolo.Model;

namespace Wrapper.Yolo
{
    public class YoloTracking
    {
        private YoloWrapper _yoloWrapper;
        private Point _trackingObject = new Point (0, 0);
        private int _maxDistance;
        public int _index;

        //distance-based validation
        public YoloTracking(YoloWrapper yoloWrapper, int maxDistance = int.MaxValue)
        {
            this._yoloWrapper = yoloWrapper;
            this._maxDistance = maxDistance;
        }

        public void SetTrackingObject(Point trackingObject)
        {
            this._trackingObject = trackingObject;
        }

        public List<YoloTrackingItem> Analyse(byte[] imageData, HashSet<string> category, Brush bboxColor)
        {
            var items = this._yoloWrapper.Track(imageData);
            if (items == null || items.Count() == 0)
            {
                return null;
            }

            var probableObject = this.FindAllMatch(items, this._maxDistance, category);
            if (probableObject.Count() == 0)
            {
                return null;
            }

            List<YoloTrackingItem> validObjects = new List<YoloTrackingItem>();
            foreach (var obj in probableObject)
            {
                var taggedImageData = this.DrawImage(imageData, obj, bboxColor);
                var croppedImageData = this.CropImage(imageData, obj);

                validObjects.Add(new YoloTrackingItem(obj, this._index, taggedImageData, croppedImageData));
                this._index++;
            }
            return validObjects;
        }

        private YoloItem FindBestMatch(IEnumerable<YoloItem> items, int maxDistance)
        {
            var distanceItems = items.Select(o => new { Distance = this.Distance(o.Center(), this._trackingObject), Item = o }).Where(o => o.Distance <= maxDistance).OrderBy(o => o.Distance);

            var bestMatch = distanceItems.FirstOrDefault();
            return bestMatch?.Item;
        }

        //find all match based on distance
        private List<YoloItem> FindAllMatch(IEnumerable<YoloItem> items, int maxDistance, HashSet<string> category)
        {
            List<YoloItem> yItems = new List<YoloItem>();
            var distanceItems = items.Select(o => new { Category = o.Type, Distance = this.Distance(o.Center(), this._trackingObject), Item = o }).Where(o => (category.Count == 0 || category.Contains(o.Category)) && o.Distance <= maxDistance).OrderBy(o => o.Distance);
            foreach (var item in distanceItems)
            {
                YoloItem yItem = item.Item;
                yItems.Add(yItem);
            }
            return yItems;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(this.Pow2(p2.X - p1.X) + Pow2(p2.Y - p1.Y));
        }

        private double Pow2(double x)
        {
            return x * x;
        }

        private byte[] DrawImage(byte[] imageData, YoloItem item, Brush color)
        {
            using (var memoryStream = new MemoryStream(imageData))
            using (var image = Image.FromStream(memoryStream))
            using (var canvas = Graphics.FromImage(image))
            using (var pen = new Pen(color, 3))
            {
                canvas.DrawRectangle(pen, item.X, item.Y, item.Width, item.Height);
                canvas.Flush();

                using (var memoryStream2 = new MemoryStream())
                {
                    image.Save(memoryStream2, ImageFormat.Bmp);
                    return memoryStream2.ToArray();
                }
            }
        }

        private byte[] CropImage(byte[] imageData, YoloItem item)
        {
            using (var memoryStream = new MemoryStream(imageData))
            using (var image = Image.FromStream(memoryStream))
            {
                Rectangle cropRect = new Rectangle(item.X, item.Y, Math.Min(image.Width-item.X, item.Width), Math.Min(image.Height-item.Y, item.Height));
                Bitmap bmpImage = new Bitmap(image);
                Image croppedImage = bmpImage.Clone(cropRect, bmpImage.PixelFormat);

                using (var memoryStream2 = new MemoryStream())
                {
                    croppedImage.Save(memoryStream2, ImageFormat.Bmp);
                    return memoryStream2.ToArray();
                }
            }
        }

        public double getDistance(Point p1)
        {
            return Distance(p1, this._trackingObject);
        }
    }
}
