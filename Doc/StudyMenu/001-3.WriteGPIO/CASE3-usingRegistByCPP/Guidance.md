# CASE 3: // C++コードによるレジストリ操作でLEDチカチカ

1. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git fetch`` もしくは、 ``git pull`` する。

2. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/writeGPIO](https://github.com/IsaoNakamura/StudyRPi/blob/wrkFirstPush/App/writeGPIO)  
``cd App/writeGPIO`` 

3. コンパイルして実行ファイルを作成する。  
``make``  

6. 管理者権限で実行ファイルを実行する。  
``sudo ./writeGPIO``  
このとき、ワイヤリングしたLEDが点滅すれば成功！！
