# 3. OpenCVを用いた顔認識
## 特別にいるもの
* UVC対応USBカメラ(動作確認済カメラ：Logicool C270)

## 手順
### 1. ワイヤリング。
USBカメラをRaspberryPiに接続する。  

### 2. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git pull`` する。

### 5. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/detectFaceCV](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/detectFaceCV)  
``cd App/detectFaceCV``  

### 6. コンパイルして実行ファイルを作成する。  
``make``  

### 7. 管理者権限で実行ファイルを実行する。  
``sudo ./detectFaceCV``  

カメラ映像のウィンドウが表示されて、顔が映ったら赤矩形で囲まれたら成功！！  