using System;
using System.Collections.Generic;
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

namespace ModelReviewFunction.UI
{
    /// <summary>
    /// UControlImage.xaml 的交互逻辑
    /// </summary>
    public partial class UControlImage : UserControl
    {
        private Image movingObject;  // 记录当前被拖拽移动的图片
        private Point StartPosition; // 本次移动开始时的坐标点位置
        private Point EndPosition;   // 本次移动结束时的坐标点位置
        public UControlImage()
        {
            InitializeComponent();
        }
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Image img = sender as Image;
            //Point centerPoint = e.GetPosition(img);
            TransformGroup tg = img.RenderTransform as TransformGroup;
            var tgnew = tg.CloneCurrentValue();
            if (tgnew != null)
            {
                ScaleTransform st = tgnew.Children[1] as ScaleTransform;
                img.RenderTransformOrigin = new Point(0.5, 0.5);
                if (st.ScaleX < 0.3 && st.ScaleY < 0.3 && e.Delta < 0)
                {
                    return;
                }
                st.ScaleX += (double)e.Delta / 3500;
                st.ScaleY += (double)e.Delta / 3500;
            }
            img.RenderTransform = tgnew;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            movingObject = null;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                EndPosition = e.GetPosition(img);

                TransformGroup tg = img.RenderTransform as TransformGroup;
                var tgnew = tg.CloneCurrentValue();
                if (tgnew != null)
                {
                    TranslateTransform tt = tgnew.Children[0] as TranslateTransform;

                    var X = EndPosition.X - StartPosition.X;
                    var Y = EndPosition.Y - StartPosition.Y;
                    tt.X += X;
                    tt.Y += Y;
                }
                // 重新给图像赋值Transform变换属性
                img.RenderTransform = tgnew;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            movingObject = img;
            StartPosition = e.GetPosition(img);
        }
    }
}
