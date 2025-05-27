using ReisingerIntelliAppV1.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.TemplateSelector
{
    public class BuildingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BuildingTemplate { get; set; }
        public DataTemplate FloorTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                BuildingDisplayModel => BuildingTemplate,
                Floor => FloorTemplate,
                _ => null
            };
        }
    }
}
