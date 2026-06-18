# STS2 Theme

<p align="center">
  <a href="https://www.bilibili.com/video/BV1UbLf6HEbR">
    <img src="Preview/logo.png" width="200" alt="STS2 Theme logo">
  </a>
</p>

## Wallpaper Manager & Runtime Theme Editor

**[STS2 Theme](https://www.bilibili.com/video/BV1UbLf6HEbR) is a Wallpaper Manager, allows you
to select and manage personalized Slay the Spire2 main menu scene from a unified interface.**  
It provides a lightweight runtime editor that allows users to create personalized wallpapers with
out the need for [Godot Engine](https://godotengine.org).  
The wallpaper will be saved in TSCN format
and support secondary editing in [Godot Engine](https://godotengine.org).
![MainMenu](https://raw.github.com/MoeNegi-Zero/STS2-Theme/tree/main/Preview/MainMenu.gif)

## Independent & Community-driven

STS2 Theme is fully independent and community-driven, empowering users to help share their Wallpaper.  
Please do not steal the works of other creators or violate the laws of your country.

## Features

### Mapping
STS2 Theme contains 2 cfg files and an rsrc file for resource mapping

The cfg used to save Theme Info will be saved in **Themes/{Title}/{Title}.cfg.**  
You can change the name or migrate it to any location in the Themes folder freely.

The cfg used to remember the default wallpaper is located in the dll folder.  
**If you delete the wallpaper you are currently using**, please modify the path in cfg or delete cfg.

The rsrc file used for mapping resources is located in Themes/{Title}/{Title}. rsrc.  
When changing the name, please ensure that the path in {Title}. cfg is also modified.


### Preview Card

Theme Card in STS2 Theme Manager has a Hover Event.

If you made a Preview Scene.

You can point the PreviewThemePath in the cfg file to the preview path. 

For example:  
PreviewThemePath=Selphina/Scenes/preview.tscn

In this case, ThemeCard will attempt to load  
**res://Themes/Selphina/Scenes/preview.tscn**  

![Preview Card](https://raw.github.com/MoeNegi-Zero/STS2-Theme/tree/main/Preview/ThemeManager.gif).

### Runtime Editor

STS2 runtime editor can help you create themes by changing the node Transfrom/Layer or adding some supported nodes in game.

Currently accepted file formats

Image: png  
Video: ogv

![RuntimeEditor](https://raw.github.com/MoeNegi-Zero/STS2-Theme/tree/main/Preview/RuntimeEditor.png)

## Buy me a coffee

![CoffeePlz](https://raw.github.com/MoeNegi-Zero/STS2-Theme/tree/main/Preview/CoffeePlz.png)