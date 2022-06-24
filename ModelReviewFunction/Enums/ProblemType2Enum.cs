using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Enums
{
    /// <summary>
    /// 问题类型2集合
    /// </summary>
    public enum ProblemType2Enum
    {
        [Description("基础规范问题")]
        Base = 0,
        [Description("轴网标高问题")]
        GridLevel = 1,
        [Description("命名错误")]
        Named = 2,
        [Description("底图问题")]
        Reproduction = 3,
        [Description("加工信息问题")]
        Process = 4,
        [Description("碰撞问题")]
        Collision = 5,
        [Description("模块问题")]
        Module = 6,
        [Description("技术遗留问题")]
        Technology = 7,
        [Description("族遗留问题")]
        Family = 8
    }
}
