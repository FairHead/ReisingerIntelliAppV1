using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace ReisingerIntelliAppV1.Views.FloorManager
{
    public partial class PlacedDeviceControl : ContentView
    {
        public static readonly BindableProperty OpenSettingsCommandProperty =
                    BindableProperty.Create(nameof(OpenSettingsCommand), typeof(ICommand), typeof(PlacedDeviceControl));

        public static readonly BindableProperty DeleteDeviceCommandProperty =
            BindableProperty.Create(nameof(DeleteDeviceCommand), typeof(ICommand), typeof(PlacedDeviceControl),
                propertyChanged: OnDeleteDeviceCommandChanged);

        public static readonly BindableProperty ToggleDoorCommandProperty =
            BindableProperty.Create(nameof(ToggleDoorCommand), typeof(ICommand), typeof(PlacedDeviceControl));

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

        public PlacedDeviceControl()
        {
            InitializeComponent();

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
    }
}