// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Runtime.Serialization;
using System;
using System.Globalization;

namespace PostProcessor.Model
{
    [DataContract(Name = "clctConsolidation")]
    public class Consolidation
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

        // @TODO: support multiple objects
        [DataMember(Name = "obj_id")]
        public int ObjID { get; set; }

        [DataMember(Name = "obj_name")]
        public string ObjName { get; set; }

        [DataMember(Name = "bbox")]
        public int[] Bbox { get; set; }

        [DataMember(Name = "prob")]
        public double Prob { get; set; }

        [DataMember(Name = "obj_moving_dir")]
        public string ObjDir { get; set; }

        [DataMember(Name = "imageUri")]
        public Uri ImageUri { get; set; }


        [DataMember(Name = "time")]
        public string Time { get; set; }
        
        [DataMember(Name = "VideoInput")]
        public string VideoInput { get; set; }

        [DataMember(Name = "YOLOCONFIG_CHEAP")]
        public string YoloCheap { get; set; }

        [DataMember(Name = "YOLOCONFIG_CHEAP_CFG")]
        public Uri YoloCheapCfg { get; set; }

        [DataMember(Name = "YOLOCONFIG_CHEAP_NAMES")]
        public Uri YoloCheapNames { get; set; }

        [DataMember(Name = "YOLOCONFIG_CHEAP_WEIGHTS")]
        public Uri YoloCheapWeights { get; set; }

        [DataMember(Name = "YOLOCONFIG_HEAVY")]
        public string YoloHeavy { get; set; }

        [DataMember(Name = "YOLOCONFIG_HEAVY_CFG")]
        public Uri YoloHeavyCfg { get; set; }

        [DataMember(Name = "YOLOCONFIG_HEAVY_NAMES")]
        public Uri YoloHeavyNames { get; set; }

        [DataMember(Name = "YOLOCONFIG_HEAVY_WEIGHTS")]
        public Uri YoloHeavyWeights { get; set; }
    }
}