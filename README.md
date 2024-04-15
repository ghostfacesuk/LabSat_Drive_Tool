# LabSat4 Drive Tool
![LabSat4 Drive Tool](https://github.com/ghostfacesuk/LabSat_Drive_Tool/blob/main/UI.png)

## Requirements
- .Net 8 
- Admin privileges on account

## Download
[Installer.Zip file](https://github.com/ghostfacesuk/LabSat_Drive_Tool/blob/main/LabSat4%20Drive%20Tool%20Installer/Debug/Installer.zip)

## Usage 
- Download zip file and extract folder to PC (Windows only)
- Run setup.exe
- Connect LabSat4 drive to PC (SATA, USB, etc)
- Launch app (Desktop shortcut)
- Select drive from dropdown menu
- Click format, wait for format complete popup box! 
- Remove drive from PC and connect to LabSat4
- Power up LabSat4 and wait for the disk LED to stop blinking (flashing blue LED on front panel)

## About
- This tool uses mke2fs to allow a user to format drives in EXT4 format. This tool will create a EXT4 volume for the total size of the disk. 


