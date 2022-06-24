using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Model
{
    public class ProblemCategoryModel
    {
        /// <summary>
        /// 问题类别
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 问题描述
        /// </summary>
        public string ProblemDes { get; set; }

        public ProblemCategoryModel(string inputProblemDes)
        {
            ProblemDes = inputProblemDes;
            CategoryName = ConvertDes(inputProblemDes);
        }

        /// <summary>
        /// 问题描述->问题类别
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string ConvertDes(string input)
        {
            string res = "";
            if (input == "项目参数信息错误" || input == "工作集错误" || input == "房间分割错误" || input == "项目基点错误" || input == "工作平面错误"
                || input == "轴网标高错误" || input == "轴网标高命名错误" || input == "文件命名错误" || input == "底图命名错误" || input == "房间命名错误"
                || input == "底图载入错误" || input == "底图平面偏移" || input == "品牌、材质型号错误" || input == "编码错误" || input == "编码多余"
                || input == "模块尺寸过短" || input == "模块尺寸过长" || input == "模块开孔错误" || input == "模块使用错误" || input == "房间计算点错误"
                || input == "房号信息错误" || input == "模块技术规则错误" || input == "模块定位错误")
            {
                res = "正确性";
            }
            else if (input == "房间分割缺少" || input == "视图多余分类" || input == "轴网标高缺失" || input == "底图载入信息缺失" || input == "底图载入缺失"
                || input == "品牌、材质型号缺少" || input == "编码缺少" || input == "模块缺少" || input == "房间计算点缺失" || input == "模块多余" || input == "房号信息缺失")
            {
                res = "完整性";
            }

            else if (input == "轴网标高未锁定" || input == "底图未锁定" || input == "后置成品与基层模块碰撞" || input == "面层与基层模块碰撞" || input == "土建与基层模块碰撞"
                || input == "点位与基层模块碰撞" || input == "面层与面层模块碰撞" || input == "后置成品与面层模块碰撞" || input == "风口与基层碰撞" || input == "模块尺寸优化"
                || input == "承重问题" || input == "优化问题" || input == "族信息填写错误")
            {
                res = "可优化性";
            }

            return res;
        }


    }
}
