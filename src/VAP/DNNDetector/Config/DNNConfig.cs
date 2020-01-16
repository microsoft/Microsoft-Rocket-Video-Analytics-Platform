// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace DNNDetector.Config
{
    public static class DNNConfig
    {
        public static double CONFIDENCE_THRESHOLD { get; set; } = 0.7; //threshold for calling heavy DNN

        public static int FRAME_SEARCH_RANGE { get; set; } = 50; // frames

        public static int ValidRange { get; set; } = 200; // pixels. Deprecated with overlap checking instead.

        public static double MIN_SCORE_FOR_TFOBJECT_OUTPUT { get; set; } = 0.5; //TFWrapper.cs

        public static double MIN_SCORE_FOR_LINEBBOX_OVERLAP_SMALL { get; set; } = 0;

        public static double MIN_SCORE_FOR_LINEBBOX_OVERLAP_LARGE { get; set; } = 0.2;
    }
}