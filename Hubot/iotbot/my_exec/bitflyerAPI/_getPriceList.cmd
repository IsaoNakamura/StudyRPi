@echo off
REM echo ***************************
REM echo * get Price from Bitflyer.
REM echo ***************************

REM SET CUR_DIR=.\my_exec\bitflyerAPI\
SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%getPriceList.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)

SET DIFF_THRESHOLD=0
SET SAMPLING_NUM=30

SET CONFIG=%CUR_DIR%config_bitflyer.txt
FOR /F "eol=# delims=, tokens=1,2" %%a in ( %CONFIG% ) do (
 IF /I "%%a" == "hostName" (
  SET HOST_URL=%%b
 )
)

SET API_TYPE=api/echo/price
SET DEST_URL=%HOST_URL%%API_TYPE%

:START_LINE
%EXEC_FILE% %DEST_URL% %DEST%\PriceList.json %DEST%\PriceList.png %DIFF_THRESHOLD% %SAMPLING_NUM%

pause > NUL
EXIT /B




