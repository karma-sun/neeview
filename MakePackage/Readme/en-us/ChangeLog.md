## ChangeLog

### 37.0 
(2020-05-29) 

#### Important

- Separated the packages into x86 and x64 versions
    - Usually use the x64 version. Use the x86 version only if your OS is 32-bit.
    - We strongly recommend that you install the installer version after uninstalling the previous version.
        - The x86 version and the x64 version are treated as separate apps, and although it makes no sense, they can be installed at the same time. The x86 version overwrites the previous version.

- .NET framework 4.8
    - Changed the supported framework to .NET framework 4.8 . [If it doesn't start, please install ".NET Framework 4.8 Runtime" from here.](https://dotnet.microsoft.com/download/dotnet-framework/net48)

- Change configuration file format
    - Changed the structure of settings and changed the format to JSON. The existing XML format setting file can also be read, and automatically converted to JSON format.
    - Backward compatibility of configuration files will be maintained for about a year. In other words, the version around the summer of 2021 will not be able to read the old XML format. The same applies to the exported setting data.

#### New

- Faster booting: Booting will be faster than previous versions, including the ZIP version.
- Navigator: Newly added navigator panel for image manipulation such as rotation and scale change.
- Navigator: Added "Base Scale" setting. The stretch applied size is further corrected.
- Navigator: Moved settings such as "Keep rotation even if you change the page" to Navigator panel.
- Navigator: When the automatic rotation setting and the keep angle are turned on, the rotation direction is forced when the book is opened.
- Script: You can now extend commands with JavaScript. See the script manual for details. (Help> Script Help)
- Script: It is disabled by default and must be enabled in settings to use it. (Options> Script)
- Command: Added "Save settings".
- Command: Added "Go back view page" and "Go next view page". Follows the internal history of each page.
- Command: Add the keyword "$NeeView" to start NeeView itself in the program path of "External app" command parameter.
- Command: Add "Random page".
- Command: Add "Random book".
- Command: "Switch prev NeeView" and "Switch next NeeView" added. Switch NeeView at multiple startup.
- Command: "Save as" The folder registered in the image save dialog can be selected.
- Command: Added the command "N-type scroll ↑" and "N-type scroll ↓" for display operation only for N-type scroll.
- Command: Add scroll time setting to command parameter of scroll type.
- System: Added setting to apply natural order to sort by name. (Options> General> Natural sort)
- System: Added a setting to disable the mouse operation when the window is activated by clicking the mouse. (Options> Window> Disable mouse data when..)
- Panel item: Added setting to open book by double click. (Options> Panels> Double click to open book)
- Panel item: Enabled to select multiple items.
- Panel item: Added popup setting for thumbnail display or banner display of list. (Options> Panel list item> *> Icon popup)
- Panel item: Added the wheel scroll speed magnification setting in the thumbnail display of the list. (Options> Panel list item> > Mouse wheel scroll speed rate in thumbnail view)
- Thumbnail: Added image resolution setting. (Options> Thumbnail> Thumbnail image resolution)
- Bookshelf: Added an orange mark indicating the currently open book.
- Bookshelf: Added setting to display the current number of items. (Options> Bookshelf> Show number of items)
- Bookshelf: Delete shortcut to move to upper folder with Back key.
- Bookshelf: Added setting to sort without distinguishing item types. (Options> Bookshelf> Sort without file type)
- Bookshelf: Default order setting "Standard default order", "Default order of playlists", "Default order of bookmarks" added. (Options> Bookshelf)
- Bookshelf, PageList: "Open in external app" is added to the context menu.
- Bookshelf, PageList: "Copy to folder" is added to the context menu.
- Bookshelf, PageList: Added "Move to folder" to context menu. Enabled to move files. Effective only when file operation is permitted.
- Bookshelf, PageList: Add move attribute to right button drag. You can move files by dropping them in Explorer. Effective only when file operation is permitted.
- PageList: Add move button.
- PageList, PagemarkList: Image files can be dragged to the outside.
- History: Added a setting to display only the history of the current book location. (HistoryPanel menu> Current folder only)
- History: Added a setting to automatically delete invalid history at startup. (Options> History> Delete invalid history automatically)
- Effects: Trimming settings added to the effects panel.
- Effects: Added application magnification setting of "Scake threshold for Keep dot". (Options> Effect panel)
- Loupe: Added setting to center the start cursor on the screen. (Options> Loupe> At the start, move the cursor position to the screen center)
- Book: Pre-reading at the end of the book is also performed in the reverse direction of page feed.
- Book: Added "Select in dialog" to end page behavior. (Options> Move> Behavior when trying to move past the end of the page)
- Book: Added setting to display dummy page at the end of page when displaying 2 pages. (Options> Book> Insert a dmmy page)
- Book: Added a notification setting when the book cannot be opened. (Options> Notification> Show message when there are no pages in the book)
- Book: Added setting to reset page when shuffled. (Options> Book> Reset page when shuffle)
- Image drag operation: "Select area to enlarge" is added. (Options> Mouse operation> Drag action settings)
- Image drag operation: A mouse drag operation "Scaling (horizontal slide, centered)" that moves to the center of the screen at the same time as enlargement is added. (Options> Mouse operation> Drag action settings)
- Startup option: Added option "\-\-script" to execute script at startup.
- Startup option: Added option "\-\-window" to specify window status.
- Options: Add search box.
- Options: Search box added to the list of command settings.
- Options: added the SVG extension. (Options> File types> SVG file extensions)
- Options: "All enable / disable" button added to Susie plugin settings.

#### Change

- Command: Change shortcut "Back", "Shift+Back" to page history operation command.
- Command: Improve the behavior of N-type scroll of "Scroll + Prev" and "Scroll + Next" command. Equalized transfer rate.
- Command: "Scroll + Prev" and "Scroll + Next" command parameter "Page move margin (sec)" is added. In addition, the "scroll stop" flag is abolished.
- System: Change delete confirmation dialog behavior of Explorer. Show only the dialog when you don't put it in the trash.
- System: Change the upper limit of internal history of bookshelves etc. to 100.
- Display: The display image position is not adjusted when the window size changes.
- Display: Don't hide the image when the caption is permanently shown in the main view.
- Panels: The left and right key operations in the panel list are disabled by default. (Options> Panels> The left and right keys input of the side panel is valid)
- Bookshelf: Change layout of search box. Search settings moved to the menu on the bookshelf panel.
- Thumbnail: Change the cache format. The previous cache is discarded.
- Thumbnail: Change cache save location from folder path to file path.
- Thumbnail: The expiration date of the cache can be set. (Options> Thumbnail> Thumbnail cache retention period)
- Thumbnail: When settings such as thumbnail image resolution are changed, cache with different settings is automatically regenerated.
- Book: added a check pattern to the background of transparent images. (Options> File types> Check background of transparent image)
- Book: The extension of standard image files can be edited. (Options> File types> Image file extensions)
- Book page: Change the operation feeling. Gestures are enabled on the book page image, and the book can be opened by double touch.
- Book page: Layout change. Removed the folder icon display.
- Book page: Added image size setting. (Options> Book> Book page image size)
- Susie: Enabled to access "C:\\Windows\\System32" by Susie plug-in in 64bitOS environment.
- Startup option: Removed the full screen option "-f".
- Options: Merged page display settings with book settings, moved dip-related settings to image format settings.
- Options: Reorganization of settings page. "Visual" items have been reorganized into groups such as "Window" and "Panels".
- Options: Abolished the external application setting, changed to specify with the command parameter of the "External app" command. Deleted protocol setting and delimiter setting.
- Options: Removed clipboard setting and changed to specify with command parameter of "Copy file" command. Removed delimiter setting.
- Others: "Settings" is changed to "Options".
- Others: The Esc key in the search box, address bar, etc. is accepted as a command shortcut.
- Others: Various library updates.
- Others: Minor layout correction.

#### fix

- Fixed a bug that may crash when thumbnail image creation fails.
- Fixed a bug that crashes when searching playlists. The playlist does not support search, so it was disabled.
- Fixed a bug that the book itself cannot be opened if there is a compressed file that cannot be opened when opening the book including subfolders. Only the file is skipped.
- Fixed a bug that the rename of compressed files may fail.
- Fixed a bug that the image position is changed by returning from window minimization.
- Other minor bug fixes.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.
