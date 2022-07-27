using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Framework
{
    class DocumentEvent
    {
        public static Boolean IdleFlag = false;
        //记录DocumentChanged事件发生改变的构件
        IList<ElementId> listId = new List<ElementId>();
        //定义一个全局UIApplication，用来注销指定事件
        public static UIApplication uiApp = null;

        #region 激活Tab事件
        public static void SetMenuBottonByTabActivate(object sender, ViewActivatedEventArgs e)
        {
            try
            {
                UIApplication uiApp = sender as UIApplication;
                var document = e.Document;
                if (document != null && document.IsValidObject)
                {

                }
                else
                {

                }

            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        /// <summary>
        /// 新建Document 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void DocumentCreatedEvent(object sender, DocumentCreatedEventArgs args)
        {
            try
            {
                Autodesk.Revit.ApplicationServices.Application revitApp = sender as Autodesk.Revit.ApplicationServices.Application;
                UIApplication uiApp = new UIApplication(revitApp);
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 打开Document事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void DocumentOpenedEvent(object sender, DocumentOpenedEventArgs args)
        {
            try
            {
                Autodesk.Revit.ApplicationServices.Application revitApp = sender as Autodesk.Revit.ApplicationServices.Application;
                UIApplication uiApp = new UIApplication(revitApp);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 关闭Document事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DocumentClosedEvent(object sender, DocumentClosedEventArgs e)
        {
            try
            {
                Autodesk.Revit.ApplicationServices.Application revitApp = sender as Autodesk.Revit.ApplicationServices.Application;
                if (revitApp.Documents.Size < 1)
                {

                }
                else
                {
                    List<Document> docs = revitApp.Documents.Cast<Document>().ToList();
                    if (docs.All(c => c.IsFamilyDocument))
                    {

                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 改变Document事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DocumentChangedForSomething(object sender, DocumentChangedEventArgs e)
        {

            ICollection<ElementId> collection = e.GetAddedElementIds();//获取创建的门的ids

            IdleFlag = true;
            uiApp.Application.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(DocumentChangedForSomething);//注销本事件
        }

        #region 插件初始化完成后事件
        public static void OnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            Autodesk.Revit.ApplicationServices.Application revitApp = sender as Autodesk.Revit.ApplicationServices.Application;
            UIApplication uiApp = new UIApplication(revitApp);
        }
        #endregion

        public static void IdlingHandler(object sender, IdlingEventArgs args)
        {
            UIApplication uiapp = sender as UIApplication;
            if (!IdleFlag)//true
            {
                return;//继续执行
            }

            IdleFlag = false;
            uiapp.Idling -= new EventHandler<IdlingEventArgs>(IdlingHandler);//注销

        }
    }
}
