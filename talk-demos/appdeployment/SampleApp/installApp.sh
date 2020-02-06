sudo killall -9 dotnet
nohup dotnet /home/ec2-user/build_output/app.dll &>/dev/null