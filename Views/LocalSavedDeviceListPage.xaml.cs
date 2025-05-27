using System;
using Microsoft.Maui.Controls;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;

namespace ReisingerIntelliAppV1.Views
{
    public partial class LocalSavedDeviceListPage : ContentPage
    {
        readonly LocalSavedDeviceListViewModel _vm;

        public LocalSavedDeviceListPage(
            LocalSavedDeviceListViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadAsync();
            _vm.StartOnlineStatusUpdater();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.StopOnlineStatusUpdater();
        }

        private async void OnConfigureButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn
                && btn.CommandParameter is DeviceModel device)
            {
                // Delegiere an das ViewModel
                if (_vm.ConfigureCommand.CanExecute(device))
                {
                    await _vm.ConfigureCommand.ExecuteAsync(device);
                }
            }
        }
    }
}