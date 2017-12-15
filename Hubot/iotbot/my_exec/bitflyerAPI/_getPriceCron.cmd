@echo off
echo ***************************
echo * kick getPriceCron.
echo ***************************

REM SET CUR_DIR=.\my_exec\bitflyerAPI\
SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%getPriceCron.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)

SET DIFF_THRESHOLD=0
SET SAMPLING_NUM=5
SET TEST=0
SET CYCLE_SEC=5

SET CONFIG=%CUR_DIR%config_bitflyer.txt
FOR /F "eol=# delims=, tokens=1,2" %%a in ( %CONFIG% ) do (
 IF /I "%%a" == "hostName" (
  SET HOST_URL=%%b
 )
)

SET API_TYPE=api/echo/price
SET DEST_URL=%HOST_URL%%API_TYPE%

:START_LINE
%EXEC_FILE% %DEST_URL% %DEST%\BtcPriceList.json %DEST%\BtcPriceGraph.png %DEST%\StopCode.txt %TEST% %CYCLE_SEC% %SAMPLING_NUM% %DIFF_THRESHOLD%

echo ***************************
echo * finished
echo ***************************

pause > NUL
EXIT /B




