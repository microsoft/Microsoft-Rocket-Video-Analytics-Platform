// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

using OpenCvSharp;

namespace DNNDetector
{
    class FrameBuffer
    {
        Queue<Mat> frameBuffer;
        int bSize;

        public FrameBuffer(int size)
        {
            bSize = size;
            frameBuffer = new Queue<Mat>(bSize);
        }

        public void Buffer(Mat frame)
        {
            frameBuffer.Enqueue(frame);
            if (frameBuffer.Count > bSize)
                frameBuffer.Dequeue();
        }

        public Mat[] ToArray()
        {
            return frameBuffer.ToArray();
        }
    }
}
