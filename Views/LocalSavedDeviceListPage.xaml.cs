using System;
using Microsoft.Maui.Controls;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Views.DeviceControlViews;
using ReisingerIntelliAppV1.Views.ViewTestings;

namespace ReisingerIntelliAppV1.Views
{
    public partial class LocalSavedDeviceListPage : ContentPage
    {
        readonly LocalSavedDeviceListViewModel _vm;
        private bool _isNavigating = false;

        public LocalSavedDeviceListPage(LocalSavedDeviceListViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Reset des Navigations-Flags, wenn die Seite wieder erscheint
            _isNavigating = false;
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
            // Mehrfache Navigationsaufrufe verhindern
            if (_vm.IsBusy || _isNavigating) return;

            _vm.IsBusy = true;
            _isNavigating = true;

            try
            {
                DeviceModel device = null;

                if (sender is Button button)
                {
                    device = button.CommandParameter as DeviceModel;
                }
                else if (sender is ImageButton imageButton)
                {
                    device = imageButton.CommandParameter as DeviceModel;
                }

                if (device == null)
                {
                    _vm.IsBusy = false;
                    _isNavigating = false;
                    return;
                }

                // Delegiere an das ViewModel
                if (_vm.ConfigureCommand.CanExecute(device))
                {
                    await _vm.ConfigureCommand.ExecuteAsync(device);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Fehler beim Öffnen der Geräteeinstellungen: {ex.Message}", "OK");
            }
            finally
            {
                _vm.IsBusy = false;
                _isNavigating = false;
            }
        }

        public async void GoToDeviceInformationsPage(object sender, EventArgs e)
        {
            // Doppelte Navigationsaktionen verhindern
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                var button = sender as ImageButton;
                var device = button?.CommandParameter as DeviceModel;
                if (device == null)
                {
                    _isNavigating = false;
                    return;
                }

                var infoPage = App.ServiceProvider.GetRequiredService<DeviceInformationsPage>();
                await infoPage.InitializeWithAsync(device);
                await Navigation.PushAsync(infoPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Navigieren zur Infoseite: {ex.Message}");
                await DisplayAlert("Fehler", "Fehler beim Öffnen der Geräteinfos", "OK");
            }
            finally
            {
                // Nach Navigation wieder freigeben
                _isNavigating = false;
            }
        }
    }
}