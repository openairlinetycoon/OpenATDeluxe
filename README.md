![Imgur](https://i.imgur.com/9eLVJQ5.png)


![](https://github.com/openairlinetycoon/OpenATDeluxe/workflows/Build%20Test/badge.svg)
# OpenATDeluxe
This is the repository of the Open Source remake of Airline Tycoon Deluxe.
It will add support for Full HD resolution, and native support for platforms like Windows (10), Linux and OSx.

## Contributing

Join our [Discord](https://discord.gg/epPf384)!

You can help by getting involved in active development of [things to do](https://github.com/openairlinetycoon/OpenATDeluxe/projects/1), by suggesting new features, reporting bugs of the original game, or by bug-testing the game in its current state. Thanks in advance !

## Progress
- Game rooms are nearly all added [#10](https://github.com/openairlinetycoon/OpenATDeluxe/issues/10)
- Data import is 90% finished [#5](https://github.com/openairlinetycoon/OpenATDeluxe/issues/5)
- The airport is implemented
- The player character is implemented

## Todo
- See [board](https://github.com/openairlinetycoon/OpenATDeluxe/projects/1)

## How to compile the Godot branch
- Download Godot 3.1 Mono or later
- Install Airline Tycoon Deluxe and download this repository
- Go to the Build Manager tab on the upper left corner of godot
- Enter the Airline Tycoon Deluxe path and hit "Extract Images" in the Images tab of the builder interface. Godot will now load all files. This will take a little while. You have to restart godot to see the new files, as godot does not update the explorer

(The game images can't be posted on github due to copyright reasons. Thats why there are files missing inside the project. Opening scenes let's godot know that there are files missing and it will delete all references to those files. If we run the project before godot checks for files, all missing files will be added!)


## How to build the project
- Go to the Build Manager tab on the upper left corner of godot
- Go to the Build Management tab.
- Click on Prepare Build. This will empty all of your image files in the Images folder.
- You can now Export the project using the export feature of godot.
(Don't forget to re extract all the image files)

## WIP Pictures
All images are work in progress!

![Imgur](https://i.imgur.com/Vc9CAym.gif)

![Imgur](https://i.imgur.com/A4toKcI.gif)

![atd.gif](https://user-images.githubusercontent.com/7768485/65977049-837dae00-e471-11e9-8426-26400f53eb59.gif)
