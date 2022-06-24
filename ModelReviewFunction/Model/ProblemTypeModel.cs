using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Model
{
    public class ProblemTypeModel
    {
        /// <summary>
        /// 问题分类1
        /// </summary>
        public string Type1Name { get; set; }

        /// <summary>
        /// 问题分类2
        /// </summary>
        public string Type2Name { get; set; }

        /// <summary>
        /// 问题描述
        /// </summary>
        public List<string> ProblemDes { get; set; }

        public ProblemTypeModel(string inputProblemType2Name)
        {
            Type2Name = inputProblemType2Name;
            Type1Name = ConvertTypeName(inputProblemType2Name);
            ProblemDes = ConvertDes(inputProblemType2Name);
        }

        /// <summary>
        /// 问题描述->问题类别
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string ConvertTypeName(string input)
        {
            string res = "";
            if (input == "基础规范问题" || input == "轴网标高问题" || input == "命名错误" || input == "底图问题" || input == "加工信息问题"
                || input == "碰撞问题" || input == "模块问题" || input == "技术遗留问题" || input == "族遗留问题")
            {
                res = "装饰模型";
            }
            return res;
        }

        List<string> ConvertDes(string input)
        {
            List<string> resList = new List<string>();

            if (input == "基础规范问题")
            {
                resList = new List<string>
                {
                    "项目参数信息错误","工作集错误","房间分割错误","项目基点错误","工作平面错误","房间分割缺少","视图多余分类"
                };
            }
            else if (input == "轴网标高问题")
            {
                resList = new List<string>
                {
                    "轴网标高错误","轴网标高未锁定","轴网标高缺失",
                };
            }
            else if (input == "命名错误")
            {
                resList = new List<string>
                {
                    "轴网标高命名错误","文件命名错误","底图命名错误","房间命名错误",
                };
            }
            else if (input == "底图问题")
            {
                resList = new List<string>
                {
                    "底图载入错误","底图载入信息缺失","底图载入缺失","底图未锁定","底图平面偏移",
                };
            }
            else if (input == "加工信息问题")
            {
                resList = new List<string>
                {
                    "品牌、材质型号错误","品牌、材质型号缺少","编码缺少","编码错误","编码多余",
                };
            }
            else if (input == "碰撞问题")
            {
                resList = new List<string>
                {
                    "后置成品与基层模块碰撞","面层与基层模块碰撞","土建与基层模块碰撞","点位与基层模块碰撞","面层与面层模块碰撞","后置成品与面层模块碰撞","风口与基层碰撞",
                };
            }
            else if (input == "模块问题")
            {
                resList = new List<string>
                {
                    "模块缺少","模块尺寸过短","模块尺寸过长","模块开孔错误","模块使用错误","房间计算点缺失","房间计算点错误",
                    "房号信息错误","模块尺寸优化","模块技术规则错误","模块定位错误","模块多余","房号信息缺失",
                };
            }
            else if (input == "技术遗留问题")
            {
                resList = new List<string>
                {
                    "承重问题","优化问题",
                };
            }
            else if (input == "族遗留问题")
            {
                resList = new List<string>
                {
                    "族信息填写错误",
                };
            }
            return resList;
        }

    }
}
