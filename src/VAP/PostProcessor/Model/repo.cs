// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Runtime.Serialization;
using System;
using System.Globalization;

namespace PostProcessor.Model
{
    [DataContract(Name = "repo")]
    public class Repository
    {
        /* universal names */
        [DataMember(Name = "_key")]
        public string Key { get; set; }

        [DataMember(Name = "_id")]
        public string ID { get; set; }

        [DataMember(Name = "_rev")]
        public string Rev { get; set; }

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

        /* camera/object collection names */
        [DataMember(Name = "camera_id")]
        public int CameraID { get; set; }

        [DataMember(Name = "VideoInput")]
        public Uri VideoInput { get; set; }

        [DataMember(Name = "YOLOCONFIG_CFG")]
        public Uri YoloCfg { get; set; }

        [DataMember(Name = "YOLOCONFIG_NAMES")]
        public Uri YoloNames { get; set; }

        [DataMember(Name = "YOLOCONFIG_WEIGHTS")]
        public Uri YoloWeights { get; set; }

        /* camera collection names */
        [DataMember(Name = "frame")]
        public int Frame { get; set; }

        [DataMember(Name = "obj_num")]
        public int ObjNum { get; set; }

        /* object collection names */
        [DataMember(Name = "obj_id")]
        public int ObjID { get; set; }

        [DataMember(Name = "obj_name")]
        public string ObjName { get; set; }

        [DataMember(Name = "track_id")]
        public int TrackID { get; set; }

        /* detection collection names */
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
    }
}