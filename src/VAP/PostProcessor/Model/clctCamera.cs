// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Runtime.Serialization;
using System;
using System.Globalization;

namespace PostProcessor.Model
{
    [DataContract(Name = "clctCamera")]
    public class Camera
    {
        [DataMember(Name = "_key")]
        public string Key { get; set; }

        [DataMember(Name = "_id")]
        public string ID { get; set; }

        [DataMember(Name = "_rev")]
        public string Rev { get; set; }

        [DataMember(Name = "camera_id")]
        public int CameraID { get; set; }

        [DataMember(Name = "frame")]
        public int Frame { get; set; }

        [DataMember(Name = "obj_num")]
        public int ObjNum { get; set; }

        [DataMember(Name = "VideoInput")]
        public Uri VideoInput { get; set; }

        [DataMember(Name = "YOLOCONFIG_CFG")]
        public Uri YoloCfg { get; set; }

        [DataMember(Name = "YOLOCONFIG_NAMES")]
        public Uri YoloNames { get; set; }

        [DataMember(Name = "YOLOCONFIG_WEIGHTS")]
        public Uri YoloWeights { get; set; }

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