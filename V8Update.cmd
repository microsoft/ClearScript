@echo off
setlocal

if "%v8rev%"=="" goto LatestRev
echo V8 revision: %v8rev%
goto SetMode
:LatestRev
echo V8 revision: Latest
set v8rev=HEAD

:SetMode
set mode=%1
if "%mode%"=="" goto ReleaseMode
if /i "%mode%"=="debug" goto DebugMode
if /i "%mode%"=="release" goto ReleaseMode
echo %mode%: Invalid build mode; please specify "Debug" or "Release"
goto Exit
:DebugMode
set mode=Debug
goto Start
:ReleaseMode
set mode=Release
goto Start

:Start
echo Build mode: %mode%
cd ClearScript\v8\v8
if errorlevel 1 goto Exit

if not exist build\ goto CreateBuildDir
echo Removing old build directory ...
rd /s /q build
:CreateBuildDir
echo Creating build directory ...
md build
if errorlevel 1 goto Error
cd build

echo Downloading V8 ...
svn checkout http://v8.googlecode.com/svn/trunk/@%v8rev% v8 >getV8.log
if errorlevel 1 goto Error1
cd v8

echo Patching V8 ...
svn patch ..\..\V8Patch.txt >patchV8.log
if errorlevel 1 goto Error2
svn diff -x --ignore-eol-style >V8Patch.txt

echo Downloading GYP ...
svn checkout http://gyp.googlecode.com/svn/trunk build/gyp >getGYP.log
if errorlevel 1 goto Error2

echo Downloading Python ...
svn checkout http://src.chromium.org/svn/trunk/tools/third_party/python_26 third_party/python_26 >getPython.log
if errorlevel 1 goto Error2

echo Downloading Cygwin ...
svn checkout http://src.chromium.org/svn/trunk/deps/third_party/cygwin third_party/cygwin >getCygwin.log
if errorlevel 1 goto Error2
cd ..

echo Building 32-bit V8 ...
md v8-ia32
if errorlevel 1 goto Error1
xcopy v8\*.* v8-ia32\ /e /y >nul
if errorlevel 1 goto Error1
cd v8-ia32
third_party\python_26\python build\gyp_v8 -Dtarget_arch=ia32 -Dcomponent=shared_library -Dv8_use_snapshot=false >gyp.log
if errorlevel 1 goto Error2
devenv /build "%mode%|Win32" tools\gyp\v8.sln >build.log
if errorlevel 1 goto Error2
cd ..

echo Building 64-bit V8 ...
md v8-x64
if errorlevel 1 goto Error1
xcopy v8\*.* v8-x64\ /e /y >nul
if errorlevel 1 goto Error1
cd v8-x64
third_party\python_26\python build\gyp_v8 -Dtarget_arch=x64 -Dcomponent=shared_library -Dv8_use_snapshot=false >gyp.log
if errorlevel 1 goto Error2
devenv /build "%mode%|x64" tools\gyp\v8.sln >build.log
if errorlevel 1 goto Error2
cd ..\..

if not exist lib\ goto CreateLibDir
echo Removing old lib directory ...
rd /s /q lib
:CreateLibDir
echo Creating lib directory ...
md lib
if errorlevel 1 goto Error

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

if not exist include\ goto CreateIncludeDir
echo Removing old include directory ...
rd /s /q include
:CreateIncludeDir
echo Creating include directory ...
md include
if errorlevel 1 goto Error

echo Importing V8 header files ...
copy build\v8\include\*.* include\ >nul
if errorlevel 1 goto Error

echo Updating patch file ...
copy build\v8\V8Patch.txt .\ >nul
if errorlevel 1 goto Error

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
