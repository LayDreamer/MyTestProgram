using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModelReviewFunction.ExternalEventSet;
using ModelReviewFunction.ViewModel;
using ModelReviewFunction.Model;
using System.IO;

namespace ModelReviewFunction.UI
{
    /// <summary>
    /// Project.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectUI : Window
    {
        public ProjectUI(CommonExternalEvent excuteHander, ExternalEvent externalEvent, Document document, UIDocument uiDocument)
        {
            InitializeComponent();
            ReviewProjectViewModel reviewProjectView = new ReviewProjectViewModel(this, excuteHander, externalEvent, document, uiDocument);
            this.DataContext = reviewProjectView;
        }
    }
}
