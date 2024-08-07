; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "KNX Boost Desktop"
#define MyAppVersion "2.2"
#define MyAppPublisher "KNXBoostTeam"
#define MyAppExeName "KNXBoostDesktop.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{607B03B5-117F-4067-9C70-6A87509C60EA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
; "ArchitecturesAllowed=x64compatible" specifies that Setup cannot run
; on anything but x64 and Windows 11 on Arm.
ArchitecturesAllowed=x64compatible
; "ArchitecturesInstallIn64BitMode=x64compatible" requests that the
; install be done in "64-bit mode" on x64 or Windows 11 on Arm,
; meaning it should use the native 64-bit Program Files directory and
; the 64-bit view of the registry.
ArchitecturesInstallIn64BitMode=x64compatible
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename=KNXBoostDesktop_installer_v2.2
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "armenian"; MessagesFile: "compiler:Languages\Armenian.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "corsican"; MessagesFile: "compiler:Languages\Corsican.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "icelandic"; MessagesFile: "compiler:Languages\Icelandic.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\de\*"; DestDir: "{app}\de\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\runtimes\*"; DestDir: "{app}\runtimes\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\DeepL.net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\KNXBoostDesktop.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\KNXBoostDesktop.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\KNXBoostDesktop.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\KNXBoostDesktop.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.DependencyInjection.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.DependencyInjection.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.Http.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.Http.Polly.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\FontAwesome.WPF.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.Options.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Extensions.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Polly.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\Polly.Extensions.Http.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\maxim\OneDrive - insa-toulouse.fr\INSA\Cours\Externes\.NET UCRM\UCRM\Installateur KNXBoostDesktop\System.Management.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

