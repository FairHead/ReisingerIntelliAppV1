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
    private double startX, startY;
    private double panStartX, panStartY;

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
        if (Parent is VisualElement parent)
        {
            if (parent.Width <= 0 || parent.Height <= 0 || 
                double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
                double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
            {
                Debug.WriteLine($"[PlacedDeviceControl] ❌ Cannot enter positioning mode: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
                return;
            }
        }
        else
        {
            Debug.WriteLine("[PlacedDeviceControl] ❌ Cannot enter positioning mode: Parent is null");
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
        if (Parent is VisualElement parent)
        {
            if (parent.Width <= 0 || parent.Height <= 0 || 
                double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
                double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
            {
                Debug.WriteLine($"[PlacedDeviceControl] ❌ Cannot start drag: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
                return;
            }
        }
        else
        {
            Debug.WriteLine("[PlacedDeviceControl] ❌ Cannot start drag: Parent is null");
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
                startX = TranslationX;
                startY = TranslationY;
                panStartX = e.TotalX;
                panStartY = e.TotalY;
                Debug.WriteLine("[PlacedDeviceControl] Pan started");
                break;

            case GestureStatus.Running:
                TranslationX = startX + (e.TotalX - panStartX);
                TranslationY = startY + (e.TotalY - panStartY);
                break;

            case GestureStatus.Completed:
                if (BindingContext is PlacedDeviceModel placedDevice)
                {
                    var parent = this.Parent as VisualElement;
                    if (parent == null)
                    {
                        Debug.WriteLine("[PlacedDeviceControl] ❌ Error: Parent is null, cannot calculate position");
                        ResetPositioningMode();
                        return;
                    }

                    // Validate parent dimensions to prevent division by zero or NaN
                    if (parent.Width <= 0 || parent.Height <= 0 || 
                        double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
                        double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Error: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
                        ResetPositioningMode();
                        return;
                    }

                    // Calculate new absolute position
                    var absX = this.X + this.TranslationX;
                    var absY = this.Y + this.TranslationY;

                    // Validate calculated absolute positions
                    if (double.IsNaN(absX) || double.IsNaN(absY) || 
                        double.IsInfinity(absX) || double.IsInfinity(absY))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Error: Invalid absolute position - X={absX}, Y={absY}");
                        ResetPositioningMode();
                        return;
                    }

                    // Calculate relative positions with bounds validation
                    var relativeX = absX / parent.Width;
                    var relativeY = absY / parent.Height;

                    // Validate and clamp relative positions to valid range [0.0, 1.0]
                    if (double.IsNaN(relativeX) || double.IsInfinity(relativeX))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Error: Invalid relativeX calculation - {relativeX}, using fallback 0.5");
                        relativeX = 0.5;
                    }
                    else
                    {
                        relativeX = Math.Clamp(relativeX, 0.0, 1.0);
                    }

                    if (double.IsNaN(relativeY) || double.IsInfinity(relativeY))
                    {
                        Debug.WriteLine($"[PlacedDeviceControl] ❌ Error: Invalid relativeY calculation - {relativeY}, using fallback 0.5");
                        relativeY = 0.5;
                    }
                    else
                    {
                        relativeY = Math.Clamp(relativeY, 0.0, 1.0);
                    }

                    // Update device position with validated values
                    placedDevice.RelativeX = relativeX;
                    placedDevice.RelativeY = relativeY;

                    // Reset translation
                    this.TranslationX = 0;
                    this.TranslationY = 0;

                    Debug.WriteLine($"[PlacedDeviceControl] ✅ Device moved to relative position: X={placedDevice.RelativeX:F3}, Y={placedDevice.RelativeY:F3}");
                    Debug.WriteLine($"[PlacedDeviceControl] Parent dimensions: Width={parent.Width:F1}, Height={parent.Height:F1}");
                    Debug.WriteLine($"[PlacedDeviceControl] Absolute position: X={absX:F1}, Y={absY:F1}");

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
                // Reset positioning mode on cancel
                ResetPositioningMode();
                Debug.WriteLine("[PlacedDeviceControl] Pan canceled");
                break;
        }
    }

    private void UpdateModelPosition()
    {
        if (BindingContext is not PlacedDeviceModel model || Parent is not VisualElement parent)
        {
            Debug.WriteLine("[PlacedDeviceControl] ❌ UpdateModelPosition: Invalid context or parent");
            return;
        }

        // Validate parent dimensions
        if (parent.Width <= 0 || parent.Height <= 0 || 
            double.IsNaN(parent.Width) || double.IsNaN(parent.Height) ||
            double.IsInfinity(parent.Width) || double.IsInfinity(parent.Height))
        {
            Debug.WriteLine($"[PlacedDeviceControl] ❌ UpdateModelPosition: Invalid parent dimensions - Width={parent.Width}, Height={parent.Height}");
            return;
        }

        double centerX = this.X + this.Width / 2;
        double centerY = this.Y + this.Height / 2;

        // Validate calculated center positions
        if (double.IsNaN(centerX) || double.IsNaN(centerY) || 
            double.IsInfinity(centerX) || double.IsInfinity(centerY))
        {
            Debug.WriteLine($"[PlacedDeviceControl] ❌ UpdateModelPosition: Invalid center position - X={centerX}, Y={centerY}");
            return;
        }

        // Calculate and validate relative positions
        var relativeX = centerX / parent.Width;
        var relativeY = centerY / parent.Height;

        if (double.IsNaN(relativeX) || double.IsInfinity(relativeX))
        {
            Debug.WriteLine($"[PlacedDeviceControl] ❌ UpdateModelPosition: Invalid relativeX - {relativeX}, using current value");
            relativeX = model.RelativeX; // Keep current value as fallback
        }
        else
        {
            relativeX = Math.Clamp(relativeX, 0.0, 1.0);
        }

        if (double.IsNaN(relativeY) || double.IsInfinity(relativeY))
        {
            Debug.WriteLine($"[PlacedDeviceControl] ❌ UpdateModelPosition: Invalid relativeY - {relativeY}, using current value");
            relativeY = model.RelativeY; // Keep current value as fallback
        }
        else
        {
            relativeY = Math.Clamp(relativeY, 0.0, 1.0);
        }

        model.RelativeX = relativeX;
        model.RelativeY = relativeY;

        Debug.WriteLine($"[PlacedDeviceControl] ✅ UpdateModelPosition: X={model.RelativeX:F3}, Y={model.RelativeY:F3}");
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
}
