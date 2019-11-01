// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Drawing;

using BGSObjectDetector;

namespace LineDetector
{
    public class DetectionLine
    {
        public static int MIN_BOX_SIZE = 1000;//smaller boxes than this will go.

        public static double DEFAULT_OCCUPANCY_THRESHOLD = 0.9; // default threhsold

        public int x1, y1, x2, y2;
        public double increment;
        public double overlapFractionThreshold = DEFAULT_OCCUPANCY_THRESHOLD;

        public DetectionLine(int a, int b, int c, int d)
        {
            x1 = a; y1 = b;
            x2 = c; y2 = d;
            double length = Math.Sqrt(Math.Pow((double)(y2 - y1), 2) + Math.Pow((double)(x2 - x1), 2));
            increment = 1 / (2 * length);
        }

        public DetectionLine(int a, int b, int c, int d, double l_threshold)
        {
            x1 = a; y1 = b;
            x2 = c; y2 = d;
            double length = Math.Sqrt(Math.Pow((double)(y2 - y1), 2) + Math.Pow((double)(x2 - x1), 2));
            increment = 1 / (2 * length);
            overlapFractionThreshold = l_threshold;
        }


        public double getFractionContainedInBox(Box b, Bitmap mask)
        {
            double eta = 0;
            double currentX = x1 + eta * (x2 - x1);
            double currentY = y1 + eta * (y2 - y1);
            double lastX = -1;
            double lastY = -1;
            int totalPixelCount = 0;
            int overlapCount = 0;

            do
            {
                if ((lastX == currentX) && (lastY == currentY)) continue;

                totalPixelCount++;

                bool isInside = b.IsPointInterior((int)currentX, (int)currentY);

                if (mask.GetPixel((int)currentX, (int)currentY).ToString() == "Color [A=255, R=255, G=255, B=255]")
                {
                    overlapCount++;
                }

                lastX = currentX; lastY = currentY;
                eta += increment;
                currentX = x1 + eta * (x2 - x1);
                currentY = y1 + eta * (y2 - y1);
            } while (eta <= 1);

            double fraction = (double)overlapCount / (double)totalPixelCount;
            return fraction;
        }

        public double getFractionInForeground(Bitmap mask)
        {
            double eta = 0;
            double currentX = x1 + eta * (x2 - x1);
            double currentY = y1 + eta * (y2 - y1);
            double lastX = -1;
            double lastY = -1;
            int totalPixelCount = 0;
            int overlapCount = 0;

            do
            {
                if ((lastX == currentX) && (lastY == currentY)) continue;

                totalPixelCount++;

                if (mask.GetPixel((int)currentX, (int)currentY).ToString() == "Color [A=255, R=255, G=255, B=255]")
                {
                    overlapCount++;
                }

                lastX = currentX; lastY = currentY;
                eta += increment;
                currentX = x1 + eta * (x2 - x1);
                currentY = y1 + eta * (y2 - y1);
            } while (eta <= 1);

            double fraction = (double)overlapCount / (double)totalPixelCount;
            return fraction;
        }


        public (double frac, Box b) getMaximumFractionContainedInAnyBox(List<Box> boxes, Bitmap mask)
        {
            double maxOverlapFraction = 0;
            Box maxB = null;
            for (int boxNo = 0; boxNo < boxes.Count; boxNo++)
            {
                Box b = boxes[boxNo];
                if (b.Area < MIN_BOX_SIZE) continue;
                double overlapFraction = getFractionContainedInBox(b, mask);
                if (overlapFraction > maxOverlapFraction)
                {
                    maxOverlapFraction = overlapFraction;
                    maxB = b;
                }
            }
            return (maxOverlapFraction, maxB);
        }

        public (bool occupied, Box box) isOccupied(List<Box> boxes, Bitmap mask)
        {
            (double frac, Box b) = getMaximumFractionContainedInAnyBox(boxes, mask);
            if (frac >= overlapFractionThreshold)
            {
                return (true, b);
            }
            else
            {
                return (false, null);
            }
        }

        public bool isOccupied(Bitmap mask)
        {
            double frac = getFractionInForeground(mask);
            if (frac >= overlapFractionThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return x1 + "\t" + y1 + "\t" + x2 + "\t" + y2 + "\t" + overlapFractionThreshold;
        }
    }
}
