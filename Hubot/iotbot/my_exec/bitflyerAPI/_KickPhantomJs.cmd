@echo off
echo ***********************************
echo * kick PhantomJs
echo ***********************************

SET CUR_DIR=.\
SET DEST=%CUR_DIR%DEST
IF NOT EXIST %DEST% (
    MKDIR %DEST%
)

:START_LINE
phantomjs --webdriver=9999

echo ***************************
echo * finished.
echo ***************************

pause > NUL
EXIT /B




