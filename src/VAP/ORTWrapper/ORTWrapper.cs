// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Wrapper.ORT
{
    public class ORTWrapper
    {
        static IYoloConfiguration cfg;
        static InferenceSession session1, session2;
        DNNMode mode = DNNMode.Unknown;

        public ORTWrapper(string modelPath, DNNMode mode)
        {
            // Optional : Create session options and set the graph optimization level for the session
            SessionOptions options = new SessionOptions();
            //options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
            cfg = new Yolov3BaseConfig();

            this.mode = mode;
            switch (mode)
            {
                case DNNMode.LT:
                case DNNMode.Frame:
                    session1 = new InferenceSession(modelPath, SessionOptions.MakeSessionOptionWithCudaProvider(0));
                    break;
                case DNNMode.CC:
                    session2 = new InferenceSession(modelPath, SessionOptions.MakeSessionOptionWithCudaProvider(0));
                    break;
            }
        }

        public List<ORTItem> UseApi(Bitmap bitmap, int h, int w)
        {
            float[] imgData = LoadTensorFromImageFile(bitmap);

            var container = new List<NamedOnnxValue>();
            //yolov3 customization
            var tensor1 = new DenseTensor<float>(imgData, new int[] { 1, 3, 416, 416 });
            container.Add(NamedOnnxValue.CreateFromTensor<float>("input_1", tensor1));
            var tensor2 = new DenseTensor<float>(new float[] { h, w }, new int[] { 1, 2 });
            container.Add(NamedOnnxValue.CreateFromTensor<float>("image_shape", tensor2));

            // Run the inference
            switch (mode)
            {
                case DNNMode.LT:
                case DNNMode.Frame:
                    using (var results = session1.Run(container))  // results is an IDisposableReadOnlyCollection<DisposableNamedOnnxValue> container
                    {
                        List<ORTItem> itemList = PostProcessing(results);
                        return itemList;
                    }
                case DNNMode.CC:
                    using (var results = session2.Run(container))  // results is an IDisposableReadOnlyCollection<DisposableNamedOnnxValue> container
                    {
                        List<ORTItem> itemList = PostProcessing(results);
                        return itemList;
                    }
            }

            return null;
        }

        public static float[] LoadTensorFromPreProcessedFile(string filename)
        {
            var tensorData = new List<float>();

            // read data from file
            using (var inputFile = new StreamReader(filename))
            {
                List<string> dataStrList = new List<string>();
                string line;
                while ((line = inputFile.ReadLine()) != null)
                {
                    dataStrList.AddRange(line.Split(new char[] { ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries));
                }

                string[] dataStr = dataStrList.ToArray();
                for (int i = 0; i < dataStr.Length; i++)
                {
                    tensorData.Add(Single.Parse(dataStr[i]));
                }
            }

            return tensorData.ToArray();
        }

        static float[] LoadTensorFromImageFile(Bitmap bitmap)
        {
            RGBtoBGR(bitmap);
            int iw = bitmap.Width, ih = bitmap.Height, w = 416, h = 416, nw, nh;

            float scale = Math.Min((float)w/iw, (float)h/ih);
            nw = (int)(iw * scale);
            nh = (int)(ih * scale);

            //resize
            Bitmap rsImg = ResizeImage(bitmap, nw, nh);
            Bitmap boxedImg = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(boxedImg))
            {
                gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 128, 128, 128)), 0, 0, boxedImg.Width, boxedImg.Height);
                gr.DrawImage(rsImg, new Point((int)((w - nw) / 2), (int)((h - nh) / 2)));
            }
            var imgData = boxedImg.ToNDArray(flat: false, copy: true);
            imgData /= 255.0;
            imgData = np.transpose(imgData, new int[] { 0, 3, 1, 2 });
            imgData = imgData.reshape(1, 3 * w * h);

            double[] doubleArray = imgData[0].ToArray<double>();
            float[] floatArray = new float[doubleArray.Length];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                floatArray[i] = (float)doubleArray[i];
            }

            return floatArray;
        }

        private static void RGBtoBGR(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                           ImageLockMode.ReadWrite, bmp.PixelFormat);

            int length = Math.Abs(data.Stride) * bmp.Height;

            byte[] imageBytes = new byte[length];
            IntPtr scan0 = data.Scan0;
            Marshal.Copy(scan0, imageBytes, 0, imageBytes.Length);

            byte[] rgbValues = new byte[length];
            for (int i = 0; i < length; i += 3)
            {
                rgbValues[i] = imageBytes[i + 2];
                rgbValues[i + 1] = imageBytes[i + 1];
                rgbValues[i + 2] = imageBytes[i];
            }
            Marshal.Copy(rgbValues, 0, scan0, length);

            bmp.UnlockBits(data);
        }

        static List<ORTItem> PostProcessing(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            List<ORTItem> itemList = new List<ORTItem>();

            List<float[]> out_boxes = new List<float[]>();
            List<float[]> out_scores = new List<float[]>();
            List<int> out_classes = new List<int>();

            var boxes = results.AsEnumerable().ElementAt(0).AsTensor<float>();
            var scores = results.AsEnumerable().ElementAt(1).AsTensor<float>();
            var indices = results.AsEnumerable().ElementAt(2).AsTensor<int>();
            int nbox = indices.Count() / 3;

            for (int ibox = 0; ibox < nbox; ibox++)
            {
                out_classes.Add(indices[0, 0, ibox * 3 + 1]);

                float[] score = new float[80];
                for (int j = 0; j < 80; j++)
                {
                    score[j] = scores[indices[0, 0, ibox * 3 + 0], j, indices[0, 0, ibox * 3 + 2]];
                }
                out_scores.Add(score);

                float[] box = new float[]
                {
                    boxes[indices[0, 0, ibox * 3 + 0], indices[0, 0, ibox * 3 + 2], 0],
                    boxes[indices[0, 0, ibox * 3 + 0], indices[0, 0, ibox * 3 + 2], 1],
                    boxes[indices[0, 0, ibox * 3 + 0], indices[0, 0, ibox * 3 + 2], 2],
                    boxes[indices[0, 0, ibox * 3 + 0], indices[0, 0, ibox * 3 + 2], 3]
                };
                out_boxes.Add(box);

                //output
                ORTItem item = new ORTItem((int)box[1], (int)box[0], (int)(box[3] - box[1]), (int)(box[2] - box[0]), out_classes[ibox], cfg.Labels[out_classes[ibox]], out_scores[ibox][out_classes[ibox]], 0, "lineName");
                itemList.Add(item);
            }

            return itemList;
        }

        public void DrawBoundingBox(Image imageOri,
            string outputImageLocation,
            string imageName,
            List<ORTItem> itemList)
        {
            Image image = (Image)imageOri.Clone();

            foreach (var item in itemList)
            {
                var x = Math.Max(item.X, 0);
                var y = Math.Max(item.Y, 0);
                var width = item.Width;
                var height = item.Height;
                string text = $"{item.ObjName} ({(item.Confidence * 100).ToString("0")}%)";
                using (Graphics thumbnailGraphic = Graphics.FromImage(image))
                {
                    thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    Font drawFont = new Font("Arial", 12, FontStyle.Bold);
                    SizeF size = thumbnailGraphic.MeasureString(text, drawFont);
                    SolidBrush fontBrush = new SolidBrush(Color.Black);
                    Point atPoint = new Point((int)(x + width / 2), (int)(y + height / 2) - (int)size.Height - 1);

                    // Define BoundingBox options
                    Pen pen = new Pen(Color.Pink, 3.2f);
                    SolidBrush colorBrush = new SolidBrush(Color.Pink);

                    thumbnailGraphic.FillRectangle(colorBrush, (int)(x + width / 2), (int)(y + height / 2 - size.Height - 1), size.Width, (int)size.Height);
                    thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

                    // Draw bounding box on image
                    thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
                    if (!Directory.Exists(outputImageLocation))
                    {
                        Directory.CreateDirectory(outputImageLocation);
                    }

                    image.Save(Path.Combine(outputImageLocation, imageName));
                }
            }
        }

        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(96, 96); //@todo: image.HorizontalResolution throw exceptions during docker build on linux
            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
