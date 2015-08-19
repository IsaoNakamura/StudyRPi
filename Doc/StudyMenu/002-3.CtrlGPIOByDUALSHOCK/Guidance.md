# 3. PS3コントローラを用いたLEDチカチカ
## 特別にいるもの
* PS3コントローラ(DUALSHOCK3 or SIXAXIS)
* USB-Bluetooth接続ドングル
* USBケーブル(A-miniBタイプ)

## 手順
### 1. RPiとPS3コントローラをBluetoothで接続できるようにする。
1. 以下のコマンドでRPiの参照するパッケージリストを更新する。  
``sudo apt-get update``
2. 以下のコマンドでRPiにパッケージをインストールする。  
``sudo apt-get install bluez-utils bluez-compat bluez-hcidump``  
``sudo apt-get install checkinstall libusb-dev  libbluetooth-dev joystick``  
3.  USB-Bluetooth接続ドングルをRPiに挿して、以下のコマンドを実行する。  
``hciconfig``  
実行後、表示される``BD Address:``にMACアドレスが表示されない場合、USB-Bluetooth接続ドングルが認識されていない。  
挿し直したり、RPiを再起動しても解決しない場合はググる。  
4. 以下のコマンドでRPiとPS3コントローラをペアリングするツールをダウンロードしてコンパイルする。  
``sudo apt-get install pyqt4-dev-tools``  
``mkdir sixpair``  
``cd sixpair``  
``wget http://www.pabr.org/sixlinux/sixpair.c``  
``gcc -o sixpair sixpair.c -lusb``  
5. PS3コントローラをRPiにUSBケーブルで接続して、以下のコマンドを実行する。  
``sudo ./sixpair``  
USB-Bluetooth接続ドングルとPS3コントローラのMACアドレスが表示されればペアリング成功。
6. 以下のコマンドでPS3コントローラを管理する「Sixaxis Joystick Manager」をダウンロードしてコンパイルする。  
``wget http://sourceforge.net/projects/qtsixa/files/QtSixA%201.5.1/QtSixA-1.5.1-src.tar.gz``  
``tar xfvz QtSixA-1.5.1-src.tar.gz``  
``cd QtSixA-1.5.1/sixad``  
``make``  
``sudo mkdir -p /var/lib/sixad/profiles``  
``sudo apt-get install checkinstall``  
``sudo checkinstall``  
7. RPiにUSBケーブルで接続しているPS3コントローラを取り外す。  
8. 以下のコマンドでPS3コントーラを接続する「sixad daemon」を起動する。  
``sudo sixad --start``  
9. PS3コントローラのPSボタンを押して、上面の赤ランプが点滅後ひとつだけ点灯すれば接続成功。  
10. 以下のコマンド実行後に、PS3コントローラのボタンや傾きなどの数値がコンソールに反映されれば認識成功。  
``sudo jstest /dev/input/js0``  
11. 「sixad daemon」をRPi起動時に自動起動させておくには以下のコマンドを実行する。  
``sudo update-rc.d sixad defaults``  
``sudo reboot``  
自動起動設定した後の再起動後や、次回の起動からPSボタンを押しても接続しない場合がある。  
その時は、以下のコマンドで「sixad daemon」を停止させてから、起動してもう一度接続するか試すこと。  
``sudo sixad --stop``  
``sudo sixad --start``  

### 2. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Wiring](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_WriteGPIO/RPi_WriteGPIO_bread.png?raw=true)

### 3. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git fetch`` もしくは、 ``git pull`` する。

### 4. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/dualshockGPIO](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/dualshockGPIO)  
``cd App/dualshockGPIO`` 

### 5. コンパイルして実行ファイルを作成する。  
``make``  

### 6. 管理者権限で実行ファイルを実行する。  
``sudo ./dualshockGPIO``  
PS3コントローラの左ボタンを押したとき、LEDが点灯すれば成功！！  
PS3コントローラの右ボタンを押したとき、LEDが消灯すれば成功！！  
