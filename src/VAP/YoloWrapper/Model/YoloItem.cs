// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Drawing;

namespace Wrapper.Yolo.Model
{
    public class YoloItem
    {
        public string Type { get; set; }
        public double Confidence { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ObjId { get; set; }
        public int TrackId { get; set; }

        public Point Center()
        {
            return new Point(this.X + this.Width / 2, this.Y + this.Height / 2);
        }

        public float[] CenterVec()
        {
            float[] vec = { this.X + this.Width / 2, this.Y + this.Height / 2 };
            return vec;
        }
    }
}
