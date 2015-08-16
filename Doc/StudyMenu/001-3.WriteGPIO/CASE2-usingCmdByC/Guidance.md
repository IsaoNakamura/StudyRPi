# CASE 2: // Cコードによるコマンド実行でLEDチカチカ

例としてGPIO17を使用する方法を明記する。  
コンソールアプリで以下のコマンドを順次実行する。

1. 作業用ディレクトリを作って侵入する。  
``mkdir commandGPIO``  
``cd commandGPIO``

2. ソースコードを新規作成し、コードエディタ(nano等)で開く。  
``nano commandGPIO.c``

3. 下記コードを記述する。  
[commandGPIO.c](https://github.com/IsaoNakamura/StudyRPi/App/commandGPIO/commandGPIO.c)  

4. ソースコードを保存して、コードエディタを終了する。  
 nanoエディタであれば、ctrl+oが保存。ctrl+xが終了。

5. コンパイルして実行ファイルを作成する。  
``cc -o commandGPIO commandGPIO.c``  

6. 管理者権限で実行ファイルを実行する。  
``sudo ./commandGPIO``  
このとき、ワイヤリングしたLEDが点滅すれば成功！！
