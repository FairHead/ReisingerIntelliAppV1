﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Controls
{
    /// <summary>
    /// <para><see href="https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/pan"/></para>
    /// <para><see href="https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/pinch"/></para>
    /// </summary>
    public class PanPinchContainer : ContentView
    {
        private readonly TapGestureRecognizer _doubleTapGestureRecognizer;

        private readonly PanGestureRecognizer _panGestureRecognizer;

        private readonly PinchGestureRecognizer _pinchGestureRecognizer;

        private double _currentScale = 1;

        private bool _isPanEnabled = true;

        private double _panX;

        private double _panY;

        private double _startScale = 1;

        private Point? _lastTouchPoint;
        private bool _interactionWithControlDetected = false;

        public PanPinchContainer()
        {
            _panGestureRecognizer = new PanGestureRecognizer();
            _panGestureRecognizer.PanUpdated += OnPanUpdatedAsync;
            GestureRecognizers.Add(_panGestureRecognizer);

            _pinchGestureRecognizer = new PinchGestureRecognizer();
            _pinchGestureRecognizer.PinchUpdated += OnPinchUpdatedAsync;
            GestureRecognizers.Add(_pinchGestureRecognizer);

            _doubleTapGestureRecognizer = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 2
            };

            _doubleTapGestureRecognizer.Tapped += DoubleTappedAsync;
            GestureRecognizers.Add(_doubleTapGestureRecognizer);

            var singleTapRecognizer = new TapGestureRecognizer();
            singleTapRecognizer.Tapped += OnSingleTapped;
            GestureRecognizers.Add(singleTapRecognizer);
        }

        private void OnSingleTapped(object sender, TappedEventArgs e)
        {
            _lastTouchPoint = e.GetPosition(this);
            _interactionWithControlDetected = IsPointOverInteractiveControl(_lastTouchPoint);
        }

        private bool IsPointOverInteractiveControl(Point? point)
        {
            if (!point.HasValue || Content == null)
                return false;

            return FindInteractiveElementAt(Content, point.Value);
        }

        private bool FindInteractiveElementAt(Element element, Point point)
        {
            if (element is Button || element is CheckBox || element is Switch)
            {
                if (element is VisualElement ve)
                {
                    var bounds = GetElementBounds(ve);
                    if (bounds.Contains(point))
                    {
                        return true;
                    }
                }
            }

            if (element is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Element childElement)
                    {
                        if (FindInteractiveElementAt(childElement, point))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private Rect GetElementBounds(VisualElement element)
        {
            var x = element.X;
            var y = element.Y;

            var parent = element.Parent as VisualElement;
            while (parent != null && parent != this)
            {
                x += parent.X;
                y += parent.Y;
                parent = parent.Parent as VisualElement;
            }

            return new Rect(x, y, element.Width, element.Height);
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            if (child is View view)
            {
                view.HorizontalOptions = LayoutOptions.Center;
                view.VerticalOptions = LayoutOptions.Center;
            }
        }

        private async Task ClampTranslationAsync(double transX, double transY, bool animate = false)
        {
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            double contentWidth = Content.Width * _currentScale;
            double contentHeight = Content.Height * _currentScale;

            if (contentWidth <= Width)
            {
                transX = -(contentWidth - Content.Width) / 2;
            }
            else
            {
                double minBoundX = ((Width - Content.Width) / 2) + contentWidth - Width;
                double maxBoundX = (Width - Content.Width) / 2;
                transX = Math.Clamp(transX, -minBoundX, -maxBoundX);
            }

            if (contentHeight <= Height)
            {
                transY = -(contentHeight - Content.Height) / 2;
            }
            else
            {
                double minBoundY = ((Height - Content.Height) / 2) + contentHeight - Height;
                double maxBoundY = (Height - Content.Height) / 2;
                transY = Math.Clamp(transY, -minBoundY, -maxBoundY);
            }

            if (animate)
            {
                await TranslateToAsync(transX, transY);
            }
            else
            {
                Content.TranslationX = transX;
                Content.TranslationY = transY;
            }
        }

        private async Task ClampTranslationFromScaleOriginAsync(double originX, double originY, bool animate = false)
        {
            double renderedX = Content.X + _panX;
            double deltaX = renderedX / Width;
            double deltaWidth = Width / (Content.Width * _startScale);
            originX = (originX - deltaX) * deltaWidth;

            double renderedY = Content.Y + _panY;
            double deltaY = renderedY / Height;
            double deltaHeight = Height / (Content.Height * _startScale);
            originY = (originY - deltaY) * deltaHeight;

            double targetX = _panX - (originX * Content.Width * (_currentScale - _startScale));
            double targetY = _panY - (originY * Content.Height * (_currentScale - _startScale));

            if (_currentScale > 1)
            {
                targetX = Math.Clamp(targetX, -Content.Width * (_currentScale - 1), 0);
                targetY = Math.Clamp(targetY, -Content.Height * (_currentScale - 1), 0);
            }
            else
            {
                targetX = (Width - (Content.Width * _currentScale)) / 2;
                targetY = Content.Height * (1 - _currentScale) / 2;
            }

            await ClampTranslationAsync(targetX, targetY, animate);
        }

        private async void DoubleTappedAsync(object? sender, TappedEventArgs e)
        {
            if (_interactionWithControlDetected)
            {
                return;
            }

            _startScale = Content.Scale;
            _currentScale = _startScale;
            _panX = Content.TranslationX;
            _panY = Content.TranslationY;

            if (_currentScale < 2)
            {
                _currentScale = 2;
            }
            else
            {
                _currentScale = 1;
            }

            var point = e.GetPosition(sender as View);

            var translateTask = Task.CompletedTask;

            if (point is not null)
            {
                translateTask = ClampTranslationFromScaleOriginAsync(point.Value.X / Width, point.Value.Y / Height, true);
            }

            var scaleTask = ScaleToAsync(_currentScale);

            await Task.WhenAll(translateTask, scaleTask);

            _panX = Content.TranslationX;
            _panY = Content.TranslationY;
        }

        private async void OnPanUpdatedAsync(object? sender, PanUpdatedEventArgs e)
        {
            if (!_isPanEnabled)
            {
                return;
            }

            if (Content.Scale <= 1)
            {
                return;
            }

            if (e.StatusType == GestureStatus.Started)
            {
                if (_interactionWithControlDetected)
                {
                    return;
                }

                _panX = Content.TranslationX;
                _panY = Content.TranslationY;

                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                if (_interactionWithControlDetected)
                {
                    return;
                }

                await ClampTranslationAsync(_panX + e.TotalX, _panY + e.TotalY);
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                _interactionWithControlDetected = false;

                _panX = Content.TranslationX;
                _panY = Content.TranslationY;
            }
            else if (e.StatusType == GestureStatus.Canceled)
            {
                _interactionWithControlDetected = false;

                Content.TranslationX = _panX;
                Content.TranslationY = _panY;
            }
        }

        private async void OnPinchUpdatedAsync(object? sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                if (_interactionWithControlDetected)
                {
                    return;
                }

                _isPanEnabled = false;

                _panX = Content.TranslationX;
                _panY = Content.TranslationY;

                _startScale = Content.Scale;

                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }

            if (e.Status == GestureStatus.Running)
            {
                if (_interactionWithControlDetected)
                {
                    return;
                }

                _currentScale += (e.Scale - 1) * _startScale;
                _currentScale = Math.Clamp(_currentScale, 0.5, 10);

                await ClampTranslationFromScaleOriginAsync(e.ScaleOrigin.X, e.ScaleOrigin.Y);

                Content.Scale = _currentScale;
            }

            if (e.Status == GestureStatus.Completed)
            {
                _interactionWithControlDetected = false;

                if (_currentScale < 1)
                {
                    var translateTask = TranslateToAsync(0, 0);
                    var scaleTask = ScaleToAsync(1);

                    await Task.WhenAll(translateTask, scaleTask);
                }

                _panX = Content.TranslationX;
                _panY = Content.TranslationY;

                _isPanEnabled = true;
            }
            else if (e.Status == GestureStatus.Canceled)
            {
                _interactionWithControlDetected = false;

                Content.TranslationX = _panX;
                Content.TranslationY = _panY;
                Content.Scale = _startScale;

                _isPanEnabled = true;
            }
        }

        private async Task ScaleToAsync(double scale)
        {
            await Content.ScaleTo(scale, 250, Easing.Linear);
            _currentScale = scale;
        }

        private async Task TranslateToAsync(double x, double y)
        {
            await Content.TranslateTo(x, y, 250, Easing.Linear);
            _panX = x;
            _panY = y;
        }
    }
}
