using System;
using System.Windows.Forms;
using System.Drawing;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Collections.Generic;

namespace testAlarm
{
    public partial class Form1 : Form
    {
        public static string API_KEY = "";

        private readonly PointLatLng _sampleLatLon = new PointLatLng(48.857607, 2.295577);
        private readonly int PINT = 3;
        private readonly GMapOverlay _markerOverlay = new GMapOverlay("markers");
        private readonly GMapOverlay _polygonOverlay = new GMapOverlay("polygons");
        private readonly GMapOverlay _polygonOverlayRed = new GMapOverlay("polygons");
        private bool _isAlarmOn = false;

        public Form1()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                System.Net.IPHostEntry e1 =
                     System.Net.Dns.GetHostEntry("www.google.com");
            }
            catch
            {
                gMapControl1.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.",
                      "GMap.NET - Demo.WindowsForms", MessageBoxButtons.OK,
                      MessageBoxIcon.Warning);
            }

            gMapControl1.Location = new Point(PINT, PINT);

            // center red cross 제거
            gMapControl1.ShowCenter = false;

            // 오버레이 추가
            gMapControl1.Overlays.Add(_markerOverlay);
            gMapControl1.Overlays.Add(_polygonOverlay);
            gMapControl1.Overlays.Add(_polygonOverlayRed);

            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            gMapControl1.CacheLocation = Application.StartupPath + "data.gmdp";
            GMapProviders.GoogleMap.ApiKey = API_KEY;
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.DragButton = MouseButtons.Left;

            gMapControl1.MaxZoom = 20;
            gMapControl1.MinZoom = 2;
            gMapControl1.Zoom = 16;

            gMapControl1.Position = _sampleLatLon;


            // 타이머
            Timer1.Interval = 500;
            Timer1.Enabled = true;
            Timer1.Start();
        }

        // On
        private void Button1_Click(object sender, EventArgs e)
        {
            _isAlarmOn = true;

            CreateCircleMarker(gMapControl1.Position, 150d);
        }

        // Off
        private void Button2_Click(object sender, EventArgs e)
        {
            _isAlarmOn = false;

            _markerOverlay.Clear();
            _polygonOverlay.Clear();
        }

        private int _redSignInterval = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            _redSignInterval++;

            if (_redSignInterval % 2 == 0)
            {
                if (_isAlarmOn)
                {
                    int width = gMapControl1.Width - 1;
                    int height = gMapControl1.Height - 1;
                    List<PointLatLng> points = new List<PointLatLng>();
                    PointLatLng p1 = gMapControl1.FromLocalToLatLng(0, 0);
                    PointLatLng p2 = gMapControl1.FromLocalToLatLng(0, height);
                    PointLatLng p3 = gMapControl1.FromLocalToLatLng(width, height);
                    PointLatLng p4 = gMapControl1.FromLocalToLatLng(width, 0);
                    points.Add(p1);
                    points.Add(p2);
                    points.Add(p3);
                    points.Add(p4);
                    GMapPolygon polygon = new GMapPolygon(points, "polygon");
                    polygon.Fill = new SolidBrush(Color.FromArgb(120, Color.IndianRed));
                    polygon.Stroke = new Pen(Color.Red, 0);
                    _polygonOverlayRed.Polygons.Add(polygon);
                }
            }
            else
            {
                if (_polygonOverlayRed.Polygons.Count > 0)
                {
                    _polygonOverlayRed.Polygons.Clear();
                }

                if (_redSignInterval > 11)
                {
                    _redSignInterval = -1;
                }
            }
        }

        private void CreateCircleMarker(PointLatLng point, double radius)
        {
            _markerOverlay.Clear();
            _polygonOverlay.Clear();

            //marker
            GMapMarker gMarker = new GMarkerGoogle(point, GMarkerGoogleType.red_dot);
            gMarker.ToolTipMode = MarkerTooltipMode.Always;
            gMarker.ToolTipText = "Alarm";
            gMarker.ToolTip.TextPadding = new Size(5, 3);
            gMarker.ToolTip.Fill = new SolidBrush(Color.DimGray);
            gMarker.ToolTip.Foreground = new SolidBrush(Color.White);
            gMarker.ToolTip.Offset = new Point(10, -30);
            gMarker.ToolTip.Stroke = new Pen(Color.Transparent, .0f);

            _markerOverlay.Markers.Add(gMarker);


            //circle
            int segments = 1080;
            List<PointLatLng> gpollist = new List<PointLatLng>();
            for (int i = 0; i < segments; i++)
            {
                gpollist.Add(FindPointAtDistanceFrom(point, i * (Math.PI / 180), radius / 1000));
            }
            GMapPolygon circle = new GMapPolygon(gpollist, "circle");
            circle.Stroke = new Pen(Color.Red, 1);
            circle.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
            _polygonOverlay.Polygons.Add(circle);
        }

        public static PointLatLng FindPointAtDistanceFrom(PointLatLng startPoint, double initialBearingRadians, double distanceKilometres)
        {
            const double radiusEarthKilometres = 6371.01;
            var distRatio = distanceKilometres / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = DegreesToRadians(startPoint.Lat);
            var startLonRad = DegreesToRadians(startPoint.Lng);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));
            var endLonRads = startLonRad + Math.Atan2(Math.Sin(initialBearingRadians) * distRatioSine * startLatCos, distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new GMap.NET.PointLatLng(RadiansToDegrees(endLatRads), RadiansToDegrees(endLonRads));
        }

        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }

        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }

    }
}
