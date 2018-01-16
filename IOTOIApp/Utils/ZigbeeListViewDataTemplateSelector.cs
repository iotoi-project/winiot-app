using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace IOTOIApp.Utils
{
    [ContentProperty(Name = "Items")]
    public class ZigbeeListViewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public Dictionary<string, DataTemplate> Items { get; set; }

        public static Dictionary<string, DataTemplate> DataTemplates { get; set; }

        public string DefaultTemplateStr = "InitialState";

        public ZigbeeListViewDataTemplateSelector()
        {
            Items = new Dictionary<string, DataTemplate>();
            DataTemplates = Items;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            //if (item == null)
            //{
            //    return DefaultTemplate;
            //}
            //var result = (
            //    from ii in item.GetType().GetTypeInfo().ImplementedInterfaces
            //    from dt in Items
            //    where ii.Name == dt.Key
            //    select dt.Value).FirstOrDefault();
            //return result ?? DefaultTemplate;

            DataTemplate DefaultTemplate;
            if (Items.TryGetValue(DefaultTemplateStr, out DefaultTemplate))
                return DefaultTemplate;
            else
                throw new Exception(String.Format("Key {0} was not found", DefaultTemplateStr));
        }
    }
}
