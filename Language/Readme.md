# How to create a language file and check its operation

If you provide the created language file (.restext), we can incorporate it into this product.

## Environment

- Visual Studio 2019
    
    Required to use ResGen.exe and Al.exe for conversion. Install the toolset ".NET Desktop Development".

- Zip version of NeeView (ver 39.0 or later or Canarry)
    
    It is for operation check.

- NeeView repository
    
    All you need is the `Language` folder.

Folder structure

    + Language\
        + Source\ ... Language file storage.
        + Tools\ ... Contains commands for conversion.
        + Readme.md ... This file.
        + Start-NVDevPowerShell.bat ... The conversion work is done at the command prompt (PowerShell) that executed this.

----

## Creating a language file and checking its operation

### Step.0 Start console

Start `Start-NVDevPowerShell.bat`.
Work on PowerShell launched with this batch file.

### Step.1 Creating a language file

Create a `Resources.[culture].restext` file in the `Source` folder.
We recommend that you copy and create other .restext files.
For `[culture]`, specify the culture code of the language to be created.
This file is in restext format. Edit with a text editor.

    e.g.
    > copy Source\Resources.restext Source\Resources.de-DE.restext
    > notepad Source\Resources.de-DE.restext

### Step.2 Creating a satellite assembly

Create a language satellite assembly `NeeView.resources.dll`.

    e.g.
    > NVRestextToResourceDll.ps1 Source\Resources.de-DE.restext

### Step.3 Placement

Create a culture folder in the NeeView folder of the product for operation check, and place the satellite assembly.

    e.g.
    + NeeView.exe
    + NeeView.exe.config
    + Libraries\
        + de-DE\ <- Create this folder.
            + NeeView.resources.dll <- Place here.

### Step.4 App settings

Edit `NeeView.exe.config` to allow you to select the culture of the language you created.
Add the culture code to `Cultures` in `<appSettings>`.

    e.g.
    <add key="Cultures" value="en,ja,de-DE" />

### Step.5 Operation check

Start NeeView.exe. Select a language from the settings and restart the app.
If the language file is reflected, it is successful.
    
----

## How to edit in Excel

You can combine .restext files into Excel files for viewing and editing in lists.

### Convert from .restext files to excel file

Create `/Language/Resources.xlsx` using NVRestextToXlsx.ps1.

    e.g.
    > NVRestextToXlsx.ps1

### Convert from excel file to .restext files

Create .restext files from an excel file using NVXlsxToRestext.ps1.
Note that all .restext files will be overwritten.

    e.g.
    > NVXlsxToRestext.ps1
