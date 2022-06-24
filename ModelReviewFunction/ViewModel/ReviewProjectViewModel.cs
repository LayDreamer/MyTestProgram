using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GalaSoft.MvvmLight.Command;
using ModelReviewFunction.Enums;
using ModelReviewFunction.ExternalEventSet;
using ModelReviewFunction.Model;
using ModelReviewFunction.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using MySql.Data.MySqlClient;

namespace ModelReviewFunction.ViewModel
{
    public class ReviewProjectViewModel : MyObservableObject
    {
        /// <summary>
        /// 全局变量
        /// </summary>
        public UIDocument _uiDocument;
        public Document _document;
        public CommonExternalEvent _excuteHander;
        public ExternalEvent _externalEvent;
        public Window _window;

        //public ObservableCollection<RoomViewModel> _rooms = new ObservableCollection<RoomViewModel>();
        //public ObservableCollection<string> _systemNames;
        public ProjectInfo _projectInfo;
        public string _docTitle;
        public string _houseType;
        public string _projectUserName;

        public List<string> filePaths = new List<string>();

        #region 项目数据字段

        /// <summary>
        /// 显示的项目集
        /// </summary>
        private ObservableCollection<Project> projects;
        public ObservableCollection<Project> Projects
        {
            get { return projects; }
            set
            {
                projects = value; RaisePropertyChanged(() => Projects);
            }
        }

        /// <summary>
        /// 已保存的项目
        /// </summary>
        private ObservableCollection<Project> savedProjects;
        public ObservableCollection<Project> SavedProjects
        {
            get { return savedProjects; }
            set
            {
                savedProjects = value; RaisePropertyChanged(() => SavedProjects);
            }
        }

        /// <summary>
        /// 选中的项目
        /// </summary>
        private Project selectedProject;
        public Project SelectedProject
        {
            get { return selectedProject; }
            set
            {
                selectedProject = value;
                RaisePropertyChanged(() => SelectedProject);
            }
        }

        /// <summary>
        /// 项目数
        /// </summary>
        private int projectNumber;
        public int ProjectNumber
        {
            get { return projectNumber; }
            set
            {
                projectNumber = value; RaisePropertyChanged(() => ProjectNumber);
            }
        }

        private List<string> systemNames;
        public List<string> SystemNames
        {
            get { return systemNames; }
            set
            {
                systemNames = value; RaisePropertyChanged(() => SystemNames);
            }
        }

        /// <summary>
        /// 是否在选择房间页面
        /// </summary>
        private bool isRoomViewer;
        public bool IsRoomViewer
        {
            get { return isRoomViewer; }
            set { isRoomViewer = value; RaisePropertyChanged(() => IsRoomViewer); }
        }

        /// <summary>
        /// 是否在选择工作系统页面
        /// </summary>
        private bool isWorkSystemViewer;
        public bool IsWorkSystemViewer
        {
            get { return isWorkSystemViewer; }
            set { isWorkSystemViewer = value; RaisePropertyChanged(() => IsWorkSystemViewer); }
        }
        #endregion

        #region 执行命令

        /// <summary>
        /// 新增项目命令
        /// </summary>
        private RelayCommand addProjectCommand;
        public RelayCommand AddProjectCommand
        {
            get
            {
                if (addProjectCommand == null)
                    addProjectCommand = new RelayCommand(() => ExcuteAddProjectCommand());
                return addProjectCommand;

            }
            set { addProjectCommand = value; }
        }

        /// <summary>
        /// 删除项目命令
        /// </summary>
        private RelayCommand delProjectCommand;
        public RelayCommand DelProjectCommand
        {
            get
            {
                if (delProjectCommand == null)
                    delProjectCommand = new RelayCommand(() => ExcuteDelProjectCommand());
                return delProjectCommand;

            }
            set { delProjectCommand = value; }
        }

        /// <summary>
        /// 项目总览命令
        /// </summary>
        private RelayCommand summaryProjectCommand;
        public RelayCommand SummaryProjectCommand
        {
            get
            {
                if (summaryProjectCommand == null)
                    summaryProjectCommand = new RelayCommand(() => ExcuteSummaryProjectCommand());
                return summaryProjectCommand;

            }
            set { summaryProjectCommand = value; }
        }

