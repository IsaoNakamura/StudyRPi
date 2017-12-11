@echo off
echo ***************************
echo * get Price from Bitflyer.
echo ***************************

SET CUR_DIR=.\my_exec\bitflyerAPI\
SET EXEC_FILE=%CUR_DIR%getPrice.pl
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)

SET CONFIG=%CUR_DIR%config_bitflyer.txt
FOR /F "eol=# delims=, tokens=1,2" %%a in ( %CONFIG% ) do (
 IF /I "%%a" == "hostName" (
  SET HOST_URL=%%b
 )
 IF /I "%%a" == "apiKey" (
 	SET API_KEY=%%b
 )
)

SET API_TYPE=api/echo/price
SET DEST_URL=%HOST_URL%%API_TYPE%

:START_LINE
%EXEC_FILE% -k %API_KEY% %DEST_URL% %DEST%\result.txt

REM pause > NUL
EXIT /B




