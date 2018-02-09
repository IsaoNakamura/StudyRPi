@echo off
echo ***************************
echo * delete Order from BINANCE.
echo ***************************

SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%deleteOrder.pl
SET HOST_URL=https://api.binance.com/
SET AUTH=%CUR_DIR%AuthBinance.json
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)

SET /P ORDER_ID=ORDER_ID:

SET API_TYPE=api/v3/order
SET DEST_URL=%HOST_URL%%API_TYPE%

:START_LINE
%EXEC_FILE% %DEST_URL% %AUTH% %DEST%\result_deleteOrder.txt %ORDER_ID%

echo ***************************
echo * finished.
echo ***************************

pause > NUL
EXIT /B




