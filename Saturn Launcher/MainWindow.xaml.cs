using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CmlLib.Core.Auth;

namespace SaturnLauncher
{
    public partial class MainWindow : Window
    {
        private LauncherLogic _logic = new LauncherLogic();
        private List<StarParticle> _stars = new List<StarParticle>();
        private Random _rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
            CheckGameFiles();
            InitializeStars();
        }

        private void InitializeStars()
        {
            for (int i = 0; i < 80; i++)
            {
                var s = new StarParticle
                {
                    Shape = new Ellipse { Fill = Brushes.White, Width = _rnd.Next(1, 3), Height = _rnd.Next(1, 3) },
                    X = _rnd.NextDouble() * 1000,
                    Y = _rnd.NextDouble() * 600,
                    Speed = _rnd.NextDouble() * 0.5 + 0.1
                };
                _stars.Add(s);
                StarCanvas.Children.Add(s.Shape);
            }
            CompositionTarget.Rendering += (s, e) => {
                foreach (var star in _stars)
                {
                    star.X -= star.Speed;
                    if (star.X < -5) star.X = 1005;
                    Canvas.SetLeft(star.Shape, star.X);
                    Canvas.SetTop(star.Shape, star.Y);
                }
            };
        }

        private void Window_ContentRendered(object sender, EventArgs e) => SilentStartupLogin();

        private async void SilentStartupLogin()
        {
            lblStatus.Text = "CHECKING SESSION...";
            var session = await _logic.TryAutoLogin();
            if (session != null) UpdateProfileUI(session);
            else lblStatus.Text = "READY";
        }

        private void UpdateProfileUI(MSession session)
        {
            txtPlayerName.Text = session.Username;
            lblOnlineStatus.Text = "Online";
            lblOnlineStatus.Foreground = Brushes.Lime;
            imgPlayerHead.Source = new BitmapImage(new Uri($"https://minotar.net/helm/{session.Username}/100.png"));
            btnLogin.Visibility = Visibility.Collapsed;
            btnLaunch.IsEnabled = true;
            lblStatus.Text = "LOGGED IN";
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblStatus.Text = "LOGGING IN...";
                var session = await _logic.Login();
                if (session != null) UpdateProfileUI(session);
            }
            catch { lblStatus.Text = "READY"; }
        }

        private void Logout_Click(object sender, RoutedEventArgs e) { _logic.Logout(); ResetProfileUI(); }

        private void ResetProfileUI()
        {
            txtPlayerName.Text = "Guest";
            lblOnlineStatus.Text = "Offline";
            imgPlayerHead.Source = null;
            btnLogin.Visibility = Visibility.Visible;
            btnLaunch.IsEnabled = false;
        }

        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            btnLaunch.IsEnabled = false;
            try
            {
                lblStatus.Text = "PREPARING...";
                await _logic.LaunchGame((msg) => lblStatus.Text = msg.ToUpper());
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnLaunch.IsEnabled = true;
                lblStatus.Text = "READY";
            }
        }

        private void CheckGameFiles()
        {
            string path = System.IO.Path.Combine(_logic.SaturnPath, "versions", "Saturn_Client");
            btnLaunch.Content = System.IO.Directory.Exists(path) ? "PLAY" : "DOWNLOAD";
        }

        private void ProfileSection_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProfileSection.ContextMenu != null)
            {
                ProfileSection.ContextMenu.PlacementTarget = ProfileSection;
                ProfileSection.ContextMenu.IsOpen = true;
            }
        }
        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    }

    public class StarParticle
    {
        public Ellipse Shape { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
    }
}