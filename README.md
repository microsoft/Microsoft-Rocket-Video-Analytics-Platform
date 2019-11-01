# Microsoft Rocket Video Analytics Platform

A highly extensible software stack to empower everyone to build practical real-world live video analytics applications for object detection and alerting with cutting edge machine learning algorithms. The repository features a hybrid edge-cloud video analytics pipeline (built on **C# .NET Core**), which allows **TensorFlow DNN model plug-in**, **GPU/FPGA acceleration**, **docker containerization/Kubernetes orchestration**, and **interactive querying** for after-the-fact analysis.

## How to run the code

Microsoft Rocket Video Analytics Platform is currently in preview.

### Step 1: Set up environment

[Microsoft Visual Studio](https://visualstudio.microsoft.com/downloads/) (VS 2017 is preferred) is recommended IDE for Rocket. While installing Visual Studio, please also add C++ 2015.3 v14.00 (v140) toolset to your local machine. Snapshot below shows how to include C++ 2015.3 v14.00 from Visual Studio Installer. 

<img src="https://mntmwg.dm.files.1drv.com/y4mqIJhU_BMCDfndscmI1apnWjXOAd0FAGvjAuyVVt5tJyGgahURnXi4L8SMO9Wxw00IvLRp0cN4PEhhM1OevN28O8ejxoU5KY7syzsn6BWEPARNyabivS28P_PG1CznLltnPKfmt9pv4qMgVo-MV38XL2Snl8g6lPMaqIa6YWbgmxFfSAzeqbULngzrabIRyTy3lSDLrd39PEFnTwK-avkrQ?width=1608&height=1033&cropmode=none" alt="C++v140" width="600">

Follow [instructions](https://dotnet.microsoft.com/download) to install .NET Core 2.2 (2.2.102 is preferred).

To enable GPU support, install [CUDA Toolkit](https://developer.nvidia.com/cuda-downloads) and [cuDNN](https://docs.nvidia.com/deeplearning/sdk/cudnn-install/index.html#download).
* **CUDA 8.0** (e.g., cuda_8.0.61_win10_network.exe) is needed for Darknet (e.g., YOLO) models.

	Please make sure files in `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\extras\visual_studio_integration\MSBuildExtensions` are copied to `C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\VC\VCTargets\BuildCustomizations`

* **CUDA 9.1** (e.g., cuda_9.1.85_win10_network.exe) is needed to support TensorFlow models.

* **cuDNN 8.0** is preferred (e.g., cudnn-8.0-windows10-x64-v7.2.1.38.zip).
	
	Copy `<installpath>\cuda\bin\cudnn64_7.dll` to `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\bin`.  
	Copy `<installpath>\cuda\ include\cudnn.h` to `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\include`.  
	Copy `<installpath>\cuda\lib\x64\cudnn.lib` to `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\lib\x64`.  
	Add Variable Name: `CUDA_PATH` with Variable Value: `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0` into Environment Variables.

* **Restart your computer** after installing CUDA and cuDNN.

### Step 2: Run the pipeline

Check out the [repository](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform/).

#### Build on Windows
* Prepare video feeds. Rocket can be fed with either live video streams (e.g., `rtsp://<url>:<port>/`) or local video files (should be put into `\media\`). A sample video file `sample.mp4` is already included in `\media\`. 
* Prepare a configuration file (should be placed into `\cfg\`) used in line-based alerting and cascaded DNN calls. Each line in the file defines a line-of-interest with the format below.  
	`<line_name> <line_id> <x_1> <y_1> <x_2> <y_2> <overlap_threshold>`  
A line configuration file `sample.txt` manually created based on `sample.mp4` is also included in the folder `\cfg\`.
* Run `Config.bat` before the first time you run Rocket to download pre-compiled OpenCV and TensorFlow binaries as well as Darknet YOLO weights files. It may take few minutes depending on your network status. Proceed only when all downloads finish. 
* Launch `VAP.sln` in `src\VAP\` from Visual Studio.
* Set pipeline config `PplConfig` in VideoPipelineCore - App.config. We have pre-compiled six configurations in the code. 
	* 0: Line-based alerting
    * 1: Darknet Yolo v3 on every frame
    * 2: TensorFlow FastRCNN on every frame
    * 3: Background subtraction-based (BGS) early filtering -> Darknet Tiny Yolo -> Darknet Yolo v3
    * 4: BGS early filtering -> Darknet Tiny Yolo -> Database (ArangoDB and blob storage on Azure)
    * 5: BGS early filtering -> TensorFlow Fast R-CNN -> Azure Machine Learning (cloud)

* (Optional) Set up your own database and Azure Machine Learning Service if `PplConfig` is set to 4 or 5.
	* Azure Database (e.g., SQL database like [MySQL](https://azure.microsoft.com/en-us/services/mysql/) or NoSQL database such as [ArangoDB](https://azuremarketplace.microsoft.com/en/marketplace/apps/arangodb.arangodb?tab=Overview)).
	* Azure Machine Learning: deploy your models to an [Azure FPGA](https://docs.microsoft.com/en-us/azure/machine-learning/service/how-to-deploy-fpga-web-service).

* Build the solution.
* Run the code.
	* Using Visual Studio: set VideoPipelineCore - Property - Debug - Application Arguments `<video_file/camera_url> <line_detection_config_file> <sampling_factor> <resolution_factor> <object_category>`. To run Rocket on the sample video, for example, arguments can be set to `sample.mp4 sample.txt 1 1 car`.
	* Using Command Line (CMD or PowerShell): run `dotnet .\VideoPipelineCore.dll <video_file/camera_url> <line_detection_config_file> <sampling_factor> <resolution_factor> <object_category>` in `\src\VAP\VideoPipelineCore\bin\Debug\netcoreapp2.2`. For instance, `dotnet .\VideoPipelineCore.dll sample.mp4 sample.txt 1 1 car`.


### Step 3: Results
Output images are generated in `\src\VAP\VideoPipelineCore\bin\output`. You should also see results in console during running.