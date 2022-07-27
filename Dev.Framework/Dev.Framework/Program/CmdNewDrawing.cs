using Autodesk.Revit.Attributes;
using Dev.Framework.Base;

namespace Dev.Framework.Program
{
    [Transaction(TransactionMode.Manual)]
    //[Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class CmdNewDrawing : CmdBase
    {
        public override string CommandName => "加工图纸V2022";

        protected override void CmdExecute()
        {
             
        }
    }
}
