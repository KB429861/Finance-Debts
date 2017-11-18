using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace Finance.Managers
{
    public abstract class AnimationManager
    {
        public static void TurnstileTransition(PhoneApplicationPage page)
        {
            try
            {
                var navigationInTransition = new NavigationInTransition
                {
                    Backward = new TurnstileTransition {Mode = TurnstileTransitionMode.BackwardIn},
                    Forward = new TurnstileTransition {Mode = TurnstileTransitionMode.ForwardIn}
                };
                var navigationOutTransition = new NavigationOutTransition
                {
                    Backward = new TurnstileTransition {Mode = TurnstileTransitionMode.BackwardOut},
                    Forward = new TurnstileTransition {Mode = TurnstileTransitionMode.ForwardOut}
                };
                TransitionService.SetNavigationInTransition(page, navigationInTransition);
                TransitionService.SetNavigationOutTransition(page, navigationOutTransition);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public static void SlideTransition(PhoneApplicationPage page)
        {
            try
            {
                var navigationInTransition = new NavigationInTransition
                {
                    Backward = new SlideTransition {Mode = SlideTransitionMode.SlideUpFadeIn},
                    Forward = new SlideTransition {Mode = SlideTransitionMode.SlideUpFadeIn}
                };
                var navigationOutTransition = new NavigationOutTransition
                {
                    Backward = new SlideTransition {Mode = SlideTransitionMode.SlideDownFadeOut},
                    Forward = new SlideTransition {Mode = SlideTransitionMode.SlideDownFadeOut}
                };
                TransitionService.SetNavigationInTransition(page, navigationInTransition);
                TransitionService.SetNavigationOutTransition(page, navigationOutTransition);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public static void ContentHorizontalAnimation(DependencyObject dependencyObject, int x1, int x2,
            EventHandler completed = null)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            var frame1 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0),
                Value = x1
            };
            var frame2 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0.2),
                Value = x2
            };
            Storyboard.SetTarget(animation, dependencyObject);
            Storyboard.SetTargetProperty(animation,
                new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            animation.KeyFrames.Add(frame1);
            animation.KeyFrames.Add(frame2);
            storyboard.Children.Add(animation);
            if (completed != null)
                storyboard.Completed += completed;
            storyboard.Begin();
        }

        public static void OpacityAnimation(DependencyObject dependencyObject, double value1, double value2)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            var frame1 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0),
                Value = value1
            };
            var frame2 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0.1),
                Value = value2
            };
            Storyboard.SetTarget(animation, dependencyObject);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.Opacity)"));
            animation.KeyFrames.Add(frame1);
            animation.KeyFrames.Add(frame2);
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        public static void ContentVerticalAnimation(DependencyObject dependencyObject, int y1, int y2,
            EventHandler completed = null)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            var frame1 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0),
                Value = y1
            };
            var frame2 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0.3),
                Value = y2
            };
            Storyboard.SetTarget(animation, dependencyObject);
            Storyboard.SetTargetProperty(animation,
                new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            animation.KeyFrames.Add(frame1);
            animation.KeyFrames.Add(frame2);
            storyboard.Children.Add(animation);
            if (completed != null)
                storyboard.Completed += completed;
            storyboard.Begin();
        }

        public static void DownUpAnimation(DependencyObject dependencyObject, int x1, int x2,
            EventHandler completed = null)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            var frame1 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0),
                Value = x1
            };
            var frame2 = new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromSeconds(0.2),
                Value = x2
            };
            Storyboard.SetTarget(animation, dependencyObject);
            Storyboard.SetTargetProperty(animation,
                new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            animation.KeyFrames.Add(frame1);
            animation.KeyFrames.Add(frame2);
            storyboard.Children.Add(animation);
            if (completed != null)
                storyboard.Completed += completed;
            storyboard.Begin();
        }
    }
}