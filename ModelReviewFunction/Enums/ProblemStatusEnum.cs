using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Enums
{
    /// <summary>
    /// 问题状态
    /// </summary>
    public enum ProblemStatusEnum
    {
        [Description("未销项")]
        NoOutput = 0,
        [Description("已销项")]
        Outputed = 1,
        [Description("审核错误")]
        ReviewError = 2,
        [Description("销项错误")]
        OutputError = 3,
        [Description("已闭环")]
        ClosedLoop = 4
    }

}
