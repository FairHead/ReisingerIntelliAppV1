using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;

namespace ReisingerIntelliAppV1.Views.FloorManager
{
    public partial class PlacedDeviceControl : ContentView
    {
        private PanGestureRecognizer _panGestureRecognizer;
        private double _startX, _startY;
        public static readonly BindableProperty OpenSettingsCommandProperty =
                    BindableProperty.Create(nameof(OpenSettingsCommand), typeof(ICommand), typeof(PlacedDeviceControl));

        public static readonly BindableProperty DeleteDeviceCommandProperty =
            BindableProperty.Create(nameof(DeleteDeviceCommand), typeof(ICommand), typeof(PlacedDeviceControl),
                propertyChanged: OnDeleteDeviceCommandChanged);

        public static readonly BindableProperty ToggleDoorCommandProperty =
            BindableProperty.Create(nameof(ToggleDoorCommand), typeof(ICommand), typeof(PlacedDeviceControl));

        public static readonly BindableProperty ToggleMoveCommandProperty =
            BindableProperty.Create(nameof(ToggleMoveCommand), typeof(ICommand), typeof(PlacedDeviceControl));

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

        public ICommand ToggleMoveCommand
        {
            get => (ICommand)GetValue(ToggleMoveCommandProperty);
            set => SetValue(ToggleMoveCommandProperty, value);
        }

        public PlacedDeviceControl()
        {
            InitializeComponent();

            // Initialize pan gesture recognizer for drag and drop
            _panGestureRecognizer = new PanGestureRecognizer();
            _panGestureRecognizer.PanUpdated += OnPanUpdated;

            // Nach der Initialisierung pr�fen, ob das DeleteButton-Element existiert
            this.Loaded += (s, e) =>
            {
                Debug.WriteLine("[PlacedDeviceControl] Control loaded");
                // Nach einer kurzen Verz�gerung pr�fen, damit alles initialisiert ist
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

                    // Monitor binding context changes to toggle pan gesture based on move mode
                    UpdatePanGestureState();
                });
            };

            // Monitor binding context changes
            this.BindingContextChanged += OnBindingContextChanged;
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            // Unsubscribe from old binding context
            if (sender is PlacedDeviceControl control && control.BindingContext is Model.Models.PlacedDeviceModel oldDevice)
            {
                oldDevice.PropertyChanged -= OnDevicePropertyChanged;
            }

            // Subscribe to new binding context
            if (BindingContext is Model.Models.PlacedDeviceModel newDevice)
            {
                newDevice.PropertyChanged += OnDevicePropertyChanged;
            }

            UpdatePanGestureState();
        }

        private void OnDevicePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Models.PlacedDeviceModel.IsInMoveMode))
            {
                UpdatePanGestureState();
            }
        }

        private void UpdatePanGestureState()
        {
            if (BindingContext is Model.Models.PlacedDeviceModel device)
            {
                if (device.IsInMoveMode)
                {
                    if (!GestureRecognizers.Contains(_panGestureRecognizer))
                    {
                        GestureRecognizers.Add(_panGestureRecognizer);
                        Debug.WriteLine("[PlacedDeviceControl] Pan gesture enabled for move mode");
                    }
                }
                else
                {
                    if (GestureRecognizers.Contains(_panGestureRecognizer))
                    {
                        GestureRecognizers.Remove(_panGestureRecognizer);
                        Debug.WriteLine("[PlacedDeviceControl] Pan gesture disabled");
                    }
                }
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!(BindingContext is Model.Models.PlacedDeviceModel device) || !device.IsInMoveMode)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _startX = device.RelativeX;
                    _startY = device.RelativeY;
                    Debug.WriteLine($"[PlacedDeviceControl] Pan started at relative position: ({_startX}, {_startY})");
                    break;

                case GestureStatus.Running:
                    // Get the parent AbsoluteLayout to calculate relative movement
                    if (FindParentAbsoluteLayout() is AbsoluteLayout absoluteLayout)
                    {
                        // Convert pan delta to relative coordinates
                        double deltaX = e.TotalX / absoluteLayout.Width;
                        double deltaY = e.TotalY / absoluteLayout.Height;

                        // Update relative position, ensuring it stays within bounds [0, 1]
                        device.RelativeX = Math.Max(0, Math.Min(1, _startX + deltaX));
                        device.RelativeY = Math.Max(0, Math.Min(1, _startY + deltaY));

                        Debug.WriteLine($"[PlacedDeviceControl] Pan running - new relative position: ({device.RelativeX:F3}, {device.RelativeY:F3})");
                    }
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    Debug.WriteLine($"[PlacedDeviceControl] Pan completed at relative position: ({device.RelativeX:F3}, {device.RelativeY:F3})");
                    // Turn off move mode when pan completes
                    device.IsInMoveMode = false;
                    break;
            }
        }

        private AbsoluteLayout FindParentAbsoluteLayout()
        {
            Element current = this.Parent;
            while (current != null)
            {
                if (current is AbsoluteLayout absoluteLayout)
                    return absoluteLayout;
                current = current.Parent;
            }
            return null;
        }
    }
}