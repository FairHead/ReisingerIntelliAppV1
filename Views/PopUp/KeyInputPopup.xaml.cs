using CommunityToolkit.Maui.Views;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Views.PopUp;

public partial class KeyInputPopup : Popup
{
    public KeyInputPopup()
    {
        InitializeComponent();
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            // Optional: Feedback anzeigen  
            //Close(null);
            return;
        }

        var authData = new AuthDataModel
        {
            Username = username,
            Password = password
        };

       // Close(authData);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
      //  Close(null);
    }
}