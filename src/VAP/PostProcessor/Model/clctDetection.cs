// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Runtime.Serialization;
using System;
using System.Globalization;

namespace PostProcessor.Model
{
    [DataContract(Name = "clctDetection")]
    public class Detection
    {
        [DataMember(Name = "_key")]
        public string Key { get; set; }

        [DataMember(Name = "_id")]
        public string ID { get; set; }

        [DataMember(Name = "_rev")]
        public string Rev { get; set; }

        [DataMember(Name = "_from")]
        public string From { get; set; }

        [DataMember(Name = "_to")]
        public string To { get; set; }

        [DataMember(Name = "bbox")]
        public int[] Bbox { get; set; }

        [DataMember(Name = "prob")]
        public double Prob { get; set; }

        [DataMember(Name = "obj_moving_dir")]
        public string ObjDir { get; set; }

        [DataMember(Name = "time")]
        private string Time { get; set; }

        [IgnoreDataMember]
        public DateTime RecordTime
        {
            get
            {
                return DateTime.ParseExact(Time, "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            }
        }
    }
}