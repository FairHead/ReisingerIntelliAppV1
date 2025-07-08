using CommunityToolkit.Maui.Behaviors;
using ReisingerIntelliAppV1.Model.Models;
using System.Diagnostics;
using System.Windows.Input;
using ReisingerIntelliAppV1.Model.ViewModels;
using System.ComponentModel;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class PlacedDeviceControl : ContentView, INotifyPropertyChanged
{
    private bool isDragging = false;
    private bool isPositioningMode = false;
    private double initialTouchOffsetX, initialTouchOffsetY;
    private PlacedDeviceModel initialDeviceModel;

    public PlacedDeviceControl()
    {
        InitializeComponent();
    }

    // Public property to expose drag status
    public bool IsDragMode => isDragging && isPositioningMode;

    // Public property to expose positioning mode status
    public bool IsPositioningMode 
    { 
        get => isPositioningMode; 
        private set 
        { 
            isPositioningMode = value; 
            OnPropertyChanged(); 
        } 
    }

    // 🔹 Bindbare Properties (für Commands von außen)
    public static readonly BindableProperty OpenSettingsCommandProperty =
        BindableProperty.Create(nameof(OpenSettingsCommand), typeof(ICommand), typeof(PlacedDeviceControl));

    public static readonly BindableProperty DeleteDeviceCommandProperty =
        BindableProperty.Create(nameof(DeleteDeviceCommand), typeof(ICommand), typeof(PlacedDeviceControl));

    public static readonly BindableProperty ToggleDoorCommandProperty =
        BindableProperty.Create(nameof(ToggleDoorCommand), typeof(ICommand), typeof(PlacedDeviceControl));


    public ICommand OpenSettingsCommand
    {
        get => (ICommand)GetValue(OpenSettingsCommandProperty);
        set => SetValue(OpenSettingsCommandProperty, value);
    }

    public ICommand DeleteDeviceCommand
    {
        get => (ICommand)GetValue(DeleteDeviceCommandProperty);
        set => SetValue(DeleteDeviceCommandProperty, value);
    }

    public ICommand ToggleDoorCommand
    {
        get => (ICommand)GetValue(ToggleDoorCommandProperty);
        set => SetValue(ToggleDoorCommandProperty, value);
    }

    // Toggle positioning mode command
    public ICommand TogglePositioningMode => new Command(() =>
    {
        // Validate that the layout is ready before allowing positioning mode
        var parent = FindParentAbsoluteLayout();
        if (parent == null)
        {
            Debug.WriteLine("[PlacedDeviceControl] ❌ Cannot enter positioning mode: Parent AbsoluteLayout not found");
            return;
        }

        if (parent.Width <= 0 || parent.Height <= 0 || 
            double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
            double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
        {
            Debug.WriteLine($"[PlacedDeviceControl] ❌ Cannot enter positioning mode: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
            return;
        }

        IsPositioningMode = !IsPositioningMode;
        if (IsPositioningMode)
        {
            isDragging = true;
            Debug.WriteLine("[PlacedDeviceControl] ✅ Positioning mode activated");
        }
        else
        {
            isDragging = false;
            Debug.WriteLine("[PlacedDeviceControl] ✅ Positioning mode deactivated");
        }
        OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
    });

    // Aktiviert Drag (von LongPress) - keep backward compatibility
    public ICommand StartDragCommand => new Command(() =>
    {
        // Validate that the layout is ready before allowing positioning mode
        var parent = FindParentAbsoluteLayout();
        if (parent == null)
        {
            Debug.WriteLine("[PlacedDeviceControl] ❌ Cannot start drag: Parent AbsoluteLayout not found");
            return;
        }

        if (parent.Width <= 0 || parent.Height <= 0 || 
            double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
            double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
        {
            Debug.WriteLine($"[PlacedDeviceControl] ❌ Cannot start drag: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
            return;
        }

        IsPositioningMode = true;
        isDragging = true;
        OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
        Debug.WriteLine("[PlacedDeviceControl] ✅ Drag activated (LongPress)");
    });

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (BindingContext is PlacedDeviceModel device)
        {
            e.Data.Properties["DeviceId"] = device.DeviceId;
        }
    }

    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!isDragging || !isPositioningMode) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                if (BindingContext is PlacedDeviceModel placedDevice)
                {
                    // Find the parent AbsoluteLayout (DeviceCanvas)
                    var parent = FindParentAbsoluteLayout();
                    if (parent == null)
                    {
                        Debug.WriteLine("[PlacedDeviceControl] ❌ Pan started: Cannot find parent AbsoluteLayout");
                        ResetPositioningMode();
                        return;
                    }

                    // Validate parent dimensions
                    if (parent.Width <= 0 || parent.Height <= 0 || 
                        double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
                        double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Pan started: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
                        ResetPositioningMode();
                        return;
                    }

                    // Store the initial device model to restore if needed
                    initialDeviceModel = new PlacedDeviceModel
                    {
                        RelativeX = placedDevice.RelativeX,
                        RelativeY = placedDevice.RelativeY
                    };

                    // Calculate the current absolute position of the control center
                    var currentAbsX = placedDevice.RelativeX * parent.Width;
                    var currentAbsY = placedDevice.RelativeY * parent.Height;

                    // Calculate offset from gesture start point to control center
                    // The gesture coordinates are relative to the control, so we need to account for 
                    // where the user touched on the control relative to its center
                    initialTouchOffsetX = currentAbsX - e.TotalX;
                    initialTouchOffsetY = currentAbsY - e.TotalY;

                    Debug.WriteLine($"[PlacedDeviceControl] ✅ Pan started - Current abs pos: ({currentAbsX:F1}, {currentAbsY:F1}), Touch offset: ({initialTouchOffsetX:F1}, {initialTouchOffsetY:F1})");
                }
                break;

            case GestureStatus.Running:
                if (BindingContext is PlacedDeviceModel placedDevice)
                {
                    var parent = FindParentAbsoluteLayout();
                    if (parent == null) return;

                    // Calculate new absolute position by adding gesture coordinates to initial offset
                    var newAbsX = e.TotalX + initialTouchOffsetX;
                    var newAbsY = e.TotalY + initialTouchOffsetY;

                    // Validate calculated absolute positions
                    if (double.IsNaN(newAbsX) || double.IsNaN(newAbsY) || 
                        double.IsInfinity(newAbsX) || double.IsInfinity(newAbsY))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Pan running: Invalid absolute position - X={newAbsX}, Y={newAbsY}");
                        return;
                    }

                    // Convert to relative coordinates
                    var newRelativeX = newAbsX / parent.Width;
                    var newRelativeY = newAbsY / parent.Height;

                    // Validate and clamp relative positions
                    if (double.IsNaN(newRelativeX) || double.IsInfinity(newRelativeX))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Pan running: Invalid relativeX calculation - {newRelativeX}");
                        return;
                    }
                    if (double.IsNaN(newRelativeY) || double.IsInfinity(newRelativeY))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Pan running: Invalid relativeY calculation - {newRelativeY}");
                        return;
                    }

                    // Clamp to valid range [0.0, 1.0]
                    newRelativeX = Math.Clamp(newRelativeX, 0.0, 1.0);
                    newRelativeY = Math.Clamp(newRelativeY, 0.0, 1.0);

                    // Update device position directly - this will trigger the converter to reposition the control
                    placedDevice.RelativeX = newRelativeX;
                    placedDevice.RelativeY = newRelativeY;
                }
                break;

            case GestureStatus.Completed:
                if (BindingContext is PlacedDeviceModel placedDevice)
                {
                    Debug.WriteLine($"[PlacedDeviceControl] ✅ Pan completed - Final position: X={placedDevice.RelativeX:F3}, Y={placedDevice.RelativeY:F3}");

                    // Save changes
                    try
                    {
                        if (Application.Current.MainPage is Shell shell &&
                            shell.CurrentPage is FloorPlanManagerPage page &&
                            page.BindingContext is FloorPlanViewModel vm)
                        {
                            await vm.SaveBuildingsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Error saving changes: {ex.Message}");
                    }
                }

                // Exit positioning mode after successful drag
                ResetPositioningMode();
                Debug.WriteLine("[PlacedDeviceControl] Pan completed, exiting positioning mode");
                break;

            case GestureStatus.Canceled:
                // Restore original position on cancel
                if (BindingContext is PlacedDeviceModel placedDevice && initialDeviceModel != null)
                {
                    placedDevice.RelativeX = initialDeviceModel.RelativeX;
                    placedDevice.RelativeY = initialDeviceModel.RelativeY;
                    Debug.WriteLine("[PlacedDeviceControl] Pan canceled, position restored");
                }
                ResetPositioningMode();
                break;
        }
    }

    /// <summary>
    /// Resets positioning mode and notifies container about drag state change
    /// </summary>
    private void ResetPositioningMode()
    {
        IsPositioningMode = false;
        isDragging = false;
        OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
    }

    /// <summary>
    /// Finds the parent AbsoluteLayout (DeviceCanvas) for coordinate calculations
    /// </summary>
    private AbsoluteLayout FindParentAbsoluteLayout()
    {
        var current = this.Parent;
        while (current != null)
        {
            if (current is AbsoluteLayout absoluteLayout)
            {
                return absoluteLayout;
            }
            current = current.Parent;
        }
        return null;
    }
}
