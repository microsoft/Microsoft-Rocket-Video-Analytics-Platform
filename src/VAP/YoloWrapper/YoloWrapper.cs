// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Wrapper.Yolo.Model;

namespace Wrapper.Yolo
{
    public class YoloWrapper : IDisposable
    {
        public const int MaxObjects = 1000;
        private const string YoloLibraryCpu = @"x64\yolo_cpp_dll_cpu.dll"; // same dll for GPU/CPU
        private const string YoloLibraryGpuLt = @"x64\yolo_cpp_dll_gpu_lt.dll";
        private const string YoloLibraryGpuCc = @"x64\yolo_cpp_dll_gpu_cc.dll";

        private Dictionary<int, string> _objectType = new Dictionary<int, string>();
        private ImageAnalyzer _imageAnalyzer = new ImageAnalyzer();

        public DetectionSystem DetectionSystem = DetectionSystem.Unknown;
        public DNNMode DnnMode = DNNMode.Unknown;
        public EnvironmentReport EnvironmentReport { get; private set; }

        #region DllImport Cpu

        [DllImport(YoloLibraryCpu, EntryPoint = "init")]
        private static extern int InitializeYoloCpu(string configurationFilename, string weightsFilename, int gpu);

        [DllImport(YoloLibraryCpu, EntryPoint = "detect_image")]
        internal static extern int DetectImageCpu(string filename, ref BboxContainer container);

        [DllImport(YoloLibraryCpu, EntryPoint = "detect_mat")]
        internal static extern int DetectImageCpu(IntPtr pArray, int nSize, ref BboxContainer container);

        [DllImport(YoloLibraryCpu, EntryPoint = "dispose")]
        internal static extern int DisposeYoloCpu();

        [DllImport(YoloLibraryCpu, EntryPoint = "track_image")]
        internal static extern int TrackImageCpu(string filename, float nms, float thresh, ref BboxContainer container);

        [DllImport(YoloLibraryCpu, EntryPoint = "track_mat")]
        internal static extern int TrackImageCpu(IntPtr pArray, int nSize, float nms, float thresh, ref BboxContainer container);
        #endregion

        #region DllImport Gpu Lt

        [DllImport(YoloLibraryGpuLt, EntryPoint = "init")]
        internal static extern int InitializeYoloGpuLt(string configurationFilename, string weightsFilename, int gpu);

        [DllImport(YoloLibraryGpuLt, EntryPoint = "detect_image")]
        internal static extern int DetectImageGpuLt(string filename, ref BboxContainer container);

        [DllImport(YoloLibraryGpuLt, EntryPoint = "detect_mat")]
        internal static extern int DetectImageGpuLt(IntPtr pArray, int nSize, ref BboxContainer container);

        [DllImport(YoloLibraryGpuLt, EntryPoint = "dispose")]
        internal static extern int DisposeYoloGpuLt();

        [DllImport(YoloLibraryGpuLt, EntryPoint = "get_device_count")]
        internal static extern int GetDeviceCountLt();

        [DllImport(YoloLibraryGpuLt, EntryPoint = "get_device_name")]
        internal static extern int GetDeviceNameLt(int gpu, StringBuilder deviceName);

        [DllImport(YoloLibraryGpuLt, EntryPoint = "track_image")]
        internal static extern int TrackImageGpuLt(string filename, float nms, float thresh, ref BboxContainer container);

        [DllImport(YoloLibraryGpuLt, EntryPoint = "track_mat")]
        internal static extern int TrackImageGpuLt(IntPtr pArray, int nSize, float nms, float thresh, ref BboxContainer container);
        #endregion

        #region DllImport Gpu Cc

        [DllImport(YoloLibraryGpuCc, EntryPoint = "init")]
        internal static extern int InitializeYoloGpuCc(string configurationFilename, string weightsFilename, int gpu);

        [DllImport(YoloLibraryGpuCc, EntryPoint = "detect_image")]
        internal static extern int DetectImageGpuCc(string filename, ref BboxContainer container);

        [DllImport(YoloLibraryGpuCc, EntryPoint = "detect_mat")]
        internal static extern int DetectImageGpuCc(IntPtr pArray, int nSize, ref BboxContainer container);

        [DllImport(YoloLibraryGpuCc, EntryPoint = "dispose")]
        internal static extern int DisposeYoloGpuCc();

        [DllImport(YoloLibraryGpuCc, EntryPoint = "get_device_count")]
        internal static extern int GetDeviceCountCc();

        [DllImport(YoloLibraryGpuCc, EntryPoint = "get_device_name")]
        internal static extern int GetDeviceNameCc(int gpu, StringBuilder deviceName);

        [DllImport(YoloLibraryGpuCc, EntryPoint = "track_image")]
        internal static extern int TrackImageGpuCc(string filename, float nms, float thresh, ref BboxContainer container);

