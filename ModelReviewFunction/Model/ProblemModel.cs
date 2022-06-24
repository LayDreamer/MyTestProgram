using ModelReviewFunction.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace ModelReviewFunction.Model
{
    public class ProblemModel
    {
        /// <summary>
        /// 楼号
        /// </summary>
        [DisplayName("楼号")]
        public string BuildingNumber { get; set; }

        /// <summary>
        /// 户型
        /// </summary>
        [DisplayName("户型")]
        public string HouseType { get; set; }

        /// <summary>
        /// 房间
        /// </summary>
        [DisplayName("空间")]
        public string RoomName { get; set; }


        /// <summary>
        /// 审核项
        /// </summary>
        [DisplayName("审核项")]
        public string AuditItem { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        [DisplayName("系统名称")]
        public string WorkSystemName { get; set; }

        /// <summary>
        /// 错误数量
        /// </summary>
        [DisplayName("错误数量")]
        public int ErrorNumber { get; set; }

        /// <summary>
        /// 问题类别
        /// </summary>
        [DisplayName("问题类别")]
        public ProblemCategoryModel ProblemCategory { get; set; }

        /// <summary>
        /// 问题归类1
        /// </summary>
        [DisplayName("问题归类1")]
        public ProblemTypeModel ProblemType1 { get; set; }

        /// <summary>
        /// 问题归类2
        /// </summary>
        public List<ProblemTypeModel> ProblemType2 { get; set; }

        /// <summary>
        /// 选中的问题归类2
        /// </summary>
        [DisplayName("问题归类2")]
        public ProblemTypeModel SelectedProblemType2 { get; set; }

        /// <summary>
        /// 问题描述
        /// </summary>
        public List<ProblemCategoryModel> ProblemDes { get; set; }

        /// <summary>
        /// 选中的问题描述
        /// </summary>
        [DisplayName("问题描述")]
        public ProblemCategoryModel SelectedProblemDes { get; set; }

        /// <summary>
        /// 问题截图
        /// </summary>
        [DisplayName("问题截图")]
        public List<ProblemScreenshotModel> ProblemScreenshots { get; set; }

        /// <summary>
        /// 选中的截图
        /// </summary>
        public ProblemScreenshotModel SelectedProblemScreenshot { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 反馈人员/时间
        /// </summary>
        [DisplayName("反馈人员/时间")]
        public string FeedbackAndDate { get; set; }

        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("问题销项截图")]
        public string OutputScreenshot { get; set; }


        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("当前解决方案")]
        public string CurrentSolution { get; set; }


        /// <summary>
        /// 销项人
        /// </summary>
        [DisplayName("销项人")]
        public string OutputMan { get; set; }


        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("原因分析")]
        public string Reason { get; set; }

        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("产生影响")]
        public string Influence { get; set; }

        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("预防方案")]
        public string Prevention { get; set; }

        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("等级分析")]
        public string Level { get; set; }

        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("是否补单")]
        public string IsReplaceOrder { get; set; }


        /// <summary>
        /// 额外增加字段
        /// </summary>
        [DisplayName("实际补单（块）")]
        public string ActualReplaceOrder { get; set; }

        /// <summary>
        /// 问题状态
        /// </summary>
        public string ProblemStatus { get; set; }

        /// <summary>
        /// 销项状态文本显示
        /// </summary>
        public string OutputText { get; set; }

        /// <summary>
        /// 复核状态文本显示
        /// </summary>
        public string ReviewText { get; set; }

        /// <summary>
        /// 问题视口
        /// </summary>
        public ProblemViewPortModel ProblemViewPort { get; set; }

        /// <summary>
        /// 操作记录
        /// </summary>
        public string OperationData { get; set; }
    }
}
