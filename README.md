# rosbridge-csharp-connection
The aim of the project is to develop C# applications which connects to ROS via RosBridge. The early release works with rosbridge 1.0 the latest ones with rosbridge 2.0 (rosbridge_suite). 



## Demo video
A demo of a C# application which connects to ROS via RosBridge. The communication is a JSON socket communication. 

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
roslaunch rosbridge_launch simple.launch 
rosrun turtlesim turtlesim_node
```

## Demo image
![alt tag](http://www.sze.hu/~herno/robotics/rosbridge-csharp-connection.png)
