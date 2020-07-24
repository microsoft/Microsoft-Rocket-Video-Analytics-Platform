// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Drawing;

namespace DNNDetector.Model
{
    public class Item
    {
        public string ObjName { get; set; }
        public double Confidence { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ObjId { get; set; }
        public int TrackId { get; set; }
        public int Index { get; set; }
        public byte[] RawImageData { get; set; }
        public byte[] TaggedImageData { get; set; }
        public byte[] CroppedImageData { get; set; }
        public string TriggerLine { get; set; }
        public int TriggerLineID { get; set; }
        public string Model { get; set; }

        public Item(int x, int y, int width, int height, int catId, string catName, double confidence, int lineID, string lineName)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.ObjId = catId;
            this.ObjName = catName;
            this.Confidence = confidence;
            this.TriggerLineID = lineID;
            this.TriggerLine = lineName;
        }

        public Item(Wrapper.ORT.ORTItem onnxYoloItem)
        {
            this.X = onnxYoloItem.X;
            this.Y = onnxYoloItem.Y;
            this.Width = onnxYoloItem.Width;
            this.Height = onnxYoloItem.Height;
            this.ObjId = onnxYoloItem.ObjId;
            this.ObjName = onnxYoloItem.ObjName;
            this.Confidence = onnxYoloItem.Confidence;
            this.TriggerLineID = onnxYoloItem.TriggerLineID;
            this.TriggerLine = onnxYoloItem.TriggerLine;
        }

        public Point Center()
        {
            return new Point(this.X + this.Width / 2, this.Y + this.Height / 2);
        }

        public float[] CenterVec()
        {
            float[] vec = { this.X + this.Width / 2, this.Y + this.Height / 2 };
            return vec;
        }

        public void Print()
        {
            Console.WriteLine("{0} {1,-5} {2} {3,-5} {4} {5,-5} {6} {7,-10} {8} {9,-10:N2}",
                                    "Index:", Index, "ObjID:", ObjId, "TrackID:", TrackId, "Type:", ObjName, "Conf:", Confidence);
        }
    }
}
