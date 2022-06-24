using ModelReviewFunction.MVVM;
using System;
using System.Collections.ObjectModel;

namespace ModelReviewFunction.Controls
{
    public class TreeNodeMVVM : MyObservableObject, ICloneable
    {
        public TreeNodeMVVM(string _name, object _theObject, TreeNodeMVVM _parent = null)
        {
            Name = _name;
            Tag = _theObject;
            Children = new ObservableCollection<TreeNodeMVVM>();
            parent = _parent;
        }
        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { isExpanded = value; }
        }

        string descrition;
        public string Descrition
        {
            get { return descrition; }
            set { descrition = value; }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                //if (Children != null)
                //{
                //    foreach (var item in Children)
                //    {
                //        item.IsSelected = isSelected;
                //    }
                //}
            }
        }
        object tag;
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        private ObservableCollection<TreeNodeMVVM> children;
        public ObservableCollection<TreeNodeMVVM> Children
        {
            get { return children; }
            set { children = value; }
        }

        TreeNodeMVVM parent;
        public TreeNodeMVVM Parent
        {
            get { return parent; }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
