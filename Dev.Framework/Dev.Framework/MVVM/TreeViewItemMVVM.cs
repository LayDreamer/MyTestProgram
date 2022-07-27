using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Framework.MVVM
{
    public class TreeViewItemMVVM : TreeViewItem
    {
        public TreeViewItemMVVM(string header, object _theObject, Style style)
        {
            this.Header = header;
            TheObject = _theObject;
            this.Style = style;
        }
        private object theObject;

        public object TheObject
        {
            get { return theObject; }
            set { theObject = value; }
        }
    }
}
