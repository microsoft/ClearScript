@echo off
setlocal

set v8testedrev=8.7.220.25

::-----------------------------------------------------------------------------
:: process arguments
::-----------------------------------------------------------------------------

:ProcessArgs

set download=true
set mode=Release
set isdebug=false
set isofficial=true

:ProcessArg
if "%1"=="" goto ProcessArgsDone
if "%1"=="/?" goto EchoUsage
if /i "%1"=="/n" goto SetDownloadFalse
if /i "%1"=="\n" goto SetDownloadFalse
if /i "%1"=="-n" goto SetDownloadFalse
if /i "%1"=="debug" goto SetDebugMode
if /i "%1"=="release" goto SetReleaseMode
goto SetV8Rev

:EchoUsage
echo Downloads, builds, and imports V8 for use with ClearScript.
echo.
echo V8UPDATE [/N] [mode] [revision]
echo.
echo   /N        Do not download; use previously downloaded files if possible.
echo   mode      Build mode: "Debug" or "Release" (default).
echo   revision  V8 revision: "Latest", "Tested" (default) or branch/commit/tag.
echo             * Examples: "candidate", "3.29.88.16".
echo             * View history at https://chromium.googlesource.com/v8/v8.git.
goto Exit

:SetDownloadFalse
set download=false
goto NextArg

:SetDebugMode
set mode=Debug
set isdebug=true
set isofficial=false
goto NextArg
:SetReleaseMode
set mode=Release
set isdebug=false
set isofficial=true
goto NextArg

:SetV8Rev
set v8rev=%1
goto NextArg

:NextArg
shift
goto ProcessArg

:ProcessArgsDone

::-----------------------------------------------------------------------------
:: check environment
::-----------------------------------------------------------------------------

:CheckOS
if /i "%PROCESSOR_ARCHITECTURE%"=="AMD64" goto CheckOSDone
if defined PROCESSOR_ARCHITEW6432 goto CheckOSDone
echo Error: This script requires a 64-bit operating system.
goto Exit
:CheckOSDone

:CheckMSVS
if "%VisualStudioVersion%"=="15.0" goto CheckMSVSDone
if "%VisualStudioVersion%"=="16.0" goto CheckMSVSDone
echo Error: This script requires a Visual Studio 2017 or Visual Studio 2019
echo Developer Command Prompt. Browse to http://www.visualstudio.com for more
echo information.
goto Exit
:CheckMSVSDone

::-----------------------------------------------------------------------------
:: main
::-----------------------------------------------------------------------------

:Main

echo Build mode: %mode%
cd V8
if errorlevel 1 goto Exit

set DEPOT_TOOLS_WIN_TOOLCHAIN=0

if /i "%download%"=="true" goto Download

if exist build\ goto SkipDownload
echo *** BUILD DIRECTORY NOT FOUND; DOWNLOAD REQUIRED ***
choice /m Continue
if errorlevel 2 goto Exit
goto Download

:SkipDownload
cd build
path %cd%\DepotTools;%path%
goto Build

::-----------------------------------------------------------------------------
:: download
::-----------------------------------------------------------------------------

:Download

:ResolveRev
if "%v8rev%"=="" goto UseTestedRev
if /i "%v8rev%"=="latest" goto UseLatestRev
if /i "%v8rev%"=="tested" goto UseTestedRev
if /i "%v8rev%"=="%v8testedrev%" goto UseTestedRev
echo V8 revision: %v8rev%
echo *** WARNING: THIS V8 REVISION MAY NOT BE COMPATIBLE WITH CLEARSCRIPT ***
choice /m Continue
if errorlevel 2 goto Exit
goto ResolveRevDone
:UseTestedRev
set v8rev=%v8testedrev%
echo V8 revision: Tested (%v8testedrev%)
goto ResolveRevDone
:UseLatestRev
set v8rev=master
echo V8 revision: Latest
echo *** WARNING: THIS V8 REVISION MAY NOT BE COMPATIBLE WITH CLEARSCRIPT ***
choice /m Continue
if errorlevel 2 goto Exit
:ResolveRevDone

:EnsureBuildDir
if not exist build\ goto CreateBuildDir
echo Removing old build directory ...
rd /s /q build
:CreateBuildDir
echo Creating build directory ...
md build
if errorlevel 1 goto Error
:EnsureBuildDirDone

cd build

