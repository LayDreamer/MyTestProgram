using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
    public class Problem : MyObservableObject
    {
        public UIDocument _uiDocument;
        public Document _document;
        public CommonExternalEvent _excuteHander;
        public ExternalEvent _externalEvent;
        public string _currentUserName;

        /// <summary>
        /// 楼号
        /// </summary>
        private string buildingNumber;
        public string BuildingNumber
        {
            get { return buildingNumber; }
            set { buildingNumber = value; RaisePropertyChanged(() => BuildingNumber); }
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
        /// 房间
        /// </summary>
        private string roomName;
        public string RoomName
        {
            get { return roomName; }
            set { roomName = value; RaisePropertyChanged(() => RoomName); }
        }

        /// <summary>
        /// 审核项
        /// </summary>
        private string auditItem;
        public string AuditItem
        {
            get { return auditItem; }
            set { auditItem = value; RaisePropertyChanged(() => AuditItem); }
        }

        /// <summary>
        /// 系统名称
        /// </summary>
        private string workSystemName;
        public string WorkSystemName
        {
            get { return workSystemName; }
            set { workSystemName = value; RaisePropertyChanged(() => WorkSystemName); }
        }

        /// <summary>
        /// 错误数量
        /// </summary>
        private int errorNumber;
        public int ErrorNumber
        {
            get { return errorNumber; }
            set { errorNumber = value; RaisePropertyChanged(() => ErrorNumber); }
        }

        /// <summary>
        /// 问题类别
        /// </summary>
        private ProblemCategoryModel problemCategory;
        public ProblemCategoryModel ProblemCategory
        {
            get { return problemCategory; }
            set { problemCategory = value; RaisePropertyChanged(() => ProblemCategory); }
        }

        /// <summary>
        /// 问题归类1
        /// </summary>
        private ProblemTypeModel problemType1;
        public ProblemTypeModel ProblemType1
        {
            get { return problemType1; }
            set { problemType1 = value; RaisePropertyChanged(() => ProblemType1); }
        }

        /// <summary>
        /// 问题归类2
        /// </summary>
        private List<ProblemTypeModel> problemType2;
        public List<ProblemTypeModel> ProblemType2
        {
            get { return problemType2; }
            set { problemType2 = value; RaisePropertyChanged(() => ProblemType2); }
        }

        /// <summary>
        /// 选中的问题归类2
        /// </summary>
        private ProblemTypeModel selectedProblemType2;
        public ProblemTypeModel SelectedProblemType2
        {
            get { return selectedProblemType2; }
            set
            {
                selectedProblemType2 = value;
                ProblemType1 = selectedProblemType2;

                ///添加问题归类2对应的问题描述集合
                var pd = new List<ProblemCategoryModel>();
                SelectedProblemType2.ProblemDes.ForEach(e => pd.Add(new ProblemCategoryModel(e)));
                ProblemDes = pd;
                RaisePropertyChanged(() => SelectedProblemType2);
            }
        }

        /// <summary>
        /// 问题描述
        /// </summary>
        private List<ProblemCategoryModel> problemDes;
        public List<ProblemCategoryModel> ProblemDes
        {
            get { return problemDes; }
            set
            {
                problemDes = value;
                RaisePropertyChanged(() => ProblemDes);
            }
        }

        /// <summary>
        /// 选中的问题描述
        /// </summary>
        private ProblemCategoryModel selectedProblemDes;
        public ProblemCategoryModel SelectedProblemDes
        {
            get { return selectedProblemDes; }
            set
            {
                selectedProblemDes = value;
                ProblemCategory = selectedProblemDes;
                RaisePropertyChanged(() => SelectedProblemDes);
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        private string remark;
        public string Remark
        {
            get { return remark; }
            set { remark = value; RaisePropertyChanged(() => Remark); }
        }

        /// <summary>
        /// 反馈人员/时间
        /// </summary>
        private string feedbackAndDate;
        public string FeedbackAndDate
        {
            get { return feedbackAndDate; }
            set { feedbackAndDate = value; RaisePropertyChanged(() => FeedbackAndDate); }
        }

        /// <summary>
        /// 销项人
        /// </summary>
        private string outputMan;
        public string OutputMan
        {
            get { return outputMan; }
            set { outputMan = value; RaisePropertyChanged(() => OutputMan); }
        }

        /// <summary>
        /// 问题状态
        /// </summary>
        private string problemStatus;
        public string ProblemStatus
        {
            get { return problemStatus; }
            set { problemStatus = value; RaisePropertyChanged(() => ProblemStatus); }
        }

        /// <summary>
        /// 销项状态文本显示
        /// </summary>
        private string outputText;
        public string OutputText
        {
            get { return outputText; }
            set { outputText = value; RaisePropertyChanged(() => OutputText); }
        }

        /// <summary>
        /// 复核状态文本显示
        /// </summary>
        private string reviewText;
        public string ReviewText
        {
            get { return reviewText; }
            set { reviewText = value; RaisePropertyChanged(() => ReviewText); }
        }

        /// <summary>
        /// 问题视口
        /// </summary>
        public ProblemViewPortModel ProblemViewPort { get; set; }

        /// <summary>
        /// 问题截图
        /// </summary>
        public ObservableCollection<ProblemScreenshot> problemScreenshots;
        public ObservableCollection<ProblemScreenshot> ProblemScreenshots
        {
            get { return problemScreenshots; }
            set { problemScreenshots = value; RaisePropertyChanged(() => ProblemScreenshots); }
        }

        /// <summary>
        /// 选中的问题截图
        /// </summary>
        public ProblemScreenshot selectedProblemScreenshot;
        public ProblemScreenshot SelectedProblemScreenshot
        {
            get { return selectedProblemScreenshot; }
            set { selectedProblemScreenshot = value; RaisePropertyChanged(() => SelectedProblemScreenshot); }
        }

        /// <summary>
        /// 操作记录
        /// </summary>
        private string operationData;
        public string OperationData
        {
            get { return operationData; }
            set { operationData = value; RaisePropertyChanged(() => OperationData); }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public Problem(CommonExternalEvent excuteHander, ExternalEvent externalEvent, Document document, UIDocument uiDocument)
        {
            _excuteHander = excuteHander;
            _externalEvent = externalEvent;
            _document = document;
            _uiDocument = uiDocument;
            _currentUserName = _document.Application.Username;
            ProblemScreenshots = new ObservableCollection<ProblemScreenshot> { new ProblemScreenshot() };
            ProblemType2 = new List<ProblemTypeModel>();
        }

        #region 执行命令

        /// <summary>
        /// 销项鼠标右击事件
        /// </summary>
        private RelayCommand outputMouseRightCommand;

        public RelayCommand OutputMouseRightCommand
        {
            get
            {
                if (outputMouseRightCommand == null)
                    outputMouseRightCommand = new RelayCommand(() => ExcuteOutputMouseRightCommand());
                return outputMouseRightCommand;

            }
            set { outputMouseRightCommand = value; }
        }

        /// <summary>
        /// 销项鼠标左击事件
        /// </summary>
        private RelayCommand outputMouseLeftCommand;

        public RelayCommand OutputMouseLeftCommand
        {
            get
            {
                if (outputMouseLeftCommand == null)
                    outputMouseLeftCommand = new RelayCommand(() => ExcuteOutputMouseLeftCommand());
                return outputMouseLeftCommand;

            }
            set { outputMouseLeftCommand = value; }
        }

        /// <summary>
        /// 复核鼠标右键事件
        /// </summary>
        private RelayCommand reviewMouseRightCommand;

        public RelayCommand ReviewMouseRightCommand
        {
            get
            {
                if (reviewMouseRightCommand == null)
                    reviewMouseRightCommand = new RelayCommand(() => ExcuteReviewMouseRightCommand());
                return reviewMouseRightCommand;

            }
            set { reviewMouseRightCommand = value; }
        }

        /// <summary>
        /// 复核鼠标左键事件
        /// </summary>
        private RelayCommand reviewMouseLeftCommand;

        public RelayCommand ReviewMouseLeftCommand
        {
            get
            {
                if (reviewMouseLeftCommand == null)
                    reviewMouseLeftCommand = new RelayCommand(() => ExcuteReviewMouseLeftCommand());
                return reviewMouseLeftCommand;

            }
            set { reviewMouseLeftCommand = value; }
        }

        /// <summary>
        /// 问题视口右击事件
        /// </summary>
        private RelayCommand createViewPortCommand;

        public RelayCommand CreateViewPortCommand
        {
            get
            {
                if (createViewPortCommand == null)
                    createViewPortCommand = new RelayCommand(() => ExcuteCreateViewPortCommand());
                return createViewPortCommand;

            }
            set { createViewPortCommand = value; }
        }

        /// <summary>
        /// 问题视口左击事件
        /// </summary>
        private RelayCommand viewPortLeftCommand;

        public RelayCommand ViewPortLeftCommand
        {
            get
            {
                if (viewPortLeftCommand == null)
                    viewPortLeftCommand = new RelayCommand(() => ExcuteViewPortLeftCommand());
                return viewPortLeftCommand;

            }
            set { viewPortLeftCommand = value; }
        }

        /// <summary>
        /// 问题视口双击
        /// </summary>
        private RelayCommand viewPortDoubleClickCommand;

        public RelayCommand ViewPortDoubleClickCommand
        {
            get
            {
                if (viewPortDoubleClickCommand == null)
                    viewPortDoubleClickCommand = new RelayCommand(() => ExcuteViewPortDoubleClickCommand());
                return viewPortDoubleClickCommand;

            }
            set { viewPortDoubleClickCommand = value; }
        }

        /// <summary>
        /// 问题状态双击事件
        /// </summary>
        private RelayCommand problemStatusDoubleClickCommand;

        public RelayCommand ProblemStatusDoubleClickCommand
        {
            get
            {
                if (problemStatusDoubleClickCommand == null)
                    problemStatusDoubleClickCommand = new RelayCommand(() => ExcuteOperationInfoCommand());
                return problemStatusDoubleClickCommand;

            }
            set { problemStatusDoubleClickCommand = value; }
        }

        #endregion

        #region 鼠标点击事件
        /// <summary>
        /// 复核鼠标右击
        /// </summary>
        public void ExcuteReviewMouseRightCommand()
        {

            if (OutputText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong))
            {
                MessageBox.Show("销项打×时，不可以进行复核操作", "错误提示");
                return;
            }
            if (!FeedbackAndDate.Contains(_currentUserName))
            {
                MessageBox.Show("当前用户与审核用户不符", "提示");
                return;
            }
            if (ReviewText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong))
            {
                return;
            }

            ReviewText = SLUtils.GetEnumDescription(CommonStatusEnum.wrong);
            OutputText = string.Empty;
            ErrorNumber += 1;
            CheckCurrentProblemStatus();
            GetOperationData("复核栏打×");
        }

        /// <summary>
        ///复核鼠标左击
        /// </summary>
        public void ExcuteReviewMouseLeftCommand()
        {
            if (OutputText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong))
            {
                MessageBox.Show("销项打×时，不可以进行复核操作", "提示");
                return;
            }

            if (!FeedbackAndDate.Contains(_currentUserName))
            {
                MessageBox.Show("当前用户与审核用户不符", "提示");
                return;
            }
            ReviewText = SLUtils.GetEnumDescription(CommonStatusEnum.right);
            CheckCurrentProblemStatus();

            GetOperationData("复核栏打√");
        }

        /// <summary>
        /// 销项鼠标右击
        /// </summary>
        public void ExcuteOutputMouseRightCommand()
        {
            OutputMan = _currentUserName;
            OutputText = SLUtils.GetEnumDescription(CommonStatusEnum.wrong);
            CheckCurrentProblemStatus();
            GetOperationData("销项栏打×");
        }

        /// <summary>
        ///销项鼠标左击
        /// </summary>
        public void ExcuteOutputMouseLeftCommand()
        {
            OutputMan = _currentUserName;
            OutputText = SLUtils.GetEnumDescription(CommonStatusEnum.right);
            CheckCurrentProblemStatus();
            GetOperationData("销项栏打√");
        }

        /// <summary>
        /// 问题视口右击，创建视口并居中高亮显示元素
        /// </summary>
        public void ExcuteCreateViewPortCommand()
        {
            if (_externalEvent != null)
            {
                _excuteHander.action = new Action<UIApplication>((app) =>
                {
                    try
                    {
                        using (Transaction tran = new Transaction(_document, "复制视口"))
                        {
                            tran.Start();
                            ProblemViewPort = SLUtils.DuplicateCurrentView(_document, _uiDocument);
                            tran.Commit();
                        }

                        ///视图切换，元素选中以及高亮
                        _uiDocument.ActiveView = _document.GetElement(new ElementId(ProblemViewPort.ProblemView)) as View;

                        List<ElementId> elementIds = new List<ElementId>();
                        ProblemViewPort.ProblemElementIds.ForEach(e => elementIds.Add(new ElementId(e)));

                        _uiDocument.Selection.SetElementIds(elementIds);
                        _uiDocument.ShowElements(elementIds);

                        GetSelectionModuleInfo(elementIds);

                        MessageBoxResult result = MessageBox.Show("是否直接创建问题截图？", "提示", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            string imagePath = SLUtils.AutoSaveImage();
                            SLUtils.RevitCurrentViewScreenShots(_document, imagePath);
                            if (!File.Exists(imagePath))
                            {
                                return;
                            }

                            System.Drawing.Image picture = System.Drawing.Image.FromFile(imagePath);
                            //byte[] imageByteArray = ImageHelper.ImageToByte(picture);

                            var initProblemScreenshot = ProblemScreenshots.Where(e => string.IsNullOrEmpty(e.ScreenshotFilePath)).FirstOrDefault();
                            if (initProblemScreenshot != null)
                            {
                                initProblemScreenshot.ScreenshotFilePath = imagePath;
                                //initProblemScreenshot.ScreenshotByteArray = imageByteArray;
                            }
                            else
                            {
                                ProblemScreenshots.Add(new ProblemScreenshot { ScreenshotFilePath = imagePath,/* ScreenshotByteArray = imageByteArray */});
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                });
                _externalEvent.Raise();
            }
        }

        /// <summary>
        /// 问题视口左击，高亮选中模块
        /// </summary>
        public void ExcuteViewPortLeftCommand()
        {
            try
            {
                if (ProblemViewPort == null) { return; }
                if (ProblemViewPort.ProblemElementIds.Count() != 0)
                {
                    OverrideGraphicSettings graphicSettings = SLUtils.GetOverrideGraphicSettings(_document, new Color(255, 0, 0));
                    ProblemViewPort.ProblemElementIds.ForEach(e => _document.ActiveView.SetElementOverrides(new ElementId(e), graphicSettings));

                    List<ElementId> elementIds = new List<ElementId>();
                    ProblemViewPort.ProblemElementIds.ForEach(e => elementIds.Add(new ElementId(e)));
                    ///元素选中以及高亮
                    _uiDocument.Selection.SetElementIds(elementIds);
                    _uiDocument.ShowElements(elementIds);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        ///问题视口双击，跳转视口，显示问题模块
        /// </summary>
        public void ExcuteViewPortDoubleClickCommand()
        {
            try
            {
                if (ProblemViewPort == null) { return; }
                ///视图切换
                if (ProblemViewPort.ProblemView != 0)
                {
                    _uiDocument.ActiveView = _document.GetElement(new ElementId(ProblemViewPort.ProblemView)) as View;/* ProblemViewPort.ProblemView;*/
                }

                ///元素选中以及高亮
                if (ProblemViewPort.ProblemElementIds.Count() != 0)
                {
                    OverrideGraphicSettings graphicSettings = SLUtils.GetOverrideGraphicSettings(_document, new Color(255, 0, 0));
                    ProblemViewPort.ProblemElementIds.ForEach(e => _uiDocument.ActiveView.SetElementOverrides(new ElementId(e), graphicSettings));

                    List<ElementId> elementIds = new List<ElementId>();
                    ProblemViewPort.ProblemElementIds.ForEach(e => elementIds.Add(new ElementId(e)));
                    _uiDocument.Selection.SetElementIds(elementIds);
                    _uiDocument.ShowElements(elementIds);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 显示操作记录
        /// </summary>
        public void ExcuteOperationInfoCommand()
        {
            MessageBox.Show(OperationData, "操作记录", MessageBoxButton.OK);
        }
        #endregion

        /// <summary>
        /// 根据当前销项和复核栏进行问题状态判定
        /// </summary>
        void CheckCurrentProblemStatus()
        {
            if (string.IsNullOrEmpty(ReviewText) && string.IsNullOrEmpty(OutputText))
            {
                ProblemStatus = SLUtils.GetEnumDescription(ProblemStatusEnum.NoOutput);
            }
            if (!string.IsNullOrEmpty(OutputText))
            {
                if (OutputText == SLUtils.GetEnumDescription(CommonStatusEnum.right))
                {
                    ProblemStatus = SLUtils.GetEnumDescription(ProblemStatusEnum.Outputed);
                }
                else if (OutputText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong))
                {
                    ProblemStatus = SLUtils.GetEnumDescription(ProblemStatusEnum.ReviewError);
                }
            }
            if (!string.IsNullOrEmpty(ReviewText))
            {
                if (ProblemStatus == SLUtils.GetEnumDescription(ProblemStatusEnum.ReviewError))
                {
                    return;
                }
                if (ReviewText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong))
                {
                    ProblemStatus = SLUtils.GetEnumDescription(ProblemStatusEnum.OutputError);
                }
                else if (ReviewText == SLUtils.GetEnumDescription(CommonStatusEnum.right))
                {
                    ProblemStatus = SLUtils.GetEnumDescription(ProblemStatusEnum.ClosedLoop);
                }
            }
        }

        /// <summary>
        /// 获取选中模块信息
        /// </summary>
        /// <param name="ids"></param>
        void GetSelectionModuleInfo(List<ElementId> ids)
        {
            List<string> systems = new List<string>();
            List<string> rooms = new List<string>();
            foreach (var id in ids)
            {
                FamilyInstance Instance = _document.GetElement(id) as FamilyInstance;


                string systemName = Instance.Symbol.GetStringValue("系统名称");
                if (!string.IsNullOrEmpty(systemName))
                {
                    systems.Add(systemName);
                }
            }
            if (systems.Count == 0)
            {
                MessageBox.Show("模块不包含系统名称", "提示");
            }
            else
            {
                if (systems.Count == 1)
                {
                    WorkSystemName = systems.FirstOrDefault();
                }
                else if (systems.Count > 1)
                {
                    WorkSystemName = ModelReviewConst.AllSpace;
                }
            }
        }

        /// <summary>
        /// 获取用户操作记录
        /// </summary>
        /// <param name="changeMessage"></param>
        void GetOperationData(string changeMessage)
        {
            OperationData += "▷ " + _currentUserName + "-" + changeMessage + "-" + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + "\r\n";
        }
    }
}
