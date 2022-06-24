using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using GalaSoft.MvvmLight.Command;
using ModelReviewFunction.Enums;
using ModelReviewFunction.ExternalEventSet;
using ModelReviewFunction.Model;
using ModelReviewFunction.MVVM;
using ModelReviewFunction.UI;

namespace ModelReviewFunction.ViewModel
{
    public class Project : MyObservableObject
    {
        /// <summary>
        /// 全局变量
        /// </summary>
        public UIDocument _uiDocument;
        public Document _document;
        public CommonExternalEvent _excuteHander;
        public ExternalEvent _externalEvent;

        /// <summary>
        /// 一条项目包含的问题集合
        /// </summary>
        public ObservableCollection<Problem> _problems { get; set; } = new ObservableCollection<Problem>();

        /// <summary>
        /// 项目名称
        /// </summary>
        private string projectName;

        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; RaisePropertyChanged(() => ProjectName); }
        }

        /// <summary>
        /// 户型
        /// </summary>
        private string houseType;

        public string HouseType
        {
            get { return houseType; }
            set { houseType = value; RaisePropertyChanged(() => HouseType); }
        }

        /// <summary>
        /// 所有的房间
        /// </summary>
        //private ObservableCollection<string> rooms;
        //public ObservableCollection<string> Rooms
        //{
        //    get { return rooms; }
        //    set { rooms = value; RaisePropertyChanged(() => Rooms); }
        //}

        /// <summary>
        /// 选中的房间
        /// </summary>
        //private string selectedRoom;
        //public string SelectedRoom
        //{
        //    get { return selectedRoom; }
        //    set { selectedRoom = value; RaisePropertyChanged(() => SelectedRoom); }
        //}

        private ObservableCollection<RoomViewModel> rooms;
        public ObservableCollection<RoomViewModel> Rooms
        {
            get { return rooms; }
            set { rooms = value; RaisePropertyChanged(() => Rooms); }
        }

        private string selectedRoomName;
        public string SelectedRoomName
        {
            get { return selectedRoomName; }
            set { selectedRoomName = value; RaisePropertyChanged(() => SelectedRoomName); }
        }

        private ObservableCollection<RoomViewModel> workSystems;
        public ObservableCollection<RoomViewModel> WorkSystems
        {
            get { return workSystems; }
            set { workSystems = value; RaisePropertyChanged(() => WorkSystems); }
        }

        /// <summary>
        /// 所有的工作系统
        /// </summary>
        //private ObservableCollection<string> workSystemNames;
        //public ObservableCollection<string> WorkSystemNames
        //{
        //    get { return workSystemNames; }
        //    set { workSystemNames = value; RaisePropertyChanged(() => WorkSystemNames); }
        //}

        /// <summary>
        /// 选中的工作系统名称
        /// </summary>
        private string selectedWorkSystemName;
        public string SelectedWorkSystemName
        {
            get { return selectedWorkSystemName; }
            set { selectedWorkSystemName = value; RaisePropertyChanged(() => SelectedWorkSystemName); }
        }

        /// <summary>
        /// 项目问题数
        /// </summary>
        private int problemNumber;
        public int ProblemNumber
        {
            get { return problemNumber; }
            set { problemNumber = value; RaisePropertyChanged(() => ProblemNumber); }
        }

        /// <summary>
        /// 统计的分数
        /// </summary>
        private double score;
        public double Score
        {
            get { return score; }
            set { score = value; RaisePropertyChanged(() => Score); }
        }

        /// <summary>
        /// 项目状态
        /// </summary>
        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged(() => Status); }
        }

        /// <summary>
        /// 项目是否被选中
        /// </summary>
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                RaisePropertyChanged(() => IsChecked);
            }
        }

        /// <summary>
        /// 项目审核是否可用
        /// </summary>
        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value; RaisePropertyChanged(() => IsEnabled);
            }
        }
        #region 执行命令
        /// <summary>
        /// 打开审核问题表
        /// </summary>
        private RelayCommand openProblemSheetCommand;

        public RelayCommand OpenProblemSheetCommand
        {
            get
            {
                if (openProblemSheetCommand == null)
                    openProblemSheetCommand = new RelayCommand(() => ExcuteOpenProblemSheetCommand());
                return openProblemSheetCommand;

            }
            set { openProblemSheetCommand = value; }
        }

        #endregion


        #region 执行方法
        /// <summary>
        /// 打开审核问题表
        /// </summary>
        void ExcuteOpenProblemSheetCommand()
        {
            try
            {
                IsEnabled = false;
                ProblemSheet problemSheet = new ProblemSheet(_excuteHander, _externalEvent, _document, _uiDocument, this);
                problemSheet.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("审核页面异常:" + ex.Message, "提醒");
            }

        }
        #endregion


        WorkSystem SystemNameToEnum(string ss)
        {
            WorkSystem workSystem = new WorkSystem();
            if (ss == SLUtils.GetEnumDescription(WorkSystem.wallSystem))
            {
                workSystem = WorkSystem.wallSystem;
            }
            else if (ss == SLUtils.GetEnumDescription(WorkSystem.floorSystem))
            {
                workSystem = WorkSystem.floorSystem;
            }
            else if (ss == SLUtils.GetEnumDescription(WorkSystem.ceilingSystem))
            {
                workSystem = WorkSystem.ceilingSystem;
            }
            else if (ss == SLUtils.GetEnumDescription(WorkSystem.kitchenSystem))
            {
                workSystem = WorkSystem.kitchenSystem;
            }
            else if (ss == SLUtils.GetEnumDescription(WorkSystem.sanitarySystem))
            {
                workSystem = WorkSystem.sanitarySystem;
            }
            return workSystem;
        }

    }
}
