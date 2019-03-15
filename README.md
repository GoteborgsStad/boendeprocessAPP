# Sambuh - App

## Overview
The Sambuh app is an Android/iOS app meant to interact with the data provided from the Sambuh API.

The app is constructed in Unity 2018.1.4f1.

https://unity3d.com/get-unity/download/archive

## Setup
1.  Clone the project.
2.  Open Unity and load the project.
3.  In the Unity project open _Master/EnvironmentConfiguration.cs.
4.  Enter the URLs for the backend and the federator's login/logout page.
5.  Configure firebase. Follow step 1 - 4 of the Firebase Setup process to add a Firebase configuration file to Unity. https://firebase.google.com/docs/unity/setup

## Building the project
Ensure that the wanted Targetsite is used in EnvironmentConfiguration.

Refer to Unity's instructions:
*  Android: https://unity3d.com/learn/tutorials/topics/mobile-touch/building-your-unity-game-android-device-testing
*  iOS: https://unity3d.com/learn/tutorials/topics/mobile-touch/building-your-unity-game-ios-device-testing