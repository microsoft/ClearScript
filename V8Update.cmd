@echo off
setlocal

::-----------------------------------------------------------------------------
:: process arguments
::-----------------------------------------------------------------------------

set v8testedrev=5.3.332.45

set gyprev=35eafcd939515d51d19556c543f9cf97faf75ee6
set cygwinrev=c89e446b273697fadf3a10ff1007a97c0b7de6df
set clangrev=2ad431ac7823581e1f39c5b770704e1e1ca6cb32
set traceeventcommonrev=54b8455be9505c2cb0cf5c26bb86739c236471aa
set gtestrev=6f8a66431cb592dad629028a50b3dd418a408c87
set gmockrev=0421b6f358139f02e102c9c332ce19a33faf75be

:ProcessArgs

set download=true
set mode=Release

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
goto NextArg
:SetReleaseMode
set mode=Release
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

:CheckMSVS
if "%VisualStudioVersion%"=="12.0" goto UseMSVS2013
if "%VisualStudioVersion%"=="14.0" goto UseMSVS2015
echo Error: This script requires a Visual Studio 2013 or 2015 Developer Command
echo Prompt. Browse to http://www.visualstudio.com for more information.
goto Exit
:UseMSVS2013
set GYP_MSVS_VERSION=2013
goto CheckMSVSDone
:UseMSVS2015
set GYP_MSVS_VERSION=2015
:CheckMSVSDone

::-----------------------------------------------------------------------------
:: main
::-----------------------------------------------------------------------------

:Main

echo Build mode: %mode%
cd ClearScript\v8\v8
if errorlevel 1 goto Exit

if /i "%download%"=="true" goto Download

if exist build\ goto SkipDownload
echo *** BUILD DIRECTORY NOT FOUND; DOWNLOAD REQUIRED ***
choice /m Continue
if errorlevel 2 goto Exit
goto Download

:SkipDownload
cd build
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

:DownloadV8
echo Downloading V8 ...
git clone -n -q https://chromium.googlesource.com/v8/v8.git
if errorlevel 1 goto Error
cd v8
git checkout -q "%v8rev%"
if errorlevel 1 goto Error
cd ..
:DownloadV8Done

cd v8

:PatchV8
echo Patching V8 ...
git apply --ignore-whitespace ..\..\V8Patch.txt 2>applyPatch.log
if errorlevel 1 goto Error
:PatchV8Done

:DownloadGYP
echo Downloading GYP ...
git clone -n -q https://chromium.googlesource.com/external/gyp.git tools/gyp
if errorlevel 1 goto Error
cd tools\gyp
git checkout -q "%gyprev%"
if errorlevel 1 goto Error
cd ..\..
:DownloadGYPDone

:DownloadCygwin
echo Downloading Cygwin ...
git clone -n -q https://chromium.googlesource.com/chromium/deps/cygwin.git third_party/cygwin
if errorlevel 1 goto Error
cd third_party/cygwin
git checkout -q "%cygwinrev%"
if errorlevel 1 goto Error
cd ..\..
:DownloadCygwinDone

:DownloadClang
echo Downloading Clang ...
git clone -n -q https://chromium.googlesource.com/chromium/src/tools/clang.git tools/clang
if errorlevel 1 goto Error
cd tools\clang
git checkout -q "%clangrev%"
if errorlevel 1 goto Error
cd ..\..
:DownloadClangDone

:DownloadTraceEventCommon
echo Downloading TraceEventCommon ...
git clone -n -q https://chromium.googlesource.com/chromium/src/base/trace_event/common.git base/trace_event/common
if errorlevel 1 goto Error
cd base\trace_event\common
git checkout -q "%traceeventcommonrev%"
if errorlevel 1 goto Error
cd ..\..\..
:DownloadTraceEventCommonDone

:DownloadGTest
echo Downloading GTest ...
git clone -n -q https://chromium.googlesource.com/external/github.com/google/googletest.git testing/gtest
if errorlevel 1 goto Error
cd testing\gtest
git checkout -q "%gtestrev%"
if errorlevel 1 goto Error
cd ..\..
:DownloadGTestDone