        [DllImport(YoloLibraryGpuCc, EntryPoint = "track_mat")]
        internal static extern int TrackImageGpuCc(IntPtr pArray, int nSize, float nms, float thresh, ref BboxContainer container);
        #endregion

        public YoloWrapper(YoloConfiguration yoloConfiguration, DNNMode dnnMode)
        {
            this.DnnMode = dnnMode;
            this.Initialize(yoloConfiguration.ConfigFile, yoloConfiguration.WeightsFile, yoloConfiguration.NamesFile, 0);
        }

        public YoloWrapper(string configurationFilename, string weightsFilename, string namesFilename, int gpu = 0)
        {
            this.Initialize(configurationFilename, weightsFilename, namesFilename, gpu);
        }

        public void Dispose()
        {
            switch (this.DetectionSystem)
            {
                case DetectionSystem.CPU:
                    DisposeYoloCpu();
                    break;
                case DetectionSystem.GPU:
                    switch (this.DnnMode)
                    {
                        case DNNMode.Frame:
                        case DNNMode.LT:
                            DisposeYoloGpuLt();
                            break;
                        case DNNMode.CC:
                            DisposeYoloGpuCc();
                            break;
                    }
                    break;
            }
        }

        private void Initialize(string configurationFilename, string weightsFilename, string namesFilename, int gpu = 0)
        {
            if (IntPtr.Size != 8)
            {
                throw new NotSupportedException("Only 64-bit process are supported");
            }

            this.EnvironmentReport = this.GetEnvironmentReport();
            //if (!this.EnvironmentReport.MicrosoftVisualCPlusPlus2017RedistributableExists)
            //{
            //    throw new DllNotFoundException("Microsoft Visual C++ 2017 Redistributable (x64)");
            //}

            this.DetectionSystem = DetectionSystem.CPU;
            if (this.EnvironmentReport.CudaExists && this.EnvironmentReport.CudnnExists)
            {
                this.DetectionSystem = DetectionSystem.GPU;
            }

            int deviceCount;
            StringBuilder deviceName;
            switch (this.DetectionSystem)
            {
                case DetectionSystem.CPU:
                    InitializeYoloCpu(configurationFilename, weightsFilename, 0);
                    break;
                case DetectionSystem.GPU:
                    switch (this.DnnMode)
                    {
                        case DNNMode.Frame:
                        case DNNMode.LT:
                            deviceCount = GetDeviceCountLt();
                            if (gpu > (deviceCount - 1))
                            {
                                throw new IndexOutOfRangeException("Graphic device index is out of range");
                            }

                            deviceName = new StringBuilder(); //allocate memory for string
                            GetDeviceNameLt(gpu, deviceName);
                            this.EnvironmentReport.GraphicDeviceName = deviceName.ToString();

                            InitializeYoloGpuLt(configurationFilename, weightsFilename, gpu);
                            break;
                        case DNNMode.CC:
                            deviceCount = GetDeviceCountCc();
                            if (gpu > (deviceCount - 1))
                            {
                                throw new IndexOutOfRangeException("Graphic device index is out of range");
                            }

                            deviceName = new StringBuilder(); //allocate memory for string
                            GetDeviceNameCc(gpu, deviceName);
                            this.EnvironmentReport.GraphicDeviceName = deviceName.ToString();

                            InitializeYoloGpuCc(configurationFilename, weightsFilename, gpu);
                            break;
                    }
                    break;
            }

            var lines = File.ReadAllLines(namesFilename);
            for (var i = 0; i < lines.Length; i++)
            {
                this._objectType.Add(i, lines[i]);
            }
        }

        private EnvironmentReport GetEnvironmentReport()
        {
            var report = new EnvironmentReport();

            ////https://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed/34209692#34209692
            //using (var registryKey = Registry.ClassesRoot.OpenSubKey(@"Installer\Dependencies\,,amd64,14.0,bundle", false))
            //{
            //    var displayName = registryKey.GetValue("DisplayName") as string;
            //    if (displayName.StartsWith("Microsoft Visual C++ 2017 Redistributable (x64)", StringComparison.OrdinalIgnoreCase))
            //    {
            //        report.MicrosoftVisualCPlusPlus2017RedistributableExists = true;
            //    }
            //}

            if (File.Exists(@"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\bin\cudnn64_7.dll"))
            {
                report.CudnnExists = true;
            }

            var envirormentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
            //if (envirormentVariables.Contains("cudnn"))
            //{
            //    //["CUDA_PATH"] = "C:\\Program Files\\NVIDIA GPU Computing Toolkit\\CUDA\\v9.2"
            //}

            if (envirormentVariables.Contains("CUDA_PATH"))
            {
                //var cudaVersion = envirormentVariables["CUDA_PATH"];
                report.CudaExists = true;
            }

            return report;
        }

        public IEnumerable<YoloItem> Detect(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Cannot find the file", filepath);
            }

