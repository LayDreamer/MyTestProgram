using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Dev.Framework.Base;
using Dev.Framework.ExternalEventSet;
using Dev.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Framework
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : CmdBase
    {
        public override string CommandName => "测试项目";

        protected override void CmdExecute()
        {
            ///注册外部事件
            CommonExternalEvent excuteHander = new CommonExternalEvent("ExternalEventTest");
            ExternalEvent externalEvent = ExternalEvent.Create(excuteHander);

            //ExampleUI exampleUI = new ExampleUI();
            //exampleUI.Show();
        }
    }
}
