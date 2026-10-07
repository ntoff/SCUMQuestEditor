using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SCUMQuestEditor.Models;

namespace SCUMQuestEditor
{
    public partial class MapViewDialog : Window
    {
        private const double xMin = -904800.0;
        private const double xMax = 619200.0;
        private const double yMin = -904800.0;
        private const double yMax = 619200.0;
        private const double mapRange = 1524000.0;

        private bool _isDragging = false;
        private Point _lastMousePosition;
        private BitmapImage _bitmap = null!;
        private Action<double, double, double>? _onCoordinatesSelected;
        private List<MapLocation>? _mapLocations = null;

        public MapViewDialog(Action<double, double, double> onCoordinatesSelected, List<MapLocation>? locations = null)
        {
            InitializeComponent();
            Loaded += MapViewDialog_Loaded;
            SizeChanged += MapViewDialog_SizeChanged;
            _onCoordinatesSelected = onCoordinatesSelected;
            _mapLocations = locations;
            LoadMapImage();
        }

        private void MapViewDialog_Loaded(object sender, RoutedEventArgs e)
        {
            FitMapToWindow();
            //CenterOnOrigin();
            RenderLocations();
        }

        private void FitMapToWindow()
        {
            if (_bitmap == null) return;

            double dpiScaleX = _bitmap.DpiX / 96.0;
            double dpiScaleY = _bitmap.DpiY / 96.0;

            double imageWidth = _bitmap.PixelWidth * dpiScaleX;
            double imageHeight = _bitmap.PixelHeight * dpiScaleY;

            double scaleX = Width / imageWidth;
            double scaleY = Height / imageHeight;
            double fitScale = Math.Min(scaleX, scaleY);

            ScaleTransform.ScaleX = fitScale;
            ScaleTransform.ScaleY = fitScale;

            UpdateZoomDisplay();

            TranslateTransform.X = Width / 2.0 - fitScale * imageWidth / 2.0;
            TranslateTransform.Y = Height / 2.0 - fitScale * imageHeight / 2.0;
        }

