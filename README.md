# WallpaperCore
A desktop application to change the desktop background image every X seconds.

# About

You use the program by inserting the system path of the folder you wish to load images from. 

Remember to check the 'Include subfolders' box if you want to include subfolders. Then click 'Start'.

<p float="left">
  <img src="https://github.com/Boenne/WallpaperCore/blob/main/readme_images/program.png" width="450" />
  <img src="https://github.com/Boenne/WallpaperCore/blob/main/readme_images/program_running.png" width="450" /> 
</p>

The 'Restart' button is used when the program is running and you add new images to the folder, 
then you click 'Restart' to load in the new images.
'Pause' simply pauses the program if you want to stay on the currently displayed desktop background image.

<h3>Bookmarks</h3>

You can add bookmarks by navigating to the 'Bookmarks' tab.

To load images from a bookmarked path simply click the '>>' button next to the bookmark in the list.

<p float="left">
  <img src="https://github.com/Boenne/WallpaperCore/blob/main/readme_images/program_bookmarks_add.png" width="450" />
  <img src="https://github.com/Boenne/WallpaperCore/blob/main/readme_images/program_bookmarks_actions.png" width="450" /> 
</p>

<h3>Settings</h3>

Under settings you have the option of specifying the system path of a folder, that you want the program to treat as your root folder.
With a root folder specified, you only have to insert the name of a given subfolder within that specified root folder, and not the whole system path.

<p float="left">
  <img src="https://github.com/Boenne/WallpaperCore/blob/main/readme_images/program_settings_rootfolder.png" width="450" />
  <img src="https://github.com/Boenne/WallpaperCore/blob/main/readme_images/program_running_rootfolder.png" width="450" /> 
</p>

<h3>Portrait images</h3>

If the images in the currently active folder are in portrait mode, the program will make a landscape copy where the image is placed to the right,
and the left is filled out with the top of the original image.

![alt tag](https://github.com/Boenne/Wallpaper/blob/master/readme_images/portrait_background.jpg)

These new images are placed in a subfolder of the currently active folder, which is deleted when closing the program or changing active folder. 
The name of the temporary folder is 'wallpaper_temps' by default, but can be changed in Settings.
