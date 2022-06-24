using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Enums
{
    /// <summary>
    /// 项目状态
    /// </summary>
    public enum ProjectStatusEnum
    {
        ///审核中，销项中，闭环
        //[Description("初始化")]
        //Init = 0,
        [Description("审核中")]
        Audit = 1,
        [Description("销项中")]
        OutPut = 2,
        [Description("闭环")]
        ClosedLoop = 3
        
    }

}
