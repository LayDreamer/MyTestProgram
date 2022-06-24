using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModelReviewFunction.ExternalEventSet;
using ModelReviewFunction.Model;
using ModelReviewFunction.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace ModelReviewFunction.UI
{
    /// <summary>
    /// ProblemsSheet.xaml 的交互逻辑
    /// </summary>
    public partial class ProblemSheet : Window
    {
        RecordProblemViewModel recordProblem;
        public ProblemSheet(CommonExternalEvent excuteHander, ExternalEvent externalEvent, Document document, UIDocument uiDocument, Project project)
        {
            InitializeComponent();
            recordProblem = new RecordProblemViewModel(this, excuteHander, externalEvent, document, uiDocument, project);
            this.DataContext = recordProblem;
        }

        /// <summary>
        /// 执行粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            if (e.Command == ApplicationCommands.Paste)
            {
                var files = Clipboard.GetFileDropList();
                var text = Clipboard.GetData(DataFormats.Text);
                var image = Clipboard.GetImage();
                e.Handled = true;
                List<string> filePaths = new List<string>();

                if (files.Count != 0)
                {
                    int count = 1;
                    foreach (var file in files)
                    {
                        string filePath = SLUtils.AutoSaveImage();
                        File.Copy(file, filePath);
                        filePaths.Add(filePath);
                        count++;
                    }
                }
                //else if (image != null)
                //{
                //    ImageHelper.SaveImageToFile(image, filePath);
                //    filePaths.Add(filePath);
                //}
                else
                {
                    return;
                }

                foreach (var filePath in filePaths)
                {
                    //System.Drawing.Image picture = System.Drawing.Image.FromFile(filePath);
                    //byte[] imageByteArray = ImageHelper.ImageToByte(picture);
                    var initProblemScreenshot = recordProblem.SelectedProblem.ProblemScreenshots.Where(x => string.IsNullOrEmpty(x.ScreenshotFilePath)).FirstOrDefault();
                    if (initProblemScreenshot != null)
                    {
                        initProblemScreenshot.ScreenshotFilePath = filePath;
                        //initProblemScreenshot.ScreenshotByteArray = imageByteArray;
                    }
                    else
                    {
                        recordProblem.SelectedProblem.ProblemScreenshots.Add(new ProblemScreenshot { ScreenshotFilePath = filePath, /*ScreenshotByteArray = imageByteArray*/ });
                    }
                }
            }
        }


        /// <summary>
        /// 左击放大图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(sender is RichTextBox))
                    return;
                RichTextBox rtb = sender as RichTextBox;
                if (recordProblem.SelectedProblem == null)
                    return;
                recordProblem.SelectedProblem.SelectedProblemScreenshot = rtb.Document.DataContext as ProblemScreenshot;
                if (recordProblem.SelectedProblem.SelectedProblemScreenshot == null || recordProblem.SelectedProblem.SelectedProblemScreenshot.ScreenshotFilePath == null)
                    return;
                recordProblem.IsImageViewer = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提醒");
            }
            #region 获取图片信息
            //IEnumerable<Image> images = rtb.Document.Blocks.OfType<BlockUIContainer>()
            //.Select(c => c.Child).OfType<Image>()
            //.Union(rtb.Document.Blocks.OfType<Paragraph>()
            //.SelectMany(pg => pg.Inlines.OfType<InlineUIContainer>())
            //.Select(inline => inline.Child).OfType<Image>());

            //foreach (Block block in (sender as RichTextBox).Document.Blocks)
            //{
            //    if (block is Paragraph)
            //    {
            //        Paragraph paragraph = (Paragraph)block;
            //        foreach (Inline inline in paragraph.Inlines)
            //        {
            //            if (inline is InlineUIContainer)
            //            {
            //                InlineUIContainer uiContainer = (InlineUIContainer)inline;
            //                if (uiContainer.Child is Image)
            //                {
            //                    Image image = (Image)uiContainer.Child;
            //                    image.Width = image.ActualWidth + 1;
            //                    image.Height = image.ActualHeight + 1;
            //                }
            //            }
            //        }
            //    }
            //    else if (block is BlockUIContainer)
            //    {
            //        var container = (BlockUIContainer)block;
            //        if (container.Child is Image)
            //        {
            //            Image image = (Image)container.Child;
            //            ScaleTransform st = new ScaleTransform
            //            {
            //                ScaleX = 1.2,
            //                ScaleY = 1.2,
            //            };
            //            image.RenderTransform = st;
            //        }
            //    }
            //}
            #endregion
        }

        /// <summary>
        /// 回退键 删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
                if (recordProblem.SelectedProblem == null)
                {
                    return;
                }
                RichTextBox rtb = sender as RichTextBox;
                ProblemScreenshot currendScreenshot = rtb.Document.DataContext as ProblemScreenshot;
                if (currendScreenshot != null)
                {
                    recordProblem.SelectedProblem.ProblemScreenshots.Remove(currendScreenshot);
                }
                if (recordProblem.SelectedProblem.ProblemScreenshots.Count() == 0)
                {
                    recordProblem.SelectedProblem.ProblemScreenshots = new ObservableCollection<ProblemScreenshot> { new ProblemScreenshot() };
                }
            }
        }
    }
}
