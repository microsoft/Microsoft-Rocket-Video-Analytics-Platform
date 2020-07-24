powershell -Command "New-Item -ItemType directory -Path ./src/VAP/TFWrapper/packages/TensorFlowSharp.1.12.0/runtimes/win7-x64/native/"
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-libtensorflow.dll', './src/VAP/TFWrapper/packages/TensorFlowSharp.1.12.0/runtimes/win7-x64/native/libtensorflow.dll')"

powershell -Command "(New-Object Net.WebClient).DownloadFile('https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-opencv_world340.dll', './src/VAP/YoloWrapper/Dependencies/opencv_world340.dll')"

powershell -Command "(New-Object Net.WebClient).DownloadFile('https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-opencv_world340d.dll', './src/VAP/YoloWrapper/Dependencies/opencv_world340d.dll')"

powershell -Command "(New-Object Net.WebClient).DownloadFile('https://pjreddie.com/media/files/yolov3.weights', './src/VAP/YoloWrapper/Yolo.Config/YoloV3Coco/yolov3.weights')"

powershell -Command "(New-Object Net.WebClient).DownloadFile('https://pjreddie.com/media/files/yolov3-tiny.weights', './src/VAP/YoloWrapper/Yolo.Config/YoloV3TinyCoco/yolov3-tiny.weights')"

powershell -Command "(New-Object Net.WebClient).DownloadFile('https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-yolov3ort.onnx', './modelOnnx/yolov3ort.onnx')"

powershell -Command "(New-Object Net.WebClient).DownloadFile('https://aka.ms/Microsoft-Rocket-Video-Analytics-Platform-yolov3tinyort.onnx', './modelOnnx/yolov3tinyort.onnx')"