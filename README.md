# OpenATDeluxe
This is the repository of the Open Source remake of Airline Tycoon Deluxe.
It adds support for a Full HD resolution, and a possibility for all SDL supported platforms.

# Progress
The main Engine is currenty in progress. Around 90% of files can be read, including *.gli .raw .csv*

The game is written in a mix between C++ and C#. All time crucial parts like SDL or reading or writing to files is written in C++ and the main game logic is inside a Mono C# instance run from the C++ side.

# How to compile
You need the following dependecies:
- SDL2
- SDL2_mixer
- Mono
For everything else take a look atat thethe wiki.
