using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TeamUp.MainWindow;
using System.Windows.Media.Animation;
using System.Windows;

namespace TeamUp
{
    internal class Animations
    {
        Storyboard StoryBoard = new Storyboard();
        Storyboard WidthStoryBoard = new Storyboard();
        private IEasingFunction MarginEasing { get; set; } = new QuarticEase
        {
            EasingMode = EasingMode.EaseInOut
        };


        private IEasingFunction WidthEasing { get; set; } = new QuadraticEase
        {
            EasingMode = EasingMode.EaseInOut
        };

        public async void ObjectSlider(DependencyObject Object, Thickness Get, Thickness Set)
        {
            ThicknessAnimation Animation = new ThicknessAnimation
            {
                From = new Thickness?(Get),
                To = new Thickness?(Set),
                Duration = TimeSpan.FromMilliseconds(1000.0),
                EasingFunction = this.MarginEasing
            };
            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath(FrameworkElement.MarginProperty));
            this.StoryBoard.Children.Add(Animation);
            this.StoryBoard.Begin();
            await Task.Delay(100);
            this.StoryBoard.Children.Remove(Animation);
        }

        public async void WidthSlider(DependencyObject Object, double Get, double Set)
        {
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = this.WidthEasing;
            Animation.From = Get;
            Animation.To = Set;

            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath(FrameworkElement.WidthProperty));
            this.WidthStoryBoard.Children.Add(Animation);
            this.WidthStoryBoard.Begin();
            await Task.Delay(1000);
            this.WidthStoryBoard.Children.Remove(Animation);
        }

    }
}
