using Autodesk.Revit.DB;
using GalaSoft.MvvmLight.Command;
using ModelReviewFunction.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ModelReviewFunction.Model
{
    /// <summary>
    /// 问题视口模型
    /// </summary>
    public class ProblemViewPortModel
    {
        /// <summary>
        /// 问题元素id
        /// </summary>
        //public List<ElementId> ProblemElementIds { get; set; } = new List<ElementId>();

        public List<int> ProblemElementIds { get; set; } = new List<int>();
        /// <summary>
        /// 问题视口
        /// </summary>
        //public View ProblemView { get; set; }

        public int ProblemView { get; set; }
    }

    /// <summary>
    /// 问题截图模型
    /// </summary>
    public class ProblemScreenshotModel
    {
        /// <summary>
        /// 截图地址
        /// </summary>
        public string ScreenshotFilePath { get; set; }

        //public byte[] ScreenshotByteArray { get; set; }
    }
}
