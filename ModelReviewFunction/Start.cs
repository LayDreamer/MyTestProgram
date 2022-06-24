using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelReviewFunction
{

    [Transaction(TransactionMode.Manual)]
    public class Start : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            //新建一个选项卡，并在该选项卡总新建一个命令栏（命令栏可以放多个命令按钮）
            application.CreateRibbonTab("NewTab");
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("NewTab", "TabBar");
            //1、建立一个可下拉的命令栏
            //1.1、新建一个可下拉按钮
            SplitButtonData sbd1 = new SplitButtonData("Name", "Text");
            SplitButton sb1 = ribbonPanel.AddItem(sbd1) as SplitButton;
            //1.2、在该按钮添加两个按钮
            PushButtonData p1 = new PushButtonData("Helloworld1", "按钮1", @"d:\desktop\桌面\TestPro\modelreviewfunction\ModelReviewFunction\bin\Debug\ModelReviewFunction.dll", "ModelReviewFunction.CmdModelReview");
            PushButton pushButton1 = sb1.AddPushButton(p1);
            PushButtonData p2 = new PushButtonData("Helloworld2", "按钮2", @"d:\desktop\桌面\工作任务\1.开发的功能\模型审核-问题管理功能\2.代码\ModelReviewFunction\ModelReviewFunction\bin\Debug\ModelReviewFunction.dll", "ModelReviewFunction.CmdModelReview");
            PushButton pushButton2 = sb1.AddPushButton(p2);
            //2、在选项卡栏添加一个普通按钮
            PushButtonData p3 = new PushButtonData("Helloworld3", "按钮3", @"d:\desktop\桌面\工作任务\1.开发的功能\模型审核-问题管理功能\2.代码\ModelReviewFunction\ModelReviewFunction\bin\Debug\ModelReviewFunction.dll", "ModelReviewFunction.CmdModelReview");
            PushButton pushButton3 = ribbonPanel.AddItem(p3) as PushButton;
            //3、先准备一张图片,后面给按钮加图片。（这里要引用PresentationCore程序集，再引用system.windows.media.imaging）
            //Uri uriImage = new Uri(@"E:\practice\HelloRevit\HelloRevit\bin\Debug\1.jpg");
            //BitmapImage largeImage = new BitmapImage(uriImage);
            //3.1、将图片赋值给按钮。PushButton有两个属性，当按钮是堆叠时，显示的是Image；当按钮是下拉或单个的时候显示的是LargeImage。
            //pushButton1.LargeImage = largeImage;
            //pushButton2.LargeImage = largeImage;
            //pushButton3.LargeImage = largeImage;
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }



}
