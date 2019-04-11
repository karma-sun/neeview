## Environment

  * Windows 7 SP1, Windows 8.1, Windows 10
  * .NET Framework 4.7.2 or later is required. If you do not start it please get it from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet-framework-runtime) and install it.


## NeeView and NeeViewS

  There are two types of executable files: NeeView and NeeViewS.  
  NeeView switches 32bit / 64bit operation depending on OS, but NeeViewS is limited to 32bit operation. 64 bit operation can use more memory.  
  The types of Susie plug-ins that can be used are different for 32-bit and 64-bit operation.

  |        |64bitOS   |   32bitOS|
  |--------|----------|----------|
  |NeeView |64bit/.sph|32bit/.spi|
  |NeeViewS|32bit/.spi|32bit/.spi|

* .spi ... [A plug-in for Takechin's image viewer Susie](http://www.digitalpad.co.jp/~takechin/)
* .sph ... [TORO's 64bit Susie plugin](http://toro.d.dooo.jp/slplugin.html)


## How to install / uninstall

### Zip version

  * NeeView<VERSION/>.zip

  Both NeeView and NeeViewS executable files are included.
  The configuration file is common.

  Installation is unnecessary. After deploying Zip, please execute `NeeView.exe` or `NeeViewS.exe` as it is.
  User data such as setting files etc. are also saved in the same place.  

  Uninstalling just deletes the file. Registry is not used.

### Installer version

  * NeeView<VERSION/>.msi

  Both NeeView and NeeViewS executable files are included.
  The configuration file is common.

  Installation starts when you run it. Follow the instructions of the installer.  
  Configuration files etc. User data is saved in the application data folder of each user.  
  You can check this folder by "Other" > "Open setting folder" in NeeView menu.  
  
  For uninstallation, use "Apps and Features" of OS.  
  However, user data such as setting data does not disappear by just uninstalling.
  Please manually erase or execute "Delete data" (installed version only function) of NeeView setting before uninstallation.
