// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Runtime.InteropServices;

namespace Wrapper.Yolo.Model
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BboxContainer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = YoloWrapper.MaxObjects)]
        internal BboxT[] candidates;
    }
}