:DownloadDepotTools
echo Downloading Depot Tools ...
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://storage.googleapis.com/chrome-infra/depot_tools.zip', 'DepotTools.zip')"
if errorlevel 1 goto Error
:DownloadDepotToolsDone

:ExpandDepotTools
echo Expanding Depot Tools ...
powershell -Command "Add-Type -AssemblyName System.IO.Compression.FileSystem; [IO.Compression.ZipFile]::ExtractToDirectory('DepotTools.zip', 'DepotTools')"
if errorlevel 1 goto Error
:ExpandDepotToolsDone

path %cd%\DepotTools;%path%

:SyncClient
echo Downloading V8 and dependencies ...
call gclient config https://chromium.googlesource.com/v8/v8 >config.log
if errorlevel 1 goto Error
call gclient sync -r %v8rev% >sync.log
if errorlevel 1 goto Error
:SyncClientDone

:ApplyPatches
echo Applying patches ...
cd v8
call git config user.name ClearScript
if errorlevel 1 goto Error
call git config user.email "ClearScript@microsoft.com"
if errorlevel 1 goto Error
call git apply --reject --ignore-whitespace ..\..\V8Patch.txt 2>applyV8Patch.log
if errorlevel 1 goto Error
cd ..
:ApplyPatchesDone

:DownloadDone

::-----------------------------------------------------------------------------
:: build
::-----------------------------------------------------------------------------

:Build

:CreatePatches
echo Creating/updating patches ...
cd v8
call git diff --ignore-space-change --ignore-space-at-eol >V8Patch.txt 2>createV8Patch.log
if errorlevel 1 goto Error
cd ..
:CreatePatchesDone

:Build32Bit
cd v8
setlocal
call "%VSINSTALLDIR%\VC\Auxiliary\Build\vcvarsall" x86 >nul
if errorlevel 1 goto Build32BitError
echo Building 32-bit V8 ...
call gn gen out\Win32\%mode% --args="enable_precompiled_headers=false fatal_linker_warnings=false is_component_build=false is_debug=%isdebug% is_official_build=%isofficial% target_cpu=\"x86\" use_custom_libcxx=false v8_embedder_string=\"-ClearScript\" v8_enable_i18n_support=false v8_enable_pointer_compression=false v8_enable_31bit_smis_on_64bit_arch=false v8_monolithic=true v8_target_cpu=\"x86\" v8_use_external_startup_data=false chrome_pgo_phase=0" >gn-Win32-%mode%.log
if errorlevel 1 goto Build32BitError
ninja -C out\Win32\%mode% obj\v8_monolith.lib >build-Win32-%mode%.log
if errorlevel 1 goto Build32BitError
endlocal
cd ..
goto Build32BitDone
:Build32BitError
endlocal
goto Error
:Build32BitDone

:Build64Bit
cd v8
setlocal
call "%VSINSTALLDIR%\VC\Auxiliary\Build\vcvarsall" x64 >nul
if errorlevel 1 goto Build64BitError
echo Building 64-bit V8 ...
call gn gen out\x64\%mode% --args="enable_precompiled_headers=false fatal_linker_warnings=false is_component_build=false is_debug=%isdebug% is_official_build=%isofficial% target_cpu=\"x64\" use_custom_libcxx=false v8_embedder_string=\"-ClearScript\" v8_enable_i18n_support=false v8_enable_pointer_compression=false v8_enable_31bit_smis_on_64bit_arch=false v8_monolithic=true v8_target_cpu=\"x64\" v8_use_external_startup_data=false chrome_pgo_phase=0" >gn-x64-%mode%.log
if errorlevel 1 goto Build64BitError
ninja -C out\x64\%mode% obj\v8_monolith.lib >build-x64-%mode%.log
if errorlevel 1 goto Build64BitError
endlocal
cd ..
goto Build64BitDone
:Build64BitError
endlocal
goto Error
:Build64BitDone

:BuildDone

::-----------------------------------------------------------------------------
:: import
::-----------------------------------------------------------------------------

:Import

cd ..

:ImportPatches
echo Importing patches ...
copy build\v8\V8Patch.txt .\ >nul
if errorlevel 1 goto Error
:ImportPatchesDone

:ImportDone

::-----------------------------------------------------------------------------
:: exit
::-----------------------------------------------------------------------------

echo Succeeded!
goto Exit

:Error
echo *** THE PREVIOUS STEP FAILED ***

:Exit
endlocal
