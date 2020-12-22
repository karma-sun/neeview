## ChangeLog

### 38.0
(2021-01-??)

#### New

- Docking side panel support. You can drag the panels to connect them.
- Floating side panel support. Right-click the panel icon or panel title and execute "Floating" to make the panel a subwindow.
- Main view window implementation. Makes the main view a separate window. (View > MainView window)
- Added "Window size adjustment" command to match the window size with the display content size.
- Added auto-hide mode setting. You can enable automatic hiding even in window states other than full screen. (Options > Panels > Auto-hide mode)
- Added AeroSnap coordinate restore settings. (Options > Launch > Restore AeroSnap window placement)
- Added slider movement setting according to the number of displayed pages. (Options > Page slider > Synchronize the...)
- Added ON / OFF setting for WIC information acquisition. (Options > File types > Use WIC information)

#### Fixed

- Fixed an issue that caused an error when trying to open the print dialog on some printers.
- Fixed a bug that may not start depending on the state of WIC.
- Fixed a bug that you cannot start if you delete all excluded folder settings.
- Fixed a bug that thumbnails are not updated when changing the history style.
- Fixed a bug that the display may not match the film strip.
- Fixed a bug that the shortcut of the main menu may not be displayed.
- Fixed a bug that folders in the archive could not be opened with the network path.
- Fixed a bug that bookmark update may not be completed when importing settings.
- Fixed a bug related to page spacing setting and stretching application by rotation.
- Fixed a bug related to scale value continuation when page is moved after rotation.
- Suppresses the phenomenon that the page advances when the book page is opened by double-clicking.
- Improved the problem that media without video such as MP3 may not be played.
- Fixed shortcut key name.

#### Changed

- Transparent side panel grip.
- Disable IME except for text boxes.
- Backup file generation is limited to once per startup.
- Moved the data storage folder for the store app version from "NeeLaboratory\NeeView.a" to "NeeLavoratory-NeeView". To solve the problem that the data may not be deleted even if it is uninstalled.
- To solve the problem that the upper folder of the opened file cannot be changed, the current directory is always in the same location as the exe.
- Changed the order of kanji in natural sort to read aloud.
- Changed to generate a default script folder only when scripts are enabled. If a non-default folder is specified, it will not be generated.
- Added a detailed message to the setting loading error dialog and added an application exit button.
- Changed the NeeView switching order to the startup order.
- Added the option to initialize the last page in "Page Position" of the page settings.
- Adjust the order of the "View" menu.
- Changed "File Information" to "Information".
- Various library updates.

#### Removed

- Abolished the setting "Do not cover the taskbar area at full screen". Substitute in auto-hide mode.
- "Place page list on bookshelf" setting abolished. Substitute with a docking panel.

#### Script

- Fixed: Fixed a bug that command parameter changes were not saved.
- Fixed: Fixed a bug that the focus did not move with "nv.Command.ToggleVisible*.Execute(true)".
- Fixed: Fixed a bug that the focus did not move to the bookshelf in the startup script.
- New: The default shortcut can be specified in the doc comments of the script file.
- New: Added nv.ShowInputDialog() instruction. This is a character string input dialog.
- New: Added sleep() instruction. Stops script processing for the specified time.
- New: Added "Cancel script" command. Stops the operation of scripts that use sleep.
- New: Addition of each panel accessor such as nv.Bookshelf. Added accessors for each panel such as bookshelves. You can get and set selection items.
- Changed: Changed to output the contents of the object in the script console output.
- Changed: Changed nv.Book page accessor acquisition from method to property.
    - nv.Book.Page(int) -> nv.Book.Pages\[int\] (The index will start at 0)
    - nv.Book.ViewPage(int) -> nv.Book.ViewPages\[int\]
    - Pages[] cannot get the page size(Width,Height). You can get it in ViewPages[].
- nv.Config
    - New: nv.Config.Image.Standard.UseWicInformation
    - New: nv.Config.MainView.IsFloating
    - New: nv.Config.MainView.IsHideTitleBar
    - New: nv.Config.MainView.IsTopmost
    - New: nv.Config.MenuBar.IsHideMenuInAutoHideMode
    - New: nv.Config.Slider.IsSyncPageMode
    - New: nv.Config.System.IsInputMethodEnabled
    - New: nv.Config.Window.IsAutoHideInFullScreen
    - New: nv.Config.Window.IsAutoHideInNormal
    - New: nv.Config.Window.IsAutoHidInMaximized
    - New: nv.Config.Window.IsRestoreAeroSnapPlacement
    - Changed: nv.Config.Bookmark.IsSelected → nv.Bookmark.IsSelected
    - Changed: nv.Config.Bookmark.IsVisible → nv.Bookmark.IsVisible
    - Changed: nv.Config.Bookshelf.IsSelected → nv.Bookshelf.IsSelected
    - Changed: nv.Config.Bookshelf.IsVisible → nv.Bookshelf.IsVisible
    - Changed: nv.Config.Effect.IsSelected → nv.Effect.IsSelected
    - Changed: nv.Config.Effect.IsVisible → nv.Effect.IsVisible
    - Changed: nv.Config.History.IsSelected → nv.History.IsSelected
    - Changed: nv.Config.History.IsVisible → nv.History.IsVisible
    - Changed: nv.Config.Information.IsSelected → nv.Information.IsSelected
    - Changed: nv.Config.Information.IsVisible → nv.Information.IsVisible
    - Changed: nv.Config.PageList.IsSelected → nv.PageList.IsSelected
    - Changed: nv.Config.PageList.IsVisible → nv.PageList.IsVisible
    - Changed: nv.Config.Pagemark.IsSelected → nv.Pagemark.IsSelected
    - Changed: nv.Config.Pagemark.IsVisible → nv.Pagemark.Visible
    - Changed: nv.Config.Panels.IsHidePanelInFullscreen → nv.Config.Panels.IsHidePanelInAutoHideMode
    - Changed: nv.Config.Slider.IsHidePageSliderInFullscreen → nv.Config.Slider.IsHidePageSliderInAutoHideMode
    - Removed: nv.Config.Bookshelf.IsPageListDocked → x
    - Removed: nv.Config.Bookshelf.IsPageListVisible → x
    - Removed: nv.Config.Window.IsFullScreenWithTaskBar → x
- nv.Command
    - New: ToggleMainViewFloating
    - New: StretchWindow
    - New: CancelScript
    - Changed: FocusPrevAppCommand → FocusPrevApp
    - Changed: FocusNextAppCommand → FocusNextApp
    - Changed: TogglePermitFileCommand → TogglePermitFile
    - Removed: TogglePageListPlacement → x

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.