:DownloadGMock
echo Downloading GMock ...
git clone -n -q https://chromium.googlesource.com/external/googlemock.git testing/gmock
if errorlevel 1 goto Error
cd testing\gmock
git checkout -q "%gmockrev%"
if errorlevel 1 goto Error
cd ..\..
:DownloadGMockDone

cd ..

:DownloadDone

::-----------------------------------------------------------------------------
:: build
::-----------------------------------------------------------------------------

:Build

set DEPOT_TOOLS_WIN_TOOLCHAIN=0
set GYP_GENERATORS=msvs

:CreatePatchFile
echo Creating patch file ...
cd v8
git diff --ignore-space-change --ignore-space-at-eol >V8Patch.txt 2>createPatch.log
if errorlevel 1 goto Error
cd ..
:CreatePatchFileDone

:Copy32Bit
echo Building 32-bit V8 ...
if exist v8-ia32\ goto Copy32BitDone
md v8-ia32
if errorlevel 1 goto Error
xcopy v8\*.* v8-ia32\ /e /y >nul
if errorlevel 1 goto Error
:Copy32BitDone

:Build32Bit
cd v8-ia32
python gypfiles\gyp_v8 -Dtarget_arch=ia32 -Dcomponent=shared_library -Dv8_use_snapshot=false -Dv8_enable_i18n_support=0 >gyp.log
if errorlevel 1 goto Error
msbuild /p:Configuration=%mode% /p:Platform=Win32 /t:v8 src\v8.sln >build.log
if errorlevel 1 goto Error
cd ..
:Build32BitDone

:Copy64Bit
echo Building 64-bit V8 ...
if exist v8-x64\ goto Copy64BitDone
md v8-x64
if errorlevel 1 goto Error
xcopy v8\*.* v8-x64\ /e /y >nul
if errorlevel 1 goto Error
:Copy64BitDone

:Build64Bit
cd v8-x64
python gypfiles\gyp_v8 -Dtarget_arch=x64 -Dcomponent=shared_library -Dv8_use_snapshot=false -Dv8_enable_i18n_support=0 >gyp.log
if errorlevel 1 goto Error
msbuild /p:Configuration=%mode% /p:Platform=x64 /t:v8 src\v8.sln >build.log
if errorlevel 1 goto Error
cd ..
:Build64BitDone

:BuildDone

::-----------------------------------------------------------------------------
:: import
::-----------------------------------------------------------------------------

:Import

cd ..

:EnsureLibDir
if not exist lib\ goto CreateLibDir
echo Removing old lib directory ...
rd /s /q lib
:CreateLibDir
echo Creating lib directory ...
md lib
if errorlevel 1 goto Error
:EnsureLibDirDone

:ImportLibs
echo Importing V8 libraries ...
copy build\v8-ia32\build\%mode%\v8-ia32.dll lib\ >nul
if errorlevel 1 goto Error
copy build\v8-ia32\build\%mode%\v8-ia32.pdb lib\ >nul
if errorlevel 1 goto Error
copy build\v8-ia32\build\%mode%\lib\v8-ia32.lib lib\ >nul
if errorlevel 1 goto Error
copy build\v8-x64\build\%mode%\v8-x64.dll lib\ >nul
if errorlevel 1 goto Error
copy build\v8-x64\build\%mode%\v8-x64.pdb lib\ >nul
if errorlevel 1 goto Error
copy build\v8-x64\build\%mode%\lib\v8-x64.lib lib\ >nul
if errorlevel 1 goto Error
:ImportLibsDone

:EnsureIncludeDir
if not exist include\ goto CreateIncludeDir
echo Removing old include directory ...
rd /s /q include
:CreateIncludeDir
echo Creating include directory ...
md include
if errorlevel 1 goto Error
:EnsureIncludeDirDone

:ImportHeaders
echo Importing V8 header files ...
copy build\v8\include\*.* include\ >nul
if errorlevel 1 goto Error
:ImportHeadersDone

:ImportPatchFile
echo Importing patch file ...
copy build\v8\V8Patch.txt .\ >nul
if errorlevel 1 goto Error
:ImportPatchFileDone

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
