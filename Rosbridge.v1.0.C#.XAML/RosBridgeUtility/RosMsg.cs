using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosBridgeUtility
{
    public class Pose
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public double ori_w { get; set; }
        public double ori_x { get; set; }
        public double ori_y { get; set; }
        public double ori_z { get; set; }
    }
    public class Twist
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public double ang_x { get; set; }
        public double ang_y { get; set; }
        public double ang_z { get; set; }
    }

    public class LaserScanMsg
    {
        public double min_angle { get; set; }
        public double angle_increment { get; set; }
        public double[] ranges { get; set; }
    }
}
