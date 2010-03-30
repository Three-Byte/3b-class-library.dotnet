using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ThreeByte.Converters
{
    public class ListViewItemStyleSelector : StyleSelector
    {
        private int i = 0;
        public override Style SelectStyle(object item, DependencyObject container) {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(container);
            if(item == ic.Items[0]) {
                i = 0;
            }
            string styleKey;
            if(i % 2 == 0) {
                styleKey = "ListViewItemStyle1";
            } else {
                styleKey = "ListViewItemStyle2";
            }
            i++;
            return (Style)(ic.FindResource(styleKey));
        }



    }

}
