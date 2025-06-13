using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ReisingerIntelliAppV1.Model.Models
{
    /// <summary>
    /// Repr�sentiert ein auf einem PDF-Bauplan platziertes Ger�t
    /// </summary>
    public partial class PlacedDeviceModel : ObservableObject
    {
        // Referenz zum Original-Ger�t
        public DeviceModel DeviceInfo { get; set; }

        // Direct properties for easy access (mirror some properties from DeviceInfo)
        [ObservableProperty]
        private string deviceId;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool isOnline;

        [ObservableProperty]
        private bool isVisible = false;

        // Position auf dem Bauplan (relativ zur PDF, Werte von 0.0 bis 1.0)
        [ObservableProperty]
        private double relativeX;

        [ObservableProperty]
        private double relativeY;

        [ObservableProperty]
        private double scale;

        // ID des Geb�udes und Stockwerks, auf dem das Ger�t platziert ist
        public int BuildingId { get; set; }
        public int FloorId { get; set; }

        // Status f�r UI-Anzeige
        [ObservableProperty]
        private bool isDoorOpen;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isInMoveMode;

        // Konstruktoren
        public PlacedDeviceModel()
        {
            IsDoorOpen = false;
            IsSelected = false;
        }

        public PlacedDeviceModel(DeviceModel device, double relativeX, double relativeY)
        {
            DeviceInfo = device;

            DeviceId = device.DeviceId;
            Name = device.Name;
            IsOnline = device.IsOnline;

            RelativeX = relativeX;
            RelativeY = relativeY;
            IsDoorOpen = false;
            IsSelected = false;
        }
    }
}