        private void MapViewDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged || e.HeightChanged)
                CenterOnOrigin();
        }

        private void LoadMapImage()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "SCUMQuestEditor._data.scum_map.png";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                MessageBox.Show($"Failed to load embedded resource: {resourceName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            using var memStream = new MemoryStream();
            stream.CopyTo(memStream);
            memStream.Position = 0;

            _bitmap = new BitmapImage();
            _bitmap.BeginInit();
            _bitmap.StreamSource = memStream;
            _bitmap.CacheOption = BitmapCacheOption.OnLoad;
            _bitmap.EndInit();
            _bitmap.Freeze();

            MapImage.Source = _bitmap;
        }

        private void CenterOnOrigin()
        {
            if (_bitmap == null) return;

            double dpiScaleX = _bitmap.DpiX / 96.0;
            double dpiScaleY = _bitmap.DpiY / 96.0;

            double imageWidth = _bitmap.PixelWidth * dpiScaleX;
            double imageHeight = _bitmap.PixelHeight * dpiScaleY;

            double originT = (yMax - 0.0) / mapRange;

            double scale = ScaleTransform.ScaleX;
            
            double centerX = Width / 2.0;
            double centerY = Height / 2.0;

            TranslateTransform.X = centerX - ScaleTransform.ScaleX * originT * imageWidth;
            TranslateTransform.Y = centerY - ScaleTransform.ScaleY * originT * imageHeight;
        }

        private void RenderLocations()
        {
            if (_mapLocations == null || _bitmap == null)
            {
                return;
            }

            CircleCanvas.Children.Clear();

            double dpiScaleX = _bitmap.DpiX / 96.0;
            double dpiScaleY = _bitmap.DpiY / 96.0;

            double scaledImageWidth = _bitmap.PixelWidth * dpiScaleX;
            double scaledImageHeight = _bitmap.PixelHeight * dpiScaleY;

            foreach (var loc in _mapLocations)
            {
                double diameterInWorldUnits = loc.SizeFactor * 30000.0;

                double diameterInScaledImagePixels = diameterInWorldUnits / mapRange * scaledImageWidth;
                double diameterInScreenPixels = diameterInScaledImagePixels * ScaleTransform.ScaleX;
                double radius = diameterInScreenPixels / 2.0;

                double imageFractionX = (loc.Location.X - xMin) / mapRange;
                double imageFractionY = (loc.Location.Y - yMin) / mapRange;

                double canvasX = (1.0 - imageFractionX) * scaledImageWidth * ScaleTransform.ScaleX + TranslateTransform.X;
                double canvasY = (1.0 - imageFractionY) * scaledImageHeight * ScaleTransform.ScaleY + TranslateTransform.Y;

                var ellipse = new Ellipse
                {
                    Width = diameterInScreenPixels,
                    Height = diameterInScreenPixels,
                    Stroke = Brushes.Orange,
                    StrokeThickness = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 255, 165, 0)),
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(ellipse, canvasX - radius);
                Canvas.SetTop(ellipse, canvasY - radius);
                CircleCanvas.Children.Add(ellipse);
            }
        }

        private void UpdateCirclePositions()
        {
            if (_mapLocations == null || _bitmap == null) return;

            double dpiScaleX = _bitmap.DpiX / 96.0;
            double dpiScaleY = _bitmap.DpiY / 96.0;

            double scaledImageWidth = _bitmap.PixelWidth * dpiScaleX;
            double scaledImageHeight = _bitmap.PixelHeight * dpiScaleY;

            for (int i = 0; i < CircleCanvas.Children.Count && i < _mapLocations.Count; i++)
            {
                if (CircleCanvas.Children[i] is Ellipse ellipse)
                {
                    var loc = _mapLocations[i];
                    double diameterInWorldUnits = loc.SizeFactor * 30000.0;
                    double diameterInScaledImagePixels = diameterInWorldUnits / mapRange * scaledImageWidth;
                    double diameterInScreenPixels = diameterInScaledImagePixels * ScaleTransform.ScaleX;
                    double radius = diameterInScreenPixels / 2.0;

                    double imageFractionX = (loc.Location.X - xMin) / mapRange;
                    double imageFractionY = (loc.Location.Y - yMin) / mapRange;

                    double canvasX = (1.0 - imageFractionX) * scaledImageWidth * ScaleTransform.ScaleX + TranslateTransform.X;
                    double canvasY = (1.0 - imageFractionY) * scaledImageHeight * ScaleTransform.ScaleY + TranslateTransform.Y;

                    Canvas.SetLeft(ellipse, canvasX - radius);
                    Canvas.SetTop(ellipse, canvasY - radius);
                }
            }
        }

        private Point WorldToScreen(double worldX, double worldY)
        {
            if (_bitmap == null) return new Point();

            double dpiScaleX = _bitmap.DpiX / 96.0;
            double dpiScaleY = _bitmap.DpiY / 96.0;

            double imageWidth = _bitmap.PixelWidth * dpiScaleX;
            double imageHeight = _bitmap.PixelHeight * dpiScaleY;

            double imageX = (worldX - xMin) / mapRange * imageWidth;
            double imageY = (worldY - yMin) / mapRange * imageHeight;

            double scale = ScaleTransform.ScaleX;

            return new Point(imageX * scale, imageY * scale);
        }

        private (double x, double y) ScreenToWorld(Point imagePos)
        {
            if (_bitmap == null) return (0, 0);

            double imagePixelX = imagePos.X * _bitmap.DpiX / 96.0;
            double imagePixelY = imagePos.Y * _bitmap.DpiY / 96.0;

            double worldX = xMax - imagePixelX / _bitmap.PixelWidth * mapRange;
            double worldY = yMax - imagePixelY / _bitmap.PixelHeight * mapRange;

            return (worldX, worldY);
        }


        protected void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(this);
                CaptureMouse();
            }
        }

        protected void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        protected void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(this);
                var delta = currentPos - _lastMousePosition;
                
                TranslateTransform.X += delta.X;
                TranslateTransform.Y += delta.Y;
                
                _lastMousePosition = currentPos;
                
                UpdateCirclePositions();
            }

            UpdateCoordinates(e.GetPosition(MapImage));
        }

        protected void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var imagePos = e.GetPosition(MapImage);
            var coords = ScreenToWorld(imagePos);
            
            var menu = new ContextMenu();
            var copyItem = new MenuItem
            {
                Header = $"Set coordinates ({coords.x:F3}, {coords.y:F3}, 0)",
                FontWeight = FontWeights.Bold
            };
            copyItem.Click += (s, args) =>
            {
                if (_onCoordinatesSelected != null)
                {
                    _onCoordinatesSelected(coords.x, coords.y, 0.0);
                }
                Clipboard.SetText($"X={coords.x:F3} Y={coords.y:F3} Z=0");
            };
            menu.Items.Add(copyItem);
            menu.IsOpen = true;
            e.Handled = true;
        }

        protected void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.2 : 1.0 / 1.2;
            
            double newScale = Math.Max(0.1, Math.Min(10.0, ScaleTransform.ScaleX * zoomFactor));

            double cursorCanvasX = TranslateTransform.X + e.GetPosition(MapImage).X * ScaleTransform.ScaleX;
            double cursorCanvasY = TranslateTransform.Y + e.GetPosition(MapImage).Y * ScaleTransform.ScaleY;

            TranslateTransform.X += (cursorCanvasX - TranslateTransform.X) * (1.0 - newScale / ScaleTransform.ScaleX);
            TranslateTransform.Y += (cursorCanvasY - TranslateTransform.Y) * (1.0 - newScale / ScaleTransform.ScaleY);

            ScaleTransform.ScaleX = newScale;
            ScaleTransform.ScaleY = newScale;

            UpdateZoomDisplay();
            UpdateCoordinates(e.GetPosition(MapImage));
            RenderLocations();
        }

        private void UpdateCoordinates(Point imagePos)
        {
            var coords = ScreenToWorld(imagePos);
            
            double clampedX = Math.Max(xMin, Math.Min(xMax, coords.x));
            double clampedY = Math.Max(yMin, Math.Min(yMax, coords.y));
            
            TxtCoords.Text = $"X: {clampedX:F3}\nY: {clampedY:F3}";
        }

        private void UpdateZoomDisplay()
        {
            double percent = ScaleTransform.ScaleX * 100;
            TxtZoomLevel.Text = $"{percent:F0}%";
        }
    }
}
