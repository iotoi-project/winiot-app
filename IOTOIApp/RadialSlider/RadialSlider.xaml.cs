using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IOTOIApp
{
    public sealed partial class RadialSlider : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(double), typeof(RadialSlider), new PropertyMetadata(null, new PropertyChangedCallback(OnMinValueChanged)));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set
            {
                SetValue(MinimumProperty, value); OnPropertyChanged();
            }
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialSlider rs = d as RadialSlider; //null checks omitted
            double v = Convert.ToDouble(e.NewValue); //null checks omitted

            rs.Minimum = v;
        }


        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(double), typeof(RadialSlider), new PropertyMetadata(null, new PropertyChangedCallback(OnMaxValueChanged)));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set
            {
                SetValue(MaximumProperty, value); OnPropertyChanged();
            }
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialSlider rs = d as RadialSlider; //null checks omitted
            double v = Convert.ToDouble(e.NewValue); //null checks omitted

            rs.Maximum = v;
        }


        public static readonly DependencyProperty TickProperty = DependencyProperty.Register(
            "Tick", typeof(double), typeof(RadialSlider), new PropertyMetadata(null, new PropertyChangedCallback(OnTickValueChanged)));
        public double Tick
        {
            get { return (double)GetValue(TickProperty); }
            set
            {
                SetValue(TickProperty, value); OnPropertyChanged();
            }
        }

        private static void OnTickValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialSlider rs = d as RadialSlider; //null checks omitted
            double v = Convert.ToDouble(e.NewValue); //null checks omitted

            rs.Tick = v;
        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(RadialSlider), new PropertyMetadata(0, new PropertyChangedCallback(OnValueChanged)));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value); OnPropertyChanged();

                PinPosition = GetPointFromDistanceAngle((PinAngleTick * (Value - Minimum)) + 180, 96, new Point(110, 110));
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialSlider rs = d as RadialSlider; //null checks omitted
            double v = Convert.ToDouble(e.NewValue); //null checks omitted

            rs.Value = v;
        }


        public static readonly DependencyProperty PinBrushProperty = DependencyProperty.Register(
            "PinBrush", typeof(Brush), typeof(RadialSlider), new PropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnPinBrushChanged)));
        public Brush PinBrush
        {
            get { return GetValue(PinBrushProperty) as Brush; }
            set
            {
                SetValue(PinBrushProperty, value); OnPropertyChanged();
                Pin.Fill = value;
            }
        }

        private static void OnPinBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialSlider rs = d as RadialSlider; //null checks omitted
            Brush fb = e.NewValue as Brush; //null checks omitted

            rs.PinBrush = fb;
        }


        public static readonly DependencyProperty UpdateProperty = DependencyProperty.Register(
            "Update", typeof(bool), typeof(RadialSlider), new PropertyMetadata(false, new PropertyChangedCallback(OnUpdateChanged)));
        public bool Update
        {
            get { return (bool)GetValue(UpdateProperty); }
            set
            {
                SetValue(UpdateProperty, value); OnPropertyChanged();

                if (value)
                    DrawPins();
            }
        }

        private static void OnUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialSlider rs = d as RadialSlider; //null checks omitted
            bool v = Convert.ToBoolean(e.NewValue); //null checks omitted

            rs.Update = v;
        }




        Polygon PolygonFill = new Polygon();
        Ellipse Pin = new Ellipse() { Width = 15, Height = 15, Fill = new SolidColorBrush(Colors.White) };

        double PinAngleTick { get { return 180d / (Maximum - Minimum); } }

        Point pinPosition;
        Point PinPosition
        {
            get { return pinPosition; }
            set
            {
                pinPosition = value; OnPropertyChanged();

                Pin.Visibility = (0 < value.X && 0 < value.Y) ? Visibility.Visible : Visibility.Collapsed;
                Pin.SetValue(Canvas.LeftProperty, value.X - 8);
                Pin.SetValue(Canvas.TopProperty, value.Y - 8);

                FillValue();
            }
        }

        List<CheckPin> CheckPoints = new List<CheckPin>();
        List<ScalePin> ScalePoints = new List<ScalePin>();


        public RadialSlider()
        {
            this.InitializeComponent();

            ContentArea.PointerReleased += Slider_PointerReleased;
            this.PointerExited += RadialSlider_PointerExited;

            this.Loaded += RadialSlider_Loaded;
        }


        private void RadialSlider_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= RadialSlider_Loaded;

            DrawBaseLine();
            DrawPins();
        }


        void DrawBaseLine()
        {
            var points = new PointCollection();

            points.Add(new Point(10, 110));

            //for (int i = 181; i <= 359; ++i)
            //{
            //    points.Add(GetPointFromDistanceAngle(i, 100, new Point(110, 110)));
            //}
            MakePathPoint(points, 181, 359, 100);

            points.Add(new Point(210, 110));

            points.Add(new Point(202, 110));

            //for (int i = 359; i >= 181; --i)
            //{
            //    points.Add(GetPointFromDistanceAngle(i, 92, new Point(110, 110)));
            //}
            MakePathPoint(points, 359, 181, 92);

            points.Add(new Point(18, 110));

            var PolygonBase = new Polygon();
            PolygonBase.Points = points;
            PolygonBase.Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0xa6, 0xa6, 0xa6));

            LinearGradientBrush gradient = new LinearGradientBrush() { StartPoint = new Point(0, 1), EndPoint = new Point(1, 1) };
            gradient.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0xFF, 0xF7, 0xAA, 0x27), Offset = 0 });
            gradient.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0xFF, 0xF9, 0x87, 0x1c), Offset = 0.25 });
            gradient.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0xFF, 0xFC, 0x58, 0x0C), Offset = 0.75 });
            gradient.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(0xFF, 0xFF, 0x32, 0x00), Offset = 1 });
            PolygonFill.Fill = gradient;

            ContentArea.Children.Add(PolygonBase);
            ContentArea.Children.Add(PolygonFill);
        }


        public void DrawPins()
        {
            if (0 < Minimum && 0 < Maximum && Minimum < Maximum)
            {
                ContentArea.Children.Remove(Pin);

                DrawScalePoints();

                PinPosition = GetPointFromDistanceAngle((PinAngleTick * (Value - Minimum)) + 180, 96, new Point(110, 110));
                ContentArea.Children.Add(Pin);

                DrawCheckPoints();
            }

            if (Update)
                Update = !Update;
        }



        void FillValue()
        {
            var points = new PointCollection();

            points.Add(new Point(10, 110));

            MakePathPoint(points, 181, Convert.ToInt32(PinAngleTick * (Value - Minimum)) + 180, 100);
            MakePathPoint(points, Convert.ToInt32(PinAngleTick * (Value - Minimum)) + 180, 181, 92);

            points.Add(new Point(18, 110));

            PolygonFill.Points.Clear();
            PolygonFill.Points = points;
        }


        void DrawScalePoints()
        {
            foreach (ScalePin p in ScalePoints)
            {
                ContentArea.Children.Remove(p);
            }
            ScalePoints.Clear();

            ScalePin ssp = new ScalePin();
            ssp.RenderTransform = new RotateTransform() { Angle = 180.5, CenterX = 110, CenterY = 110 };

            ScalePoints.Add(ssp);
            ContentArea.Children.Add(ssp);

            for (int i = 1; i <= Convert.ToInt32(Maximum - Minimum); i += Convert.ToInt32(Tick))
            {
                ScalePin nsp = new ScalePin();
                nsp.RenderTransform = new RotateTransform() { Angle = 180 + PinAngleTick * i, CenterX = 110, CenterY = 110 };

                ScalePoints.Add(nsp);
                ContentArea.Children.Add(nsp);
            }
        }

        void DrawCheckPoints()
        {
            foreach (CheckPin p in CheckPoints)
            {
                p.PinCheck.PointerPressed -= PinCheck_PointerPressed;
                p.PinCheck.PointerEntered -= NP_PointerEntered;

                ContentArea.Children.Remove(p);
            }
            CheckPoints.Clear();

            for (int i = 0; i <= Convert.ToInt32(Maximum - Minimum); i += Convert.ToInt32(Tick))
            {
                CheckPin np = new CheckPin();
                np.RenderTransform = new RotateTransform() { Angle = 180 + PinAngleTick * i, CenterX = 110, CenterY = 110 };
                np.PinCheck.Tag = Minimum + i;

                np.PinCheck.PointerPressed += PinCheck_PointerPressed;
                np.PinCheck.PointerEntered += NP_PointerEntered;

                CheckPoints.Add(np);
                ContentArea.Children.Add(np);
            }
        }


        bool OnPointer = false;
        private void PinCheck_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnPointer = true;
        }


        private void NP_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (OnPointer)
            {
                Rectangle target = sender as Rectangle;

                double v = Convert.ToDouble(target.Tag);

                Value = v;

                //target.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
            }
        }


        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            OnPointer = false;
        }


        private void RadialSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            OnPointer = false;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        void MakePathPoint(PointCollection pc, int start, int end, int distance)
        {
            if (end > start)
            {
                for (int i = start; i <= end; ++i)
                {
                    pc.Add(GetPointFromDistanceAngle(i, distance, new Point(110, 110)));
                }
            }
            else
            {
                for (int i = start; i >= end; --i)
                {
                    pc.Add(GetPointFromDistanceAngle(i, distance, new Point(110, 110)));
                }
            }
        }

        public double DistanceToLine(Point lineStart, Point lineEnd, Point point)
        {
            double xDelta = lineEnd.X - lineStart.X;
            double yDelta = lineEnd.Y - lineStart.Y;

            double u = ((point.X - lineStart.X) * xDelta + (point.Y - lineStart.Y) * yDelta) / (xDelta * xDelta + yDelta * yDelta);

            Point closestPoint;
            if (u < 0)
                closestPoint = lineStart;
            else if (u > 1)
                closestPoint = lineEnd;
            else
                closestPoint = new Point(lineStart.X + u * xDelta, lineStart.Y + u * yDelta);

            return DistanceBetweenTwoPoints(closestPoint, point);  // closestPoint.DistanceTo(point);
        }

        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="angle">The degree value to convert</param>
        /// <returns>Radian value</returns>
        public double DegreesToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public double RadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        /// <summary>
        /// Get the distance, in pixels between two points.
        /// </summary>
        public double DistanceBetweenTwoPoints(Point pt1, Point pt2)
        {

            double a = pt2.X - pt1.X;
            double b = pt2.Y - pt1.Y;
            return Math.Sqrt(a * a + b * b);
        }


        /// <summary>
        /// A utility function to get a point that is a given distance and angle from another point.
        /// </summary>
        /// <param name="angle">The angle to travel</param>
        /// <param name="distance">The distance (in pixels) to go</param>
        /// <param name="ptStart">The start point</param>
        /// <returns></returns>
        public Point GetPointFromDistanceAngle(double angle, double distance, Point ptStart)
        {
            double theta = angle * 0.0174532925;
            Point p = new Point();
            p.X = ptStart.X + distance * Math.Cos(theta);
            p.Y = ptStart.Y + distance * Math.Sin(theta);
            return p;
        }
    }
}
