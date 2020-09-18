# Microsoft Rocket Video Analytics Platform

A highly extensible software stack to empower everyone to build practical real-world live video analytics applications for object detection and counting/alerting with cutting edge machine learning algorithms. The repository features a hybrid edge-cloud video analytics pipeline (built on **C# .NET Core**), which allows **TensorFlow DNN model plug-in**, **GPU/FPGA acceleration**, **docker containerization/Kubernetes orchestration**, and **interactive querying** for after-the-fact analysis. A brief summary of Rocket platform can be found inside :memo:[Rocket-features-and-pipelines.pdf](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf). 

Feel free to check out our :memo:[webinar on Rocket](https://note.microsoft.com/MSR-Webinar-Microsoft-Rocket-Registration-On-Demand.html) from Dec 2019. 

## How to run the code

### Step 1: Set up environment

#### Setup on Windows

* [Microsoft Visual Studio](https://visualstudio.microsoft.com/downloads/) (VS 2017 is preferred) is recommended IDE for Rocket on Windows 10. While installing Visual Studio, please also add C++ 2015.3 v14.00 (v140) toolset to your local machine. Snapshot below shows how to include C++ 2015.3 v14.00 from Visual Studio Installer.  
	<img src="https://mntmwg.dm.files.1drv.com/y4mqIJhU_BMCDfndscmI1apnWjXOAd0FAGvjAuyVVt5tJyGgahURnXi4L8SMO9Wxw00IvLRp0cN4PEhhM1OevN28O8ejxoU5KY7syzsn6BWEPARNyabivS28P_PG1CznLltnPKfmt9pv4qMgVo-MV38XL2Snl8g6lPMaqIa6YWbgmxFfSAzeqbULngzrabIRyTy3lSDLrd39PEFnTwK-avkrQ?width=1608&height=1033&cropmode=none" alt="C++v140" width="1000">

* Follow [instructions](https://dotnet.microsoft.com/download) to install .NET Core 2.2 (2.2.102 is preferred).

* To enable GPU support, install [CUDA Toolkit](https://developer.nvidia.com/cuda-downloads) and [cuDNN](https://docs.nvidia.com/deeplearning/sdk/cudnn-install/index.html#download). Please also make sure your [NVIDIA driver](https://www.nvidia.com/Download/index.aspx?lang=en-us) is up-to-date. 
	* **CUDA 8.0** (e.g., cuda_8.0.61_win10_network.exe) is needed for Darknet (e.g., YOLO) models.

		After installation, please make sure files in `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\extras\visual_studio_integration\MSBuildExtensions` are copied to `C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\VC\VCTargets\BuildCustomizations`

	* **CUDA 9.1** (e.g., cuda_9.1.85_win10_network.exe) is needed to support TensorFlow models.

	* **cuDNN v7** is preferred (e.g., cudnn-8.0-windows10-x64-v7.2.1.38.zip).
	
		Copy `<installpath>\cuda\bin\cudnn64_7.dll` to `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\bin`.  
		Copy `<installpath>\cuda\ include\cudnn.h` to `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\include`.  
		Copy `<installpath>\cuda\lib\x64\cudnn.lib` to `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\lib\x64`.  
		Add Variable Name: `CUDA_PATH` with Variable Value: `C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0` into Environment Variables.  
		<img src="https://mntnwg.dm.files.1drv.com/y4mqF18Exoc2MwmT_HccCRrH9_DkoCAa19frsS7tyQqnNGqfjFuowZTfAZsCysgmzMPsdfw4pN6SkYkLKRCcQfhDzD_uNYA3PlyqtPe9b9EXuXZhNfH3yZudjeJq9tVi5Gm_VVLeE2Y0j4AKW01ANs6e0qSHR027D2hKtrwCODB4yQp-f_SH3DAzi0HeoUZunwnExihbfQwZq1mNii-CZStmw?width=1950&height=1303&cropmode=none" alt="PathVariable" width="600">

	* **Restart your computer** after installing CUDA and cuDNN.

#### Setup on Linux
[Docker](https://www.docker.com/) is recommended to run Rocket on Linux. Below we use Ubuntu 16.04 as an example to walk through the steps of building Rocket docker image and run it with GPU acceleration. 

* Install .NET Core 2.2 SDK ([2.2.301](https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/sdk-2.2.301) is preferred).
* Install [docker-ce](https://docs.docker.com/install/linux/docker-ce/ubuntu/) (version 18.09.7 is preferred).
* Install [NVIDIA driver](https://github.com/NVIDIA/nvidia-docker/wiki/Frequently-Asked-Questions#how-do-i-install-the-nvidia-driver) based on your GPU model (e.g., 418.67 for Tesla GPU). 
* Install [nvidia-docker2](https://github.com/NVIDIA/nvidia-docker). The NVIDIA Container Toolkit allows users to build and run GPU accelerated Docker containers. Note that you do NOT need to install the CUDA toolkit on the host, but GPU driver needs to be installed. 

### Step 2: Run the pipeline

Check out the [repository](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform/).

#### Prepare video feeds and line configuration
* Prepare video feeds. Rocket can be fed with either live video streams (e.g., `rtsp://<url>:<port>/`) or local video files (should be put into `\media\`). A sample video file `sample.mp4` is already included in `\media\`. 
* Prepare a configuration file (should be placed into `\cfg\`) used in line-based counting/alerting and cascaded DNN calls. Each line in the file defines a line-of-interest with the format below.  
	`<line_name> <line_id> <x_1> <y_1> <x_2> <y_2> <overlap_threshold>`  
A line configuration file `sample.txt` manually created based on `sample.mp4` is also included in the folder `\cfg\`.
	<img src="https://y0q1qa.dm.files.1drv.com/y4mkb3rylNVwm6m-V5ZbyLr6B0kgnqlLr29Y7qGYP6RirrOntpoCKSKEwgSj5yeRLCP4WgqDUd9O7B77wqmP18h_nEHaH4_J1djnGQ0gIN1XDbArx2Unuo6sZVndCBJl1R_Iq8DIrDJoTo-trJz5CZ3LAMqlJ_UYrxb0PslDnhLj9hJf2sB0dTOuGCXKr_VEZSlp3jf55VEYSbTYYee-O5nXg?width=721&height=406&cropmode=none" alt="sampleline" width="700">

#### Build on Windows
* Run `Config.bat` before the first time you run Rocket to download pre-compiled OpenCV and TensorFlow binaries as well as Darknet YOLO weights files. It may take few minutes depending on your network status. Proceed only when all downloads finish. YOLOv3 and Tiny YOLOv3 are already included in Rocket. You can plug-in other [YOLO models](https://pjreddie.com/darknet/yolo/) as you wish.
* Launch `VAP.sln` in `src\VAP\` from Visual Studio.
* Set pipeline config `PplConfig` in VideoPipelineCore - App.config. We have pre-compiled six configurations in the code. Pipeline descriptions are also included in :memo:[Rocket-features-and-pipelines.pdf](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf).
	* 0: Line-based counting
    * 1: Darknet Yolo v3 on every frame ([slide #7](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf#page=7))
    * 2: TensorFlow FastRCNN on every frame ([slide #8](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf#page=8))
    * 3: Background subtraction-based (BGS) early filtering -> Darknet Tiny Yolo -> Darknet Yolo v3 ([slide #9](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf#page=9))
    * 4: BGS early filtering -> Darknet Tiny Yolo -> Database (ArangoDB and blob storage on Azure) ([slide #10](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf#page=10))
    * 5: BGS early filtering -> TensorFlow Fast R-CNN -> Azure Machine Learning (cloud) ([slide #11](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf#page=11))

* (Optional) Set up your own database and Azure Machine Learning service if `PplConfig` is set to 4 or 5.
	* **Azure Database**:
		* Deploy SQL database like [MySQL](https://docs.microsoft.com/en-us/azure/mysql/quickstart-create-mysql-server-database-using-azure-portal) or NoSQL database such as [ArangoDB](https://azuremarketplace.microsoft.com/en/marketplace/apps/arangodb.arangodb?tab=Overview) on Azure by creating a VM.
		* Supply database settings (e.g., server name, user name, credentials etc.) to Rocket in `App.Config`. 
		* You can also set up your cloud storage (e.g., Azure Blob Storage) to store images/videos. In pipeline 4, Rocket sends detection images to an Azure storage account and metadata to an Azure database.
		<img src="https://nk3c5a.dm.files.1drv.com/y4mViPKlFjblzARGYHPtEZq4arRSeNKYQH7irePU24VPBm9RVjjSw4rmEvR2HA_m2bJpifNheKlPmHSbOWetE3YRADh_b5F7ktXRRdH66Kl_fdixLfZB4CaXN4HzExothajMONt4iZ2y2wBl0kBD7a6Ip4T3N5YKPi1zCT6oeFm15aIzYRQXW-9QOWoUjR1G-4wwU_SL2iWAnxVoc8P2XAKSw?width=1689&height=289&cropmode=none" alt="azureconfig" width="800">
	* **Azure Machine Learning**: 
		* Deploy your deep learning models to Azure (e.g., using Azure Kubernetes Service or AKS) for inference with [GPU](https://docs.microsoft.com/en-us/azure/machine-learning/service/how-to-deploy-inferencing-gpus) or [FPGA](https://docs.microsoft.com/en-us/azure/machine-learning/service/how-to-deploy-fpga-web-service). 
		* After deploying your model as a web service, provide host URL, key, and service ID to VAP in `App.Config`. Rocker will handle the communication between local modules and the cloud service. 
		<img src="https://nykehw.dm.files.1drv.com/y4mPiLLJwAuWpNQYCYjxEeFGf0JO_pLRSQ4Pup8qWPNZPO4ATkQmzkYbMX0UPht3xJbsmMxqMPQ-_oZiycn76avbnidZAZZBAiEfRQFRQzi2soB2fSYERJ2hGpRjFues4fjCX4l9paDR40ivKSgK8wd7yduO1W171uoH7uVJ9db5N5pNWsyLxIlz50O9T987eJLWguZZWfCQ3S57ywDZqRpOw?width=1645&height=185&cropmode=none" alt="azureconfig" width="800">

* Build the solution.
* Run the code.
	* Using Visual Studio: set VideoPipelineCore - Property - Debug - Application Arguments `<video_file/camera_url> <line_detection_config_file> <sampling_factor> <resolution_factor> <object_category>`. To run Rocket on the sample video, for example, arguments can be set to `sample.mp4 sample.txt 1 1 car`.
	* Using Command Line (CMD or PowerShell): run `dotnet .\VideoPipelineCore.dll <video_file/camera_url> <line_detection_config_file> <sampling_factor> <resolution_factor> <object_category>` in `\src\VAP\VideoPipelineCore\bin\Debug\netcoreapp2.2`. For instance, `dotnet .\VideoPipelineCore.dll sample.mp4 sample.txt 1 1 car`.

#### Build on Linux
We have pre-built a Rocket docker image from [docker branch](https://github.com/microsoft/Microsoft-Rocket-Video-Analytics-Platform/tree/docker/) with local processing only ([slide #12](https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-Rocket-features-and-pipelines.pdf#page=12) without cloud parts). The image is hosted on [Docker Hub](https://hub.docker.com/), a public library and community for container images, and you will be asked to login before pull/push images (sign up first if you don't have an account).

To test on the pre-built Rocket image, run  
`docker pull ycshu086/rocket-sample-edgeonly:0.1`

Once pulled, run the command below to start Rocket with NVIDIA GPU.  
`docker run --runtime=nvidia -v <local directory>:/app/output ycshu086/rocket-sample-edgeonly:0.1 sample.mp4 sample.txt 1 1 car`

* **Build your own Rocket pipeline on Linux**
	* Pull base docker image with CUDA toolkit and OpenCV. This image is needed to build Rocket docker image.  
	`docker pull ycshu086/ubuntu-dotnetcore-opencv-opencvsharp-cuda-cudnn:<version>`.
	* Git clone [docker branch](https://github.com/microsoft/Microsoft-Rocket-Video-Analytics-Platform/tree/docker/) for source code to dockerize Rocket on Linux.
	* [Create line configuration file(s)](#prepare-video-feeds-and-line-configuration) inside `\cfg`. If you are running Rocket on a pre-recorded video, please also [copy the video file](#prepare-video-feeds-and-line-configuration) into `\media`.
	* (Optional) Update `\src\VAP\VideoPipelineCore\App.Config` to set proper parameters for database and Azure Machine Learning service connection.
	* Run `sudo chmod 744 Config.sh` and `sudo ./Config.sh` before the first time you build Rocket image to download pre-compiled TensorFlow binaries. 
	* Run `docker build` to [build](https://docs.docker.com/engine/reference/commandline/build/) Rocket image using `Dockerfile.VAP`.  
	`docker build -t <repository>/<image>:<version> -f Dockerfile.VAP .`
	* (Optional) Push Rocket image to a cloud repository (e.g., docker hub, Azure Container Registry etc.) if you need to run it somewhere else.  
	`docker push -t <repository>/<image>:<version>`

* **Run Rocket image on Linux**
	* Pull a pre-built Rocket docker image to the local machine. You can use `docker images` to check existing images.  
`docker pull -t <repository>/<image>:<version>`  

	* Mount volume into the container and run Rocket image with NVIDIA GPU.  
`docker run --runtime=nvidia -v <local directory>:/app/output <repository>/<image>:<version> sample.mp4 sample.txt 1 1 car`

### Step 3: Results
Output images are generated in folders in `\src\VAP\VideoPipelineCore\bin\` (Windows), or the local directory you mount during `docker run` on Linux. Results from different modules are sent to different directories (e.g., `output_bgsline` for background subtraction-based detector) whereas `output_all` has images from all modules. Name of each file consists of frame ID, module name, and confidence score. Below are few sample results from running pipeline 3 and pipeline 5 on `sample.mp4`. You should also see results printed in console during running.
<img src="https://xaiwzw.dm.files.1drv.com/y4mEvHlKolV_qDdm08IWsk9r3vyecfb1cyu1wYgZ1s5YUQ8Fi9o-_zMzUpxTI_7SlGaRyngn3ScbGSPUXjEcHqeqLN129dVBW2Yja8MdpZW5Tv497MQwPzxhqBZrKkniFxj9-_KkrYL3PUXDkyubagosUHoQpu6pv41ZoMps7lEnsE8ToQtod7TcOTaklkq5sQ0srSy3907Zcwql_I7CdQ_mg?width=1280&height=720&cropmode=none" alt="output" width="1280">
The above illustration on pipeline 3 shows that at frame 2679, background subtraction detected an object, tiny Yolo DNN confirmed it was a car with a confidence of 0.24, and heavy Yolo v3 confirmed it with a confidence of 0.92. Likewise, for pipeline 5 where the TensorFlow FastRNN model had a confidence of 0.55 and AzureML (in the cloud) came back with a confidence of 0.76 for the same object.
