// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Threading.Tasks;
using Tensorflow.Serving;

namespace AML.Client
{
    public interface IPredictionServiceClient
    {
        Task<PredictResponse> PredictAsync(PredictRequest predictRequest);
    }

    public class PredictionServiceClientWrapper : IPredictionServiceClient
    {
        private readonly PredictionService.PredictionServiceClient _predictionServiceClient;

        public PredictionServiceClientWrapper(PredictionService.PredictionServiceClient predictionServiceClient)
        {
            _predictionServiceClient = predictionServiceClient;
        }

        public Task<PredictResponse> PredictAsync(PredictRequest predictRequest) => _predictionServiceClient.PredictAsync(predictRequest).ResponseAsync;
    }
}