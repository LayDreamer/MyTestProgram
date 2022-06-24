using ModelReviewFunction.MVVM;

namespace ModelReviewFunction.ViewModel
{
    public class ProblemScreenshot : MyObservableObject
    {
        /// <summary>
        /// 截图地址
        /// </summary>
        private string screenshotFilePath;
        public string ScreenshotFilePath
        {
            get { return screenshotFilePath; }
            set { screenshotFilePath = value; RaisePropertyChanged(() => ScreenshotFilePath); }
        }

        public byte[] ScreenshotByteArray { get; set; }
        //public string ScreenshotByteArray { get; set; }
    }
}
