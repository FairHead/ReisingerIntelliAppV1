using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ReisingerIntelliAppV1.Model.Models
{
    /// <summary>
    /// Repräsentiert ein auf einem PDF-Bauplan platziertes Gerät
    /// </summary>
    public partial class PlacedDeviceModel : ObservableObject
    {
        // Referenz zum Original-Gerät
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

        // ID des Gebäudes und Stockwerks, auf dem das Gerät platziert ist
        public int BuildingId { get; set; }
        public int FloorId { get; set; }

        // Status für UI-Anzeige
        [ObservableProperty]
        private bool isDoorOpen;

        [ObservableProperty]
        private bool isSelected;



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