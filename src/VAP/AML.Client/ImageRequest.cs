// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.IO;
using System.Linq;
using Google.Protobuf;
using Tensorflow;
using Tensorflow.Serving;
using DataType = Tensorflow.DataType;

namespace AML.Client
{
    public class ImageRequest : IScoringRequest
    {
        private readonly ModelSpec _modelSpec;
        private readonly TensorProto _proto;
        private readonly string _inputName;

        public ImageRequest(string inputName, params Stream[] images)
        {
            _modelSpec = new ModelSpec();
            _proto = new TensorProto { Dtype = DataType.DtString };

            var bytes = images.Select(ByteString.FromStream);
            _proto.StringVal.AddRange(bytes);
            _proto.TensorShape = new TensorShapeProto();
            _proto.TensorShape.Dim.Add(new TensorShapeProto.Types.Dim());
            _proto.TensorShape.Dim[0].Size = images.Length;
            _inputName = inputName;
        }

        public PredictRequest MakePredictRequest()
        {
            var request = new PredictRequest { ModelSpec = _modelSpec };

            request.Inputs[this._inputName] = _proto;
            return request;
        }
    }
}