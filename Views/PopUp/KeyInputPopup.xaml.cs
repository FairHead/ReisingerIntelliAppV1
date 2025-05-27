using CommunityToolkit.Maui.Views;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Views.PopUp;

public partial class KeyInputPopup : Popup
{
    // Ensure these fields are explicitly defined to avoid ambiguity  
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
            this.Close(null);
            return;
        }

        var authData = new AuthDataModel
        {
            Username = username,
            Password = password
        };

        this.Close(authData);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        this.Close(null);
    }
}
