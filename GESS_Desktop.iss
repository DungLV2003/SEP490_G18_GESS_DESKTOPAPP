; GESS_Desktop.iss - Script tạo trình cài đặt cho WPF App

[Setup]
AppName=GESS Exam
AppVersion=1.0.0
DefaultDirName={autopf}\GESS Exam
DefaultGroupName=GESS Exam
OutputDir=InstallerOutput

OutputBaseFilename=GESS_Exam_Installer
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
SetupIconFile=Assets\favicon.ico
  

[Files]
Source: "bin\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\GESS Exam"; Filename: "{app}\GESS Exam.exe"
Name: "{commondesktop}\GESS Exam"; Filename: "{app}\GESS Exam.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\GESS Exam.exe"; Description: "Launch GESS Exam"; Flags: nowait postinstall skipifsilent
