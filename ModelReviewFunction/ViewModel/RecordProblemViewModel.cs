using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using GalaSoft.MvvmLight.Command;
using ModelReviewFunction.Enums;
using ModelReviewFunction.ExternalEventSet;
using ModelReviewFunction.Model;
using ModelReviewFunction.MVVM;
using ModelReviewFunction.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace ModelReviewFunction.ViewModel
{
    public class RecordProblemViewModel : MyObservableObject
    {
        /// <summary>
        /// 全局变量
        /// </summary>
        public UIDocument _uiDocument;
        public Document _document;
        public CommonExternalEvent _excuteHander;
        public ExternalEvent _externalEvent;
        public Window _window;
        public Project _project;

        public ObservableCollection<Room> _rooms;
        public ObservableCollection<string> _systemNames;
        public ProjectInfo _projectInfo;
        public string _docTitle;
        public string _houseType;
        public string _buildingNumber;
        public string _userName;
        #region 项目数据字段

        /// <summary>
        /// 显示的问题表
        /// </summary>
        private ObservableCollection<Problem> displayProblems;
        public ObservableCollection<Problem> DisplayProblems
        {
            get { return displayProblems; }
            set
            {
                displayProblems = value; RaisePropertyChanged(() => DisplayProblems);
            }
        }


        /// <summary>
        /// 选中的问题
        /// </summary>
        private Problem selectedProblem;
        public Problem SelectedProblem
        {
            get { return selectedProblem; }
            set
            {
                selectedProblem = value;
                if (selectedProblem == null)
                {

                }
                RaisePropertyChanged(() => SelectedProblem);
            }
        }

        /// <summary>
        /// 是否查看图片模式
        /// </summary>
        private bool isImageViewer;
        public bool IsImageViewer
        {
            get { return isImageViewer; }
            set { isImageViewer = value; RaisePropertyChanged(() => IsImageViewer); }
        }

        #endregion

        #region 执行命令

        /// <summary>
        /// 新增项目
        /// </summary>
        private RelayCommand addProblemCommand;

        public RelayCommand AddProblemCommand
        {
            get
            {
                if (addProblemCommand == null)
                    addProblemCommand = new RelayCommand(() => ExcuteAddProblemCommand());
                return addProblemCommand;

            }
            set { addProblemCommand = value; }
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        private RelayCommand delProblemCommand;

        public RelayCommand DelProblemCommand
        {
            get
            {
                if (delProblemCommand == null)
                    delProblemCommand = new RelayCommand(() => ExcuteDelProblemCommand());
                return delProblemCommand;

            }
            set { delProblemCommand = value; }
        }

        /// <summary>
        /// 问题总览
        /// </summary>
        private RelayCommand summaryProblemCommand;

        public RelayCommand SummaryProblemCommand
        {
            get
            {
                if (summaryProblemCommand == null)
                    summaryProblemCommand = new RelayCommand(() => ExcuteSummaryProblemCommand());
                return summaryProblemCommand;

            }
            set { summaryProblemCommand = value; }
        }

        private RelayCommand<CancelEventArgs> closingProblemSheetCommand;

        public RelayCommand<CancelEventArgs> ClosingProblemSheetCommand
        {
            get
            {
                if (closingProblemSheetCommand == null)
                    closingProblemSheetCommand = new RelayCommand<CancelEventArgs>((e) => ExcuteClosingProblemSheetCommand(e));
                return closingProblemSheetCommand;

            }
            set { closingProblemSheetCommand = value; }
        }

        /// <summary>
        /// 关闭用户控件
        /// </summary>
        private RelayCommand closeUserControlCommand;

        public RelayCommand CloseUserControlCommand
        {
            get
            {
                if (closeUserControlCommand == null)
                    closeUserControlCommand = new RelayCommand(() => ExcuteCloseUserControlCommand());
                return closeUserControlCommand;

            }
            set { closeUserControlCommand = value; }
        }

        public RelayCommand<MouseEventArgs> CmdMouseMove => new RelayCommand<MouseEventArgs>(MouseMove);

        #endregion

        #region 执行方法

        /// <summary>
        /// 新增
        /// </summary>
        public void ExcuteAddProblemCommand()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");// 2008-09-04

            Problem problem = new Problem(_excuteHander, _externalEvent, _document, _uiDocument)
            {
                BuildingNumber = _buildingNumber,
                HouseType = _houseType,
                RoomName = ModelReviewConst.AllSpace,
                WorkSystemName = ModelReviewConst.AllSpace,
                AuditItem = "模型审核",
                ErrorNumber = 1,
                FeedbackAndDate = _userName + "\r\n" + date,
                ProblemStatus = SLUtils.GetEnumDescription(ProblemStatusEnum.NoOutput),
                OperationData = "▷ " + _userName + "-" + "新建问题" + "-" + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + "\r\n"
            };

            AddProblemType2(problem);
            DisplayProblems = new ObservableCollection<Problem>
            {
                problem
            };
            _project._problems.Add(problem);
            _project.ProblemNumber = _project._problems.Count();
        }

        /// <summary>
        /// 问题总览
        /// </summary>
        public void ExcuteSummaryProblemCommand()
        {
            if (_project._problems != null && _project._problems.Count() >= 0)
            {
                DisplayProblems = _project._problems;
            }
        }

        /// <summary>
        /// 删除新增项
        /// </summary>
        public void ExcuteDelProblemCommand()
        {
            Problem lastProblem = _project._problems.LastOrDefault();
            if (lastProblem != null)
            {
                if (lastProblem.ProblemStatus == SLUtils.GetEnumDescription(ProblemStatusEnum.NoOutput) && lastProblem.FeedbackAndDate.Contains(_userName))
                {
                    _project._problems.Remove(lastProblem);
                    _project.ProblemNumber = _project._problems.Count();
                }
            }
        }


        /// <summary>
        /// 执行关闭窗口的方法
        /// </summary>
        public void ExcuteClosingProblemSheetCommand(CancelEventArgs e)
        {
            _project.IsEnabled = true;
            if (_project._problems.Any(x => string.IsNullOrEmpty(x.Remark)))
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("有备注栏为空,确认关闭?", "提醒", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    if (e != null)
                    {
                        e.Cancel = true;
                    }
                }
            }

            if (_project._problems.All(x => string.IsNullOrEmpty(x.OutputText) && string.IsNullOrEmpty(x.ReviewText)))
            {
                _project.Status = SLUtils.GetEnumDescription(ProjectStatusEnum.Audit);
            }
            else
            {
                if (_project._problems.Any(x => (string.IsNullOrEmpty(x.ReviewText))
                                                || (!string.IsNullOrEmpty(x.ReviewText) && x.ReviewText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong))))
                {
                    _project.Status = SLUtils.GetEnumDescription(ProjectStatusEnum.OutPut);
                }
                else if (_project._problems.All(x => !string.IsNullOrEmpty(x.ReviewText) && x.ReviewText == SLUtils.GetEnumDescription(CommonStatusEnum.right)))
                {
                    _project.Status = SLUtils.GetEnumDescription(ProjectStatusEnum.ClosedLoop);
                }
            }
        }


        /// <summary>
        /// 关闭用户控件
        /// </summary>
        void ExcuteCloseUserControlCommand()
        {
            IsImageViewer = false;
        }

        private void MouseMove(EventArgs e)
        {
            // 显示鼠标所在位置

        }

        #endregion

        public RecordProblemViewModel(Window window, CommonExternalEvent excuteHander, ExternalEvent externalEvent, Document document, UIDocument uiDocument, Project project)
        {
            _window = window;
            _excuteHander = excuteHander;
            _externalEvent = externalEvent;
            _document = document;
            _uiDocument = uiDocument;
            _project = project;

            try
            {
                ExcuteSummaryProblemCommand();
                _projectInfo = _document.ProjectInformation;
                _houseType = _projectInfo.GetStringValue("户型");
                _buildingNumber = _projectInfo.GetStringValue("楼号");
                _userName = _document.Application.Username;
            }
            catch (Exception ex)
            {
                MessageBox.Show("问题审核表信息初始化失败", "提醒");
            }
        }

        /// <summary>
        /// 获取模型中的系统名称
        /// </summary>
        /// <returns></returns>
        List<string> GetWorkSystemName()
        {
            List<string> systemNames = new List<string>();
            List<FamilyInstance> allFamilyInstance = _document.FilterElement<FamilyInstance>(BuiltInCategory.OST_GenericModel).ToList();
            if (allFamilyInstance.Count() == 0) { return systemNames; }

            foreach (var item in allFamilyInstance)
            {
                string systemName = item.Symbol.GetStringValue("系统名称");
                if (!string.IsNullOrEmpty(systemName))
                {
                    if (systemNames.Any(e => e == systemName)) { continue; }
                    systemNames.Add(systemName);
                }
            }
            return systemNames;
        }

        /// <summary>
        /// 添加问题描述和问题归类2
        /// </summary>
        /// <param name="problem"></param>
        void AddProblemType2(Problem problem)
        {
            /////添加问题描述集合
            //problem.ProblemDes = new List<Model.ProblemCategoryModel>();
            //ModelReviewConst.ProblemDesList.ForEach(e => problem.ProblemDes.Add(new Model.ProblemCategoryModel(e)));

            ///添加问题分类2集合
            //problem.ProblemType2 = new List<Model.ProblemTypeModel>();
            ModelReviewConst.ProblemType2List.ForEach(e => problem.ProblemType2.Add(new Model.ProblemTypeModel(e)));
        }
    }
}
