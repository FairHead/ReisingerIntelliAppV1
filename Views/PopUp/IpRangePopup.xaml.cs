using System;
using System.Linq;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;

namespace ReisingerIntelliAppV1.Views.PopUp
{
    // Ergebnis-Typ, der an den Aufrufer zurückgegeben wird
    public class IpRangePopupResult
    {
        public string StartIp { get; set; }
        public string EndIp { get; set; }
    }

    public partial class IpRangePopup : Popup
    {
        public IpRangePopup()
        {
            InitializeComponent();
        }

        private async void OnOkClicked(object sender, EventArgs e)
        {
            // Versuche, jedes Oktett zu parsen
            if (!TryParseOctet(S1.Text, out var s1) ||
                !TryParseOctet(S2.Text, out var s2) ||
                !TryParseOctet(S3.Text, out var s3) ||
                !TryParseOctet(S4.Text, out var s4) ||
                !TryParseOctet(E1.Text, out var e1) ||
                !TryParseOctet(E2.Text, out var e2) ||
                !TryParseOctet(E3.Text, out var e3) ||
                !TryParseOctet(E4.Text, out var e4))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ungültige Eingabe",
                    "Bitte geben Sie in jedem Feld eine Zahl zwischen 0 und 255 ein.",
                    "OK");
                return;
            }

            // Zusammensetzen zu IP-Strings
            string startIp = $"{s1}.{s2}.{s3}.{s4}";
            string endIp = $"{e1}.{e2}.{e3}.{e4}";

            // Numerisch vergleichen: Start ? Ende?
            uint numStart = IpToUint(s1, s2, s3, s4);
            uint numEnd = IpToUint(e1, e2, e3, e4);

            if (numStart > numEnd)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Bereich vertauscht",
                    "Die Start-IP muss kleiner oder gleich der End-IP sein.",
                    "OK");
                return;
            }

            // Alles validiert ? Popup schließen und Ergebnis zurückgeben
            Close(new IpRangePopupResult
            {
                StartIp = startIp,
                EndIp = endIp
            });
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close(null);
        }

        /// <summary>
        /// Versucht, den Text als Byte (0–255) zu parsen.
        /// Liefert false, wenn null/leer oder außerhalb des Bereichs.
        /// </summary>
        private bool TryParseOctet(string text, out byte value)
        {
            // kein Text ? invalid
            if (string.IsNullOrWhiteSpace(text))
            {
                value = 0;
                return false;
            }

            // Byte.TryParse erlaubt "0", "10", "255" usw.
            return byte.TryParse(text.Trim(), out value);
        }

        /// <summary>
        /// Wandelt vier Oktette in eine 32-Bit-Integer (Big-Endian) um.
        /// </summary>
        private uint IpToUint(byte a, byte b, byte c, byte d)
        {
            // .Reverse wegen Little-Endian Plattform-Ordnung
            return BitConverter.ToUInt32(new[] { d, c, b, a }, 0);
        }
    }
}
