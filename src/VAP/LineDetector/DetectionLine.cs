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

        public Point p1;
        public Point p2;
        public double increment;
        public double overlapFractionThreshold = DEFAULT_OCCUPANCY_THRESHOLD;

        public DetectionLine(int a, int b, int c, int d)
        {
            p1 = new Point(a, b);
            p2 = new Point(c, d);
            double length = Math.Sqrt(Math.Pow((double)(p2.Y - p1.Y), 2) + Math.Pow((double)(p2.X - p1.X), 2));
            increment = 1 / (2 * length);
        }

        public DetectionLine(int a, int b, int c, int d, double l_threshold)
        {
            p1 = new Point(a, b);
            p2 = new Point(c, d);
            double length = Math.Sqrt(Math.Pow((double)(p2.Y - p1.Y), 2) + Math.Pow((double)(p2.X - p1.X), 2));
            increment = 1 / (2 * length);
            overlapFractionThreshold = l_threshold;
        }


        public double getFractionContainedInBox(Box b, Bitmap mask)
        {
            double eta = 0;
            double currentX = p1.X + eta * (p2.X - p1.X);
            double currentY = p1.Y + eta * (p2.Y - p1.Y);
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
                currentX = p1.X + eta * (p2.X - p1.X);
                currentY = p1.Y + eta * (p2.Y - p1.Y);
            } while (eta <= 1);

            double fraction = (double)overlapCount / (double)totalPixelCount;
            return fraction;
        }

        public double getFractionInForeground(Bitmap mask)
        {
            double eta = 0;
            double currentX = p1.X + eta * (p2.X - p1.X);
            double currentY = p1.Y + eta * (p2.Y - p1.Y);
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
                currentX = p1.X + eta * (p2.X - p1.X);
                currentY = p1.Y + eta * (p2.Y - p1.Y);
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
            return p1.X + "\t" + p1.Y + "\t" + p2.X + "\t" + p2.Y + "\t" + overlapFractionThreshold;
        }
    }
}
