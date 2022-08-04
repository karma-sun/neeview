## ChangeLog

----

### 39.5
(2022-08-xx)

#### Fixed

- Restored the SQLite library to the previous one to reduce the error phenomenon when closing the application.
- Fixed a bug that read-only shortcuts could not be processed.
- Language file update (zh-TW).

----

### 39.4
(2022-07-04)

#### New
- Supports Windows11 snap layouts

#### Fixed
- Fixed a bug that the bookshelf exclusion filter does not work when adding files
- Fixed a bug that caused the thumbnail operation to become very slow due to certain operations.
- Fixed a bug that the coordinates of the image shift when returning from minimization to full screen.
- Fixed a bug that folder thumbnails are not updated.
- Fixed a bug that the "Move focus to search box" command may not be focused.
- Fixed a bug that the "Load subfolders at this location" setting does not work when opening previous or next workbooks.
- Fixed a bug when cruising from a book loading a subfolder.
- Fixed a bug when an invalid path is passed to the path specification dialog.
- Fixed a bug that shortcut files are not recognized when opening playlists as books.
- Fixed a bug when dragging and dropping shortcuts for multiple archive files.
- Fixed a bug that the shortcut of UNICODE name could not be recognized.
- Fixed a bug that may not be reflected even if deleted from the history list.
- Fixed a bug that an error occurs in the "Prev(Next) playlist item" command when the playlist is "Current book only".
- Fixed the problem that the brightness may change when applying the resize filter.
- Script: Fixed a bug where the effects of Patch() would continue to remain.
- Script: Fixed an issue with large arrays.
- Correcting typographical errors.

#### Changed
- Libraries update.
- Language file update (zh-TW).

----

### 39.3
(2021-07-17)

####  New

- Language: Supports 中文(中国)

#### Fixed

- Fixed a bug that the taskbar is displayed when returning from minimization to full screen
- Improved the problem that the taskbar is not displayed when the window is maximized when the taskbar is set to be hidden automatically.
- Fixed a bug that an error occurs in the "Prev/Next History" command.
- Fixed a bug where you couldn't rename in a floating panel
- Fixed initial selection bug when renaming Quick Access
- Fixed a bug that shortcut keys are not displayed in the context menu of the folder tree
- Fixed a bug that theme loading fails when the app is placed in the path containing "#"

#### Changed

- Added "Text copy" setting to "Copy file" command parameter. Select the type of text that will be copied to the clipboard.

----

### 39.2
(2021-06-26)

#### Fixed

- Fixed the main menu not to take focus.
- Fixed a bug where the layout of the startup help dialog was broken.
- Fixed tilt wheel operation to be one command. (Settings > Command > Limit tilt wheel operation to one time)

----

### 39.1
(2021-06-20)

#### Fixed

- Fixed a bug that the scroll type changes to "Diagonal scroll" when the parameter of "Scroll + Prev" command is set.
- Fixed a bug that could cause blurring when applying the resize filter.
- Fixed a bug where the README file could not be opened when the application was placed in a path containing multibyte characters or spaces.

----

### 39.0
(2021-06-??)

#### Important

##### Integrate Pagemark into Playlist

- Pagemark have been abolished. The previous pagemarks will be carried over as a playlist named "Pagemark".
- A new playlist panel has been added.
- You can create multiple playlists and switch between them. You can treat the selected playlist like a Pagemark.
- The playlists managed in the Playlist panel are limited to those placed in a dedicated folder, but existing playlist files can still be used.
- In the page mark, it was grouped by book, but in the playlist, it is grouped by folder or compressed file.

##### Renewal of appearance

