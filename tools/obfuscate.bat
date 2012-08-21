@ECHO OFF

if "%1"=="prebuild" SET ARGS=beforebuild 
if "%1"=="postbuild" SET ARGS=overwriteinputfiles

SET COEXE=%ProgramFiles%\LogicNP Software\Crypto Obfuscator For .Net 2010 R2\.

if exist %ProgramFiles(x86)%" SET COEXE="%ProgramFiles(x86)%\LogicNP Software\Crypto Obfuscator For .Net 2010 R2\.

SET PROJDIR=%CD%

if not exist "%COEXE%" GOTO CLEARERROR

cd %COEXE%

if not exist co.exe GOTO RESETDIR

SET PATH=%PATH%;%COEXE%

cd %PROJDIR%

start /B /WAIT co.exe "projectfile=%2" hidden=true %ARGS%=true

GOTO EOF

:RESETDIR

cd %PROJDIR%

:CLEARERROR

verify >nul

:EOF
