# VoiDPlugins [.NET](.github/workflows/dotnet.yml)

VoiDPlugins is a collection of extensions for [OpenTabletDriver](https://github.com/OpenTabletDriver/OpenTabletDriver).

# [WindowsInk](https://github.com/X9VoiD/VoiDPlugins/wiki/WindowsInk)

      Pen pressure support
      100% compliance to Windows Ink
      Upto 8192 levels of pressure

# About

This fork will focus on WindowsInk plugin

# Fixes and improvement

- Merged the [WindowsInk: improve Pen Button behavior (right click hover) #55](https://github.com/X9VoiD/VoiDPlugins/pull/55)

- Always flush

# Using

You can also to read the [wiki](https://github.com/X9VoiD/VoiDPlugins/wiki/WindowsInk)

## Installation

Install [VMulti driver](https://github.com/X9VoiD/vmulti-bin/releases/latest)

Download the [WindowsInk Plugin](https://github.com/cueavyqwp/VoiDPlugins/releases/latest)

`OpenTabletDriver` > `Plugins` > `Open Plugin Manager` > `File` > `Install plugin` > select the WindowsInk Plugin which you downloaded

## Setup

Change the output mode to `Windows Ink Absolute Mode` or `Windows Ink Relative Mode`

Go to the Pen Settings, then change the bindings

Click the `...` button to use the `Advanced Binding Editor`

Select `Windows Ink` for the `Binding Type`

![Advanced Binding Editor](https://user-images.githubusercontent.com/10338031/103213787-b69b3f80-4949-11eb-8f69-695a1096c139.png)

Bind `Tip Binding` to `Pen Tip`

Bind `Eraser Binding` to `Eraser (Toggle)` or `Eraser (Hold)`

Bind `Pen Buttons` as you need

![Pen Buttons](https://user-images.githubusercontent.com/10338031/103214016-58bb2780-494a-11eb-9633-a80fcc213564.png)

Then click the `Apply` button to apply
