@echo off
setlocal

::-----------------------------------------------------------------------------
:: process arguments
::-----------------------------------------------------------------------------

set testedRevision=18635
set testedVersion=3.24.17

set gyprev=1831
set pythonrev=89111
set cygwinrev=66844

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
echo   mode      Build mode: "Debug" or "Release".
echo   revision  V8 revision: "Latest", "Tested", or revision number.
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
if /i "%v8rev%"=="%testedRevision%" goto UseTestedRev
echo V8 revision: r%v8rev%
echo *** WARNING: THIS V8 REVISION MAY NOT BE COMPATIBLE WITH CLEARSCRIPT ***
choice /m Continue
if errorlevel 2 goto Exit
goto ResolveRevDone
:UseTestedRev
set v8rev=%testedRevision%
echo V8 revision: Tested (r%v8rev%, Version %testedVersion%)
goto ResolveRevDone
:UseLatestRev
set v8rev=HEAD
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
svn checkout http://v8.googlecode.com/svn/trunk@%v8rev% v8 >getV8.log
if errorlevel 1 goto Error1
:DownloadV8Done

cd v8

:PatchV8
echo Patching V8 ...
svn patch --ignore-whitespace ..\..\V8Patch.txt >patchV8.log
if errorlevel 1 goto Error2
:PatchV8Done

:DownloadGYP
echo Downloading GYP ...
svn checkout http://gyp.googlecode.com/svn/trunk@%gyprev% build/gyp >getGYP.log
if errorlevel 1 goto Error2
:DownloadGYPDone

:DownloadPython
echo Downloading Python ...
svn checkout http://src.chromium.org/svn/trunk/tools/third_party/python_26@%pythonrev% third_party/python_26 >getPython.log
if errorlevel 1 goto Error2
:DownloadPythonDone

:DownloadCygwin
echo Downloading Cygwin ...
svn checkout http://src.chromium.org/svn/trunk/deps/third_party/cygwin@%cygwinrev% third_party/cygwin >getCygwin.log
if errorlevel 1 goto Error2
:DownloadCygwinDone

cd ..

:DownloadDone

::-----------------------------------------------------------------------------
:: build
::-----------------------------------------------------------------------------

:Build

:SetMSVSVersion
if "%VisualStudioVersion%"=="12.0" goto UseMSVS2013
set GYP_MSVS_VERSION=2012
goto SetMSVSVersionDone
:UseMSVS2013
set GYP_MSVS_VERSION=2013
:SetMSVSVersionDone

set PYTHONHOME=
set PYTHONPATH=

:CreatePatchFile
echo Creating patch file ...
cd v8
svn diff -x --ignore-space-change -x --ignore-eol-style >V8Patch.txt
if errorlevel 1 goto Error2
cd ..
:CreatePatchFileDone

:Copy32Bit
echo Building 32-bit V8 ...
if exist v8-ia32\ goto Copy32BitDone
md v8-ia32
if errorlevel 1 goto Error1
xcopy v8\*.* v8-ia32\ /e /y >nul
if errorlevel 1 goto Error1
:Copy32BitDone

:Build32Bit
cd v8-ia32
third_party\python_26\python build\gyp_v8 -Dtarget_arch=ia32 -Dcomponent=shared_library -Dv8_use_snapshot=false -Dv8_enable_i18n_support=0 >gyp.log
if errorlevel 1 goto Error2
msbuild /p:Configuration=%mode% /p:Platform=Win32 /t:v8 tools\gyp\v8.sln >build.log
if errorlevel 1 goto Error2
cd ..
:Build32BitDone

:Copy64Bit
echo Building 64-bit V8 ...
if exist v8-x64\ goto Copy64BitDone
md v8-x64
if errorlevel 1 goto Error1
xcopy v8\*.* v8-x64\ /e /y >nul
if errorlevel 1 goto Error1
:Copy64BitDone

:Build64Bit
cd v8-x64
third_party\python_26\python build\gyp_v8 -Dtarget_arch=x64 -Dcomponent=shared_library -Dv8_use_snapshot=false -Dv8_enable_i18n_support=0 >gyp.log
if errorlevel 1 goto Error2
msbuild /p:Configuration=%mode% /p:Platform=x64 /t:v8 tools\gyp\v8.sln >build.log
if errorlevel 1 goto Error2
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
goto End

:Error2
cd ..
:Error1
cd ..
:Error
echo *** THE PREVIOUS STEP FAILED ***

:End
cd ..\..\..

:Exit
endlocal
