using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestBridgeUtility
{
    class Program
    {

        static void Main(string[] args)
        {
            RosBridgeUtility.RosBridgeLogic logic = new RosBridgeUtility.RosBridgeLogic();
            logic.Initialize("ws://10.2.94.154:9090");
            logic.Connect();
            logic.initializeCollection();            
            /*
            Dictionary<String, double> lin = new Dictionary<string, double>()
            {
                {"x",1.5},
                {"y",0.0},
                {"z",0.0},
            };
            Dictionary<String, double> ang = new Dictionary<string, double>()
            {
                {"x",0.0},
                {"y",0.0},
                {"z",3.14},
            };
            Object[] vals = { lin, ang };
             * */

            //logic.PublishMessage("/turtle1/command_velocity", keys, vals);
            //logic.PublishMessage("/turtle1/cmd_vel", keys, vals);
            Object[] lin = {2.0, 0.0, 0.0};
            Object[] ang = {0.0, 0.0, 3.14};
            logic.PublishTwistMsg("/turtle1/cmd_vel", lin, ang);

            logic.StartCollection("/turtle1/pose");
            logic.StartCollection("/turtle1/color_sensor");
            //logic.StartCollection("/turtle1/cmd_vel");
            logic.StartCollection("/base_scan");
            System.Threading.Thread.Sleep(2500);
            logic.RemoveCollection("/turtle1/pose");
            logic.RemoveCollection("/turtle1/color_sensor");
            //logic.RemoveCollection("/turtle1/cmd_vel");
            logic.RemoveCollection("/base_scan");
            var x = logic.StopCollection();
            
            foreach (var item in logic.getResponseAttribute("/turtle1/pose","theta"))
            {
                Console.Out.WriteLine(item.ToString());
            }
            
            /*
            foreach (var item in logic.getResponseAttribute("/turtle1/cmd_vel","linear.y.y"))
            {
                Console.Out.WriteLine(item.ToString());
            }
             * */
            foreach (var item in logic.getResponseAttribute("/base_scan", "intensities"))
            {
                Console.Out.WriteLine(item.ToString());
            }
            
        }
    }
}
