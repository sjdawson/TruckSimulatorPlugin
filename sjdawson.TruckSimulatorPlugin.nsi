Unicode True

!define PRODUCT_NAME "sjdawson.TruckSimulatorPlugin"
!define PRODUCT_VERSION "v2.0.3"
!define PRODUCT_PUBLISHER "sjdawson"
!define PRODUCT_WEB_SITE "https://github.com/sjdawson/trucksimulatorplugin"

Function .onInit
    ReadRegStr $INSTDIR HKCU "SOFTWARE\SimHub" InstallDirectory
FunctionEnd

SetCompressor lzma

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "bin/Release/assets/truck.ico"
!define MUI_WELCOMEPAGE_TITLE "${PRODUCT_NAME} Setup"
!define MUI_WELCOMEPAGE_TEXT "Welcome to the setup of ${PRODUCT_NAME} ${PRODUCT_VERSION} for SimHub."
!define MUI_FINISHPAGE_TITLE "Setup complete for ${PRODUCT_NAME}"
!define MUI_FINISHPAGE_TEXT "Setup has finished installing ${PRODUCT_NAME}, you'll need to restart SimHub if it's currently running in order to have it pick up that the plugin has been installed."
!define MUI_DIRECTORYPAGE_TEXT_DESTINATION "Choose your SimHub folder (where SimHubWPF.exe is located)."

!define MUI_FINISHPAGE_SHOWREADME "https://sjdawson.gitbook.io/trucksimulatorplugin"
!define MUI_FINISHPAGE_SHOWREADME_TEXT "Open documentation and changelog?"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!insertmacro MUI_PAGE_FINISH

; Language files
!insertmacro MUI_LANGUAGE "English"

; Reserve files
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "bin/Release/sjdawson.TruckSimulatorPluginInstall.exe"
ShowInstDetails show

Section "MainSection" SEC01
  SetOutPath $INSTDIR
  SetOverwrite ifnewer
  File "..\..\sjdawson.TruckSimulatorPlugin.dll"
  SetOutPath "$INSTDIR\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\zh_tw.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\zh_cn.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\vi_vn.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\uk_uk.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\tr_tr.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\sv_se.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\sr_sr.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\sr_sp.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\sl_sl.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\sk_sk.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\ru_ru.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\ro_ro.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\pt_pt.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\pt_br.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\pl_si.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\pl_pl.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\no_no.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\nl_nl.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\mk_mk.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\lv_lv.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\lt_lt.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\ko_kr.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\ka_ge.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\ja_jp.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\it_it.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\hu_hu.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\hr_hr.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\gl_es.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\fr_fr.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\fi_fi.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\eu_es.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\et_ee.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\es_la.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\es_es.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\en_us.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\en_gb.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\el_gr.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\de_de.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\da_dk.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\cs_cz.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\ca_es.json"
  File "..\..\PluginsData\ETS2\sjdawson.TruckSimulatorPlugin.Translations\bg_bg.json"
SectionEnd

Section -Post
SectionEnd
