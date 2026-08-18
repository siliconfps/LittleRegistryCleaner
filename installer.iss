; Script Inno Setup para Little Registry Cleaner
; Produz o instalador completo (Setup.exe) para Windows

#define MyAppName "Little Registry Cleaner"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Little Apps"
#define MyAppURL "https://github.com/siliconfps/LittleRegistryCleaner"
#define MyAppExeName "Little Registry Cleaner.exe"

[Setup]
; Identificador unico do aplicativo (NAO altere para manter compatibilidade com atualizacoes futuras)
AppId={{8E478C2F-E649-4D88-82B5-FA1B12DF568E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=Little Registry Cleaner\gpl.txt
; Requer privilegios de administrador para acessar e limpar registros do sistema
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=Output
OutputBaseFilename=LittleRegistryCleaner_v{#MyAppVersion}_Setup
SetupIconFile=Little Registry Cleaner\little registry cleaner.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Executaveis principais e configuracoes
Source: "Little Registry Cleaner\bin\Release\Little Registry Cleaner.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Little Registry Cleaner\bin\Release\Little Registry Cleaner.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "Little Registry Cleaner\bin\Release\Little Startup Manager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Little Registry Cleaner\bin\Release\Little Startup Manager.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "Little Registry Cleaner\bin\Release\Little Uninstall Manager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Little Registry Cleaner\bin\Release\Little Uninstall Manager.exe.config"; DestDir: "{app}"; Flags: ignoreversion

; Bibliotecas DLLs
Source: "Little Registry Cleaner\bin\Release\Common Tools.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Little Registry Cleaner\bin\Release\AutoUpdater.NET.dll"; DestDir: "{app}"; Flags: ignoreversion

; Pacotes de idiomas / Traducoes (Satellite Resource Assemblies)
Source: "Little Registry Cleaner\bin\Release\*.resources.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Documentacao e Licenca
Source: "Little Registry Cleaner\gpl.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "CHANGELOG.md"; DestDir: "{app}"; DestName: "ChangeLog.txt"; Flags: ignoreversion
Source: "Little Registry Cleaner\little registry cleaner.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Atalhos no Menu Iniciar
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\Little Startup Manager"; Filename: "{app}\Little Startup Manager.exe"; IconFilename: "{app}\Little Startup Manager.exe"
Name: "{group}\Little Uninstall Manager"; Filename: "{app}\Little Uninstall Manager.exe"; IconFilename: "{app}\Little Uninstall Manager.exe"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Atalho na Area de Trabalho (opcional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Executar o Little Registry Cleaner ao finalizar a instalacao
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
