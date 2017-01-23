module.exports = (robot) ->
  robot.hear /isaox/i, (res) ->
    res.send "isaox? isaox is my master!!"

  robot.respond /git (.*)/i, (msg) ->
    arg = msg.match[1]
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/git_cmd.sh #{arg}"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?

  robot.respond /git_pull/, (msg) ->
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/git_pull.sh"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?

  robot.respond /reboot/, (msg) ->
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_reboot.sh"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?

  robot.respond /shutdown/, (msg) ->
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_shutdown.sh"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?

  robot.respond /make_bin/, (msg) ->
    @exec = require('child_process').exec
    command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/make_bin.sh"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?

  robot.respond /make (.*)/i, (msg) ->
    target = msg.match[1]
    @exec = require('child_process').exec
    command = "pwd"
    msg.send "Command: #{command}"
    @exec command, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?
    path = "cd #{target}"
    msg.send "Path: #{path}"
    @exec path, (error, stdout, stderr) ->
      msg.send error if error?
      msg.send stdout if stdout?
      msg.send stderr if stderr?


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