            var container = new BboxContainer();
            var count = 0;
            switch (this.DetectionSystem)
            {
                case DetectionSystem.CPU:
                    count = DetectImageCpu(filepath, ref container);
                    break;
                case DetectionSystem.GPU:
                    switch (this.DnnMode)
                    {
                        case DNNMode.Frame:
                        case DNNMode.LT:
                            count = DetectImageGpuLt(filepath, ref container);
                            break;
                        case DNNMode.CC:
                            count = DetectImageGpuCc(filepath, ref container);
                            break;
                    }
                    break;
            }

            if (count == -1)
            {
                throw new NotImplementedException("c++ dll compiled incorrectly");
            }

            return this.Convert(container);
        }

        public IEnumerable<YoloItem> Track(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Cannot find the file", filepath);
            }

            var container = new BboxContainer();
            var count = 0;
            switch (this.DetectionSystem)
            {
                case DetectionSystem.CPU:
                    count = TrackImageCpu(filepath, (float)0.3, (float)0.2, ref container);
                    break;
                case DetectionSystem.GPU:
                    switch (this.DnnMode)
                    {
                        case DNNMode.Frame:
                        case DNNMode.LT:
                            count = TrackImageGpuLt(filepath, (float)0.3, (float)0.2, ref container);
                            break;
                        case DNNMode.CC:
                            count = TrackImageGpuCc(filepath, (float)0.3, (float)0.2, ref container);
                            break;
                    }
                    break;
            }

            if (count == -1)
            {
                throw new NotImplementedException("c++ dll compiled incorrectly");
            }

            return this.Convert(container);
        }

        public IEnumerable<YoloItem> Detect(byte[] imageData)
        {
            //if (!this._imageAnalyzer.IsValidImageFormat(imageData))
            //{
            //    throw new Exception("Invalid image data, wrong image format");
            //}

            var container = new BboxContainer();
            var size = Marshal.SizeOf(imageData[0]) * imageData.Length;
            var pnt = Marshal.AllocHGlobal(size);

            try
            {
                // Copy the array to unmanaged memory.
                Marshal.Copy(imageData, 0, pnt, imageData.Length);
                var count = 0;
                switch (this.DetectionSystem)
                {
                    case DetectionSystem.CPU:
                        count = DetectImageCpu(pnt, imageData.Length, ref container);
                        break;
                    case DetectionSystem.GPU:
                        switch (this.DnnMode)
                        {
                            case DNNMode.Frame:
                            case DNNMode.LT:
                                count = DetectImageGpuLt(pnt, imageData.Length, ref container);
                                break;
                            case DNNMode.CC:
                                count = DetectImageGpuCc(pnt, imageData.Length, ref container);
                                break;
                        }
                        break;
                }

                if (count == -1)
                {
                    throw new NotImplementedException("c++ dll compiled incorrectly");
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }

            return this.Convert(container);
        }

        public IEnumerable<YoloItem> Track(byte[] imageData)
        {
            if (!this._imageAnalyzer.IsValidImageFormat(imageData))
            {
                throw new Exception("Invalid image data, wrong image format");
            }

            var container = new BboxContainer();
            var size = Marshal.SizeOf(imageData[0]) * imageData.Length;
            var pnt = Marshal.AllocHGlobal(size);

            try
            {
                // Copy the array to unmanaged memory.
                Marshal.Copy(imageData, 0, pnt, imageData.Length);
                var count = 0;
                switch (this.DetectionSystem)
                {
                    case DetectionSystem.CPU:
                        count = TrackImageCpu(pnt, imageData.Length, (float)0.3, (float)0.2, ref container);
                        break;
                    case DetectionSystem.GPU:
                        switch (this.DnnMode)
                        {
                            case DNNMode.Frame:
                            case DNNMode.LT:
                                count = TrackImageGpuLt(pnt, imageData.Length, (float)0.3, (float)0.2, ref container);
                                break;
                            case DNNMode.CC:
                                count = TrackImageGpuCc(pnt, imageData.Length, (float)0.3, (float)0.2, ref container);
                                break;
                        }
                        break;
                }

                if (count == -1)
                {
                    throw new NotImplementedException("c++ dll compiled incorrectly");
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }

            return this.Convert(container);
        }

        private IEnumerable<YoloItem> Convert(BboxContainer container)
        {
            var yoloItems = new List<YoloItem>();
            foreach (var item in container.candidates.Where(o => o.h > 0 || o.w > 0))
            {
                var objectType = this._objectType[(int)item.obj_id];
                var yoloItem = new YoloItem() { X = (int)item.x, Y = (int)item.y, Height = (int)item.h, Width = (int)item.w,
                    Confidence = item.prob, Type = objectType, ObjId = (int)item.obj_id, TrackId = (int)item.track_id };
                yoloItems.Add(yoloItem);
            }

            return yoloItems;
        }
    }
}
