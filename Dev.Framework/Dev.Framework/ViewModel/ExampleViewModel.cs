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
using Dev.Framework.ExternalEventSet;
using Dev.Framework.MVVM;
using GalaSoft.MvvmLight.Command;


namespace ModelReviewFunction.ViewModel
{
    public class ExampleViewModel : MyObservableObject
    {
        /// <summary>
        /// 全局变量
        /// </summary>
        public UIDocument _uiDocument;
        public Document _document;
        public CommonExternalEvent _excuteHander;
        public ExternalEvent _externalEvent;
        public Window _parentWindow;


        /// <summary>
        /// 测试字段
        /// </summary>
        private bool isTest;
        public bool IsTest
        {
            get { return isTest; }
            set
            {
                isTest = value; RaisePropertyChanged(() => IsTest);
            }
        }
        #region 执行命令
        /// <summary>
        /// 测试命令
        /// </summary>
        private RelayCommand testCommand;

        public RelayCommand TestCommand
        {
            get
            {
                if (testCommand == null)
                    testCommand = new RelayCommand(() => ExcuteTestCommand());
                return testCommand;

            }
            set { testCommand = value; }
        }

        #endregion


        #region 执行方法
        /// <summary>
        /// 测试方法
        /// </summary>
        void ExcuteTestCommand()
        {
            try
            {
                _parentWindow.WindowState = WindowState.Minimized;


            }
            catch (Exception ex)
            {
                MessageBox.Show("测试页:" + ex.Message, "提醒");
            }
        }


        void ExternalEventCommand()
        {
            if (_externalEvent != null)
            {
                _excuteHander.action = new Action<UIApplication>((app) =>
                {
                    try
                    {
                        using (Transaction tran = new Transaction(_document, "修改模型方法"))
                        {
                            tran.Start();
                            ///页面内对模型进行操作  
                            tran.Commit();
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

        #endregion
    }
}
