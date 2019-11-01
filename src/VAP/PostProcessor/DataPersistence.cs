// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using DNNDetector.Model;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Utils.Config;

namespace PostProcessor
{
    public enum Position
    {
        Right,
        Left,
        Up,
        Down,
        Unknown
    }

    public class DataPersistence
    {
        //string blobUri_BGS = null;
        static AzureBlobProcessor blobProcessor = new AzureBlobProcessor();
        
        // force precise initialization
        static DataPersistence() { }

        public static void PersistResult(string dbCollectionName, string videoUrl, int cameraID, int frameIndex, List<Item> detectionResult, Position[] objDir, string YOLOCONFIG, string YOLOCONFIG_HEAVY,
            string azureContainerName)
        {
            if (detectionResult != null && detectionResult.Count != 0)
            {
                foreach (Item it in detectionResult)
                {
                    var fileList = Directory.GetFiles(@OutputFolder.OutputFolderAll, $"frame-{frameIndex}*");
                    string blobName = Path.GetFileName(fileList[fileList.Length-1]);
                    //string blobName = it.Model == "Cheap" ? $@"frame-{frameIndex}-Cheap-{it.Confidence}.jpg" : $@"frame-{frameIndex}-Heavy-{it.Confidence}.jpg";
                    string blobUri = SendDataToCloud(azureContainerName, blobName, @OutputFolder.OutputFolderAll + blobName);
                    string serializedResult = SerializeDetectionResult(videoUrl, cameraID, frameIndex, it, objDir, blobUri, YOLOCONFIG, YOLOCONFIG_HEAVY);
                    WriteDB(dbCollectionName, serializedResult);
                }
            }
        }

        public static string SendDataToCloud(string containerName, string blobName, string sourceFile)
        {
            return blobProcessor.UploadFileAsync(containerName, blobName, sourceFile).GetAwaiter().GetResult();
        }

        private static string SerializeDetectionResult(string videoUrl, int cameraID, int frameIndex, Item item, Position[] objDir, string imageUri, string YOLOCONFIG, string YOLOCONFIG_HEAVY)
        {
            Model.Consolidation detectionConsolidation = new Model.Consolidation();
            detectionConsolidation.Key = Guid.NewGuid().ToString();
            detectionConsolidation.CameraID = cameraID;
            detectionConsolidation.Frame = frameIndex;

            detectionConsolidation.ObjID = item.ObjId;
            detectionConsolidation.ObjName = item.ObjName;
            detectionConsolidation.Bbox = new int[] { item.X, item.Y, item.Height, item.Width };
            detectionConsolidation.Prob = item.Confidence;
            detectionConsolidation.ObjDir = objDir[0].ToString() + objDir[1].ToString();
            detectionConsolidation.ImageUri = new Uri(imageUri);

            detectionConsolidation.VideoInput = videoUrl;
            detectionConsolidation.YoloCheap = YOLOCONFIG;
            detectionConsolidation.YoloHeavy = YOLOCONFIG_HEAVY;
            detectionConsolidation.Time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");

            //Create a stream to serialize the object to.  
            MemoryStream ms = new MemoryStream();

            // Serializer the User object to the stream.  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Model.Consolidation));
            ser.WriteObject(ms, detectionConsolidation);
            byte[] json = ms.ToArray();
            ms.Close();
            return System.Text.Encoding.UTF8.GetString(json, 0, json.Length);
        }

        private static int WriteDB(string collectionName, string content)
        {
            //var createCltResult = Client.CreateCollection().Result;
            var createDocResult = DBClient.CreateDocument(collectionName, content).Result;
            return (int)createDocResult;
        }
    }
}
