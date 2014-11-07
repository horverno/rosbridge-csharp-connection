using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RosBridgeDotNet;

namespace RosTestTurtlesim
{
    class Program
    {
        static void Main(string[] args)
        {
            ////////////////////////
            ////'/DriveCommands', 'neo_serrelayboard/DriveCommas', '{"angularVelocity":[0.1,0.1],"driveActive":[1,1],"quickStop":[0,0],"disableBrake":[1,1]}'
            RosBridgeDotNet.RosBridgeDotNet kapcs = new RosBridgeDotNet.RosBridgeDotNet("10.2.5.194");
            string topic = "/DriveCommands";
            string msgtype = "neo_serrelayboard/DriveCommands";
            double[] sebesseg = { 0.0, 0.0 };
            bool[] igaz = { true, true };
            bool[] hamis = { false, false };
            //RosBridgeDotNet.RosBridgeDotNet.FreeString = new RosBridgeDotNet.RosBridgeDotNet.FreeString();
            RosBridgeDotNet.RosBridgeDotNet.Neobotix neo = new RosBridgeDotNet.RosBridgeDotNet.Neobotix(sebesseg, igaz, hamis, igaz);

            Console.WriteLine(kapcs.Connected ? "Kapcsolodva." : "Sikertelen kapcs.");
            Console.ReadKey();
            //kapcs.Subscribe();
            kapcs.Publish(topic, msgtype, neo);

            Console.ReadKey();
            kapcs.CloseConnection();

            ////////////////////////
            ////'/turtle1/command_velocity', 'turtlesim/Velocity', '{"linear":1, "angular":0}'
            //RosBridgeDotNet.RosBridgeDotNet r = new RosBridgeDotNet.RosBridgeDotNet("10.2.5.194");

            //string receiver = "/turtle1/command_velocity";
            //string type = "turtlesim/Velocity";
            //RosBridgeDotNet.RosBridgeDotNet.TurtleSim turtle = new RosBridgeDotNet.RosBridgeDotNet.TurtleSim(1.8, 0.5);

            //Console.WriteLine(r.Connected ? "Connected" : "Not connected.");

            //Console.ReadKey();
            ////rostopic pub -1 /turtle1/command_velocity turtlesim/Velocity -- 1 0
            //r.Publish(receiver, type, turtle);
            //turtle.linear = 1;
            //Console.WriteLine("1");

            //Console.ReadKey();
            //r.Publish(receiver, type, turtle);
            //Console.WriteLine("2");

            //Console.ReadKey();
            //r.Publish(receiver, type, turtle);
            //r.CloseConnection();
            //Console.WriteLine("Closed");
        }
    }


}