        /// <summary>
        /// 保存项目命令
        /// </summary>
        private RelayCommand saveProjectCommand;
        public RelayCommand SaveProjectCommand
        {
            get
            {
                if (saveProjectCommand == null)
                    saveProjectCommand = new RelayCommand(() => ExcuteSaveProjectCommand());
                return saveProjectCommand;

            }
            set { saveProjectCommand = value; }
        }

        private RelayCommand updateProjectCommand;
        public RelayCommand UpdateProjectCommand
        {
            get
            {
                if (updateProjectCommand == null)
                    updateProjectCommand = new RelayCommand(() => ExcuteUpdateProjectCommand());
                return updateProjectCommand;

            }
            set { updateProjectCommand = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private RelayCommand selectedChangedCommand;
        public RelayCommand SelectedChangedCommand
        {
            get
            {
                if (selectedChangedCommand == null)
                    selectedChangedCommand = new RelayCommand(() => ExcuteSelectedChangedCommand());
                return selectedChangedCommand;

            }
            set { selectedChangedCommand = value; }
        }

        /// <summary>
        /// 表头统计分数
        /// </summary>
        private RelayCommand calculateScoreCommand;
        public RelayCommand CalculateScoreCommand
        {
            get
            {
                if (calculateScoreCommand == null)
                    calculateScoreCommand = new RelayCommand(() => ExcuteCalculateScoreCommand());
                return calculateScoreCommand;

            }
            set { calculateScoreCommand = value; }
        }


        /// <summary>
        /// 表头选择栏
        /// </summary>
        private RelayCommand checkedCommand;

        public RelayCommand CheckedCommand
        {
            get
            {
                if (checkedCommand == null)
                    checkedCommand = new RelayCommand(() => ExcuteCheckedCommand());
                return checkedCommand;

            }
            set { checkedCommand = value; }
        }


        /// <summary>
        /// 导出项目
        /// </summary>
        private RelayCommand exportProjectCommand;

        public RelayCommand ExportProjectCommand
        {
            get
            {
                if (exportProjectCommand == null)
                    exportProjectCommand = new RelayCommand(() => ExcuteExportProjectCommand());
                return exportProjectCommand;

            }
            set { exportProjectCommand = value; }
        }

        /// <summary>
        /// 选择房间
        /// </summary>
        private RelayCommand chooseRoomCommand;

        public RelayCommand ChooseRoomCommand
        {
            get
            {
                if (chooseRoomCommand == null)
                    chooseRoomCommand = new RelayCommand(() => ExcuteChooseRoomCommand());
                return chooseRoomCommand;

            }
            set { chooseRoomCommand = value; }
        }

        /// <summary>
        /// 选择工作系统
        /// </summary>
        private RelayCommand chooseWorkSystemCommand;

        public RelayCommand ChooseWorkSystemCommand
        {
            get
            {
                if (chooseWorkSystemCommand == null)
                    chooseWorkSystemCommand = new RelayCommand(() => ExcuteChooseWorkSystemCommand());
                return chooseWorkSystemCommand;

            }
            set { chooseWorkSystemCommand = value; }
        }

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
        #endregion

        #region 执行方法

        /// <summary>
        /// 新增
        /// </summary>
        public void ExcuteAddProjectCommand()
        {
            Project newProject = new Project
            {
                _excuteHander = this._excuteHander,
                _externalEvent = this._externalEvent,
                _document = this._document,
                _uiDocument = this._uiDocument,
                ProjectName = _docTitle,
                HouseType = _houseType,
                //Rooms = new ObservableCollection<RoomViewModel>(),
                //WorkSystems = new ObservableCollection<RoomViewModel>(),
                Rooms = new ObservableCollection<RoomViewModel>(GetRooms()),
                WorkSystems = new ObservableCollection<RoomViewModel>(GetWorkSystemName()),
                IsEnabled = true,
            };
            Projects.Add(newProject);
        }

        /// <summary>
        /// 总览
        /// </summary>
        public void ExcuteSummaryProjectCommand()
        {
            //ObservableCollection<Room> rooms = SLUtils.ListToObservableCollection(_rooms);
            //Project newProject = new Project
            //{
            //    ProjectName = _docTitle,
            //    HouseType = _houseType,
            //    Rooms = rooms,
            //};
            //Projects = allProjects;

            //ProjectNumber = Projects.Count();
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        public void ExcuteDelProjectCommand()
        {
            string errors = "";

            if (Projects.All(e => e.IsChecked == false))
            {
                MessageBox.Show("请勾选需要删除的项目", "提醒");
                return;
            }
            ObservableCollection<Project> delProjects = new ObservableCollection<Project>();
            for (int i = 0; i < Projects.Count; i++)
            {
                if (!Projects[i].IsChecked) { continue; }
                if (_projectUserName != _document.Application.Username)
                {
                    errors += $"第{i + 1}个项目无法删除：非审核人" + "\r\n";
                    continue;
                }
                if (Projects[i].Status != SLUtils.GetEnumDescription(ProjectStatusEnum.Audit))
                {
                    errors += $"第{i + 1}个项目无法删除：项目状态非审核中" + "\r\n";
                    continue;
                }
                if (Projects[i].IsChecked)
                {
                    delProjects.Add(Projects[i]);
                }
            }
            foreach (var item in delProjects)
            {
                Projects.Remove(item);
            }
            if (!string.IsNullOrEmpty(errors))
            {
                MessageBox.Show(errors, "提醒");
            }
        }

        /// <summary>
        /// 上传数据
        /// </summary>
        public void ExcuteSaveProjectCommand()
        {
            try
            {
                string savePath = SLUtils.AutoSaveJson();
                ObservableCollection<Project> selectedProjects = new ObservableCollection<Project>();
                foreach (Project item in Projects)
                {
                    if (!item.IsChecked)
                        continue;
                    selectedProjects.Add(item);
                }
                ///选中内容即上传
                if (selectedProjects.Count() == 0)
                {
                    MessageBox.Show("请选择需要上传的项目！", "提醒");
                    return;
                }
                List<ProjectModel> projectModels = ProjectViewModelToModel(selectedProjects);

                SvnHelper svnHelper = new SvnHelper(SLUtils.SVNImageSavePath, ModelReviewConst.FunctionName);

                foreach (ProjectModel item in projectModels)
                {
                    if (item.Problems.Count > 0)
                    {
                        item.Problems.ForEach(e => filePaths.AddRange(e.ProblemScreenshots.Select(x => x.ScreenshotFilePath)));
                    }

                    string tableName = item.ProjectName + item.HouseType + item.SelectedRoom + item.SelectedWorkSystemName;
                    string projectInfo = JsonHelper.SerializeObject(item);

                    svnHelper.Add(filePaths, null);
                    svnHelper.Commit(out SharpSvn.SvnCommitResult svnCommit);
                    
                    MySqlFunction.UploadOrDownLoadData(tableName.Trim(), projectInfo, false);
                }

                #region  将测试内容存在本地文件中
                //string json = JsonHelper.SerializeObject(projectModels);
                //FileHelper.MakeDir(savePath, true);
                //using (StreamWriter output = File.CreateText(savePath))
                //{
                //    output.WriteLine(json);
                //    output.Close();
                //}
                #endregion


                MessageBox.Show("数据上传成功！", "提醒");
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// 同步更新数据
        /// </summary>
        public void ExcuteUpdateProjectCommand()
        {
            try
            {
                ObservableCollection<Project> updateProjects = new ObservableCollection<Project>();
                ObservableCollection<Project> newProjects = new ObservableCollection<Project>();
                if (Projects.All(e => !e.IsChecked))
                {
                    MessageBox.Show("请选择需要同步的项目！", "提醒");
                    return;
                }

                ///svn下载图片到本地文件夹
                SvnHelper svnHelper = new SvnHelper(SLUtils.SVNImageSavePath, ModelReviewConst.FunctionName);
                svnHelper.CheckOut(out SharpSvn.SvnUpdateResult svnUpdateResult);

                Dictionary<string, string> projectDic = MySqlFunction.UploadOrDownLoadData();
                if (projectDic.Count() == 0) { return; }

                foreach (var item in Projects)
                {
                    if (item._problems.Count() != 0 || !item.IsChecked)
                    {
                        newProjects.Add(item);
                        continue;
                    }
                    else
                    {
                        foreach (var projectData in projectDic)
                        {
                            string nameJson = projectData.Key;
                            string infoJson = projectData.Value;
                            string pattern = @"(\\[^bfrnt\\/'\""])";
                            infoJson = Regex.Replace(infoJson, pattern, "\\$1");
                            ProjectModel downloadProject = JsonHelper.DeserializeJsonToObject<ProjectModel>(infoJson);
                            try
                            {
                                if (item.ProjectName == downloadProject.ProjectName && item.HouseType == downloadProject.HouseType
                                                                   && item.SelectedRoomName.Trim() == downloadProject.SelectedRoom.Trim()
                                                                   && item.SelectedWorkSystemName.Trim() == downloadProject.SelectedWorkSystemName.Trim())
                                {
                                    Project webProject = ProjectModelToViewModel(new List<ProjectModel> { downloadProject }).FirstOrDefault();
                                    updateProjects.Add(webProject);
                                    newProjects.Add(webProject);
                                }
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                    }
                }

                #region 读取存在本地的文件
                //if (!File.Exists(savePath))
                //    return;
                //var localFile = FileHelper.GetTextFromFile(savePath);
                //if (string.IsNullOrEmpty(localFile))
                //    return;
                //List<ProjectModel> projects = JsonHelper.DeserializeJsonToList<ProjectModel>(localFile);
                #endregion

                if (updateProjects.Count == 0)
                {
                    MessageBox.Show("更新失败，服务器没有匹配项目！", "提醒");
                    return;
                }

                Projects = newProjects;
                ProjectNumber = Projects.Where(e => e.IsChecked).Count();
                FileHelper.CopyFiles(Path.Combine(SLUtils.SVNImageSavePath, ModelReviewConst.FunctionName), SLUtils.LocalImageSavePath);
                MessageBox.Show("数据更新成功！", "提醒");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新失败！", "提醒");
                return;
            }
        }

        /// <summary>
        /// 项目选择数字变化
        /// </summary>
        public void ExcuteSelectedChangedCommand()
        {
            ProjectNumber = Projects.Where(e => e.IsChecked).Count();
        }

        /// <summary>
        /// 项目分数计算
        /// </summary>
        void ExcuteCalculateScoreCommand()
        {
            if (Projects.All(e => e.IsChecked == false))
            {
                MessageBox.Show("请勾选需要统计的项目", "提醒");
                return;
            }
            List<Project> resProjects = new List<Project>();
            string error = "";
            foreach (var project in Projects)
            {
                if (project.IsChecked)
                {
                    if (project._problems.Count == 0)
                    {
                        error = "选中项目中有问题表为空!";
                        break;
                    }
                    if (project._problems.Any(y => y.ReviewText != SLUtils.GetEnumDescription(CommonStatusEnum.right)))
                    {
                        error = "统计分数前，请确认选中项目的审核栏全部打√";
                        break;
                    }
                    if (project._problems.Any(e => e.SelectedProblemDes == null))
                    {
                        error = "选中项中，有问题描述未选择!";
                        break;
                    }
                    resProjects.Add(project);
                }
            }

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "提醒");
                return;
            }
            foreach (var resProject in resProjects)
            {
                resProject.Score = StatisticalScores(resProject._problems);
            }

        }

        /// <summary>
        /// 项目全选/全不选
        /// </summary>
        void ExcuteCheckedCommand()
        {
            if (Projects.All(e => e.IsChecked))
            {
                Projects.Where(e => e.IsChecked).ToList().ForEach(x => x.IsChecked = false);
            }
            else
            {
                Projects.Where(e => !e.IsChecked).ToList().ForEach(x => x.IsChecked = true);
            }
        }

        /// <summary>
        /// 导出项目
        /// </summary>
        void ExcuteExportProjectCommand()
        {
            DataTable newTB = new DataTable();
            var selectedProjects = Projects.Where(e => e.IsChecked);
            if (selectedProjects.Count() == 0)
            {
                MessageBox.Show("请选择需要导出的项目！", "提醒");
                return;
            }
            foreach (var project in selectedProjects)
            {
                List<ProblemModel> list = ProblemViewModelToModel(project._problems);
                newTB = SLUtils.IEnumerableToDataTable(list);
                SLUtils.ExcelExport1(newTB, "具体问题记录表");
            }
        }

        /// <summary>
        /// 显示房间选择页面
        /// </summary>
        void ExcuteChooseRoomCommand()
        {
            IsRoomViewer = true;
        }

        /// <summary>
        /// 显示工作系统选择页面
        /// </summary>
        void ExcuteChooseWorkSystemCommand()
        {
            IsWorkSystemViewer = true;
        }

        /// <summary>
        /// 关闭用户控件
        /// </summary>
        void ExcuteCloseUserControlCommand()
        {
            try
            {
                IsRoomViewer = false;
                IsWorkSystemViewer = false;
                GetRoomInfo();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "提醒");
            }
        }

        #endregion

        public ReviewProjectViewModel(Window window, CommonExternalEvent excuteHander, ExternalEvent externalEvent, Document document, UIDocument uiDocument)
        {
            _window = window;
            _excuteHander = excuteHander;
            _externalEvent = externalEvent;
            _document = document;
            _uiDocument = uiDocument;
            try
            {
                _projectInfo = document.ProjectInformation;
                _houseType = _projectInfo.GetStringValue("户型");

                _docTitle = document.Title;
                int index = _docTitle.IndexOf("项目");
                if (index > 1)
                {
                    _docTitle = _docTitle.Substring(0, index + 2);
                }

                //SLUtils.AllRooms(document).ForEach(e => _rooms.Add(new RoomViewModel { RoomName = e.Name, RoomId = e.Id.IntegerValue }));
                //SLUtils.AllRooms(document).ForEach(e => _rooms.Add( e.Name));

                //var systemNames = GetWorkSystemName();
                //_systemNames = new ObservableCollection<string>(systemNames);
                _projectUserName = _document.Application.Username;

                Projects = new ObservableCollection<Project>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("模型审核项目信息初始化失败", "提醒");
            }

            //Projects.Add((new Project
            //{
            //    ProjectName = project.ProjectName,
            //    HouseType = project.HouseType,
            //    SelectedWorkSystemName = project.SelectedWorkSystemName,
            //}));
        }

        /// <summary>
        /// 显示选中项目的房间分割和工作系统的名称
        /// </summary>
        void GetRoomInfo()
        {
            if (SelectedProject == null || SelectedProject.Rooms == null || SelectedProject.WorkSystems == null)
                return;
            var selectedRooms = SelectedProject.Rooms.Where(e => e.IsChecked);
            var selectedWorkSystems = SelectedProject.WorkSystems.Where(e => e.IsChecked);

            string roomRes = "";
            foreach (var item in selectedRooms)
            {
                roomRes += item.Name;
            }
            roomRes = Regex.Replace(roomRes, @"\d", "");
            SelectedProject.SelectedRoomName = roomRes;

            string systemRes = "";
            foreach (var item in selectedWorkSystems)
            {
                systemRes += item.Name + " ";
            }
            SelectedProject.SelectedWorkSystemName = systemRes;
        }

        /// <summary>
        /// 统计分数
        /// </summary>
        /// <param name="problems"></param>
        /// <returns></returns>
        double StatisticalScores(ObservableCollection<Problem> problems)
        {
            double scores = 100;
            int c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13;
            c1 = c2 = c3 = c4 = c5 = c6 = c7 = c8 = c9 = c10 = c11 = c12 = c13 = 1;
            try
            {
                foreach (var problem in problems)
                {
                    ///销项人打×，复核人打√，忽略该项问题
                    if (problem.OutputText == SLUtils.GetEnumDescription(CommonStatusEnum.wrong) && problem.ReviewText == SLUtils.GetEnumDescription(CommonStatusEnum.right))
                    {
                        continue;
                    }

                    if (problem.SelectedProblemDes.ProblemDes == "项目参数信息错误" || problem.SelectedProblemDes.ProblemDes == "工作集错误" || problem.SelectedProblemDes.ProblemDes == "房间分割错误")
                    {
                        if (c1 > 3) continue;
                        c1 += 1;
                        scores -= 1;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "项目基点错误" || problem.SelectedProblemDes.ProblemDes == "工作平面错误" || problem.SelectedProblemDes.ProblemDes == "房间分割缺少")
                    {
                        if (c2 > 1) continue;
                        c2 += 1;
                        scores -= 1;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "视图多余分类")
                    {
                        if (c3 > 1) continue;
                        c3 += 1;
                        scores -= 1;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "轴网标高错误" || problem.SelectedProblemDes.ProblemDes == "轴网标高未锁定")
                    {
                        if (c4 > 2) continue;
                        if (c4 == 2)
                        {
                            c4 += 1;
                            scores -= 7;
                            continue;
                        }
                        c4 += 1;
                        scores -= 5;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "轴网标高缺失")
                    {
                        if (c5 > 1) continue;
                        c5 += 1;
                        scores -= 3;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "轴网标高命名错误")
                    {
                        if (c6 > 1) continue;
                        c6 += 1;
                        scores -= 3;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "文件命名错误" || problem.SelectedProblemDes.ProblemDes == "底图命名错误" || problem.SelectedProblemDes.ProblemDes == "房间命名错误")
                    {
                        if (c7 > 2) continue;
                        if (c7 == 2)
                        {
                            c7 += 1;
                            scores -= 1.5;
                            continue;
                        }
                        c7 += 1;
                        scores -= 0.5;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "底图载入错误" || problem.SelectedProblemDes.ProblemDes == "底图载入信息缺失" || problem.SelectedProblemDes.ProblemDes == "底图载入缺失"
                        || problem.SelectedProblemDes.ProblemDes == "底图未锁定" || problem.SelectedProblemDes.ProblemDes == "底图平面偏移")
                    {
                        if (c8 > 3) continue;
                        c8 += 1;
                        scores -= 5;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "品牌、材质型号错误" || problem.SelectedProblemDes.ProblemDes == "品牌、材质型号缺少" || problem.SelectedProblemDes.ProblemDes == "编码缺少"
                        || problem.SelectedProblemDes.ProblemDes == "编码错误" || problem.SelectedProblemDes.ProblemDes == "加工说明错误")
                    {
                        if (c9 > 4) continue;
                        if (c9 == 4)
                        {
                            c9 += 1;
                            scores -= 4;
                            continue;
                        }
                        c9 += 1;
                        scores -= 3;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "编码多余")
                    {
                        if (c10 > 1) continue;
                        c10 += 1;
                        scores -= 2;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "后置成品与基层模块碰撞" || problem.SelectedProblemDes.ProblemDes == "面层与基层模块碰撞" || problem.SelectedProblemDes.ProblemDes == "土建与基层模块碰撞"
                        || problem.SelectedProblemDes.ProblemDes == "点位与基层模块碰撞" || problem.SelectedProblemDes.ProblemDes == "面层与面层模块碰撞" || problem.SelectedProblemDes.ProblemDes == "后置成品与面层模块碰撞"
                        || problem.SelectedProblemDes.ProblemDes == "风口与基层碰撞")
                    {
                        if (c11 > 6) continue;
                        if (c11 == 6)
                        {
                            c11 += 1;
                            scores -= 5;
                            continue;
                        }
                        c11 += 1;
                        scores -= 3;

                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "模块缺少" || problem.SelectedProblemDes.ProblemDes == "模块尺寸过短" || problem.SelectedProblemDes.ProblemDes == "模块尺寸过长"
                       || problem.SelectedProblemDes.ProblemDes == "模块开孔错误" || problem.SelectedProblemDes.ProblemDes == "模块使用错误" || problem.SelectedProblemDes.ProblemDes == "房间计算点缺失"
                       || problem.SelectedProblemDes.ProblemDes == "房间计算点错误" || problem.SelectedProblemDes.ProblemDes == "房号信息错误" || problem.SelectedProblemDes.ProblemDes == "模块尺寸优化"
                       || problem.SelectedProblemDes.ProblemDes == "模块技术规则错误")
                    {
                        if (c12 > 6) continue;
                        if (c12 == 6)
                        {
                            c12 += 1;
                            scores -= 10;
                            continue;
                        }
                        c12 += 1;
                        scores -= 2;
                    }
                    else if (problem.SelectedProblemDes.ProblemDes == "模块定位错误" || problem.SelectedProblemDes.ProblemDes == "模块多余" || problem.SelectedProblemDes.ProblemDes == "房号信息缺失")
                    {
                        if (c13 > 3) continue;
                        if (c13 == 3)
                        {
                            c13 += 1;
                            scores -= 3;
                            continue;
                        }
                        c13 += 1;
                        scores -= 1;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return scores;
        }

        /// <summary>
        /// 获取模型中的系统名称
        /// </summary>
        /// <returns></returns>
        List<RoomViewModel> GetWorkSystemName()
        {
            List<RoomViewModel> roomSystems = new List<RoomViewModel>();
            List<FamilyInstance> allFamilyInstance = _document.FilterElement<FamilyInstance>(BuiltInCategory.OST_GenericModel).ToList();
            if (allFamilyInstance.Count() == 0) { return roomSystems; }

            foreach (var item in allFamilyInstance)
            {
                string systemName = item.Symbol.GetStringValue("系统名称");
                if (!string.IsNullOrEmpty(systemName))
                {
                    if (roomSystems.Any(e => e.Name == systemName)) { continue; }
                    RoomViewModel roomSystem = new RoomViewModel
                    {
                        Name = systemName,
                    };
                    roomSystems.Add(roomSystem);
                }
            }
            return roomSystems;
        }

        /// <summary>
        /// 获取模型中的房间名称
        /// </summary>
        /// <returns></returns>
        List<RoomViewModel> GetRooms()
        {
            List<RoomViewModel> rooms = new List<RoomViewModel>();
            SLUtils.AllRooms(_document).ForEach(e => rooms.Add(new RoomViewModel { Name = e.Name, RoomId = e.Id.IntegerValue }));
            return rooms;
        }

        #region 模型转换

        /// <summary>
        /// 项目数据模型->视图模型
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        ObservableCollection<Project> ProjectModelToViewModel(List<ProjectModel> projects)
        {
            ObservableCollection<Project> displayProjects = new ObservableCollection<Project>();
            foreach (var project in projects)
            {
                Project displayProject = new Project
                {
                    _excuteHander = this._excuteHander,
                    _externalEvent = this._externalEvent,
                    _document = this._document,
                    _uiDocument = this._uiDocument,
                    _problems = ProblemModelToViewModel(project.Problems),
                    ProjectName = project.ProjectName,
                    HouseType = project.HouseType,
                    Rooms = new ObservableCollection<RoomViewModel>(project.Rooms),
                    SelectedRoomName = project.SelectedRoom,
                    WorkSystems = new ObservableCollection<RoomViewModel>(project.WorkSystemNames),
                    SelectedWorkSystemName = project.SelectedWorkSystemName,
                    ProblemNumber = project.ProblemNumber,
                    Score = project.Score,
                    Status = project.Status,
                    IsChecked = project.IsChecked,
                    IsEnabled = project.IsEnabled,
                };
                displayProjects.Add(displayProject);
            }
            return displayProjects;
        }

        /// <summary>
        /// 项目视图模型->数据模型
        /// </summary>
        /// <param name="displayProjects"></param>
        /// <returns></returns>
        List<ProjectModel> ProjectViewModelToModel(ObservableCollection<Project> displayProjects)
        {
            List<ProjectModel> projects = new List<ProjectModel>();
            foreach (var item in displayProjects)
            {
                ProjectModel project = new ProjectModel
                {
                    Problems = ProblemViewModelToModel(item._problems),
                    ProjectName = item.ProjectName,
                    HouseType = item.HouseType,
                    Rooms = item.Rooms.ToList(),
                    SelectedRoom = item.SelectedRoomName,
                    WorkSystemNames = item.WorkSystems.ToList(),
                    SelectedWorkSystemName = item.SelectedWorkSystemName,
                    ProblemNumber = item.ProblemNumber,
                    Score = item.Score,
                    Status = item.Status,
                    IsChecked = item.IsChecked,
                    IsEnabled = item.IsEnabled,
                };
                projects.Add(project);
            }
            return projects;
        }

        /// <summary>
        /// 问题视图模型->数据模型
        /// </summary>
        /// <param name="problems"></param>
        /// <returns></returns>
        List<ProblemModel> ProblemViewModelToModel(ObservableCollection<Problem> problems)
        {
            List<ProblemModel> problemModels = new List<ProblemModel>();
            foreach (var item in problems)
            {
                ProblemModel problemModel = new ProblemModel
                {
                    BuildingNumber = item.BuildingNumber,
                    HouseType = item.HouseType,
                    RoomName = item.RoomName,
                    AuditItem = item.AuditItem,
                    WorkSystemName = item.WorkSystemName,
                    ErrorNumber = item.ErrorNumber,
                    ProblemCategory = item.ProblemCategory,
                    ProblemType1 = item.ProblemType1,
                    ProblemType2 = item.ProblemType2,
                    SelectedProblemType2 = item.SelectedProblemType2,
                    ProblemDes = item.ProblemDes,
                    SelectedProblemDes = item.SelectedProblemDes,
                    Remark = item.Remark,
                    FeedbackAndDate = item.FeedbackAndDate,
                    OutputMan = item.OutputMan,
                    ProblemStatus = item.ProblemStatus,
                    OutputText = item.OutputText,
                    ReviewText = item.ReviewText,
                    ProblemViewPort = item.ProblemViewPort,
                    ProblemScreenshots = ProblemScreenshotViewModelToModel(item.ProblemScreenshots),
                    OperationData = item.OperationData,
                };
                problemModels.Add(problemModel);
            }
            return problemModels;
        }

        /// <summary>
        /// 问题数据模型->视图模型
        /// </summary>
        /// <param name="problems"></param>
        /// <returns></returns>
        ObservableCollection<Problem> ProblemModelToViewModel(List<ProblemModel> problemModels)
        {
            ObservableCollection<Problem> problems = new ObservableCollection<Problem>();
            foreach (var item in problemModels)
            {
                Problem problem = new Problem(_excuteHander, _externalEvent, _document, _uiDocument)
                {
                    OperationData = item.OperationData,
                    BuildingNumber = item.BuildingNumber,
                    HouseType = item.HouseType,
                    RoomName = item.RoomName,
                    AuditItem = item.AuditItem,
                    WorkSystemName = item.WorkSystemName,
                    ErrorNumber = item.ErrorNumber,
                    ProblemCategory = item.ProblemCategory,
                    ProblemType1 = item.ProblemType1,
                    ProblemType2 = item.ProblemType2,
                    SelectedProblemType2 = item.ProblemType2.FirstOrDefault(e => e.Type2Name == item.SelectedProblemType2.Type2Name),
                    ProblemDes = item.ProblemDes,
                    SelectedProblemDes = item.ProblemDes.FirstOrDefault(e => e.ProblemDes == item.SelectedProblemDes.ProblemDes),
                    Remark = item.Remark,
                    FeedbackAndDate = item.FeedbackAndDate,
                    OutputMan = item.OutputMan,
                    ProblemStatus = item.ProblemStatus,
                    OutputText = item.OutputText,
                    ReviewText = item.ReviewText,
                    ProblemViewPort = item.ProblemViewPort,
                    ProblemScreenshots = ProblemScreenshotModelToViewModel(item.ProblemScreenshots),
                };
                problems.Add(problem);
            }
            return problems;
        }

        /// <summary>
        /// 截图数据模型->视图模型
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        ObservableCollection<ProblemScreenshot> ProblemScreenshotModelToViewModel(List<ProblemScreenshotModel> problemScreenshots)
        {
            ObservableCollection<ProblemScreenshot> displayProblemScreenshots = new ObservableCollection<ProblemScreenshot>();
            foreach (var item in problemScreenshots)
            {
                ProblemScreenshot displayProject = new ProblemScreenshot
                {
                    //ScreenshotByteArray = item.ScreenshotByteArray,
                    ScreenshotFilePath = item.ScreenshotFilePath,
                };
                //if (!File.Exists(item.ScreenshotFilePath))
                //{
                //    string imagePath = SLUtils.AutoSaveImage();
                //    System.Drawing.Image image = ImageHelper.ByteToImage(item.ScreenshotByteArray, imagePath);
                //    displayProject.ScreenshotFilePath = imagePath;
                //}
                //else
                //{
                //displayProject.ScreenshotFilePath = item.ScreenshotFilePath;
                //}

                displayProblemScreenshots.Add(displayProject);
            }
            return displayProblemScreenshots;
        }

        /// <summary>
        /// 截图视图模型->数据模型
        /// </summary>
        /// <param name="displayProjects"></param>
        /// <returns></returns>
        List<ProblemScreenshotModel> ProblemScreenshotViewModelToModel(ObservableCollection<ProblemScreenshot> displayProblemScreenshots)
        {
            List<ProblemScreenshotModel> problemScreenshots = new List<ProblemScreenshotModel>();
            foreach (var item in displayProblemScreenshots)
            {
                ProblemScreenshotModel problemScreenshot = new ProblemScreenshotModel
                {
                    ScreenshotFilePath = item.ScreenshotFilePath,
                    //ScreenshotByteArray = item.ScreenshotByteArray,
                };
                problemScreenshots.Add(problemScreenshot);
            }
            return problemScreenshots;
        }

        #endregion
    }
}
