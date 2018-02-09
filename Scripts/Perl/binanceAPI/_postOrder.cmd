@echo off
echo ***************************
echo * post Order to BINANCE.
echo ***************************

SET CUR_DIR=.\
SET EXEC_FILE=%CUR_DIR%postOrder.pl
SET HOST_URL=https://api.binance.com/
SET AUTH=%CUR_DIR%AuthBinance.json
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)


SET API_TYPE=api/v3/order
SET DEST_URL=%HOST_URL%%API_TYPE%

SET /P SYMBOL=SYMBOL:
SET /P SIDE=SIDE:
SET TYPE=LIMIT
SET TIME_INF_FORCE=GTC
SET /P QUANTITY=QUANTITY:
SET /P PRICE=PRICE:
SET RECV_WINDOW=5000

:START_LINE
%EXEC_FILE% %DEST_URL% %AUTH% %DEST%\result.txt %SYMBOL% %SIDE% %TYPE% %TIME_INF_FORCE% %QUANTITY% %PRICE% %RECV_WINDOW%

echo ***************************
echo * finished.
echo ***************************

pause > NUL
EXIT /B




