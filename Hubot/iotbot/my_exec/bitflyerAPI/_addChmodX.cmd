@echo off
echo ***************************
echo * add Chmod+x to file.
echo ***************************

cd /d %1\..

git update-index --add --chmod=+x %1

echo ***************************
echo * finished
echo ***************************

pause > NUL
EXIT /B




