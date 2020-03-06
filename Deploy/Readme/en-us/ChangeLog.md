## ChangeLog


### 36.1
(2020-03-07)

#### Changed

- In the installer version, you can select to add a Explorer's context menu during installation.
- Added setting of Explorer confirmation dialog when deleting files, because deletion confirmation dialog may be displayed twice. (Setting > General > Show Explorer confirmation dialog when deleting files)

#### Fixed

- Fixed a bug that "Show panels" command did not work.
- Fixed a bug that menu could not be read in high contrast mode.

----

### 36.0
(2020-02-29)

#### New

- Added gap adjustment setting when maximizing window. (Setting > Visual > Gap adjustment of window maximization with tite bar hidden)
- Added setting to add Explorer's context menu "Open with NeeView".  (Setting > General > Register in the Explorer context menu)
  - In the installer version, it is set automatically at the time of installation. Store app version is not supported.
- Added "Focus on main view" command.
- Added page display log output function. (Setting > Page view recording)
- Added confirmation dialog when deleting files that are too large for the recycle bin.
- Playing movies with compressed files. Temporarily output and play.
- Command parameter setting window can be called directly by right-clicking menu item.
- Added "Save (Shift+Ctrl+S)" command. Execute saving directly with the information set in the command parameters. Duplicate file names are numbered.

#### Changed

- Renewal of the function to hide the panel automatically. (Setting > Visual > Auto-hide panel)
- Focus is moved when the display of the filmstrip is turned on.
- File information panel : "Date" is separated into file date and shooting date. Along with this, "Use EXIF ​​date" setting is abolished.
- Deleting a book using a command is now closer to deleting a book on the bookshelf.
- Books opened from playlists can also be deleted. The playlist data will not be changed, but will not appear in the list as a result because the file does not exist.
- Changed initial window size to system dependent.
- Saved window state before full screen.
- Improved TAB key movement for focus.
- Made case-insensitive when excluding folders.
- Right button drag can be assigned to other drag operations.
- Automatic recursion judgment is performed only for normal folders.
- Moved bookshelf-related settings for "History" to the "Bookshelf" page. Separated the "Restore bookshelf location" setting and added it to the "Launch" page.
- Since the parent of the tree item in the setting window and the first child are the same, only the parent page is displayed.
- New "Save As" command. Output the image as it is displayed by reflecting the enlargement rotation etc. by "Save view". 
- Changed Susie connection timeout from 30 seconds to 1 second and retry 10 times.
- Updated various libraries.

#### Fixed

- Fixed a bug where repeated messages were not displayed when the notification was set to "Tiny".
- Fixed a bug of drag operation "Scaling" when the center of scaling is "Cursor position".
- Fixed a bug that the setting to restore full screen state might not work.
- Fixed a bug that GIF file shortcut does not animate.
- Fixed a bug where the taskbar could not be hidden during full screen.
- Fixed a bug that extra resize processing is executed when turning pages on a display where the display scale is not 100%.
- Fixed a bug that only uppercase and lowercase letters in the folder name cannot be changed.
- Fixed a bug that could cause abnormal termination when setting the distance between two pages.
- Fixed a bug that window may not be maximized when releasing full screen in tablet mode.
- Fixed a bug that command rotation is not possible when the rotation snap value is large.
- Corrected the phenomenon that the next window may stick out when the window is maximized on multi display.
- Fixed a bug that window was not active when trying to start multiple with no arguments with multiple startup disabled.
- Fixed a bug that the menu notation of the shortcut key PageDown is Next.
- Suppress extra window state changes when trying to set full screen again.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.
