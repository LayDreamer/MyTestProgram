using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Enums
{
    /// <summary>
    /// 问题类别
    /// </summary>
    public enum ProblemCategoryEnum
    {
        [Description("正确性")]
        correctness = 1,
        [Description("可优化性")]
        optimized = 2,
        [Description("完整性")]
        integrality = 3
    }
}
