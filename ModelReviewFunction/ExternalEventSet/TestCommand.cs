using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.ExternalEventSet
{

    public class TestCommand : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction trans = new Transaction(doc, "外部事件测试"))
            {
                trans.Start();
                SLUtils.DuplicateCurrentView(doc, uidoc);
                trans.Commit();
            }
        }

        public string GetName()
        {
            return "ExternalTestCommand";
        }
    }

}
