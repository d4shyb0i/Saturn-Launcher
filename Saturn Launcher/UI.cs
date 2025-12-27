using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System;

namespace SaturnLauncher
{
    public static class UI
    {
        // Script to allow dragging the window (since we removed the standard bar)
        public static void DragWindow(Window window)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                window.DragMove();
            }
        }

        // Script to create a smooth fade-in for the launcher
        public static void ApplyFadeIn(Window window)
        {
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1.5)
            };
            window.BeginAnimation(Window.OpacityProperty, fadeIn);
        }
    }
}