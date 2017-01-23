# GitHub + Slack + RaspberryPiを用いたHUBOT開発環境の構築

## １．はじめに
どうも、ISAOXです。
最近、仲間内で開発とか雑談とかに使うチャットツールとしてSlackを使い始めました。
このSlack上ではBOTなるものがチャットのお手伝いをしてくれます。
このBOTはGitHub社製のHUBOTというものを使えば自分で作れるそうです。
HUBOTはRaspberryPiのOS(Raspbian)でも動作します。
家には放置されているRaspberryPi(以下RPi)があります。
RPiはGPIO等を備えているのでカメラとかサーボとか色々なデバイスと連携できます。
なので、Slackチャットに面白い投稿をしてくれるBOTアイデアが広がりそうです！
そこで、私なりのHUBOT開発環境の構築を紹介します。

## 2. 前提条件

## 3. 開発サイクル
面倒くさがり屋の私は毎度毎度RPiをモニタにつないだりPCとつないだりとか考えるだけでやる気が失せます。
なので、そういうのにつなげるのは最初の環境構築とデバイスを追加する時だけにしたいです。
よって、以下のような流れで開発サイクルを回していける環境構築を行います。
 1. 私がローカルPC上でHUBOT用のスクリプトを書く。
 2. 私がGitHubのMyリポジトリにスクリプトをPUSHして更新する。
 3. 私がSlack上でBOTにRPi上のスクリプトを最新状態に更新する命令を出し、更新させる。
 4. 必要なら、私がSlack上でBOTにRPiを再起動する命令を出す。
 5. RPi上のHUBOTが更新された最新のスクリプトで動作する。
 6. 1～5を繰り返してBOTの開発を行う。

## 4. 開発環境構築
上記の開発サイクルにするには以下を実現する必要があります。
* RPi上でHUBOTが読み取るスクリプトの階層パスをMyリポジトリの格納先と同じにする。
* RPiが起動したらHUBOTが自動起動できるようにする。
* Slack上でBOTに『git pull』を送信したらRPi上でMyリポジトリを更新できるようにする。
* Slack上でBOTに『reboot』を送信したらRPiを再起動できるようにする。

※階層パスやファイル名は各自の環境に合わせてください

### 4-1. RPi上でHUBOTが読み取るスクリプトの階層パスをMyリポジトリの格納先と同じにする。
。

### 4-2. RPiが起動したらHUBOTが自動起動できるようにする。
。

### 4-3. Slack上でBOTに『git pull』を送信したらRPi上でMyリポジトリを更新できるようにする。
以下のような階層パスと内容のShellスクリプトを用意します。

``/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/git_cmd.sh``
```
#!/bin/sh

cd /home/pi/GitHub/StudyRPi
git $@
```
HUBOTが読み込むcoffeeスクリプトに上記Shellスクリプトを呼び出す命令を追記します。
``/home/pi/GitHub/StudyRPi/Hubot/iotbot/scripts/test.coffee``
```
  robot.respond /git (.*)/i, (msg) ->
    arg = msg.match[1]
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/git_cmd.sh #{arg}"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?
```

### 4-4. Slack上でBOTに『reboot』を送信したらRPiを再起動できるようにする。
以下のような階層パスと内容のShellスクリプトを用意します。

``/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_reboot.sh``
```
#!/bin/sh

sudo reboot
```
HUBOTが読み込むcoffeeスクリプトに上記Shellスクリプトを呼び出す命令を追記します。
``/home/pi/GitHub/StudyRPi/Hubot/iotbot/scripts/test.coffee``
```
  robot.respond /reboot/, (msg) ->
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_reboot.sh"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?
```

## 0. RaspberryPiにインストールされているパッケージのアップデート・アップグレード
### 
``sudo apt-get update``  
``sudo apt-get upgrade``

## 1. node.js(v0.12)のインストール
``curl -sL https://deb.nodesource.com/setup_0.12 | sudo bash -``  
``sudo apt-get install nodejs``

## 2. npmのインストールとアップデート
``sudo npm install -g npm``  
``sudo npm install -g n``

## 3. hubot関係モジュールをインストール
```sudo npm install -g yo generator-hubot coffee-script```

## 4.botの作成
```
mkdir mybot
cd mybot
yo hubot
```
