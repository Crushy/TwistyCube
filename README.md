# TwistyCube

An implementation of a classic Rubik's cube in Unity3D (2019.1.1f1 to be precise). Aimed at mobile and PC.
I've tried to keep everything short, simple and modular (well, as best as I could considering I did it in a week and have a full-time job).

## Architecture
The gameplay itself is contained in two classes: [MagicCube](../master/Assets/Scripts/Gameplay%20logic/MagicCube.cs) and [GameManager](../master/Assets/Scripts/Managers/GameManager.cs). `GameManager` is responsible for doing most of the underlying interactions betweens systems (namely the [Input Handling](../master/Assets/Scripts/Managers/InputManager.cs) and the [In-game UI](../master/Assets/Scripts/UI/InGameUIController.cs)). `MagicCube` focuses on calculating how to rotate the cube, generate a new game from a set of prefabs and a given size, what configuration counts as a victory and serializing most of the game state (in that particular case, MagicCube handles serializing the cube itself, while game manager serializes other game data such as the current timer).

The [PerSessionData](../master/Assets/Scripts/Structures/PerSessionData.cs) static class contains information that will be passed around scenes. It merely contains data relevant to the current game session that cannot be considered to belong to any particular class.

## Future work
* Some cube rotation methods were half-hardcoded due to lack of time but I'm confident it's quite possible to remove some 50 lines of code and replace them with pure math.
* Checking for victory requires going through every possible rotation axis along a single pivot and seeing if the local rotation of each cube is properly lined up. While this isn't quite a demanding operation, I'm sure there's smarter ways to accomplish the same result.

## External libraries
Most of the game was coded from scratch, including the cube models. However the following extras were used:
* My own (slightly changed) unity boilerplate found [here](https://gist.github.com/Crushy/062a2474ee09f8ebf240/).
* A few menu icons and a font from the ever-wonderful [Kenney](https://www.kenney.nl/assets).

## Other notes
* Whenever rotations are being carried over between methods, there is intentional use of a `RubikCubeRotation` data structure rather than using conventional vectors and/or quaternions. This is to avoid rounding errors and having improper rotations being passed around (to put it succintly, I wanted harsher type checking for cube rotations).
* Liberal use was made of `#pragma warning disable 649` to try and not get warnings for every single inspector parameter.
