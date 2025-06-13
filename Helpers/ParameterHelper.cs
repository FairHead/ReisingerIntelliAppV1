using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Helpers
{
    public static class ParameterHelper
    {
        public static void AssignParametersToEntries(Dictionary<string, string> parameters, Layout pageContent)
        {
            if (parameters == null || !parameters.Any())
            {
              
                return;
            }

            if (pageContent == null)
            {
       
                return;
            }

            foreach (var param in parameters)
            {
              
                // Extrahiere die ID aus dem Key (z. B. "id1" -> 1)
                if (int.TryParse(param.Key.Replace("id", ""), out int id))
                {
                    // Finde das entsprechende Entry-Feld anhand des x:FloorName (z. B. "EntryParam1")
                    var entryField = FindEntryFieldByName(pageContent, $"EntryParam{id}");

                    if (entryField != null)
                    {
                        // Weise den Wert dem Entry-Feld zu
                      
                        entryField.Text = param.Value;
                    }
                    else
                    {
                     
                    }
                }
                else
                {
                    Console.WriteLine($"Key {param.Key} konnte nicht geparst werden.");
                }
            }
        }

        private static Entry FindEntryFieldByName(Layout layout, string name)
        {
            if (layout == null)
            {
                Console.WriteLine("Fehler: Layout ist null.");
                return null;
            }

            // Suche das Element mit dem Namen (x:FloorName)
            return layout.FindByName<Entry>(name);
        }
    }
}
