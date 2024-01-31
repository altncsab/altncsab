; Main constants - define following constants as you want them displayed in your installation wizard
!define PRODUCT_NAME "SqlScriptRunner"
!define PRODUCT_VERSION "1.0.1-alpha06"
!define PRODUCT_PUBLISHER "altncsab"
!define PRODUCT_WEB_SITE "https://github.com/altncsab/altncsab"

; Following constants you should never change
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!include "MUI.nsh"
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\win-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\win-uninstall.ico"

; Wizard pages
!insertmacro MUI_PAGE_WELCOME
; Note: you should create License.txt in the same folder as this file, or remove following line.
!insertmacro MUI_PAGE_LICENSE "License.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

; Replace the constants bellow to hit suite your project
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Setup${PRODUCT_NAME}_${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES\Altncsab\${PRODUCT_NAME}"
ShowInstDetails show
ShowUnInstDetails show

; Following lists the files you want to include, go through this list carefully!
Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  File "..\SqlScriptRunner\bin\Release\${PRODUCT_NAME}.exe"
  File "..\UsersGuide.html"
; Note: my system has a config template, which should manually be edited. This is a nice trick to save your username/password somewhere,
; but you can entirely skip this by deleting the following line. 
 ; File /oname=SqlScriptRunner.exe.config "App.config.template"

; It is pretty clear what following line does: just rename the file name to your project startup executable.
  CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_NAME}.exe" ""
  CreateDirectory "$SMPROGRAMS\Altncsab\${PRODUCT_NAME}"
  CreateShortCut "$SMPROGRAMS\Altncsab\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_NAME}.exe" 0
SectionEnd 

Section -Post
  ;Following lines will make uninstaller work - do not change anything, unless you really want to.
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  
  ; COOL STUFF: Following line will add a registry setting that will add the INSTDIR into the list of folders from where
  ; the assemblies are listed in the Add Reference in C# or Visual Studio.
  ; This is super-cool if your installation package contains assemblies that someone will use to build more applications - 
  ; and it doesn't hurt even if it is placed there, it will only make the VS a bit slower to find all assemblies when adding references.
  ; WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "SOFTWARE\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\ZWare\ZawsCC" "" "$INSTDIR"
SectionEnd

; Replace the following strings to suite your needs
Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "${PRODUCT_NAME} Application was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove ${PRODUCT_NAME} and all of its components?" IDYES +2
  Abort
FunctionEnd


; Remove any file that you have added above - removing uninstallation and folders last.
; Note: if there is any file changed or added to these folders, they will not be removed. Also, parent folder (which in my example 
; is company name ZWare) will not be removed if there is any other application installed in it.
Section Uninstall
  Delete "$INSTDIR\${PRODUCT_NAME}.exe"
  Delete "$INSTDIR\UsersGuide.html"
  Delete "$INSTDIR\uninst.exe"

  RMDir "$INSTDIR"
  RMDir "$INSTDIR\.."

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  ; Change following to be exactly as above
  ; DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "SOFTWARE\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\ZWare\ZAwsCC" 

  SetAutoClose true
SectionEnd
