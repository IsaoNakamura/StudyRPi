request = require "request";
fs = require "fs";

cron = require('cron').CronJob;

module.exports = (robot) ->

  cron_job = null;

  robot.respond /testcron (.*)|testcron/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      msg.send "Arg[0]: #{arg}" if arg?
      channel = msg.message.room
      msg.send "respond from #{channel}."
      # cron's 1st parameter
      #   seconds      : 0-59
      #   Minutes      : 0-59
      #   Hours        : 0-23
      #   Day of Month : 1-31
      #   Months       : 0-11
      #   Day of Week  : 0-6
      if cron_job != null
        msg.send "cron_job is exist."
      else
        msg.send "cron_job is-not exist."
        cron_job = new cron '15 * * * * *', () =>
          robot.send {room: channel}, "I send msg with regularity."
        , null, true, "Asia/Tokyo"
        msg.send "created cron_job."
    else
      msg.send "get out !!"

  robot.respond /timesignal (.*)|timesignal/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      msg.send "Arg[0]: #{arg}" if arg?
      channel = msg.message.room
      #msg.send "respond from #{channel}."
      if cron_job != null
        msg.send "cron_job is exist."
      else
        msg.send "cron_job is-not exist."
        cron_job = new cron '0 * * * * *', () =>
          # get time.
          dt = new Date()
          year = dt.getFullYear()
          month = ("0"+( dt.getMonth() + 1 )).slice(-2)
          date = ("0"+dt.getDate()).slice(-2)
          hour = ("0"+dt.getHours()).slice(-2)
          min = ("0"+dt.getMinutes()).slice(-2)
          sec = ("0"+dt.getSeconds()).slice(-2)
          # time_msg = "いまは#{month}がつ#{date}にち#{hour}じ#{min}ふん#{sec}びょうです"
          time_msg = "#{hour}じ、#{min}ふん、になりました。"
          time_msg = "#{time_msg}#{arg}" if arg?
          robot.send {room: channel}, "#{time_msg}"
          #create command
          @exec = require('child_process').execSync
          command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/talkpi.sh #{time_msg}"
          @exec command, (error, stdout, stderr) ->
            msg.send error if error?
            msg.send stdout if stdout?
            msg.send stderr if stderr?
        , null, true, "Asia/Tokyo"
    else
      msg.send "get out !!"

  robot.respond /raspistill (.*)|raspistill/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      arg = msg.match[1]
      dt = new Date()
      year = dt.getFullYear()
      month = ("0"+( dt.getMonth() + 1 )).slice(-2)
      date = ("0"+dt.getDate()).slice(-2)
      hour = ("0"+dt.getHours()).slice(-2)
      min = ("0"+dt.getMinutes()).slice(-2)
      sec = ("0"+dt.getSeconds()).slice(-2)
      file_path = "/home/pi/picam/"
      file_name = "#{year}-#{month}-#{date}_#{hour}#{min}_#{sec}.jpg"
      msg.send "file_name: #{file_name}"
      @execSync = require('child_process').execSync
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/stillpi.sh #{file_path}#{file_name} "
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @execSync command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
      api_url = "https://slack.com/api/"
      channel = msg.message.room
      options = {
        token: process.env.HUBOT_SLACK_TOKEN,
        filename: file_name,
        file: fs.createReadStream("#{file_path}#{file_name}"),
        channels: channel
      }

      request
        .post {url:api_url + 'files.upload', formData: options}, (error, response, body) ->
          if !error && response.statusCode == 200
            msg.send "OK"
          else
            msg.send "NG status code: #{response.statusCode}"
    else
      msg.send "get out !!"

  robot.hear /isaox/i, (res) ->
    res.send "isaox? isaox is my master!!"

  robot.hear /(くそが)|(くっそ)/i, (msg) ->
    kusoga_arry = ["https://pbs.twimg.com/media/C5k-X9aU4AAx2Pk.jpg:large","http://livedoor.blogimg.jp/guran2016_ms06/imgs/b/8/b8a2dc96-s.jpg"]
    kusoga_msg = msg.random kusoga_arry
    msg.send "#{kusoga_msg}"

  robot.hear /who am I/i, (msg) ->
    msg.send "You are #{msg.message.user.name}"

  robot.hear /who are You/i, (msg) ->
    msg.send "My name is iotbot! I am HUBOT!!"

  robot.respond /lsusb (.*)|lsusb/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/lsusb_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /dpkg (.*)|dpg/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/dpkg_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /ifconfig (.*)|ifconfig/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/ifconfig_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /git (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/git_cmd.sh #{arg}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /apt-get (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/apt-get_cmd.sh #{arg}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /reboot/, (msg) ->
    if msg.message.user.name == "isaox"
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_reboot.sh"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /shutdown/, (msg) ->
    if msg.message.user.name == "isaox"
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_shutdown.sh"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /make (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      target = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/make_bin.sh #{target}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /talkpi (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/talkpi.sh #{arg}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /upload (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      file_name = msg.match[1]
      msg.send "file_name: #{file_name}"
      api_url = "https://slack.com/api/"
      channel = msg.message.room
      options = {
        token: process.env.HUBOT_SLACK_TOKEN,
        filename: file_name,
        file: fs.createReadStream(file_name),
        channels: channel
      }

      request
        .post {url:api_url + 'files.upload', formData: options}, (error, response, body) ->
          if !error && response.statusCode == 200
            msg.send "OK"
          else
            msg.send "NG status code: #{response.statusCode}"

    else
      msg.send "get out !!"