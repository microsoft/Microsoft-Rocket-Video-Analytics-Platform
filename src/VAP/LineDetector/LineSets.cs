// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.IO;

namespace LineDetector
{
    class LineSets
    {
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
                    
                    //noLines has to be 1 in Line Detector
                    int x1 = (int)(Convert.ToInt32(fields[2 + 0 * 5]) * imageScaling);
                    int y1 = (int)(Convert.ToInt32(fields[3 + 0 * 5]) * imageScaling);
                    int x2 = (int)(Convert.ToInt32(fields[4 + 0 * 5]) * imageScaling);
                    int y2 = (int)(Convert.ToInt32(fields[5 + 0 * 5]) * imageScaling);
                    double threshold = Convert.ToDouble(fields[6 + 0 * 5]);
                    SingleLineCrossingDetector lineDetector = new SingleLineCrossingDetector(x1, y1, x2, y2, threshold, sFactor);

                    ret.Add(directionName, lineDetector);
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
