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
            RosBridgeUtility.RosBridgeConfig conf1 = new RosBridgeUtility.RosBridgeConfig();
            conf1.readConfig("XMLFile1.xml");
            RosBridgeUtility.RosBridgeLogic logic = new RosBridgeUtility.RosBridgeLogic();
            logic.AddStoppedListener(logic_MonitoringStopped);
            logic.Initialize(conf1.URI);
            logic.Connect();
            logic.InitializeCollection();
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
            */
            //logic.PublishMessage("/turtle1/command_velocity", keys, vals);
            //logic.PublishMessage("/turtle1/cmd_vel", keys, vals);
            Object[] lin = { 2.0, 0.0, 0.0 };
            Object[] ang = { 0.0, 0.0, 3.14 };
            foreach (var item in conf1.getPublicationList())
            {
                logic.PublishTwistMsg(item, lin, ang);
            }
            logic.StartCollections(conf1.getTopicList());
            Object[] linNeo = { 2.1, 2.1 };
            Object[] driveActive = { true, true };
            Object[] quickStop = { false, false };
            Object[] disableBrake = { true, true };
            logic.PublishNeobotixCommandMsg("/DriveCommands", linNeo, driveActive, quickStop, disableBrake);
            System.Threading.Thread.Sleep(1000);

            logic.RemoveCollections(conf1.getTopicList());
            var x = logic.StopCollection();
            foreach (var attr in conf1.ProjectedAttributes())
            {
                Console.Out.WriteLine(attr.Item2);
                foreach (var item in logic.getResponseAttribute(attr.Item1, attr.Item2))
                {
                    Console.Out.WriteLine(((List<Double>)item)[0]);
                    //Console.Out.WriteLine("{0}: {1} {2}", attr.Item1, ((List<Double>)item)[0], ((List<Double>)item)[1]);
                }
            }
            
        }

        static void logic_MonitoringStopped(Object Sender, EventArgs e)
        {
            Console.Out.WriteLine("Monitoring stopped here");
        }
    }
}
