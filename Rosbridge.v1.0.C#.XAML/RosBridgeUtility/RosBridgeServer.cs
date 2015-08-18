using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace RosBridgeUtility
{
    public class OdometryMsg: Pose
    {
        
    }

    public class ServerStorage
    {
        public OdometryMsg lastOdom { get; set; }
        public Twist lastTeleopMsg { get; set; }
        public LaserScanMsg lastLaserScanMsg { get; set; }
    }

    public class StoringBehavior : WebSocketBehavior
    {
        protected ServerStorage serverStorage;
        public StoringBehavior() : this(null) { }

        public StoringBehavior(ServerStorage ss)
        {
            this.serverStorage = ss;
        }
        
    }

    public class OdometryBehavior : StoringBehavior
    {
        public OdometryBehavior(ServerStorage ss) : base(ss) { }
        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            Send(JsonConvert.SerializeObject(serverStorage.lastOdom));
        }
    }

    public class LaserScanBehavior : StoringBehavior
    {
        public LaserScanBehavior(ServerStorage ss) : base(ss) { }
        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            Send(JsonConvert.SerializeObject(serverStorage.lastLaserScanMsg));
        }
    }

    public class TeleopKeyBehavior : StoringBehavior
    {
        protected IROSWebTeleopController teleopController;
        public TeleopKeyBehavior(ServerStorage ss, IROSWebTeleopController tele): base(ss) 
        {
            teleopController = tele;
        }
        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            teleopController.teleopTarget(e.Data);
        }
        public void setTeleop(IROSWebTeleopController tele)
        {
            teleopController = tele;
        }
    }
}
