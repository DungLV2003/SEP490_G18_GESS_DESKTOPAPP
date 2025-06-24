using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    public static class AnimationHelper
    {
        public static void ApplyFadeIn(Window window, double durationMs = 500)
        {
            window.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs));
            fadeIn.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            window.BeginAnimation(Window.OpacityProperty, fadeIn);
        }
        public static void FadeOutAndSwitch(Window current, Window next, int durationMs = 300)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(durationMs));
            fadeOut.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            fadeOut.Completed += (s, e) =>
            {
                next.Show();
                current.Close();
            };
            current.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
    }
}
