@echo off
REM echo ***************************
REM echo * get Price from Bitflyer.
REM echo ***************************

REM SET CUR_DIR=.\my_exec\bitflyerAPI\
SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%genPriceGraph.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)



SET SAMPLING_NUM=30


:START_LINE
%EXEC_FILE% %DEST%\PriceList.json %DEST%\PriceGraph.png %SAMPLING_NUM%

pause > NUL
EXIT /B




