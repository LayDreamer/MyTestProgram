using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Enums
{
    /// <summary>
    /// 普通状态
    /// </summary>
    public enum CommonStatusEnum
    {
        [Description("×")]
        wrong = 0,
        [Description("√")]
        right = 1,
    }
}
