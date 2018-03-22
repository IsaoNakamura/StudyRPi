@echo off
echo ***************************
echo * create StopCode-file.
echo ***************************

REM SET CUR_DIR=.\
SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%createStopCode.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)
:START_LINE
%EXEC_FILE% .\StopCode.txt

echo ***************************
echo * finished
echo ***************************

pause > NUL
EXIT /B




