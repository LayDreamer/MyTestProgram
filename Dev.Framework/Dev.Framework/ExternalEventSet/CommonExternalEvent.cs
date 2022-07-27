using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Framework.ExternalEventSet
{
    public class CommonExternalEvent : IExternalEventHandler
    {
        /// <summary>
        /// 外部事件，在视图模型中进行功能实现
        /// </summary>

        public string Name { get; private set; }

        public Action<UIApplication> action { get; set; }

        public CommonExternalEvent(string name)
        {
            Name = name;
        }

        public void Execute(UIApplication app)
        {
            action?.Invoke(app);
        }

        public string GetName()
        {
            return Name;
        }

    }
}
