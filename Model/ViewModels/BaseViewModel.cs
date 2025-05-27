using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;
    public bool IsNotBusy => !IsBusy;

}
