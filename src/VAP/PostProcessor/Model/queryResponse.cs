// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Runtime.Serialization;
using System.Collections.Generic;

namespace PostProcessor.Model
{
    [DataContract(Name = "queryResponse")]
    public class QueryRaw
    {
        [DataMember(Name = "result")]
        public List<Consolidation> QResult { get; set; }

        [DataMember(Name = "hasmore")]
        public bool Hasmore { get; set; }

        [DataMember(Name = "cached")]
        public bool Cached { get; set; }

        //[DataMember(Name = "extra")]
        //public string Extra { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "code")]
        public int Code { get; set; }
    }
}