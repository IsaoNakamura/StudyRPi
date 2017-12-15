@echo off
echo ***************************
echo * create StopCode-file.
echo ***************************

REM SET CUR_DIR=.\my_exec\bitflyerAPI\
SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%createStopCode.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)
:START_LINE
%EXEC_FILE% %DEST%\StopCode.txt

echo ***************************
echo * finished
echo ***************************

pause > NUL
EXIT /B




