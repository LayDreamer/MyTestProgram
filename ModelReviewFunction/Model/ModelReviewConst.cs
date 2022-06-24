using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Model
{
    public class ModelReviewConst
    {
        ///添加问题描述集合
        public static readonly List<string> ProblemDesList = new List<string>
            {
                "项目参数信息错误","工作集错误","房间分割错误","项目基点错误","工作平面错误","房间分割缺少","视图多余分类",
                "轴网标高错误" ,"轴网标高未锁定", "轴网标高缺失",
                "轴网标高命名错误" ,  "文件命名错误" , "底图命名错误" , "房间命名错误",
                "底图载入错误" ,"底图载入信息缺失",  "底图载入缺失" ,"底图未锁定", "底图平面偏移",
                "品牌、材质型号错误", "品牌、材质型号缺少" ,"编码缺少","编码错误", "编码多余","加工说明错误",
                "后置成品与基层模块碰撞" , "面层与基层模块碰撞" , "土建与基层模块碰撞", "点位与基层模块碰撞","面层与面层模块碰撞" ,"后置成品与面层模块碰撞", "风口与基层碰撞",
                "模块缺少" ,"模块尺寸过短" ,"模块尺寸过长", "模块开孔错误" , "模块使用错误","房间计算点缺失" ,"房间计算点错误","房号信息错误","模块尺寸优化" ,"模块技术规则错误","模块定位错误","模块多余","房号信息缺失",
                "承重问题" ,"优化问题","族信息填写错误",
            };


        ///添加问题分类2集合
        public static readonly List<string> ProblemType2List = new List<string>
            {
                "基础规范问题",
                "轴网标高问题",
                "命名错误",
                "底图问题",
                "加工信息问题",
                "碰撞问题",
                "模块问题",
                "技术遗留问题",
                "族遗留问题"
            };


        public const string AllSpace = "全空间";
        public const string DefaultTestPath = @"d:\desktop\桌面\测试模型\ProjectInfo.json";
        public const string FunctionName = "ProjectReview";
    }
}
