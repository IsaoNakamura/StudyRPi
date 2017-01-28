module.exports = (robot) ->
  robot.hear /isaox/i, (res) ->
    res.send "isaox? isaox is my master!!"

  robot.hear /who am I/i, (msg) ->
    msg.send "You are #{msg.message.user.name}"

  robot.hear /who are You/i, (msg) ->
    msg.send "My name is iotbot! I am HUBOT!!"

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

  robot.respond /stillpi|stillpi (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/stillpi.sh"
      arg = msg.match[1]
      msg.send "arg: #{arg}"
      msg.send "arg is defined" if arg?
      msg.send "arg is undefined" unless arg?
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

#  robot.respond /cd (.*)/i, (msg) ->
#    directory = msg.match[1]
#    msg.send "directory is #{directory}"
#    @exec = require('child_process').exec
#    command = "cd #{directory}"
#    @exec command, (error, stdout, stderr) ->
#      msg.send error if error?
#      msg.send stdout if stdout?
#      msg.send stderr if stderr?

#  robot.respond /cmd (.*)/i, (msg) ->
#    command = msg.match[1]
#    @exec = require('child_process').exec
#    msg.send "Command: #{command}"
#    @exec command, (error, stdout, stderr) ->
#      msg.send error if error?
#      msg.send stdout if stdout?
#      msg.send stderr if stderr?
