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
            logic.StartCollect("/turtle1/pose");
            System.Threading.Thread.Sleep(1000);
            foreach (String s in logic.StopCollection("/turtle1/pose"))
            {
                Console.WriteLine(s);
            }
        }
    }
}
