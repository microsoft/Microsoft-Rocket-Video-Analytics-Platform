// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.IO;

namespace LineDetector
{
    class LineSets
    {
        /// <summary>
        /// Reads a line set from the provided CSV text file.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        /// <param name="sFactor">The </param>
        /// <param name="imageScaling"></param>
        /// <returns>Returns a Dictionary with all lines contained in the provided file, indexed by name.</returns>
        /// <remarks>
        /// The file provided to this function is expected to contain the following values in order, separated by the tab character:
        ///   <list type="bullet">
        ///     <item><description>The name of the line. (a string)</description></item>
        ///     <item><description>The number of lines in the file. (an integer)</description></item>
        ///     <item><description>The x coordinate of the first point of the line within the video frame. (an integer)</description></item>
        ///     <item><description>The y coordinate of the first point of the line within the video frame. (an integer)</description></item>
        ///     <item><description>The x coordinate of the second point of the line within the video frame. (an integer)</description></item>
        ///     <item><description>The y coordinate of the second point of the line within the video frame. (an integer)</description></item>
        ///     <item><description>The overlap fraction threshold.</description></item>
        ///   </list>
        /// </remarks>
        public static Dictionary<string, ILineBasedDetector> readLineSet_LineDetector_FromTxtFile(string fileName, int sFactor, double imageScaling)
        {
            Dictionary<string, ILineBasedDetector> ret = new Dictionary<string, ILineBasedDetector>();
            try
            {
                StreamReader r = new StreamReader(fileName);
                do
                {
                    string line = r.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    string[] fields = line.Split('\t');
                    string directionName = fields[0];
                    int noLines = Convert.ToInt32(fields[1]);
                    List<ISingleLineCrossingDetector> lineDetectors = new List<ISingleLineCrossingDetector>();

                    for (int i = 0; i < noLines; i++)
                    {
                        int x1 = (int)(Convert.ToInt32(fields[2 + 0 * 5]) * imageScaling);
                        int y1 = (int)(Convert.ToInt32(fields[3 + 0 * 5]) * imageScaling);
                        int x2 = (int)(Convert.ToInt32(fields[4 + 0 * 5]) * imageScaling);
                        int y2 = (int)(Convert.ToInt32(fields[5 + 0 * 5]) * imageScaling);
                        double threshold = Convert.ToDouble(fields[6 + 0 * 5]);
                        SingleLineCrossingDetector lineDetector = new SingleLineCrossingDetector(x1, y1, x2, y2, threshold, sFactor);

                        lineDetectors.Add(lineDetector);
                    }
                    List<int> minLags = new List<int>();
                    for (int i = 0; i < noLines - 1; i++)
                    {
                        int lag = Convert.ToInt32(fields[noLines * 5 + 2 + i]);
                        minLags.Add(lag);
                    }
                    List<int> maxLags = new List<int>();
                    for (int i = 0; i < noLines - 1; i++)
                    {
                        int lag = Convert.ToInt32(fields[noLines * 6 + 1 + i]);
                        maxLags.Add(lag);
                    }
                    CascadedLinesDetector counter = new CascadedLinesDetector(lineDetectors, minLags, maxLags);

                    ret.Add(directionName, counter);
                } while (true);
                r.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return ret;
        }
    }
}
