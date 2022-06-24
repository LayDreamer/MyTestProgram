using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ModelReviewFunction.MVVM
{
    [Serializable]
    public class MyObject : MyObservableObject
    {
        private bool isSelected;
        [XmlAttribute]
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; RaisePropertyChanged(() => IsSelected); }
        }

    }
}
