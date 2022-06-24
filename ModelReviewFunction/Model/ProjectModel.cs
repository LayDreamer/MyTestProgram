using Autodesk.Revit.DB.Architecture;
using ModelReviewFunction.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction.Model
{
    public class ProjectModel
    {

        public List<ProblemModel> Problems { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 项目户型
        /// </summary>
        public string HouseType { get; set; }

        public List<RoomViewModel> Rooms { get; set; }

        public string SelectedRoom { get; set; }

        /// <summary>
        /// 所有的工作系统
        /// </summary>
        public List<RoomViewModel> WorkSystemNames { get; set; }


        /// <summary>
        /// 选中的工作系统名称
        /// </summary>

        public string SelectedWorkSystemName { get; set; }


        /// <summary>
        /// 项目问题数
        /// </summary>

        public int ProblemNumber { get; set; }


        /// <summary>
        /// 统计的分数
        /// </summary>
        public double Score { get; set; }


        /// <summary>
        /// 项目状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 项目是否被选中
        /// </summary>
        public bool IsChecked { get; set; }


        /// <summary>
        /// 项目审核是否可用
        /// </summary>
        public bool IsEnabled { get; set; }


        public ProjectModel()
        {
            Problems = new List<ProblemModel>();
            Rooms = new List<RoomViewModel>();
            WorkSystemNames = new List<RoomViewModel>();
        }
    }
}
