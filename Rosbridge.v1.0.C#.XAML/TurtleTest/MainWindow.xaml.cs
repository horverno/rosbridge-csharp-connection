using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Configuration;
using System.Linq;
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
using WebSocketSharp.Server;
using WebSocketSharp.Net;
using System.IO;
using Microsoft.Kinect;

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


        private static string showState = "\"/DriveStates\"";
        //private static string showState = "\"/turtle1/pose\"";
        //private static string laserScan = "/sick_s300/scan";
        private static string laserScan = "/base_scan";
        private static string odometry = "/base_odometry/odom";

        private RosBridgeUtility.RotRPY currRot;

        private static double scaleFactor = 10;

        // Kinect specific attributes
        private KinectSensor sensor;
        private WriteableBitmap kinect_rgb;
        private byte[] kinect_pixels;
        // Kinect depth
        private WriteableBitmap kinect_depth;
        private byte[] depth_color;
        private DepthImagePixel[] depth_pixels;
        
        // Kinect specific methods
        public void enumerateKinect()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            if (this.sensor != null)
            {
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.kinect_pixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.kinect_rgb = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
                    this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                // Depth stream
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.depth_pixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                this.depth_color = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];
                this.kinect_depth = new WriteableBitmap(this.sensor.DepthStream.FrameWidth,
                    this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                
                this.kinect_screen.Source = this.kinect_rgb;
                this.kinect_depth_screen.Source = this.kinect_depth;
                this.sensor.ColorFrameReady += sensor_ColorFrameReady;
                this.sensor.DepthFrameReady += sensor_DepthFrameReady;
                try
                {
                    this.sensor.Start();
                    Console.WriteLine("Kinect setup completed");
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyDepthImagePixelDataTo(this.depth_pixels);
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    int colorPixelIndex = 0;

                    for (int i = 0; i < this.depth_pixels.Length; ++i)
                    {
                        short depth = depth_pixels[i].Depth;
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                        this.depth_color[colorPixelIndex++] = intensity;
                        this.depth_color[colorPixelIndex++] = intensity;
                        this.depth_color[colorPixelIndex++] = intensity;
                        ++colorPixelIndex;
                    }
                    this.kinect_depth.WritePixels(
                        new Int32Rect(0, 0, this.kinect_depth.PixelWidth, this.kinect_depth.PixelHeight),
                        this.depth_color, this.kinect_depth.PixelWidth * sizeof(int), 0);
                    BitmapSource source = this.kinect_depth_screen.Source as BitmapSource;
                    wcon.updateDepthState(ref source, source.PixelHeight, source.PixelWidth);
                }
            }
        }

        
        void updateKinectImage()
        {
            BitmapSource source = kinect_screen.Source as BitmapSource;
            wcon.updateRGBState(ref source, source.PixelHeight, source.PixelWidth);
        }

        int kinectread=0;

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(this.kinect_pixels);
                    this.kinect_rgb.WritePixels(
                        new Int32Rect(0, 0, this.kinect_rgb.PixelWidth, this.kinect_rgb.PixelHeight),
                        this.kinect_pixels,
                        this.kinect_rgb.PixelWidth * sizeof(int), 0);

                    BitmapSource source = this.kinect_screen.Source as BitmapSource;
                    wcon.updateRGBState(ref source, source.PixelHeight, source.PixelWidth);
                    kinectread++;
                }
            }
        }

        // Web controller
        private WebController wcon;

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
            //bridgeLogic.currentVelocityState = new RosBridgeUtility.VelocityState();
            stackControls.Visibility = System.Windows.Visibility.Hidden;
            this.DataContext = _responseObj;
            this.bridgeLogic = new RosBridgeUtility.RosBridgeLogic();
            this.connectionState = (int)ConnectionState.Disconnected;
            this.subscriptionState = (int)SubscriptionState.Unsubscribed;
            bridgeLogic.setSubject(this);
            this.bridgeConfig = new RosBridgeUtility.RosBridgeConfig();
            bridgeConfig.readConfig("XMLFile1.xml");
            Console.WriteLine("Ipaddress: {0}",bridgeConfig.ipaddress);
            bridgeLogic.currentTarget = bridgeConfig.target;
            bridgeLogic.initVelocityThreshold(bridgeConfig.min_vel, bridgeConfig.max_vel, 
                bridgeConfig.inc_vel, bridgeConfig.init_vel);
            bridgeLogic.initAngularVelocityThreshold(bridgeConfig.min_ang, bridgeConfig.max_ang,
                bridgeConfig.inc_ang, bridgeConfig.init_ang);
            txtLgth.Text = bridgeLogic.current_velocity.ToString();
            txtIP.Text = bridgeConfig.ipaddress;
            txtPort.Text = bridgeConfig.port.ToString();
            txtTheta.Text = bridgeLogic.current_angVelocity.ToString();
            bridgeLogic.currentVelocityState.inc_vel = bridgeConfig.inc_vel;
            bridgeLogic.currentVelocityState.max_vel = bridgeConfig.max_vel;
            bridgeLogic.currentVelocityState.min_vel = bridgeConfig.min_vel;
            bridgeLogic.currentVelocityState.current_vel = bridgeConfig.init_vel;
            bridgeLogic.currentVelocityState.currentTheta = bridgeConfig.init_ang;
            try
            {
                showState = bridgeConfig.showState;
                laserScan = bridgeConfig.laserFieldTopic;
                odometry = bridgeConfig.odometryTopic;
                scaleFactor = bridgeConfig.vis_scaleFactor;
                wcon = new WebController(bridgeConfig, bridgeLogic);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Error during read: {0}",e.Data);
            }
            wcon.updateVelocityState(bridgeLogic.currentVelocityState);
            var u1 = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory,"test1.jpg"));
            BitmapImage img1 = new BitmapImage(u1);

            kinect_screen.Source = img1;

            BitmapSource source = kinect_screen.Source as BitmapSource;
            //NeobotixStateServer = new WebSocketServer("ws://localhost:9091");
            wcon.updateRGBState(ref source, img1.PixelHeight, img1.PixelWidth);
            source = kinect_depth_screen.Source as BitmapSource;
            //NeobotixStateServer = new WebSocketServer("ws://localhost:9091");
            //wcon.updateDepthState(ref source, img1.PixelHeight, img1.PixelWidth);
            // Kinect refresher timer
            
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
                        wcon.startWebServer();
                    }
                }
                else
                {
                    DisconnectFromRos();
                    connectionState = (int)ConnectionState.Disconnected;
                    btnConnect.Background = Brushes.OrangeRed;
                    btnConnect.Content = "Connect";
                    stackControls.Visibility = System.Windows.Visibility.Hidden;
                    wcon.stopWebServer();
                }                
            }
            catch (System.Net.Sockets.SocketException se)
            {
                MessageBox.Show(se.ToString());
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
            bridgeLogic.setCurrentVelocity(Double.Parse(txtLgth.Text));
            bridgeLogic.moveForward(bridgeConfig.getPublicationList());
            updateVelocityState();            
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            moveForward();
        }

        private void moveBackward()
        {

            bridgeLogic.setCurrentVelocity(Double.Parse(txtLgth.Text));
            bridgeLogic.moveBackward(bridgeConfig.getPublicationList());
            updateVelocityState();            
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            moveBackward();
        }

        private void moveLeft()
        {
            bridgeLogic.setCurrentVelocity(Double.Parse(txtLgth.Text));
            bridgeLogic.current_angVelocity = convertTextBlocktoRadians();
            bridgeLogic.moveLeft(bridgeConfig.getPublicationList());
            updateVelocityState();
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            moveLeft();
        }

        private void moveRight()
        {
            bridgeLogic.setCurrentVelocity(Double.Parse(txtLgth.Text));
            bridgeLogic.current_angVelocity = convertTextBlocktoRadians();
            bridgeLogic.moveRight(bridgeConfig.getPublicationList());
            updateVelocityState();            
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            moveRight();            
        }
        /// <summary>
        /// Return true if connection was succesful
        /// </summary>
        /// <returns></returns>
        /// 
        public void updateVelocityState()
        {
            bridgeLogic.currentVelocityState.current_vel = bridgeLogic.current_velocity;
            bridgeLogic.currentVelocityState.currentTheta = bridgeLogic.current_angVelocity;
            Console.WriteLine(bridgeLogic.currentVelocityState.current_vel);
            wcon.updateVelocityState(bridgeLogic.currentVelocityState);
        }             
        

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
            catch (System.Net.Sockets.SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public delegate void UpdateTextElements(String data);

        private Polyline laser_segment = new Polyline();
        private Polyline plot = new Polyline();

        public void laserScanCanvas(List<JToken> data, Double inc_angle, Double min_angle)
        {
            sensor_projection.Children.Clear();
            laser_field.Children.Remove(plot);
            laser_field.Children.Remove(laser_segment);
            laser_segment = new Polyline();
            plot = new Polyline();
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
                points.Add(new Point(x,scaleFactor*yVal));
                field.Add(new Point(scaleFactor * yVal * Math.Cos(currRot.yaw + currentAngle), 
                    scaleFactor * yVal * Math.Sin(currRot.yaw + currentAngle)));
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
            Canvas.SetLeft(laser_segment, last_posX);
            Canvas.SetTop(laser_segment, last_posY);
            laser_field.Children.Add(laser_segment);
        }

        

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

        private double lastMeasuredDistance = 0;
        private double lastMeasuredAngle = 0;
        private Ellipse predictedPosition = new Ellipse();
        private Line orientationCursor = new Line();
        private PointCollection odometryData = new PointCollection();
        private Polyline odometryLine = new Polyline();
        private int odometryPointCnt = 0;

        private double last_posX = 0;
        private double last_posY = 0;
        private double new_posX = 0;
        private double new_posY = 0;
        // Velocity data
        private double last_posX_dot = 0;
        private double last_posY_dot = 0;
        private double new_posX_dot = 0;
        private double new_posY_dot = 0;

        private void visualizeOdometry(Double newDistance, Double newAngle)
        {
            double diffDistance = newDistance - lastMeasuredDistance;
            double diffAngle = newAngle - lastMeasuredAngle;
            lastMeasuredDistance += diffDistance;
            lastMeasuredAngle += diffAngle;
            laser_field.Children.Remove(predictedPosition);
            laser_field.Children.Remove(orientationCursor);
            laser_field.Children.Remove(odometryLine);
            predictedPosition = new Ellipse();
            orientationCursor = new Line();
            predictedPosition.Width = 20;
            predictedPosition.Height = 20;
            
            predictedPosition.Stroke = System.Windows.Media.Brushes.DarkMagenta;
            predictedPosition.StrokeThickness = 2;
            laser_field.Children.Add(predictedPosition);
            orientationCursor.Stroke = System.Windows.Media.Brushes.Indigo;
            orientationCursor.StrokeThickness = 10;
            orientationCursor.X1 = 0; orientationCursor.Y1 = 0;
            orientationCursor.X2 = scaleFactor * 4 * Math.Cos(lastMeasuredAngle);
            orientationCursor.Y2 = scaleFactor * 4 * Math.Sin(lastMeasuredAngle);
            laser_field.Children.Add(orientationCursor);
            new_posX += 10 * Math.Cos(lastMeasuredAngle) * diffDistance;
            new_posY += 10 * Math.Sin(lastMeasuredAngle) * diffDistance;
            new_posX_dot = new_posX - last_posX;
            new_posY_dot = new_posY - last_posY;
            Console.WriteLine("Distance function: {0}",
                Math.Sqrt(Math.Pow(new_posX - last_posX, 2) + Math.Pow(new_posY - last_posY, 2)));
            Console.WriteLine("Acceleration, velocity: {0} {1}", new_posX_dot, new_posX_dot - last_posX_dot);
            if (
                Math.Sqrt(Math.Pow(new_posX-last_posX,2)+Math.Pow(new_posY-last_posY,2)) > 0.1
                &&
                Math.Sqrt(Math.Pow(new_posX_dot - last_posX_dot, 2) + 
                Math.Pow(new_posY_dot - last_posY_dot, 2)) > 0.1)
            {
                odometryData.Add(new Point(last_posX, last_posY));
                odometryLine = new Polyline();
                odometryLine.Stroke = System.Windows.Media.Brushes.MediumVioletRed;
                odometryLine.StrokeThickness = 2;
                odometryLine.Points = odometryData;
            }
            else
            {
                odometryPointCnt++;
            }
            // Refresh curve values
            last_posX = new_posX;
            last_posY = new_posY;
            last_posX_dot = new_posX_dot;
            last_posY_dot = new_posY_dot;
            /*
            Console.WriteLine("(X,Y): {0} {1}",last_posX,last_posY);
            Console.WriteLine("(dist,angle): {0} {1}",diffDistance,diffAngle);
            Console.WriteLine("(dist,angle): {0} {1}", lastMeasuredDistance, lastMeasuredAngle);
            */
            laser_field.Children.Add(odometryLine);
            //Console.WriteLine("{0},{1}", last_posX, last_posY);
            Canvas.SetLeft(predictedPosition, last_posX-5);
            Canvas.SetTop(predictedPosition, last_posY-5);
            Canvas.SetLeft(orientationCursor, last_posX);
            Canvas.SetTop(orientationCursor, last_posY);
            
        }

        private void visualizeOdometry(Double x, Double y, Double newAngle)
        {
            
            laser_field.Children.Remove(predictedPosition);
            laser_field.Children.Remove(orientationCursor);
            laser_field.Children.Remove(odometryLine);
            

            predictedPosition = new Ellipse();
            orientationCursor = new Line();

            predictedPosition.Width = 20;
            predictedPosition.Height = 20;
            predictedPosition.Stroke = System.Windows.Media.Brushes.DarkMagenta;
            predictedPosition.StrokeThickness = 2;

            orientationCursor.Stroke = System.Windows.Media.Brushes.Indigo;
            orientationCursor.StrokeThickness = 10;
            orientationCursor.X1 = 0; orientationCursor.Y1 = 0;
            orientationCursor.X2 = scaleFactor * 10 * Math.Cos(newAngle);
            orientationCursor.Y2 = scaleFactor * 10 * Math.Sin(newAngle);
            laser_field.Children.Add(predictedPosition);
            laser_field.Children.Add(orientationCursor);
            double neobot_scale = 10;
            double offsetX = 100;
            double offsetY = 100;
            double posX = scaleFactor * neobot_scale * x + offsetX;
            double posY = scaleFactor * neobot_scale * y + offsetY;
            
            odometryData.Add(new Point(posX, posY));
            odometryLine = new Polyline();
            odometryLine.Stroke = System.Windows.Media.Brushes.MediumVioletRed;
            odometryLine.StrokeThickness = 2;
            odometryLine.Points = odometryData;
            // Refresh curve values
            
            laser_field.Children.Add(odometryLine);
            Canvas.SetLeft(predictedPosition, posX - 5);
            Canvas.SetTop(predictedPosition, posY - 5);
            Canvas.SetLeft(orientationCursor, posX);
            Canvas.SetTop(orientationCursor, posY);
            last_posX = posX;
            last_posY = posY;
        }

        int odometryCount = 0;

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
                if (jsonData["msg"]["angle_increment"].ToString().Contains(","))
                {
                    Double.TryParse(jsonData["msg"]["angle_increment"].ToString(),
                        NumberStyles.Number,
                        CultureInfo.CreateSpecificCulture("hu-HU"),
                        out angle_inc);
                    Double.TryParse(jsonData["msg"]["angle_min"].ToString(),
                        NumberStyles.Number,
                        CultureInfo.CreateSpecificCulture("hu-HU"),
                        out min_angle);
                }
                else
                {
                    Double.TryParse(jsonData["msg"]["angle_increment"].ToString(),
                        NumberStyles.Number,
                        CultureInfo.CreateSpecificCulture("en-US"),
                        out angle_inc);
                    Double.TryParse(jsonData["msg"]["angle_min"].ToString(),
                        NumberStyles.Number,
                        CultureInfo.CreateSpecificCulture("en-US"),
                        out min_angle);
                }
                Dispatcher.Invoke(new Action(() =>
                    laserScanCanvas(jsonData["msg"]["ranges"].ToList(), angle_inc, min_angle)));

            }
            else if (jsonData["topic"].ToString().Replace("\"", "").Equals(odometry))
            {
                odometryCount++;
                Double measuredDistance = 0;
                Double measuredAngle = 0;
                if (bridgeConfig.target == "neobotix_mp500" || bridgeConfig.target == "pr2")
                {
                    /*Console.WriteLine(Double.TryParse(jsonData["msg"]["pose"]["pose"].ToString(),
                        NumberStyles.Number,
                        CultureInfo.CreateSpecificCulture("en-US"),
                        out measuredDistance));
                    */
                    Double x, y, angleX, angleY, angleZ, angleW = 0;
                    if (jsonData["msg"]["pose"]["pose"]["position"]["x"].ToString().Contains(","))
                    {
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["position"]["x"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out x);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["position"]["y"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out y);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["x"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out angleX);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["y"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out angleY);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["z"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out angleZ);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["w"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out angleW);
                    }
                    else
                    {
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["position"]["x"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out x);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["position"]["y"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out y);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["x"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out angleX);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["y"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out angleY);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["z"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out angleZ);
                        Double.TryParse(jsonData["msg"]["pose"]["pose"]["orientation"]["w"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out angleW);
                    }
                    if (odometryCount % 100 == 0)
                    {
                        //Console.WriteLine("X: {0}, Y: {1}, Z: {2}, W: {3}", angleX, angleY, angleZ, angleW);
                        currRot = bridgeLogic.convertQuaternionToEuler(angleX, angleY, angleZ, angleW);
                        //Console.WriteLine("Roll: {0}, Pitch: {1}, Yaw: {2}", currRot.roll, currRot.pitch, currRot.yaw);
                        Dispatcher.Invoke(new Action(() => visualizeOdometry(x, y, currRot.yaw)));
                    }
                }
                else
                {
                    if (jsonData["msg"]["distance"].ToString().Contains(","))
                    {
                        Double.TryParse(jsonData["msg"]["distance"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out measuredDistance);
                        Double.TryParse(jsonData["msg"]["angle"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("de-DE"),
                            out measuredAngle);
                        Dispatcher.Invoke(new Action(() => visualizeOdometry(measuredDistance, measuredAngle)));
                    }
                    {
                        Double.TryParse(jsonData["msg"]["distance"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out measuredDistance);
                        Double.TryParse(jsonData["msg"]["angle"].ToString(),
                            NumberStyles.Number,
                            CultureInfo.CreateSpecificCulture("en-US"),
                            out measuredAngle);
                        Dispatcher.Invoke(new Action(() => visualizeOdometry(measuredDistance, measuredAngle)));
                    }
                }
                
            }
        }

        

        public void ReceiveUpdate(String data)
        {
            JObject jsonData = JObject.Parse(data);
            try
            {
                // Debug messages
                pushView(jsonData);
                wcon.updateMessages(jsonData);
            }
            catch (ArgumentNullException)
            {
                Console.Out.WriteLine("Received null argument on {0}",jsonData["topic"]);
            }
        }

        private void moveStopped()
        {
            bridgeLogic.stopTarget(bridgeConfig.getPublicationList());
            updateVelocityState();
            /*
            bridgeLogic.moveTarget(0, 0,
                bridgeConfig.target, bridgeConfig.getPublicationList());
             * */
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            moveStopped();
        }

        private double radToDeg(double rad)
        {
            return rad * 180 / Math.PI;
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
                    bridgeLogic.increaseVelocity();
                    updateVelocityState();            
                    txtLgth.Text = bridgeLogic.current_velocity.ToString();
                    //txtLgth.Text = (vel + 0.1).ToString();
                    break;
                case Key.Subtract:
                    Double.TryParse(txtLgth.Text,out vel);
                    bridgeLogic.decreaseVelocity();
                    updateVelocityState();
                    txtLgth.Text = bridgeLogic.current_velocity.ToString();
                    //txtLgth.Text = (vel - 0.1).ToString();
                    break;
                case Key.K:
                    Double.TryParse(txtTheta.Text, out vel);
                    //txtTheta.Text = (vel + 15).ToString();
                    bridgeLogic.increaseAngVelocity();
                    txtTheta.Text = radToDeg(bridgeLogic.current_angVelocity).ToString();
                    break;
                case Key.L:
                    Double.TryParse(txtTheta.Text, out vel);
                    //txtTheta.Text = (vel - 15).ToString();
                    bridgeLogic.decreaseAngVelocity();
                    txtTheta.Text = radToDeg(bridgeLogic.current_angVelocity).ToString();
                    break;
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            enumerateKinect();
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
        }


        

    }
}
