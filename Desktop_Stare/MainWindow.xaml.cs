using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Desktop_Stare
{
    public partial class MainWindow : Window
    {
        private int _fps = 30;
        private string _normalFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Img", "Frames_Normal");
        private string _hoverFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Img", "Frames_Hover");

        private DispatcherTimer _timer;
        private DispatcherTimer _rngTimer;
        private string[] _normalFrames;
        private string[] _hoverFrames;
        private string[] _activeFrames;

        private bool _allowRandom = true;
        private bool _closeOff = false;
        private int _rngInterval = 10;

        private readonly double[] rotationSteps = { 0, 180 };
        private int _rotationIndex = 0;

        private readonly double[] scaleSteps = { 1.0,0.75,0.5, 0.25 };
        private int _scaleIndex = 0;

        private int _currentFrame = 0;
        private Random _rng = new Random();

        public MainWindow()
        {
            InitializeComponent();
            ReadConfig();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            _normalFrames = LoadFrames(_normalFolder);
            _hoverFrames = LoadFrames(_hoverFolder);

            if (_normalFrames.Length == 0 || _hoverFrames.Length != _normalFrames.Length)
            {
                MessageBox.Show("Frame folders mismatch.");
                return;
            }

            _activeFrames = _normalFrames;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.0 / _fps)
            };
            _timer.Tick += NextFrame;
            _timer.Start();         
            MediaManager.PlaySound("intro.wav");
        }
        private string[] LoadFrames(string folder)
        {
            return Directory.GetFiles(folder, "*.png").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))).ToArray();
        }
        private void NextFrame(object sender, EventArgs e)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(_activeFrames[_currentFrame], UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            FrameImage.Source = bitmap;

            _currentFrame++;
            if (_currentFrame >= _activeFrames.Length)
            {
                _currentFrame = 0;
            }
        }

        private void FrameImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_closeOff)
            {
                _activeFrames = _hoverFrames;
                MediaManager.PlaySound("hover.wav", 3.0);
            }
        }
        private void FrameImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _activeFrames = _normalFrames;
        }
        private void FrameImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MediaManager.PlaySound("intro.wav");

            if(_rotationIndex != 0)
            {
                return;
            }

            _scaleIndex++;
            if (_scaleIndex >= scaleSteps.Length)
            {
                _scaleIndex = 0;
            }

            double scale = scaleSteps[_scaleIndex];
            ImageScale.ScaleX = scale;
            ImageScale.ScaleY = scale;

            UpdateTransformOrigin();
        }
        private void FrameImage_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(_scaleIndex != 0)
            {
                return;
            }
            MediaManager.PlaySound("intro.wav");

            _rotationIndex++;
            if (_rotationIndex >= rotationSteps.Length)
            {
                _rotationIndex = 0;
            }

            ImageRotate.Angle = rotationSteps[_rotationIndex];
            UpdateTransformOrigin();
        }
        private void UpdateTransformOrigin()
        {
            if (_rotationIndex == 0)
            {
                FrameImage.RenderTransformOrigin = new Point(0.5, 1);
            }
            else
            {
                FrameImage.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
        private void RandomClick(object sender, EventArgs e)
        {
            bool leftClick = _rng.NextDouble() < 0.30;
            if (leftClick)
            {
                PerformLeftClick();
            }
            else
            {
                PerformRightClick();
            }
        }

        private void PerformLeftClick()
        {
            MediaManager.PlayRandomSoundFromFolder();
            if (_rotationIndex != 0)
            {
                return;
            }
            _scaleIndex++;
            if (_scaleIndex >= scaleSteps.Length)
            {
                _scaleIndex = 0;
            }
            double scale = scaleSteps[_scaleIndex];
            ImageScale.ScaleX = scale;
            ImageScale.ScaleY = scale;

            UpdateTransformOrigin();
        }

        private void PerformRightClick()
        {
            MediaManager.PlayRandomSoundFromFolder();
            if (_scaleIndex != 0)
            {
                PerformLeftClick();
                return;
            }
            _rotationIndex++;
            if (_rotationIndex >= rotationSteps.Length)
            {
                _rotationIndex = 0;
            }
            ImageRotate.Angle = rotationSteps[_rotationIndex];
            UpdateTransformOrigin();
        }

        private void ReadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
            if (!File.Exists(configPath))
            {
                return;
            }

            var lines = File.ReadAllLines(configPath);

            foreach (var line in lines)
            {
                // Ignore empty lines or comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split('='); // split into key=value
                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                switch (key)
                {
                    case "allowRandom":
                        if (bool.TryParse(value, out bool allow))
                            _allowRandom = allow;
                        break;

                    case "closeOff":
                        if (bool.TryParse(value, out bool close))
                            _closeOff = close;
                        break;

                    case "intervalRng":
                        if (int.TryParse(value, out int interval))
                            _rngInterval = interval;
                        break;
                }
            }
            if (_allowRandom)
            {
                _rngTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(_rngInterval)
                };
                _rngTimer.Tick += RandomClick;
                _rngTimer.Start();
            }
            if (_closeOff)
            {
                this.AllowsTransparency = false;
                this.ShowInTaskbar = false;
            }
            else
            {
                this.AllowsTransparency = true;
                this.ShowInTaskbar = true;
            }
        }

    }
}