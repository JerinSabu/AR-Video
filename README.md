# AR-Video

#Project Description
This is an Android Augmented Reality (AR) app built with Unity and AR Foundation. It detects flat walls in a room and lets you tap the screen to place a video screen directly onto the wall.  Videos are hosted on a remote server and stream over the internet when you click a button. The app automatically resizes the screen to match the video's dimensions so it does not look stretched , and it uses a custom shader to remove the video background so the video archives transparency with alpha channel.

#Setup Steps
Open the project folder inside the Unity Editor.  
Go to Project Settings, select XR Plug-in Management, and turn on the ARCore loader for Android.  
Open the main scene file from your Project window.
Select the XR Origin object in the hierarchy and confirm that the AR Placement Manager and AR Video Content Loader scripts are attached in the Inspector.
Build the project as an APK file, install it on your phone, and run it.

#CDN Details
The remote assets are hosted using a public GitHub Pages repository as a Content Delivery Network
URL to the CDN repository : https://github.com/JerinSabu/ar-video-cdn
The two videos are linked to the system using the exact names Video_01 and Video_02.  
The files were processed using FFmpeg to change the audio format to Vorbis so it plays correctly on Android mobile speakers, while keeping the WebM VP8 format to protect the transparent background layer.
