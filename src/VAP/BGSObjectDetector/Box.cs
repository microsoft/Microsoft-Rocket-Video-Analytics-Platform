// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Drawing;

using OpenCvSharp;

namespace BGSObjectDetector
{
    public class Box
    {
        public Rectangle Rectangle { get; private set; }

        public uint ID { get; private set; }
        public string ObjectCategory { get; private set; }

        public int Timestamp { get; set; }
        public DateTime? Time { get; set; }
        public int X0 { get { return Rectangle.X; } }
        public int X1 { get { return Rectangle.X + Rectangle.Width; } }
        public int Y0 { get { return Rectangle.Y; } }
        public int Y1 { get { return Rectangle.Y + Rectangle.Height; } }

        public PointF Center { get { return new PointF(X0 + Width / 2, Y0 + Height / 2); } }
        public int Width { get { return Rectangle.Width; } }
        public int Height { get { return Rectangle.Height; } }

        public int Area { get { return Width * Height; } }

        public bool IsEligibleToStartTrack { get; set; }

        public bool IsPointInterior(int x, int y)
        {
            System.Diagnostics.Debug.Assert(X0 < X1);
            System.Diagnostics.Debug.Assert(Y0 < Y1);
            return Rectangle.Contains(x, y);
        }

        // for storing histogram value
        public float[] hist;
        public void SetHist(float[] h)
        {
            for (int i = 0; i < h.Length; i++) { hist[i] = h[i]; }
        }

        // for storing feature values
        public Mat desc;
        public void SetDesc(Mat d)
        {
            try { desc = d.Clone(); }
            catch (Exception) {; }
        }

        //public Box(string objectCategory, int x0, int x1, int y0, int y1, int timestamp, uint id = 0) // old 
        public Box(string objectCategory, int x0, int x1, int y0, int y1, int timestamp, uint id = 0, DateTime? time = null, bool isEligibleToStartTrack = true)
        {
            this.ObjectCategory = objectCategory;

            this.Timestamp = timestamp;
            this.Time = time;
            this.Rectangle = new Rectangle(x0, y0, x1 - x0, y1 - y0);
            //this.X0 = x0;
            //this.X1 = x1;
            //this.Y0 = y0;
            //this.Y1 = y1;

            this.ID = id;

            this.IsEligibleToStartTrack = isEligibleToStartTrack;

            // used for different similarity metrics
            this.hist = new float[256];
            this.desc = null;
        }

        public static double MaxOverlapFraction(Box b0, Box b1)
        {
            Rectangle intersection = Rectangle.Intersect(b0.Rectangle, b1.Rectangle);
            double f0 = (intersection.Width * intersection.Height) / (double)(b0.Width * b0.Height);
            double f1 = (intersection.Width * intersection.Height) / (double)(b1.Width * b1.Height);
            return Math.Max(f0, f1);
        }
    }
}
