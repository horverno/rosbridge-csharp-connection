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
            logic.Initialize("ws://10.2.94.144:9090");
            logic.Connect();
            logic.initializeCollection();
            logic.StartCollection("/turtle1/pose");
            logic.StartCollection("/turtle1/color_sensor");
            System.Threading.Thread.Sleep(1000);
            logic.RemoveCollection("/turtle1/pose");
            logic.RemoveCollection("/turtle1/color_sensor");
            var x = logic.StopCollection();            
            foreach (var item in logic.getResponseAttribute("/turtle1/pose","x"))
            {
                Console.Out.WriteLine(item.ToString());
            }
            foreach (var item in logic.getResponseAttribute("/turtle1/color_sensor", "r"))
            {
                Console.Out.WriteLine(item.ToString());
            }
            foreach (var item in logic.getResponseAttribute("/turtle1/color_sensor", "g"))
            {
                Console.Out.WriteLine(item.ToString());
            }
        }
    }
}
