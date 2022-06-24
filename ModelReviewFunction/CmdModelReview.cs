using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModelReviewFunction.ExternalEventSet;
using ModelReviewFunction.Framework;
using ModelReviewFunction.Model;
using ModelReviewFunction.UI;
using ModelReviewFunction.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace ModelReviewFunction
{
    [Transaction(TransactionMode.Manual)]
    //[Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    //[Journaling(Autodesk.Revit.Attributes.JournalingMode.UsingCommandData)]
    public class CmdModelReview : CmdBase
    {
        public override string CommandName => "模型审核-问题管理";

        protected override void CmdExecute()
        {
            CommonExternalEvent excuteHander = new CommonExternalEvent("ProjectReview");
            ExternalEvent externalEvent = ExternalEvent.Create(excuteHander);
            MaterialDesignThemes.Wpf.Theme theme = new MaterialDesignThemes.Wpf.Theme();
            MySqlFunction.CheckConnectState();

            ProjectUI projectWindow = new ProjectUI(excuteHander, externalEvent, _document, _uiDocument);
            projectWindow.Show();
            //MySqlFunction.UploadOrDownLoadData("测试", "的沙发沙发", false);
            //System.Windows.Interop.WindowInteropHelper mainUI = new System.Windows.Interop.WindowInteropHelper(projectWindow);
            //mainUI.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        }
    }
}
