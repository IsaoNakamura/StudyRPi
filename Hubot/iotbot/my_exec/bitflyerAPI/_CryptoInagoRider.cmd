@echo off
echo ***********************************
echo * CryptoInagoRider
echo ***********************************

SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%CryptoInagoRider.pl
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




