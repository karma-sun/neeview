# NeeView <VERSION/> - ChangeLog

----

## 34.4
(2019-06-08)

- Fixed a bug that does not restore in maximized state in sub display.
- Fixed a bug that the load does not end if you switch the book in the loupe mode when the resize filter is enabled.
- Fixed a bug that caused a crash in the image scaling operation when the image is not open.
- Fixed a bug that could cause Susie plug-in image reading to fail when solid compressed file pre-memory expansion is enabled.
- Fixed a bug that sometimes fails to delete a book file.
- Fixed a bug that the application can not be switched with Windows + number keys at full screen.
- Measures for the phenomenon that the task bar shortcut is invalidated by updating with the MSI installer. It works from the next update.

----

## 34.3
(2019-05-18)

- Fixed a bug that crashed when the last open playlist is deleted.

----

## 34.2
(2019-05-11)

- Fixed a bug that the mark does not disappear while loading the page.
- Fixed a bug that the initial position when moving to the previous page is in the upper corner with N-shaped scrolling.
- Fixed a bug that the window size can not be changed when the title bar is hidden.
- Fixed a bug that page move priority OFF does not work when resize filter is ON.
- Fixed a bug that caused an error when hiding the title bar in the sub display.

----

## 34.1
(2019-05-07)

- Fixed a bug that caused an error when moving a book by page feed.

----

## 34.0
(2019-05-06)


### Memory & Performance 

Added "Memory & Performance" page to settings. Memory related settings are summarized here. (Setting > Memory & Performance)

- Added cache memory size setting for books. Cache images and speed up redisplay. 
- Change the prefetching to the specified number of pages, and perform prefetching within the range of cache memory capacity. 
- Added setting to use pre-decompression destination of solid compressed file as memory. 
- Calculated the judgment size of solid compressed file pre-expansion by the size after expansion. 

### Improving the behavior of the book

- Reduce darkening when switching GIF animation page
- Reduction of slow response when switching between videos and GIF files at high speed
- We reduce phenomenon that image change is visible at the time of resizing filter application

### Add page type

- Books (folders and compressed files) contained in books can now be displayed as book pages. (Setting > Environment > File type to be page)
    - The book page displays the book thumbnails. You can open the book by double-clicking it.
    - A folder icon is displayed on the book page of the page list. You can open the book by double-clicking it.
- Supported SVG file (Setting > Image)

### Page setting renewal

- New page settings. The behavior for each setting item is summarized on one setting page. (Setting > Page settings)

### Add page move command

- Added "Prev folder" and "Next folder" command. Move to the first page of different subfolders only if the sort order is by name order.

### Add book move command

- “Go back (Alt+Left)” and “Go next (Alt+Right)” follow a simple book history. Because of this, you may open the same book. It is different information from "History list".
    - Corresponds to the left and right arrow buttons on the address bar. Up until now it was a move of the history list, so it's a change of behavior.
    - The history list move command has been renamed to "Prev History" and "Next History".
- "Go to parent (Alt+Up)" opens the current book's upper hierarchy as a book.
    - Corresponds to the up arrow button on the address bar.
- "Go to child (Alt+Down)" opens the book if the page is a book page.

### Bookshelf

- Folder thumbnails are searched including images in subfolders.
- Supports expansion of compressed file directory hierarchy unit. (Setting > Environment > Compressed file handling)
- Add settings to show hidden files in Bookshelf. (Setting > Bookshelf > Advanced Setting)
- Enabled incremental search to work while IME input in search box.
- Add search option:
    - `/ire` Ignore-case regular expression search.
    - `/since`, `/until` Date and time range specification. Please refer to the search option help for details. (Help > Search options help)

### Playlist

A playlist is a list of file paths. You can list all files, such as folders, compressed files, and image files. 
It will work as a kind of folder if you open it in a bookshelf, and it will be a page if you open it as a book.

- When multiple files are dropped collectively, it is displayed as a temporary playlist.
- If multiple files are specified as startup arguments, they will also be displayed as temporary playlists.
- You can save the bookshelf status as a playlist file (.nvpls). (Bookshelf detail menu> Save playlist...)
- `.nvpls` The file format is simple JSON format, so you can edit it with Notepad.

### View operation

- Added "Cursor Position" to the center setting of rotation, scaling, and flipping, and can be set individually. (Setting > View operation general > View operation)
- Add "Rotation (horizontal slide)" to drag action. (Settings > Mouse operation > Drag action settings)
- The drag movement restriction of the image was canceled by rotation and scale change.
- Separated the automatic rotation command into two commands, "Toggle auto left rotation" and "Toggle auto right rotation"

### Window appearance

- Dark color support for menus. Set separately from the entire theme. (Setting > Visual General > Theme)
- Window definition maintenance when the title bar is hidden. Change the type of frame to "None" and "Standard" only. (Settings > Visual Genera > Advanced Setting)
- Title text is displayed in the menu bar margin when the title bar is hidden.
- You can change the main menu to a hamburger menu. (Settings > Visual Genera > Advanced Setting)
- Black checkered pattern added to background.

### Susie plug-in

You can use Susie plug-ins with the normal version of NeeView. However, 64-bit OS supports only the [64-bit Susie plug-in (.sph) proposed by TORO](http://toro.d.dooo.jp/slplugin.html). When using the standard Susie plug-in (.spi) on 64bit OS, use NeeVewS as before.

- Supports 64-bit Susie plug-in (.sph)
- The plugin file is not accessed when not using the Susie plugin.
- Add extension settings for each Susie plug-in.

### Other

- While loading a book, the Reload button on the address bar changes to a close button.
- When the cursor was moved out of the window when side panel automatic hiding, it was made to hide also.
- Video slider wheel support.
- Divide the menu "Others" into "Options" and "Help".
- The previous setting page is displayed when the setting window is reopened.
- Added the command description on the command setting page.
- Complementary path display format of panel list items can be unified. You can be switched to standard display by setting. (Setting > Panel list item > Content style)
- Add settings to reflect image resolution information. The aspect ratio will be the same as the image information. (Setting > Image)
- Move the setting of "Play animated GIF" and "A file whose extension is unknown is regarded as an image file" to the image format page. (Setting > Image)
- Add pagemark list path order setting to the detail menu. (Pagemark list detail menu > Path order)
- Add `$PageL` and `$PageR` as keywords for title character. (Setting > Window title)
- Update history is included in the package.

### Obsolete seggins

- Abolished the setting of "System manages memory"
- 7-Zip "Pre Extract" setting abolished.
- Automatic subfolder "Determine including out-of-page files" setting abolished
- Eliminated setting to substitute thumbnail image for page display during loading. It will be 2 choices of whether to display the previous page or to display it as a dummy.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.
