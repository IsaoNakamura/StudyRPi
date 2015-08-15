# 1. RaspberryPiのSetUp
## いるもの
* PC(Windows or MacOSX)＋micorSDカードリーダ/ライタ
* microSDカード
* USBキーボード、USBマウス
* モニタ、HDMIケーブル
* USBmicroBケーブル、USB電源

## 手順
### 1. microSDにOSインストーラを格納
1. 必要であればmicroSDを初期化。フォーマット形式はFAT32とかexFATとかかな。
2. OSインストーラ「NOOBS」を公式サイトからダウンロードして解凍する。
3. PCでNOOBSファイル群を全てmicroSDカード直下へコピーする。  
(SDカードのルート直下にフォルダじゃなくてファイル群をコピーする)

### 2. OS(Raspbian)をRaspberryPiにインストール
1. 以下のものをRaspberryPiにブスッと挿す。  
microSDカード  
USBキーボード、USBマウス  
モニタにつないだHDMIケーブル  
USB電源につないだUSBmicroBケーブル

2. モニタにダイアログが表示され、インストールするOSを聞いてくるのでひとまずRaspbianを選択する。  
3. 処理が開始されSuccess的なダイアログが出れば完了。Bootが始まりConfiguration画面が表示される。  
　
### 3. OSの設定変更
最近のバージョンでは初期状態で有効になっている。  
1. ~~「Expand Filesystem」を選択してOK。SDカードの領域が拡張される。~~  
2. ~~「Advanced Options」⇒「SSH」を選択して、enableにする。SSHが有効になる。~~  
3. ~~Configuration画面のFinishを押して再起動させる。~~  


### ログイン  
1. 初期ユーザ名は``pi``、初期パスワードは``raspberry``でログインできる。

### シャットダウン
1. コンソール上で以下のコマンドを実行すればシャットダウンできる。  
``sudo shutdown -h now``

### 再起動
1. コンソール上で以下のコマンドを実行すれば再起動できる。  
``sudo reboot``

### X-Window(GUI)の起動
1. コンソール上で以下のコマンドを実行すればX-Window(GUI)を起動できる。  
``startx``