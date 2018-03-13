@echo off
echo ****************************
echo * get Markets from Bitflyer.
echo ****************************

SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%getMarkets.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)


REM SET DEST_URL=%HOST_URL%%API_TYPE%


:START_LINE
%EXEC_FILE%

echo ***************************
echo * finished.
echo ***************************

pause > NUL
EXIT /B