- Almost all UI controls have been tuned.
- We increased the theme. The theme color setting in the menu section has been abolished. (Settings > Window > Theme)
- It is now possible to freely color by creating a custom theme. See [here](https://bitbucket.org/neelabo/neeview/wiki/Theme) for the theme file format.
- Themes are now applied to the settings window as well.
- The font settings have been totally revised. (Settings > Fonts)

##### Information panel renewal

- Changed to display a lot of EXIF information.
- Enabled to switch the display information when displaying 2 pages.

#### New

- Language: Compatible with Chinese(Taiwan). (Thanks to the provider!)
- Setting: Added settings for the web browser and text editor to be used. (Settings > General)
- Setting: Add scripts and custom themes to your export data.
- Command: The command can be cloned. Right-click the command in the command list of settings and select "Clone" to create it. Only commands with parameters can be cloned.
- Command: Added "Delete invalid history items".
- Command: Tilt wheel compatible.
- MainView: Hover scroll. (Menu > Image > Hover scroll)
- MainView: Added view margin settings. (Settings > Window > Main view margin)
- MainView: Corresponds to the loupe by pressing and holding the touch.
- QuickAccess: Enabled to change the name. You can also change the reference path from the quick access properties.
- Navigator: Added display area thumbnails. (Detailed menu in the navigator panel)
- Navigator: Added settings to maintain rotation expansion and contraction even when the book is changed. Change from the context menu of the pushpin button in the navigator panel.
- PageSlider: Added slider display ON / OFF command. (Menu > View > Slider)
- PageSlider: Added playlist registration mark display ON / OFF setting for slider. (Setting > Slider)
- Filmstrip: Display the playlist registration mark. (Setting > Filmstrip)
- Filmstrip: Implemented context menu on filmstrip.
- Script: Added error level setting. (Setting > Script > Obsolete member access error level)
- Script: Changed to monitor changes in the script folder.
- Script: Added script command argument nv.Args[]. Specify in the command parameter of the script command.
- Script: Added page switching event OnPageChanged.
- Script: Added instruction nv.Book.Wait() to wait for page loading to complete.
- Script: Added nv.Environment
- Develop: We have prepared a multilingual development environment. See [here](https://bitbucket.org/neelabo/neeview/src/master/Language/Readme.md) for more information.

#### Fixed

- Setting: Fixed a bug that data is incorrect when using a semicolon in the extension setting.
- Setting: Fixed a bug that the initialization button of the extension setting does not work.
- Setting: Fixed a bug that the list box disappears after searching for settings.
- Other: Fixed a bug that page recording is not working.
- Window: Fixed a bug that thumbnail images pop up in rare cases.
- Window: Fixed a bug that the panel may also be hidden when the context menu is closed.
- Window: Fixed a bug that the display size of certain pop-up thumbnails is incorrect.
- Window: Fixed multiple selection behavior of list.
- MainView: Fixed a bug that the aspect ratio may be incorrect when rotating the RAW camera image.
- Bookshelf: Fixed a bug that the mark indicating the current book may not be displayed.
- ScriptConsole: Fixed a bug that the application terminates abnormally with "exit".
- Script: Fixed a bug that the image size was the value after the limit.
- Script: Fixed a bug that the Enter key input of ShowInputDialog affects the main window.
- Script: Enabled to get the path with the default path setting.

#### Changed

- Setting: The file operation permission in the initial state has been turned off. (Menu > Option > File operation)
- Network: When the network access permission setting is OFF, when connecting to the Internet with a Web browser, a confirmation dialog is displayed instead of being invalid.
- Command: Added command parameters to change N-type scroll to Z-type scroll.
- Command: Added a stop parameter for line breaks to the N-type scroll command.
- Command: Added working directory settings for external apps.
- Command: Added a mode to open from the left page when opening multiple pages with an external application.
- Command: Added command parameters to import and export commands.
- Book: Added registration order in page order. Only works for playlists. Otherwise it works as a name order.
- Window: Added automatic display judgment setting for the overlapping part of the side panel and menus and sliders. (Settings > Panels)
- Window: The area width of the automatic display judgment is divided into the vertical direction and the horizontal direction. (Settings > Panels)
- Window: The tab movement of the entire main window has been adjusted from the upper left to the lower right.
- MainView: Changed to process non-animated GIF as an image.
- MainView: Added parameters to mouse drag operation. (Settings > Mouse operation)
- Bookshelf: A search path is also valid for "Home Location".
- PageList: Changed to open the current book as a selection page by moving the parent.
- Effect: Expanded custom size function.
- PageSlider: Added thickness setting. (Settings > Slider)
- PageSlider: Changed the playlist registration mark display design.
- Script: Changed to create folders and samples when first opening the script folder.

#### Removed

- Command: Removed "Toggle title bar" command.
- Panels: Supplemental text opacity setting is abolished. Can be set with a custom theme.
- Bookshelf: Removed "Save playlist" from the details menu.
- Filmstrip: Abolished the "Display background" setting. Linked to the opacity of the page slider.
- Script: Some members have been deleted. See "Obsolete members" in Script Help for more information.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.
