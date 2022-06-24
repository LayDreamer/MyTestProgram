
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Enums
{
    public enum WorkSystem
    {
        [Description("墙面系统")]
        wallSystem = 0,
        [Description("地面系统")]
        floorSystem = 1,
        [Description("顶面系统")]
        ceilingSystem = 2,
        [Description("厨房系统")]
        kitchenSystem = 3,
        [Description("卫浴系统")]
        sanitarySystem = 4,
    }
}
