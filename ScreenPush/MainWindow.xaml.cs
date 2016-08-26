using HooksLibrary;
using System;
using System.Collections.Generic;
using Drawing = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;
using WindowsPoint = System.Windows.Point;
using ScreenPush.SupportClasses;

namespace ScreenPush
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            account = new Account("dfjpsan1u", "293465886757451", "NNKxAxE6h0VXwsBwkdNQvRqutSg");
            cloudinary = new Cloudinary(account);

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;

            this.WindowStyle = System.Windows.WindowStyle.None;
            this.Topmost = true;
            this.AllowsTransparency = true;
            this.Visibility = Visibility.Hidden;

            settings = new AppSettings();
            customBrush = Brushes.Indigo;

            if (!this.CheckIfConfigExsist()) this.CreateDefaultConfigFile();
            this.ReadFromConfigFile();

            _hotKey = new HotKey(settings.ActivateKey, settings.ActivateMod | settings.ActivateSecondaryMod, OnHotKeyHandler);
        }

        private readonly string imageRepo = "imageRepo";
        private readonly string appSettings = "appSettings.xml";

        private WindowsPoint startPoint;
        private WindowsPoint endPoint;
        private Rectangle rect;
        private HotKey _hotKey = null;
        private Timer timer;
        private AppSettings settings;
        private Brush customBrush;
        private Account account;
        Cloudinary cloudinary;
        private string LastImgPath
        { get; set; }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            this.Visibility = Visibility.Visible;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);

            rect = new Rectangle
            {
                Stroke = customBrush,
                StrokeThickness = 3
            };
            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.X);
            canvas.Children.Add(rect);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            endPoint = e.GetPosition(canvas);
            (sender as Canvas).Children.Clear();
            this.Visibility = Visibility.Hidden;
            timer = new Timer(TakeScreenshot, rect, 0, 500);
            rect = null;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _hotKey.Dispose();
        }


        private void TakeScreenshot(object rect)
        {
            if (!(rect is Rectangle))
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
                MessageBox.Show("Somethig went wrong!");
            }

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                Rectangle bounds = rect as Rectangle;
                using (Drawing.Bitmap bitmap = new Drawing.Bitmap((int)bounds.Width - 6, (int)bounds.Height - 6))
                {
                    using (Drawing.Graphics g = Drawing.Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen((int)this.startPoint.X - 3, (int)this.startPoint.Y - 3, 0, 0,
                            new Drawing.Size((int)bounds.Width, (int)bounds.Height));
                    }
                    string formName = string.Format("[d {0:dd-MM-yy}][t {0:H.mm.ss}]{1}.jpg", DateTime.Now, bitmap.GetHashCode());
                    if (!Directory.Exists(imageRepo))
                    {
                        Directory.CreateDirectory(imageRepo);
                    }
                    string path = System.IO.Path.GetFullPath(imageRepo) + "\\" + formName;
                    bitmap.Save(path, ImageFormat.Jpeg);
                    LastImgPath = path;

                    this.UploadImage(path);
                }
            }));
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        private bool CheckIfConfigExsist()
        {
            return File.Exists(this.appSettings);
        }

        private void CreateDefaultConfigFile()
        {
            var serSettings = Serializer.Serialize(this.settings);

            File.AppendAllText(this.appSettings, serSettings, Encoding.UTF8);
        }

        private MColor DrawingColorToMediaColor(DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void ReadFromConfigFile()
        {
            Key configKey;
            KeyModifier configMod;
            XDocument doc = null;
            try
            {
                doc = XDocument.Load(this.appSettings);

                string keyboardKey = doc.Root.Elements("ActivateKey").Select(x => x.Value).FirstOrDefault();
                string keyboardMod = doc.Root.Elements("ActivateMod").Select(x => x.Value).FirstOrDefault();
                string keyboardSecMod = doc.Root.Elements("ActivateSecondaryMod").Select(x => x.Value).FirstOrDefault();
                string borderColor = doc.Root.Elements("CustomBrushName").Select(x => x.Value).FirstOrDefault();

                if (Enum.IsDefined(typeof(Drawing.KnownColor), borderColor))
                {
                    MColor color = DrawingColorToMediaColor(DColor.FromName(borderColor));
                    this.customBrush = new SolidColorBrush(color);
                }
                if (Enum.IsDefined(typeof(Key), keyboardKey) && Enum.TryParse(keyboardKey, true, out configKey))
                {
                    if ((configKey >= Key.A && configKey <= Key.Z) || (configKey >= Key.F1 && configKey <= Key.F12))
                        this.settings.ActivateKey = configKey;
                }
                if (Enum.IsDefined(typeof(KeyModifier), keyboardMod) && Enum.TryParse(keyboardMod, true, out configMod))
                {
                    if ((configMod >= KeyModifier.None && configMod <= KeyModifier.Ctrl) || (configMod >= KeyModifier.Shift && configMod <= KeyModifier.Win))
                        this.settings.ActivateMod = configMod;
                }
                if (Enum.IsDefined(typeof(KeyModifier), keyboardSecMod) && Enum.TryParse(keyboardSecMod, true, out configMod))
                {
                    if ((configMod >= KeyModifier.None && configMod <= KeyModifier.Ctrl) || (configMod >= KeyModifier.Shift && configMod <= KeyModifier.Win))
                        this.settings.ActivateSecondaryMod = configMod;
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                this.taskbIcon.ShowBalloonTip("Error!", "Config file not found! Program will create new config file with next start", 
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
            }
            catch (NullReferenceException e)
            {
                this.taskbIcon.ShowBalloonTip("Error!", "Some key does not found in config file... Please delete config file after shutdown program.",
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
            }
        }

        

        private void UploadImage(string path)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(path)
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            var jsToken = uploadResult.JsonObj;
            if (uploadResult.Error == null)
            {
                if (jsToken["url"] != null)
                {
                    Clipboard.SetText(jsToken["url"].ToString());
                    this.taskbIcon.ShowBalloonTip("Image Created!", jsToken["url"].ToString(), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
            }
            else
                this.taskbIcon.ShowBalloonTip("Error!", "Some internet error!", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuMouseEnter(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        private void MenuMauseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
        }
    }
}
