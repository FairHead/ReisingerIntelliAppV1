using System;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Behaviors
{
    public class OctetValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnTextChanged;
            bindable.Unfocused += OnUnfocused;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnTextChanged;
            bindable.Unfocused -= OnUnfocused;
        }

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            // nur Ziffern erlauben
            if (string.IsNullOrEmpty(entry.Text)) return;
            var filtered = string.Concat(entry.Text.Where(char.IsDigit));
            if (filtered != entry.Text)
                entry.Text = filtered;
        }

        void OnUnfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            if (int.TryParse(entry.Text, out var val))
            {
                // clamp auf 0–255
                if (val > 255) entry.Text = "255";
                else if (val < 0) entry.Text = "0";
            }
            else
            {
                // Default 0
                entry.Text = "0";
            }
        }
    }
}