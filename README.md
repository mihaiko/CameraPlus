# Dynamic Camera

## Description
Beat Saber plugin based on the original CameraPlus.  
While in 3rd person, the camera will automatically avoid the incoming walls in order to keep the player/avatar in sight.

**!! Please keep in mind that this is a replacement for the CameraPlus plugin !!**  
**!! It will not work with both plugins installed !!**  

### Watch it in action [HERE](https://www.youtube.com/watch?v=y0fMcUkKPFE)!

## Installing
1. Use the [mod installer](https://github.com/Umbranoxio/BeatSaberModInstaller/releases) to get everything setup before using this.  
2. Download the latest `CameraPlus.dll` from the [releases](https://github.com/mihaiko/DynamicCamera/releases) page.  
3. Copy the downloaded `CameraPlus.dll` into your BeatSaber `plugins` folder.  


## Usage
After you run the game once, a `DynamicCamera.cfg` file is created within the Beat Saber folder.  

Edit that file to configure the plugin:  
* `fov` The horizontal field of view of the camera  
* `positionSmooth` How much position should smooth `SMALLER NUMBER = SMOOTHER`  
* `rotationSmooth` How much rotation should smooth `SMALLER NUMBER = SMOOTHER`  
* `thirdPerson` Whether third person camera is enabled  
* `avoidWalls` Whether the camera should avoid walls or not    
* `moveCameraInGame` Being able to move the camera in game `set to "False" for more FPS`  
* `cameraPreview` In game camera preview (next to the white box) `set to "False" for more FPS`  
* `3rdPersonCameraDistance` How far back the camera should be from the avatar/player  
* `3rdPersonCameraUpperHeight`, `3rdPersonCameraLowerHeight`, `3rdPersonCameraLateralNear`, `3rdPersonCameraLateralFar` Used to define the 8 possible camera positions  
* `3rdPersonCameraForwardPrediction` This is used to look for the walls `bigger value -> looks for the walls further`  
* `3rdPersonCameraSpeed` How fast the camera will move when switching positions  
* `lookAtPosX`, `lookAtPosY`, `lookAtPosZ` Define the position where the camera is looking  

While in game, you can press <kbd>F1</kbd> to toggle between first and third person.  

Credits to [xyonico](https://github.com/xyonico) for the original [CameraPlus](https://github.com/xyonico/CameraPlus).  
If you need help, ask us at the [Beat Saber Mod Group Discord Server](https://discord.gg/Cz6PTM5).
