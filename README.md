# rosbridge-csharp-connection
The aim of the project is to develop C# applications which connects to ROS via RosBridge. The early release works with rosbridge 1.0 the latest ones with rosbridge 2.0 (rosbridge_suite). 



## Demo video
A demo of a C# application which connects to ROS via RosBridge. The communication is a JSON socket communication. [Watch the video on YouTube](https://youtu.be/cVuTH-KwDqA).

## Usage
### Rosbridge 1.0
In ROS you have to start:
```
roscore
rosrun rosbridge rosbridge.py
rosrun turtlesim turtlesim_node
```

### Rosbridge 2.0
In ROS you have to start:
```
roscore
roslaunch rosbridge_launch simple.launch   or
roslaunch rosbridge_launch http.launch
rosrun turtlesim turtlesim_node
```

## Demo image
![alt tag](http://www.sze.hu/~herno/robotics/rosbridge-csharp-connection.png)

## Related links
* [ROS official](http://ros.org/)
* [Rosbridge wiki](http://wiki.ros.org/rosbridge_suite) 
* [Rosbridge on GitHub](https://github.com/RobotWebTools/rosbridge_suite)
* [Newtonsoft.Json](http://www.newtonsoft.com/json)
* [Newtonsoft.Json on GitHub](https://github.com/JamesNK/Newtonsoft.Json)
