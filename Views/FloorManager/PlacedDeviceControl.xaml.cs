using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace ReisingerIntelliAppV1.Views.FloorManager
{
    public partial class PlacedDeviceControl : ContentView, IDisposable
    {
        // Drag mode related properties
        private bool _isDragMode = false;
        private Point _lastPanPoint;
        private PanGestureRecognizer _dragPanGestureRecognizer;
        private System.Timers.Timer _longPressTimer;
        private TapGestureRecognizer _moveButtonTapGestureRecognizer;
        public static readonly BindableProperty OpenSettingsCommandProperty =
                    BindableProperty.Create(nameof(OpenSettingsCommand), typeof(ICommand), typeof(PlacedDeviceControl));

        public static readonly BindableProperty DeleteDeviceCommandProperty =
            BindableProperty.Create(nameof(DeleteDeviceCommand), typeof(ICommand), typeof(PlacedDeviceControl),
                propertyChanged: OnDeleteDeviceCommandChanged);

        public static readonly BindableProperty ToggleDoorCommandProperty =
            BindableProperty.Create(nameof(ToggleDoorCommand), typeof(ICommand), typeof(PlacedDeviceControl));

        public static readonly BindableProperty IsDragModeProperty =
            BindableProperty.Create(nameof(IsDragMode), typeof(bool), typeof(PlacedDeviceControl), false);

        public static readonly BindableProperty UpdateDevicePositionCommandProperty =
            BindableProperty.Create(nameof(UpdateDevicePositionCommand), typeof(ICommand), typeof(PlacedDeviceControl));

        private static void OnDeleteDeviceCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Debug.WriteLine($"[PlacedDeviceControl] DeleteDeviceCommand changed: {oldValue} -> {newValue}");
        }

        public ICommand OpenSettingsCommand
        {
            get => (ICommand)GetValue(OpenSettingsCommandProperty);
            set => SetValue(OpenSettingsCommandProperty, value);
        }

        public ICommand DeleteDeviceCommand
        {
            get
            {
                var cmd = (ICommand)GetValue(DeleteDeviceCommandProperty);
                Debug.WriteLine($"[PlacedDeviceControl] GetValue DeleteDeviceCommand: {cmd}");
                return cmd;
            }
            set
            {
                Debug.WriteLine($"[PlacedDeviceControl] SetValue DeleteDeviceCommand: {value}");
                SetValue(DeleteDeviceCommandProperty, value);
            }
        }

        public ICommand ToggleDoorCommand
        {
            get => (ICommand)GetValue(ToggleDoorCommandProperty);
            set => SetValue(ToggleDoorCommandProperty, value);
        }

        public bool IsDragMode
        {
            get => (bool)GetValue(IsDragModeProperty);
            set => SetValue(IsDragModeProperty, value);
        }

        public ICommand UpdateDevicePositionCommand
        {
            get => (ICommand)GetValue(UpdateDevicePositionCommandProperty);
            set => SetValue(UpdateDevicePositionCommandProperty, value);
        }

        public PlacedDeviceControl()
        {
            InitializeComponent();

            // Set up gesture recognizers for drag functionality
            SetupGestureRecognizers();

            // Nach der Initialisierung prüfen, ob das DeleteButton-Element existiert
            this.Loaded += (s, e) =>
            {
                Debug.WriteLine("[PlacedDeviceControl] Control loaded");
                // Nach einer kurzen Verzögerung prüfen, damit alles initialisiert ist
                Dispatcher.DispatchAsync(() =>
                {
                    var deleteButton = this.FindByName<Button>("DeleteButton");
                    if (deleteButton != null)
                    {
                        Debug.WriteLine("[PlacedDeviceControl] Delete button found");
                    }
                    else
                    {
                        Debug.WriteLine("[PlacedDeviceControl] Delete button NOT found - check XAML element name");
                    }
                });
            };
        }

        private void SetupGestureRecognizers()
        {
            // Timer for simulating long press
            _longPressTimer = new System.Timers.Timer(800); // 800ms for long press
            _longPressTimer.Elapsed += OnLongPressTimerElapsed;
            _longPressTimer.AutoReset = false;

            // Tap gesture recognizer for MoveAndPlace button - using single tap to avoid conflicts
            _moveButtonTapGestureRecognizer = new TapGestureRecognizer();
            _moveButtonTapGestureRecognizer.Tapped += OnMoveButtonTapped;

            // Pan gesture recognizer for dragging
            _dragPanGestureRecognizer = new PanGestureRecognizer();
            _dragPanGestureRecognizer.PanUpdated += OnDragPanUpdated;

            // Find the MoveAndPlace button and add gestures
            this.Loaded += (sender, e) =>
            {
                var moveButton = this.FindByName<Button>("MoveAndPlace");
                if (moveButton != null)
                {
                    moveButton.GestureRecognizers.Add(_moveButtonTapGestureRecognizer);
                    
                    // Add pointer events for press and release
                    moveButton.Pressed += OnMoveButtonPressed;
                    moveButton.Released += OnMoveButtonReleased;
                    
                    Debug.WriteLine("[PlacedDeviceControl] Gesture recognizers added to MoveAndPlace button");
                }
            };
        }

        private void OnMoveButtonTapped(object sender, TappedEventArgs e)
        {
            // Handle tap separately from press/release - this prevents other gestures from interfering
            Debug.WriteLine("[PlacedDeviceControl] Move button tapped");
        }

        private void OnMoveButtonPressed(object sender, EventArgs e)
        {
            Debug.WriteLine("[PlacedDeviceControl] Move button pressed - starting long press timer");
            _longPressTimer.Start();
        }

        private void OnMoveButtonReleased(object sender, EventArgs e)
        {
            Debug.WriteLine("[PlacedDeviceControl] Move button released");
            _longPressTimer.Stop();
            
            // If we were in drag mode but button was released, exit drag mode
            if (_isDragMode)
            {
                CompleteDragOperation();
            }
        }

        private void OnLongPressTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Only enter drag mode if not already in it
                if (!_isDragMode)
                {
                    OnLongPressStarted();
                }
            });
        }

        private void OnLongPressStarted()
        {
            Debug.WriteLine("[PlacedDeviceControl] Long press detected - entering drag mode");
            _isDragMode = true;
            IsDragMode = true; // Update bindable property to notify parent

            // Add pan gesture to the entire control for dragging
            if (!this.GestureRecognizers.Contains(_dragPanGestureRecognizer))
            {
                this.GestureRecognizers.Add(_dragPanGestureRecognizer);
            }

            // Visual feedback - change background color and add some opacity
            this.BackgroundColor = Colors.LightBlue.WithAlpha(0.5f);
            this.Opacity = 0.8;
            
            // Provide haptic feedback if available
            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            }
            catch
            {
                // Haptic feedback not available on this platform
            }
        }

        private void OnDragPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!_isDragMode) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    Debug.WriteLine("[PlacedDeviceControl] Drag started");
                    _lastPanPoint = new Point(0, 0);
                    break;

                case GestureStatus.Running:
                    Debug.WriteLine($"[PlacedDeviceControl] Dragging: TotalX={e.TotalX}, TotalY={e.TotalY}");
                    
                    // Update translation to show visual movement during drag
                    this.TranslationX = e.TotalX;
                    this.TranslationY = e.TotalY;
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    Debug.WriteLine("[PlacedDeviceControl] Drag completed/canceled");
                    CompleteDragOperation();
                    break;
            }
        }

        private void CompleteDragOperation()
        {
            _isDragMode = false;
            IsDragMode = false; // Update bindable property to notify parent

            // Remove pan gesture recognizer
            if (this.GestureRecognizers.Contains(_dragPanGestureRecognizer))
            {
                this.GestureRecognizers.Remove(_dragPanGestureRecognizer);
            }

            // Restore visual feedback
            this.BackgroundColor = Colors.Transparent;
            this.Opacity = 1.0;

            // Calculate new relative position and update the view model
            UpdateDevicePosition();

            // Reset translation
            this.TranslationX = 0;
            this.TranslationY = 0;
        }

        private void UpdateDevicePosition()
        {
            // Find the parent container to calculate relative position
            var parent = this.Parent;
            while (parent != null && !(parent is AbsoluteLayout))
            {
                parent = parent.Parent;
            }

            if (parent is AbsoluteLayout absoluteLayout && absoluteLayout.Width > 0 && absoluteLayout.Height > 0)
            {
                // Get current layout bounds (these are in proportional coordinates 0-1)
                var currentBounds = AbsoluteLayout.GetLayoutBounds(this);
                
                // Calculate the actual pixel position
                var currentPixelX = currentBounds.X * absoluteLayout.Width;
                var currentPixelY = currentBounds.Y * absoluteLayout.Height;
                
                // Add the translation (which is in pixels)
                var newPixelX = currentPixelX + this.TranslationX;
                var newPixelY = currentPixelY + this.TranslationY;
                
                // Convert back to relative coordinates and clamp
                var newRelativeX = Math.Clamp(newPixelX / absoluteLayout.Width, 0, 1);
                var newRelativeY = Math.Clamp(newPixelY / absoluteLayout.Height, 0, 1);

                Debug.WriteLine($"[PlacedDeviceControl] Position update: Current=({currentBounds.X:F3},{currentBounds.Y:F3}), Translation=({this.TranslationX:F1},{this.TranslationY:F1}), New=({newRelativeX:F3},{newRelativeY:F3})");

                // Update position through command if available
                if (UpdateDevicePositionCommand?.CanExecute(null) == true)
                {
                    var positionData = new { DeviceModel = this.BindingContext, RelativeX = newRelativeX, RelativeY = newRelativeY };
                    UpdateDevicePositionCommand.Execute(positionData);
                }
            }
        }

        public void Dispose()
        {
            _longPressTimer?.Stop();
            _longPressTimer?.Dispose();
            _longPressTimer = null;
        }
    }
}