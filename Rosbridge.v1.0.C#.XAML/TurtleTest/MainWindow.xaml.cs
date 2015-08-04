using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TurtleTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, RosBridgeUtility.IROSBridgeController
    {
        RosBridgeUtility.RosBridgeLogic bridgeLogic;
        RosBridgeUtility.RosBridgeConfig bridgeConfig;


        private RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse _responseObj = new RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse();
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        enum ConnectionState
        {
            Disconnected = 0, Connected
        }
        int connectionState;
        enum SubscriptionState
        {
            Unsubscribed = 0, Subscribed
        }
        int subscriptionState;
       
        public MainWindow()
        {
            InitializeComponent();
            stackControls.Visibility = System.Windows.Visibility.Hidden;
            this.DataContext = _responseObj;
            this.bridgeLogic = new RosBridgeUtility.RosBridgeLogic();
            this.connectionState = (int)ConnectionState.Disconnected;
            this.subscriptionState = (int)SubscriptionState.Unsubscribed;
            bridgeLogic.setSubject(this);
            this.bridgeConfig = new RosBridgeUtility.RosBridgeConfig();
            bridgeConfig.readConfig("XMLFile1.xml");
            Console.WriteLine("Ipaddress: {0}",bridgeConfig.ipaddress);
            txtIP.Text = bridgeConfig.ipaddress;
            txtPort.Text = bridgeConfig.port.ToString();
        }
        

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (connectionState == (int)(ConnectionState.Disconnected))
                {
                    if (ConnectToRos())
                    {
                        connectionState = (int)ConnectionState.Connected;
                        btnConnect.Background = Brushes.LightGreen;
                        btnConnect.Content = "Disconnect";
                        stackControls.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                else
                {
                    DisconnectFromRos();
                    connectionState = (int)ConnectionState.Disconnected;
                    btnConnect.Background = Brushes.OrangeRed;
                    btnConnect.Content = "Connect";
                    stackControls.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch (Exception se)
            {
                MessageBox.Show("Socket exception: {0}", se.Data.ToString());
            }
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            if (subscriptionState == (int)SubscriptionState.Unsubscribed)
            {
                try
                {
                    foreach (var item in bridgeConfig.getTopicList())
                    {
                        bridgeLogic.sendSubscription(item.name,item.throttle);
                    }
                    subscriptionState = (int)SubscriptionState.Subscribed;
                    btnSubscribe.Content = "Unsubscribe";
                    bridgeLogic.SetUpdateListener();                    
                }
                catch (Exception se)
                {
                    MessageBox.Show(se.Message);
                }
            }
            else
            {
                try
                {
                    foreach (var item in bridgeConfig.getTopicList())
                    {
                        bridgeLogic.sendUnsubscribe(item.name);
                    }
                    //bridgeLogic.sendUnsubscribe("/turtle1/pose");
                    subscriptionState = (int)SubscriptionState.Unsubscribed;
                    btnSubscribe.Content = "Subscribe";
                }
                catch (Exception se)
                {
                    MessageBox.Show(se.Message);
                }
            }
        }

        private double convertTextBlocktoRadians()
        {
            double deg = Double.Parse(txtTheta.Text);
            return deg * Math.PI / 180.0;
        }

        private void moveForward()
        {
            bridgeLogic.moveTarget(Double.Parse(txtLgth.Text), 0,
                bridgeConfig.target, bridgeConfig.getPublicationList());
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            moveForward();
        }

        private void moveBackward()
        {
            bridgeLogic.moveTarget(-Double.Parse(txtLgth.Text), 0,
                bridgeConfig.target, bridgeConfig.getPublicationList());
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            //PublishturtleMessage(-Double.Parse(txtLgth.Text), 0);
            moveBackward();
        }

        private void moveLeft()
        {
            bridgeLogic.moveTarget(Double.Parse(txtLgth.Text), convertTextBlocktoRadians(),
                bridgeConfig.target, bridgeConfig.getPublicationList());
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            //PublishturtleMessage(Double.Parse(txtLgth.Text), convertTextBlocktoRadians());
            moveLeft();
        }

        private void moveRight()
        {
            bridgeLogic.moveTarget(Double.Parse(txtLgth.Text), -convertTextBlocktoRadians(),
                bridgeConfig.target, bridgeConfig.getPublicationList());
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            moveRight();
            //PublishturtleMessage(Double.Parse(txtLgth.Text), -convertTextBlocktoRadians());
            //Update();
        }
        /// <summary>
        /// Return true if connection was succesful
        /// </summary>
        /// <returns></returns>
        /// 

        

        bool ConnectToRos()
        {
            if (txtIP.Text == "" || txtPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server\n");
                return false;
            }
            
            try
            {
                bridgeLogic.Initialize(bridgeConfig.protocol+ "://" + txtIP.Text + ":" + txtPort.Text, this);
                //bridgeLogic.Initialize(bridgeConfig.URI);
                bridgeLogic.Connect();
                return bridgeLogic.getConnectionState();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }
        void DisconnectFromRos()
        {
            bridgeLogic.Disconnect();
        }
                
        private void PublishturtleMessage(double linear, double angular)
        {
            try
            {
                var v = new { linear = linear, angular = angular };
                Object[] lin = { linear, 0.0, 0.0 };
                Object[] ang = { 0.0, 0.0, angular };
                foreach (var item in bridgeConfig.getPublicationList())
                {
                    bridgeLogic.PublishTwistMsg(item, lin, ang);
                }
                
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public delegate void UpdateTextElements(String data);

        public void laserScanCanvas(List<JToken> data, Double inc_angle, Double min_angle)
        {
            sensor_projection.Children.Clear();
            laser_field.Children.Clear();
            Polyline plot = new Polyline();
            Polyline laser_segment = new Polyline();
            plot.Stroke = System.Windows.Media.Brushes.MediumVioletRed;
            laser_segment.Stroke = System.Windows.Media.Brushes.DarkViolet;
            plot.StrokeThickness = 1;
            laser_segment.StrokeThickness = 1;
            PointCollection points = new PointCollection();
            PointCollection field = new PointCollection();
            //var converter = TypeDescriptor.GetConverter(typeof(Double));
            Double x = 0;
            Double currentAngle = min_angle;
            Double min_val = 0;            
            foreach (var item in data)
            {
                System.Diagnostics.Debug.WriteLine(item);
                Double yVal;
                Double.TryParse(item.ToString(), NumberStyles.Number, 
                    CultureInfo.CreateSpecificCulture("en-US"),
                    out yVal);                
                points.Add(new Point(x,10*yVal));
                field.Add(new Point(20*yVal * Math.Cos(currentAngle), 20*yVal * Math.Sin(currentAngle)));
                x+= 1;
                currentAngle += inc_angle;
                if (yVal < min_val)
                {
                    min_val = yVal;
                }
            }
            plot.Points = points;
            laser_segment.Points = field;
            sensor_projection.Children.Add(plot);
            laser_field.Children.Add(laser_segment);
        }

        private static string showState = "\"/DriveStates\"";
        //private static string showState = "\"/turtle1/pose\"";
        private static string laserScan = "/sick_s300/scan";
        //private static string laserScan = "/scan_front";

        private String valueToView(JObject jsonData, String attr)
        {
            String result = "";
            try
            {
                var topic = Array.ConvertAll(jsonData["msg"][attr].ToArray(), o => (double)o);
                foreach (var item in topic)
                {
                    result += item.ToString() + "\t";
                }
            }
            catch (InvalidOperationException)
            {
                result = jsonData["msg"][attr].ToString();
            }
            return result;
        }

        private void pushView(JObject jsonData)
        {
            if (jsonData["topic"].ToString().Equals(showState))
            {
                Dispatcher.Invoke(new Action(() => labelX.Content = "x: " + valueToView(jsonData, bridgeConfig.ProjectedAttributes()[0].Item2)));
                Dispatcher.Invoke(new Action(() => labelY.Content = "y: " + valueToView(jsonData, bridgeConfig.ProjectedAttributes()[1].Item2)));
            }
            else if (jsonData["topic"].ToString().Replace("\"", "").Equals(laserScan))
            {
                /*
                var x1 = jsonData["msg"]["ranges"].ToList();
                foreach (var itemx in ((IList<JToken>)x1))
                {
                    Console.WriteLine(itemx);
                }
                */
                Double angle_inc;
                Double min_angle;
                Double.TryParse(jsonData["msg"]["angle_increment"].ToString(),
                    NumberStyles.Number,
                    CultureInfo.CreateSpecificCulture("en-US"), 
                    out angle_inc);
                Double.TryParse(jsonData["msg"]["angle_min"].ToString(),
                    NumberStyles.Number,
                    CultureInfo.CreateSpecificCulture("en-US"),
                    out min_angle);

                Dispatcher.Invoke(new Action(() =>
                    laserScanCanvas(jsonData["msg"]["ranges"].ToList(), angle_inc, min_angle)));

            }
        }

        public void ReceiveUpdate(String data)
        {
            JObject jsonData = JObject.Parse(data);
            try
            {
                // Debug messages
                pushView(jsonData);
            }
            catch (ArgumentNullException)
            {
                Console.Out.WriteLine("Received null argument on {0}",jsonData["topic"]);
            }
        }

        private void moveStopped()
        {
            bridgeLogic.moveTarget(0, 0,
                bridgeConfig.target, bridgeConfig.getPublicationList());
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            moveStopped();
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            double vel;
                    
            switch (e.Key)
            {
                case Key.W:
                    lblTargetState.Content = "Target is moving forward";
                    moveForward();
                    break;
                case Key.A:
                    lblTargetState.Content = "Target is moving left";
                    moveLeft();
                    break;
                case Key.D:
                    lblTargetState.Content = "Target is moving right";
                    moveRight();
                    break;
                case Key.S:
                    lblTargetState.Content = "Target is moving backwards";
                    moveBackward();
                    break;
                case Key.X:
                    lblTargetState.Content = "Target is stopped";
                    moveStopped();
                    break;
                case Key.Add:
                    Double.TryParse(txtLgth.Text,out vel);
                    txtLgth.Text = (vel + 0.1).ToString();
                    break;
                case Key.Subtract:
                    Double.TryParse(txtLgth.Text,out vel);
                    txtLgth.Text = (vel - 0.1).ToString();
                    break;
                case Key.K:
                    Double.TryParse(txtTheta.Text, out vel);
                    txtTheta.Text = (vel + 15).ToString();
                    break;
                case Key.L:
                    Double.TryParse(txtTheta.Text, out vel);
                    txtTheta.Text = (vel - 15).ToString();
                    break;
            }
        }
        

    }
}
