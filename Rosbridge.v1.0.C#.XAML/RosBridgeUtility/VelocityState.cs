using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosBridgeUtility
{
    public class VelocityState
    {
        public double max_vel { get; set; }
        public double min_vel { get; set; }
        public double inc_vel { get; set; }
        public double current_vel { get; set; }
    }
}
