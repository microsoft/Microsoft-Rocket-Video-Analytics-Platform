// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;

namespace Wrapper.ORT
{
    public class ORTItem
    {
        public string ObjName { get; set; }
        public double Confidence { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ObjId { get; set; }
        public string TriggerLine { get; set; }
        public int TriggerLineID { get; set; }

        public ORTItem(int x, int y, int width, int height, int catId, string catName, double confidence, int lineID, string lineName)
        {
            this.X = Math.Max(0, x);
            this.Y = Math.Max(0, y);
            this.Width = width;
            this.Height = height;
            this.ObjId = catId;
            this.ObjName = catName;
            this.Confidence = confidence;
            this.TriggerLineID = lineID;
            this.TriggerLine = lineName;
        }
    }
}
