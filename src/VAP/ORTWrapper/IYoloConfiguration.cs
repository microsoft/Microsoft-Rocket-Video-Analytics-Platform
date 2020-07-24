// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Wrapper.ORT
{
    public interface IYoloConfiguration
    {
        uint ImageHeight { get; }
        uint ImageWidth { get; }
        string[] Labels { get; }
    }
}