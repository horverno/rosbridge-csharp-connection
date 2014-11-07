using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RosBridgeDotNet;

namespace RosTestTurtlesim
{
    class Turtlesim
    {
        static void Main(string[] args)
        {
            RosBridgeDotNet.RosBridgeDotNet turtleConnection = new RosBridgeDotNet.RosBridgeDotNet("10.2.230.124");
            string topic = "/turtle1/command_velocity";
            string msgtype = "turtlesim/Velocity";

            Console.WriteLine(turtleConnection.Connected ? "Connected." : "No connection :(");
            if (turtleConnection.Connected)
            {
                
                ////'/turtle1/command_velocity', 'turtlesim/Velocity', '{"linear":20, "angular":20}'
                RosBridgeDotNet.RosBridgeDotNet.TurtleSim turtleGo1 = new RosBridgeDotNet.RosBridgeDotNet.TurtleSim(20, 20);
                turtleConnection.Publish(topic, msgtype, turtleGo1);
                //Console.WriteLine("Published: " + topic + " " + msgtype + " " + turtleGo1.linear + " " + turtleGo1.angular);
                //turtleConnection.Subscribe();
                
                turtleConnection.CloseConnection();
                Console.WriteLine("Connection closed, press a key.");
            }
            else
                Console.WriteLine("End.");
            Console.ReadKey();
        }
    }


}
