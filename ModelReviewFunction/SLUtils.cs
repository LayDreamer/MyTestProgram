using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ModelReviewFunction.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using MySql.Data.MySqlClient;
using SharpSvn;
using SharpSvn.Security;

namespace ModelReviewFunction
{
    public static class SLUtils
    {
        public static string LocalImageSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\LocalImageCache";
        public static string SVNImageSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\SVNImageCache";

        #region 房间类

        /// <summary>
        /// 获取所有的房间
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<Room> AllRooms(Document doc)
        {
            #region 过滤房间
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementCategoryFilter roomfilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            RoomFilter roomFilter = new RoomFilter();
            IList<Element> rooms = collector.WherePasses(roomFilter).ToElements();
            rooms = rooms.Where(x => x.IsValidObject).ToList();
            if (rooms == null || rooms.Count <= 0)
            {
                return null;
            }
            List<Room> allrooms = allrooms = rooms.Cast<Room>().ToList().Where(c => c.Area > 0).ToList();
            if (allrooms == null || allrooms.Count <= 0)
            {
                return null;
            }
            #endregion
            return allrooms;
        }

        /// <summary>
        /// 获取房间闭合构建
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static IList<IList<BoundarySegment>> BoundarySegments(
            Room room,
            SpatialElementBoundaryLocation boundaryLocation = SpatialElementBoundaryLocation.Center)
        {
            IList<IList<BoundarySegment>> blist = room.GetBoundarySegments(
                                                        new SpatialElementBoundaryOptions()
                                                        {
                                                            SpatialElementBoundaryLocation = boundaryLocation
                                                        }
                                                        );
            return blist;
        }

        /// <summary>
        /// 获取房间闭合构建的Id,所有元素可能不闭合
        /// （Revit房间中有加入线的构建，没有ElementId）
        /// </summary>
        /// <param name="room"></param>
        /// <param name="boundaryLocation"></param>
        /// <returns></returns>
        public static List<ElementId> BoundarySegmentIds(
            Room room,
            SpatialElementBoundaryLocation boundaryLocation = SpatialElementBoundaryLocation.Center)
        {
            IList<IList<BoundarySegment>> blist = room.GetBoundarySegments(
                                                       new SpatialElementBoundaryOptions()
                                                       {
                                                           SpatialElementBoundaryLocation = boundaryLocation
                                                       }
                                                       );
            List<ElementId> ids = new List<ElementId>();
            if (null == blist || blist.Count < 1)
                return ids;
            foreach (var seg in blist)
            {
                foreach (BoundarySegment bs in seg)
                {
                    ElementId elementId = bs.ElementId;
                    if (elementId == null || ElementId.InvalidElementId == elementId)
                        continue;
                    if (ids.Any(c => c.IntegerValue == elementId.IntegerValue))
                        continue;
                    ids.Add(elementId);
                }
            }
            return ids;
        }
        /// <summary>
        /// 获取房间的中间轮廓线(直线)
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Line> RoomLines(Room room)
        {
            IList<IList<BoundarySegment>> bList = BoundarySegments(room);
            return RoomLines(bList);
        }
        /// <summary>
        /// 获取房间的中间轮廓线与闭合元素的关系（直线）
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static Dictionary<Line, Element> RoomElemLines(Room room)
        {
            IList<IList<BoundarySegment>> bList = BoundarySegments(room);
            return RoomSegmentElemLines(room.Document, bList);
        }

        /// <summary>
        /// 获取房间的内轮廓线(直线)
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Line> RoomInnerLines(Room room)
        {
            IList<IList<BoundarySegment>> bList = BoundarySegments(room, SpatialElementBoundaryLocation.Finish);
            return RoomLines(bList);
        }
        /// <summary>
        /// 获取房间的内轮廓线与元素的（直线）
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static Dictionary<Line, Element> RoomInnerElemLines(Room room)
        {
            IList<IList<BoundarySegment>> bList = BoundarySegments(room, SpatialElementBoundaryLocation.Finish);
            return RoomSegmentElemLines(room.Document, bList);
        }
        /// <summary>
        /// 获取房间的轮廓线（直线）
        /// </summary>
        /// <param name="BoundarySegments"></param>
        /// <returns></returns>
        public static List<Line> RoomLines(IList<IList<BoundarySegment>> BoundarySegments)
        {
            List<Line> lines = new List<Line>();
            foreach (var wallList in BoundarySegments)
            {
                List<Curve> edage = new List<Curve>();
                foreach (BoundarySegment bs in wallList)
                {
                    ElementId elementId = bs.ElementId;
                    Curve curve = bs.GetCurve();
                    Line line = curve as Line;
                    lines.Add(line);
                }
            }
            return lines;
        }
        /// <summary>
        /// 获取房间闭合元素的线对应的Element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="BoundarySegments"></param>
        /// <returns>
        /// 线对应的Element
        /// null == elem 补的线
        /// elem is Wall//系统墙
        ///     (elem as Wall).WallType.Kind == WallKind.Basic基本墙 
        /// elem is ModelLine房间分割线
        /// </returns>
        public static Dictionary<Line, Element> RoomSegmentElemLines(Document doc, IList<IList<BoundarySegment>> BoundarySegments)
        {
            Dictionary<Line, Element> lineElems = new Dictionary<Line, Element>();

            foreach (var wallList in BoundarySegments)
            {
                List<Curve> edage = new List<Curve>();
                foreach (BoundarySegment bs in wallList)
                {

                    Curve curve = bs.GetCurve();
                    Line line = curve as Line;

                    ElementId elementId = bs.ElementId;
                    Element elem = doc.GetElement(elementId);
                    //if (null == elem)//补的线
                    //{}
                    //else if (elem is Wall)//系统墙
                    //{
                    //    if ((elem as Wall).WallType.Kind == WallKind.Basic)//基本墙 
                    //    { }
                    //    else { }//幕墙
                    //}
                    //else if (elem is ModelLine)//房间分割线
                    //{}
                    //else{}
                    lineElems.Add(line, elem);
                }
            }
            return lineElems;
        }

        #endregion

        #region 视图类

        /// <summary>
        /// 复制当前视图
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="uiDoc"></param>
        public static ProblemViewPortModel DuplicateCurrentView(Document doc, UIDocument uiDoc)
        {
            ProblemViewPortModel problemViewPort = new ProblemViewPortModel();
            View docView = doc.ActiveView;
            List<ElementId> selectedElementIds = new List<ElementId>();
            IList<Reference> references = uiDoc.Selection.PickObjects(ObjectType.Element);
            if (references.Count != 0)
            {
                foreach (var reference in references)
                {
                    Element element = doc.GetElement(reference);
                    if (element is Group)
                    {
                        var ids = (element as Group).GetMemberIds();
                        selectedElementIds.AddRange(ids);
                    }
                    else
                    {
                        selectedElementIds.Add(element.Id);
                    }
                }
            }
            if (selectedElementIds.Count != 0)
            {
                try
                {
                    View copyView = doc.GetElement(docView.Duplicate(ViewDuplicateOption.WithDetailing)) as View;
                    copyView.IsolateElementsTemporary(selectedElementIds);

                    //FilteredElementCollector fillPatternElementFilter = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement));
                    //FillPatternElement fillPatternElement = fillPatternElementFilter.FirstOrDefault(f => (f as FillPatternElement).GetFillPattern().IsSolidFill) as FillPatternElement;
                    //Color red = new Color(255, 0, 0);
                    //OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();
                    //graphicSettings.SetSurfaceForegroundPatternVisible(true);
                    //graphicSettings.SetSurfaceForegroundPatternId(fillPatternElement.Id);
                    //graphicSettings.SetSurfaceForegroundPatternColor(red);

                    OverrideGraphicSettings graphicSettings = GetOverrideGraphicSettings(doc, new Color(255, 0, 0));

                    selectedElementIds.ForEach(e => copyView.SetElementOverrides(e, graphicSettings));
                    List<int> elementIds = new List<int>();
                    selectedElementIds.ForEach(e => elementIds.Add(e.IntegerValue));
                    problemViewPort = new ProblemViewPortModel()
                    {
                        ProblemElementIds = elementIds,
                        ProblemView = copyView.Id.IntegerValue,
                    };
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return problemViewPort;
        }

        public static OverrideGraphicSettings GetOverrideGraphicSettings(Document doc, Color color)
        {
            FilteredElementCollector fillPatternElementFilter = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement));
            FillPatternElement fillPatternElement = fillPatternElementFilter.FirstOrDefault(f => (f as FillPatternElement).GetFillPattern().IsSolidFill) as FillPatternElement;
            //Color red = new Color(255, 0, 0);
            OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();
            graphicSettings.SetSurfaceForegroundPatternVisible(true);
            graphicSettings.SetSurfaceForegroundPatternId(fillPatternElement.Id);
            graphicSettings.SetSurfaceForegroundPatternColor(color);
            return graphicSettings;
        }


        ///将当前视图以JPG图片的形式导出到桌面上；
        ///如果只想截取当前视窗中可见的部分（截图），
        ///可将options.ExportRange设置为VisibleRegionOfCurrentView。
        public static string RevitCurrentViewScreenShots(Document document, string filePath)
        {
            ImageExportOptions options = new ImageExportOptions();
            options.ZoomType = ZoomFitType.FitToPage;
            options.ExportRange = ExportRange.CurrentView;
            options.FitDirection = FitDirectionType.Horizontal;
            options.HLRandWFViewsFileType = ImageFileType.JPEGMedium;
            options.ShadowViewsFileType = ImageFileType.JPEGMedium;
            options.PixelSize = 1920;
            //string filePath = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".jpg");
            options.FilePath = filePath;
            document.ExportImage(options);
            return filePath;
        }

        /// <summary>
        /// bitmap转BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(string filePath)
        {
            BinaryReader binReader = new BinaryReader(File.Open(filePath, FileMode.Open));
            FileInfo fileInfo = new FileInfo(filePath);
            byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
            binReader.Close();

            // Init bitmap
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            return bitmap;
        }

        #endregion

        #region 数据保存类
        /// <summary>
        /// 自动保存图片地址（根据时间作为文件名称）
        /// </summary>
        /// <returns></returns>
        public static string AutoSaveImage(string suffix = ".jpg")
        {
            string resPath = LocalImageSavePath;
            //string Opath = Environment.CurrentDirectory;
            if (resPath.Substring(resPath.Length - 1, 1) != @"\")
                resPath = resPath + @"\";
            //string resPath = Path.Combine(Opath, secondName);
            if (!Directory.Exists(resPath))
                Directory.CreateDirectory(resPath);
            resPath = Path.Combine(resPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + suffix);
            resPath = ImageHelper.GetNewPathForDupes(resPath);
            return resPath;
        }



        /// <summary>
        /// 自动保存到本地的json文件
        /// </summary>
        /// <param name="secondName"></param>
        /// <param name="nameSuffix"></param>
        /// <returns></returns>
        public static string AutoSaveJson(string secondName = "ProjectCache", string nameSuffix = "ProjectInfo.json")
        {
            string Opath = Environment.CurrentDirectory;
            string photoname = DateTime.Now.Ticks.ToString();
            if (Opath.Substring(Opath.Length - 1, 1) != @"\")
            {
                Opath = Opath + @"\";
            }
            string resPath = Path.Combine(Opath, secondName);
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            resPath = Path.Combine(resPath, nameSuffix);
            return resPath;
        }

        public static byte[] ImageToByte(System.Drawing.Image picture)
        {
            MemoryStream ms = new MemoryStream();
            if (picture == null)
            {
                return new byte[ms.Length];
            }
            picture.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] Bpicture = new byte[ms.Length];
            Bpicture = ms.GetBuffer();
            return Bpicture;
        }

        public static byte[] ToByteArr(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            byte[] byteArr = ms.ToArray();
            return byteArr;
        }

        public static T ToObj<T>(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            Object obj = bf.Deserialize(ms);
            return (T)obj;
        }


        #endregion


        #region 枚举类
        /// <summary>
        /// 获取枚举值的描述信息DescriptionAttribute中内容
        /// [DescriptionAttribute("")]
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0)
                return value;
            DescriptionAttribute description = (DescriptionAttribute)objs[0];
            return description.Description;
        }

        #endregion

        #region 导出Excel类

        /// <summary>
        ///IEnumerable集合转换成DataTable
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static DataTable IEnumerableToDataTable<T>(this IEnumerable<T> array)
        {
            DataTable resTable = new DataTable();

            ///遍历属性
            foreach (PropertyDescriptor dp in TypeDescriptor.GetProperties(typeof(T)))
            {
                ///获取属性相关特性
                var attribute = dp.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                //ret.Columns.Add(dp.Name);
                if (attribute != null)
                {
                    ///获取特性名称(Excel中显示的表头名称)
                    resTable.Columns.Add(attribute.DisplayName);
                }
            }

            ///遍历元素集
            foreach (T item in array)
            {
                var Row = resTable.NewRow();
                foreach (PropertyDescriptor dp in TypeDescriptor.GetProperties(typeof(T)))
                {
                    var attribute = dp.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                    var currentModel = dp.GetValue(item);
                    if (attribute == null || currentModel == null) { continue; }
                    object value = null;
                    if (dp.PropertyType == typeof(ProblemCategoryModel))
                    {
                        if (attribute.DisplayName == "问题类别")
                        {
                            value = (currentModel as ProblemCategoryModel).CategoryName;
                        }
                        else if (attribute.DisplayName == "问题描述")
                        {
                            value = (currentModel as ProblemCategoryModel).ProblemDes;
                        }
                    }
                    else if (dp.PropertyType == typeof(ProblemTypeModel))
                    {
                        if (attribute.DisplayName == "问题归类1")
                        {
                            value = (currentModel as ProblemTypeModel).Type1Name;
                        }
                        else if (attribute.DisplayName == "问题归类2")
                        {
                            value = (currentModel as ProblemTypeModel).Type2Name;
                        }
                    }
                    else if (dp.PropertyType == typeof(List<ProblemScreenshotModel>))
                    {
                        if (attribute.DisplayName == "问题截图")
                        {
                            List<ProblemScreenshotModel> problemScreenshots = currentModel as List<ProblemScreenshotModel>;
                            string image = "";
                            for (int i = 0; i < problemScreenshots.Count; i++)
                            {
                                if (i > 0)
                                {
                                    image += ";";
                                }
                                image += problemScreenshots[i].ScreenshotFilePath;

                            }
                            value = image;
                        }
                    }
                    else
                    {
                        value = currentModel;
                    }
                    if (value != null)
                    {
                        Row[attribute.DisplayName] = value;
                    }

                }
                resTable.Rows.Add(Row);
            }
            return resTable;
        }

        public static DataTable IEnumerableToDataTableCommon<T>(this IEnumerable<T> array)
        {
            var ret = new DataTable();

            foreach (PropertyDescriptor dp in TypeDescriptor.GetProperties(typeof(T)))
            {
                ///特性名称
                var attribute = dp.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                //属性名称
                //ret.Columns.Add(dp.Name);
                if (attribute != null)
                {
                    ret.Columns.Add(attribute.DisplayName);
                }
            }
            foreach (T item in array)
            {
                var Row = ret.NewRow();
                foreach (PropertyDescriptor dp in TypeDescriptor.GetProperties(typeof(T)))
                {
                    var attribute = dp.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                    if (attribute == null) { continue; }
                    Row[attribute.DisplayName] = dp.GetValue(item);
                }
                ret.Rows.Add(Row);
            }
            return ret;
        }

        /// <summary>
        /// Excel导出
        /// </summary>
        /// <param name="DT"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string ExcelExport(DataTable DT, string title)
        {
            try
            {
                //创建Excel
                Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook ExcelBook = ExcelApp.Workbooks.Add(System.Type.Missing);
                //创建工作表（即Excel里的子表sheet） 1表示在子表sheet1里进行数据导出
                Microsoft.Office.Interop.Excel.Worksheet ExcelSheet = (Microsoft.Office.Interop.Excel.Worksheet)ExcelBook.Worksheets[1];
                //如果数据中存在数字类型 可以让它变文本格式显示
                ExcelSheet.Cells.NumberFormat = "@";
                //设置工作表名
                ExcelSheet.Name = title;
                //设置Sheet标题
                string start = "A1";
                string end = ChangeASC(DT.Columns.Count) + "1";
                Microsoft.Office.Interop.Excel.Range _Range = ExcelSheet.get_Range(start, end);
                _Range.Merge(0);                     //单元格合并动作(要配合上面的get_Range()进行设计)
                _Range = ExcelSheet.get_Range(start, end);
                _Range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                _Range.Font.Size = 22; //设置字体大小
                _Range.Font.Name = "宋体"; //设置字体的种类 
                ExcelSheet.Cells[1, 1] = title;    //Excel单元格赋值
                _Range.EntireColumn.AutoFit(); //自动调整列宽
                //写表头
                for (int m = 1; m <= DT.Columns.Count; m++)
                {
                    ExcelSheet.Cells[2, m] = DT.Columns[m - 1].ColumnName.ToString();
                    start = "A2";
                    end = ChangeASC(DT.Columns.Count) + "2";
                    _Range = ExcelSheet.get_Range(start, end);
                    _Range.Font.Size = 14; //设置字体大小
                    _Range.Font.Name = "宋体"; //设置字体的种类  
                    _Range.EntireColumn.AutoFit(); //自动调整列宽 
                    _Range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                }
                //写数据
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    for (int j = 1; j <= DT.Columns.Count; j++)
                    {
                        //Excel单元格第一个从索引1开始
                        // if (j == 0) j = 1;
                        ExcelSheet.Cells[i + 3, j] = DT.Rows[i][j - 1].ToString();
                    }
                }
                //表格属性设置
                for (int n = 0; n < DT.Rows.Count + 1; n++)
                {
                    start = "A" + (n + 3).ToString();
                    end = ChangeASC(DT.Columns.Count) + (n + 3).ToString();
                    //获取Excel多个单元格区域
                    _Range = ExcelSheet.get_Range(start, end);
                    _Range.Font.Size = 12; //设置字体大小
                    _Range.Font.Name = "宋体"; //设置字体的种类
                    _Range.EntireColumn.AutoFit(); //自动调整列宽
                    _Range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter; //设置字体在单元格内的对其方式 _Range.EntireColumn.AutoFit(); //自动调整列宽 
                }
                ExcelApp.DisplayAlerts = false; //保存Excel的时候，不弹出是否保存的窗口直接进行保存 
                ////弹出保存对话框,并保存文件
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.DefaultExt = ".xlsx";
                sfd.Filter = "Office 2007 File|*.xlsx|Office 2000-2003 File|*.xls|所有文件|*.*";
                if (sfd.ShowDialog() == true)
                {
                    if (sfd.FileName != "")
                    {
                        ExcelBook.SaveAs(sfd.FileName);  //将其进行保存到指定的路径
                        System.Windows.MessageBox.Show("导出文件已存储为: " + sfd.FileName, "温馨提示");
                    }
                }
                //释放可能还没释放的进程
                ExcelBook.Close();
                ExcelApp.Quit();
                return sfd.FileName;
            }
            catch
            {
                System.Windows.MessageBox.Show("导出文件保存失败,可能原因该文件已打开！", "警告！");
                return null;
            }
        }

        /// <summary>
        /// 获取当前列列名,并得到EXCEL中对应的列
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string ChangeASC(int count)
        {
            string ascstr = "";
            switch (count)
            {
                case 1:
                    ascstr = "A";
                    break;
                case 2:
                    ascstr = "B";
                    break;
                case 3:
                    ascstr = "C";
                    break;
                case 4:
                    ascstr = "D";
                    break;
                case 5:
                    ascstr = "E";
                    break;
                case 6:
                    ascstr = "F";
                    break;
                case 7:
                    ascstr = "G";
                    break;
                case 8:
                    ascstr = "H";
                    break;
                case 9:
                    ascstr = "I";
                    break;
                case 10:
                    ascstr = "J";
                    break;
                case 11:
                    ascstr = "K";
                    break;
                case 12:
                    ascstr = "L";
                    break;
                case 13:
                    ascstr = "M";
                    break;
                case 14:
                    ascstr = "N";
                    break;
                case 15:
                    ascstr = "O";
                    break;
                case 16:
                    ascstr = "P";
                    break;
                case 17:
                    ascstr = "Q";
                    break;
                case 18:
                    ascstr = "R";
                    break;
                case 19:
                    ascstr = "S";
                    break;
                case 20:
                    ascstr = "T";
                    break;
                case 21:
                    ascstr = "U";
                    break;
                case 22:
                    ascstr = "V";
                    break;
                case 23:
                    ascstr = "W";
                    break;
                case 24:
                    ascstr = "X";
                    break;
                case 25:
                    ascstr = "Y";
                    break;
                default:
                    ascstr = "Z";
                    break;
            }
            return ascstr;
        }

        public static string ExcelExport1(DataTable DT, string title)
        {
            try
            {
                //创建Excel
                Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook ExcelBook = ExcelApp.Workbooks.Add(System.Type.Missing);
                //创建工作表（即Excel里的子表sheet） 1表示在子表sheet1里进行数据导出
                Microsoft.Office.Interop.Excel.Worksheet ExcelSheet = (Microsoft.Office.Interop.Excel.Worksheet)ExcelBook.Worksheets[1];
                //如果数据中存在数字类型 可以让它变文本格式显示
                ExcelSheet.Cells.NumberFormat = "@";
                //设置工作表名
                ExcelSheet.Name = title;
                //设置Sheet标题
                string start = "A1";
                string end = ChangeASC(DT.Columns.Count) + "1";
                Microsoft.Office.Interop.Excel.Range _Range = ExcelSheet.get_Range(start, end);
                _Range.Merge(0);                     //单元格合并动作(要配合上面的get_Range()进行设计)
                _Range = ExcelSheet.get_Range(start, end);
                _Range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                _Range.Font.Size = 22; //设置字体大小
                _Range.Font.Name = "宋体"; //设置字体的种类 
                ExcelSheet.Cells[1, 1] = title;    //Excel单元格赋值
                _Range.EntireColumn.AutoFit(); //自动调整列宽

                //写表头
                for (int m = 1; m <= DT.Columns.Count; m++)
                {
                    ExcelSheet.Cells[2, m] = DT.Columns[m - 1].ColumnName.ToString();
                    start = "A2";
                    end = ChangeASC(DT.Columns.Count) + "2";
                    _Range = ExcelSheet.get_Range(start, end);
                    _Range.Font.Size = 14; //设置字体大小
                    _Range.Font.Name = "宋体"; //设置字体的种类  
                    _Range.EntireColumn.AutoFit(); //自动调整列宽 
                    _Range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                }
                //写数据
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    for (int j = 1; j <= DT.Columns.Count; j++)
                    {
                        //Excel单元格第一个从索引1开始
                        // if (j == 0) j = 1;

                        string value = DT.Rows[i][j - 1].ToString();
                        if (value.Contains("\\"))
                        {
                            string[] res = value.Contains(";") ? value.Split(';') : new string[] { value };
                            foreach (var item in res)
                            {
                                Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range)ExcelSheet.Cells[i + 3, j];
                                float Left = (float)((double)range.Left);
                                float Top = (float)((double)range.Top);
                                const float ImageSize = 100;
                                if (!File.Exists(item))
                                {
                                    continue;
                                }
                                System.Drawing.Image image = System.Drawing.Image.FromFile(item);
                                float imgWidth = Convert.ToSingle(image.Width) / 5;
                                float imgHeight = Convert.ToSingle(image.Height) / 5;//高度像素值
                                var pic = ExcelSheet.Shapes.AddPicture2(item, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, Left, Top, imgWidth, imgHeight, Microsoft.Office.Core.MsoPictureCompress.msoPictureCompressFalse);
                                //var pic = ExcelSheet.Shapes.AddPicture2(item, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, Left, Top, -1, -1, Microsoft.Office.Core.MsoPictureCompress.msoPictureCompressFalse);

                                // 图片宽度
                                float flPicWidth = pic.Width;
                                // 图片高度
                                float flPicHeight = pic.Height;

                                // 锁定纵横比
                                pic.LockAspectRatio = Microsoft.Office.Core.MsoTriState.msoFalse;

                                float sdd = 0.35f;

                                // 缩放宽度 比例
                                //float flScaleWidth = 1 / (flPicWidth / ImageSize);
                                // 缩放比列小于<0.3f时，大的图片会被压缩
                                //pic.ScaleWidth(sdd, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoScaleFrom.msoScaleFromTopLeft);

                                // 缩放高度 比例
                                //float flScaleHeight = 1 / (flPicHeight / ImageSize);
                                pic.ScaleHeight(sdd, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoScaleFrom.msoScaleFromTopLeft);

                                //if (flPicWidth != flPicHeight)
                                //{
                                //    // 如果指定的形状在调整大小时其原始比例保持不变，则此属性为 True。 如果调整大小时可以分别更改形状的高度和宽度，则此属性为 False
                                //    // 解除纵横比
                                //    //pic.LockAspectRatio = Microsoft.Office.Core.MsoTriState.msoTrue;
                                //    // 缩放高度 比例
                                //    float flScaleHeight = 1 / (flPicHeight / ImageSize);
                                //    pic.ScaleHeight(flScaleHeight, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoScaleFrom.msoScaleFromTopLeft);
                                //}
                            }
                        }
                        else
                        {
                            ExcelSheet.Cells[i + 3, j] = DT.Rows[i][j - 1].ToString();
                        }
                    }
                }
                //表格属性设置
                for (int n = 0; n < DT.Rows.Count + 1; n++)
                {
                    start = "A" + (n + 3).ToString();
                    end = ChangeASC(DT.Columns.Count) + (n + 3).ToString();
                    //获取Excel多个单元格区域
                    _Range = ExcelSheet.get_Range(start, end);
                    _Range.Font.Size = 12; //设置字体大小
                    _Range.Font.Name = "宋体"; //设置字体的种类
                    _Range.EntireColumn.AutoFit(); //自动调整列宽
                    _Range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter; //设置字体在单元格内的对其方式 _Range.EntireColumn.AutoFit(); //自动调整列宽 
                }
                ExcelApp.DisplayAlerts = false; //保存Excel的时候，不弹出是否保存的窗口直接进行保存 
                ////弹出保存对话框,并保存文件
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.DefaultExt = ".xlsx";
                sfd.Filter = "Office 2007 File|*.xlsx|Office 2000-2003 File|*.xls|所有文件|*.*";
                if (sfd.ShowDialog() == true)
                {
                    if (sfd.FileName != "")
                    {
                        ExcelBook.SaveAs(sfd.FileName);  //将其进行保存到指定的路径
                        System.Windows.MessageBox.Show("导出文件已存储为: " + sfd.FileName, "温馨提示");
                    }
                }
                //释放可能还没释放的进程
                ExcelBook.Close();
                ExcelApp.Quit();
                return sfd.FileName;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("导出文件保存失败！", "警告！");
                return null;
            }
        }

        #endregion
    }



    /// <summary>
    ///获取修改element参数类
    /// </summary>
    public static class ParamHelper
    {
        #region get

        public static string GetStringValue(this Element instance, BuiltInParameter builtInParameter)
        {
            string value = string.Empty;
            if (instance != null)
            {
                Parameter para = instance.get_Parameter(builtInParameter);

                if (para != null && para.StorageType == StorageType.String)
                    using (para)
                        value = para.AsString();
            }

            return value;
        }
        public static string GetStringValue(this Element instance, string strName)
        {
            string value = string.Empty;

            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                Parameter para = instance.LookupParameter(strName);
                if (para != null && para.StorageType == StorageType.String)
                    using (para)
                        value = para.AsString();
            }

            return value;
        }

        public static ElementId GetElementId(this Element instance, BuiltInParameter builtInParameter)
        {
            ElementId value = ElementId.InvalidElementId;
            if (instance != null)
            {
                Parameter para = instance.get_Parameter(builtInParameter);
                if (para != null && para.StorageType == StorageType.ElementId)
                    using (para)
                        value = para.AsElementId();
            }

            return value;
        }
        public static ElementId GetElementId(this Element instance, string strName)
        {
            ElementId value = ElementId.InvalidElementId;

            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                Parameter para = instance.LookupParameter(strName);
                if (para != null && para.StorageType == StorageType.ElementId)
                    using (para)
                        value = para.AsElementId();
            }

            return value;
        }
        public static double GetDoubleValue(this Element instance, BuiltInParameter builtInParameter)
        {
            double value = 0.0;
            if (instance != null)
            {
                Parameter para = instance.get_Parameter(builtInParameter);
                if (para != null && para.StorageType == StorageType.Double)
                    using (para)
                        value = para.AsDouble();
            }

            return value;
        }
        public static string GetValueAsString(this Element instance, BuiltInParameter builtInParameter, FormatOptions formatOptions = null)
        {
            string value = string.Empty;

            if (instance != null)
            {
                Parameter para = instance.get_Parameter(builtInParameter);
                if (para != null)
                    using (para)
                        value = formatOptions == null ? para.AsValueString() : para.AsValueString(formatOptions);
            }

            return value;
        }
        public static string GetValueAsString(this Element instance, string strName, FormatOptions formatOptions = null)
        {
            string value = string.Empty;

            if (instance != null)
            {
                Parameter para = instance.LookupParameter(strName);
                if (para != null)
                    using (para)
                        value = formatOptions == null ? para.AsValueString() : para.AsValueString(formatOptions);
            }

            return value;
        }
        public static int GetInteger(this Element instance, BuiltInParameter builtInParameter)
        {
            int value = 0;
            if (instance != null)
            {
                Parameter para = instance.get_Parameter(builtInParameter);
                if (para != null && para.StorageType == StorageType.Integer)
                    using (para)
                        value = para.AsInteger();
            }

            return value;
        }
        public static int GetInteger(this Element instance, string strName)
        {
            int value = 0;
            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                Parameter para = instance.LookupParameter(strName);
                if (para != null && para.StorageType == StorageType.Integer)
                    using (para)
                        value = para.AsInteger();
            }

            return value;
        }

        public static double GetDoubleValue(this Element instance, string strName)
        {
            double value = 0.0;
            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                Parameter para = instance.LookupParameter(strName);
                if (para != null && para.StorageType == StorageType.Double)
                    using (para)
                        value = para.AsDouble();
            }

            return value;
        }

        #endregion

        #region set

        public static void SetDoubleValue(this Element instance, BuiltInParameter builtInParameter, double value)
        {
            if (instance != null)
            {
                var param = instance.get_Parameter(builtInParameter);
                if (
                    param != null &&
                    param.StorageType == StorageType.Double &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }
        public static void SetDoubleValue(this Element instance, string strName, double value)
        {
            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                var param = instance.LookupParameter(strName);
                if (
                    param != null &&
                    param.StorageType == StorageType.Double &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }

        public static void SetInteger(this Element instance, string strName, int value)
        {
            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                var param = instance.LookupParameter(strName);
                if (
                    param != null &&
                    param.StorageType == StorageType.Integer &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }
        public static void SetStringValue(this Element instance, string strName, string value)
        {
            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                var param = instance.LookupParameter(strName);
                if (
                    param != null &&
                    param.StorageType == StorageType.String &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }
        public static void SetStringValue(this Element instance, BuiltInParameter builtInParameter, string value)
        {
            if (instance != null)
            {
                var param = instance.get_Parameter(builtInParameter);
                if (
                    param != null &&
                    param.StorageType == StorageType.String &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }
        public static void SetInteger(this Element instance, BuiltInParameter builtInParameter, int value)
        {
            if (instance != null)
            {
                var param = instance.get_Parameter(builtInParameter);
                if (
                    param != null &&
                    param.StorageType == StorageType.Integer &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }
        public static void SetElmentId(this Element instance, BuiltInParameter builtInParameter, ElementId value)
        {
            if (instance != null)
            {
                var param = instance.get_Parameter(builtInParameter);
                if (
                    param != null &&
                    param.StorageType == StorageType.ElementId &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }
        public static void SetElmentId(this Element instance, string strName, ElementId value)
        {
            if (instance != null && !string.IsNullOrEmpty(strName))
            {
                var param = instance.LookupParameter(strName);
                if (
                    param != null &&
                    param.StorageType == StorageType.ElementId &&
                    !param.IsReadOnly)
                    using (param)
                        param.Set(value);
            }
        }

        #endregion
    }

    public static class FilterElementHelper
    {
        /// <summary>
        /// 过滤元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="document"></param>
        /// <param name="category">元素类别</param>
        /// <param name="view">执行过滤的视图</param>
        /// <returns></returns>
        public static IEnumerable<T> FilterElement<T>(
            this Document document,
            BuiltInCategory category = BuiltInCategory.INVALID,
            View view = null) where T : Element
        {
            System.Diagnostics.Debug.Assert(document != null);

            ElementFilter filter = new ElementClassFilter(typeof(T));
            if (category != BuiltInCategory.INVALID)
            {
                var catefilter = new ElementCategoryFilter(category);
                filter = new LogicalAndFilter(filter, catefilter);
            }

            FilteredElementCollector collector =
                view == null ?
                new FilteredElementCollector(document) :
                new FilteredElementCollector(document, view.Id);

            return collector.WherePasses(filter).ToElements().Cast<T>();
        }
    }

    /// <summary>
    /// Json帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o)
        {
            string json = JsonConvert.SerializeObject(o);
            return json;
        }

        public static T Clone<T>(T obj) where T : new()
        {
            string json = JsonConvert.SerializeObject(obj);
            var tmp = JsonConvert.DeserializeObject(json);
            return (T)tmp;
        }

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        /// <summary>
        /// 反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }

        public static string ToJson(DataTable dt)
        {
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    string s_value = dataRow[dataColumn.ColumnName].ToString();

                    dictionary.Add(dataColumn.ColumnName, s_value);
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }

            return JsonConvert.SerializeObject(arrayList);
        }

        /// <summary>  
        /// 对象序列化成 XML String  
        /// </summary>  
        public static string XmlSerialize<T>(T obj)
        {
            string xmlString = string.Empty;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, obj);
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlString;
        }

        /// <summary>  
        /// XML String 反序列化成对象  
        /// </summary>  
        public static T XmlDeserialize<T>(string xmlString)
        {
            T t = default(T);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            {
                using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                {
                    Object obj = xmlSerializer.Deserialize(xmlReader);
                    t = (T)obj;
                }
            }
            return t;
        }
    }

    /// <summary>
    /// 文件类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 遍历文件夹下所有文件获取文件路径
        /// </summary>
        /// <param name="sPath">文件夹路径</param>
        /// <returns></returns>
        public static string[] OpenFiles(string sPath)
        {
            return OpenFiles(sPath, null);
        }
        /// <summary>
        /// 遍历文件夹下所有文件获取文件路径
        /// </summary>
        /// <param name="sPath">文件夹路径</param>
        /// <param name="FileExtensionArray">指定文件格式</param>
        /// <returns></returns>
        public static string[] OpenFiles(string sPath, string[] FileExtensionArray)
        {
            List<string> Files = null;
            if (sPath == string.Empty)
            {
                return null;
            }
            Files = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(sPath);
            if (FileExtensionArray == null)
            {
                GetAllFiles(dir, null);
            }
            else
            {
                GetAllFiles(dir, FileExtensionArray.ToList());
            }
            return Files.ToArray();
        }
        /// <summary>
        /// 遍历文件夹下所有文件获取文件路径
        /// </summary>
        /// <param name="DirInfo"></param>
        /// <param name="FileExtensionList"></param>
        private static void GetAllFiles(DirectoryInfo DirInfo, List<string> FileExtensionList)
        {
            List<string> Files = null;
            foreach (FileInfo fInfo in DirInfo.GetFiles())
            {
                if (FileExtensionList != null)
                {
                    if (FileExtensionList.Contains(fInfo.Extension))
                    {
                        Files.Add(fInfo.FullName);
                    }
                }
                else
                {
                    Files.Add(fInfo.FullName);
                }
            }
            foreach (DirectoryInfo dInfo in DirInfo.GetDirectories())
            {
                GetAllFiles(dInfo, FileExtensionList);
            }

        }
        /// <summary>
        /// 文件夹路径中是否包含不能包含的字符
        /// </summary>
        /// <param name="spath">文件夹路径</param>
        /// <returns>不能包含的字符</returns>
        public static string MakeValidPathName(string spath)
        {
            var invalidPathNameChars = Path.GetInvalidPathChars();
            foreach (var c in spath)
            {
                if (invalidPathNameChars.Contains(c))
                {
                    return c.ToString();
                }
            }
            return "";
        }
        /// <summary>
        /// 创建路径中的不存在的文件夹（文件）
        /// 
        /// 没有的情况下进行创建
        /// </summary>
        /// <param name="sPath">路径</param>
        /// <param name="isFile">路径是文件</param>
        /// <returns></returns>
        public static string MakeDir(string sPath, bool isFile)
        {
            string InvalidPathChar = MakeValidPathName(sPath);
            if (!string.IsNullOrEmpty(InvalidPathChar))
                return "路径存在非法字符'" + InvalidPathChar + "'";

            string DirectoryFullName = string.Empty;
            if (isFile)
            {
                FileInfo FI = new FileInfo(sPath);
                DirectoryFullName = FI.Directory.FullName;
            }
            else
            {
                DirectoryFullName = sPath;
            }
            string pathRoot = Path.GetPathRoot(DirectoryFullName);
            string[] Dirs = DirectoryFullName.Replace(pathRoot, "").Split('\\');
            string tmpS = pathRoot;
            foreach (string s in Dirs)
            {
                tmpS = Path.Combine(tmpS, s);
                if (!Directory.Exists(tmpS))
                { Directory.CreateDirectory(tmpS); }
            }
            return "";
        }

        /// <summary>
        /// 获取文件中的文本
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static string GetTextFromFile(string sPath)
        {
            string InvalidPathChar = MakeValidPathName(sPath);
            if (!InvalidPathChar.Equals("")) return null;

            List<string> tmp = new List<string>();
            string Allline;
            StreamReader sr = new StreamReader(sPath, Encoding.UTF8);
            Allline = sr.ReadToEnd();
            sr.Close();
            return Allline;
        }

        #region 判断文件是否被占用
        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrit);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        /// <summary>
        /// 通过Stream判断文件是否被其它程序占用
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileInUseByStream(string filePath)
        {
            bool inUse = true;
            FileStream stream = null;
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;
                if (!File.Exists(filePath))
                    return false;
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch (Exception ex) { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return inUse;
        }
        /// <summary>
        /// 通过文件占用的Handle判断文件是否被其它程序占用
        /// 该方法判断占用时容易导致未占用的文件被过程占用。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileInUseByHandle(string filePath)
        {
            bool inUse = false;
            if (string.IsNullOrEmpty(filePath))
                return false;
            if (!File.Exists(filePath))
                return false;
            IntPtr errorHandle = new IntPtr(-1);
            IntPtr vHandle = _lopen(filePath, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == errorHandle)
            {
                inUse = true;
            }
            else
            {
                CloseHandle(vHandle);
            }
            return inUse;
        }
        /// <summary>
        /// 通过文件能否被打开判断文件是否被占用
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileInUseByOpen(string filePath)
        {
            bool inUse = true;
            FileStream stream = null;
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;
                if (!File.Exists(filePath))
                    return false;
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch (Exception ex) { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return inUse;
        }

        public static void CopyFiles(string srcFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            DirectoryInfo directoryInfo = new DirectoryInfo(srcFolder);
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".jpg")
                {
                    string targetFile = Path.Combine(destFolder, file.Name);
                    if (File.Exists(targetFile))
                        continue;
                    file.CopyTo(targetFile, true);
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// 图片类
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// 图片 => 字节数组
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ImageToByte(System.Drawing.Image picture)
        {
            MemoryStream ms = new MemoryStream();
            if (picture == null)
            {
                return new byte[ms.Length];
            }
            picture.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] Bpicture = new byte[ms.Length];
            Bpicture = ms.GetBuffer();
            return Bpicture;
        }

        /// <summary>
        /// 字节数组 => 图片
        /// </summary>
        /// <param name="btImage"></param>
        /// <returns></returns>
        public static System.Drawing.Image ByteToImage(byte[] btImage, string filePath)
        {
            if (btImage.Length == 0)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream(btImage);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            image.Save(filePath);
            ms.Dispose();
            return image;
        }

        public static string ImageToBase64String(System.Drawing.Image picture)
        {
            MemoryStream ms = new MemoryStream();
            if (picture == null)
            {
                return "";
            }
            picture.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] Bpicture = new byte[ms.Length];
            Bpicture = ms.GetBuffer();
            string Bpicture64String = Convert.ToBase64String(Bpicture).ToString();
            return Bpicture64String;
        }
        public static System.Drawing.Image Base64StringToImage(string btImage, string filePath)
        {
            if (btImage.Length == 0)
            {
                return null;
            }
            //对解析后的二进制字符串进行base64 解码
            byte[] tBytes = Convert.FromBase64String(btImage.ToString());
            MemoryStream ms = new MemoryStream(tBytes);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            image.Save(filePath);
            ms.Dispose();
            return image;
        }

        /// <summary>
        /// 重名文件自动添加新文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetNewPathForDupes(string path)
        {
            string newFullPath = path.Trim();
            if (System.IO.File.Exists(path))
            {
                string directory = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);
                int counter = 1;
                do
                {
                    string newFilename = string.Format("{0}({1}){2}", filename, counter, extension);
                    newFullPath = Path.Combine(directory, newFilename);
                    counter++;
                } while (System.IO.File.Exists(newFullPath));
            }
            return newFullPath;
        }
    }


    /// <summary>
    /// MySql类
    /// </summary>
    /// 

    #region  tempHide


    public class MySqlHelper
    {
        // 数据库连接字符串
        //public static string Conn = "Database='数据库名';Data Source='数据库服务器地址';User Id='数据库用户名';Password='密码';charset='utf8';pooling=true ";
        public static string Conn = "server=10.10.12.171;database=bds_checkmsg;user=bds_checkmsg;password=bds_checkmsg@123;Charset=utf8;pooling=true ";

        //public static string Conn = "Database='wp';Data Source='localhost';User Id='root';Password='root';charset='utf8';pooling=true ";

        //  用于缓存参数的HASH表
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        ///   <summary>
        ///   给定连接的数据库用假设参数执行一个sql命令（不返回数据集）
        ///   </summary>
        ///   <param name="connectionString"> 一个有效的连接字符串 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns> 执行命令所影响的行数 </returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {

            MySqlCommand cmd = new MySqlCommand();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        ///   <summary>
        ///  用现有的数据库连接执行一个sql命令（不返回数据集）
        ///   </summary>
        ///   <param name="connection"> 一个现有的数据库连接 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns> 执行命令所影响的行数 </returns>
        public static int ExecuteNonQuery(MySqlConnection connection, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        ///   <summary>
        /// 使用现有的SQL事务执行一个sql命令（不返回数据集）
        ///   </summary>
        ///   <remarks>
        /// 举例:
        ///   int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///   </remarks>
        ///   <param name="trans"> 一个现有的事务 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns> 执行命令所影响的行数 </returns>
        public static int ExecuteNonQuery(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        ///   <summary>
        ///  用执行的数据库连接执行一个返回数据集的sql命令
        ///   </summary>
        ///   <remarks>
        ///  举例:
        ///   MySqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///   </remarks>
        ///   <param name="connectionString"> 一个有效的连接字符串 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns> 包含结果的读取器 </returns>
        public static MySqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            // 创建一个MySqlCommand对象
            MySqlCommand cmd = new MySqlCommand();
            // 创建一个MySqlConnection对象
            MySqlConnection conn = new MySqlConnection(connectionString);

            // 在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，
            // 因此commandBehaviour.CloseConnection 就不会执行
            try
            {
                // 调用 PrepareCommand 方法，对 MySqlCommand 对象设置参数
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                // 调用 MySqlCommand  的 ExecuteReader 方法
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                // 清除参数
                cmd.Parameters.Clear();
                return reader;
            }
            catch (Exception ex)
            {
                // 关闭连接，抛出异常
                conn.Close();
                throw ex;
            }
        }



        ///   <summary>
        ///  返回DataSet
        ///   </summary>
        ///   <param name="connectionString"> 一个有效的连接字符串 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns></returns>
        public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            // 创建一个MySqlCommand对象
            MySqlCommand cmd = new MySqlCommand();
            // 创建一个MySqlConnection对象
            MySqlConnection conn = new MySqlConnection(connectionString);

            // 在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，

            try
            {
                // 调用 PrepareCommand 方法，对 MySqlCommand 对象设置参数
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                // 调用 MySqlCommand  的 ExecuteReader 方法
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet ds = new DataSet();

                adapter.Fill(ds);
                // 清除参数
                cmd.Parameters.Clear();
                conn.Close();
                return ds;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        ///   <summary>
        ///  用指定的数据库连接字符串执行一个命令并返回一个数据集的第一列
        ///   </summary>
        ///   <remarks>
        /// 例如:
        ///   Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///   </remarks>
        /// <param name="connectionString"> 一个有效的连接字符串 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns> 用 Convert.To{Type}把类型转换为想要的  </returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        ///   <summary>
        ///  用指定的数据库连接执行一个命令并返回一个数据集的第一列
        ///   </summary>
        ///   <remarks>
        ///  例如:
        ///   Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///   </remarks>
        ///   <param name="connection"> 一个存在的数据库连接 </param>
        ///   <param name="cmdType"> 命令类型(存储过程, 文本, 等等) </param>
        ///   <param name="cmdText"> 存储过程名称或者sql命令语句 </param>
        ///   <param name="commandParameters"> 执行命令所用参数的集合 </param>
        ///   <returns> 用 Convert.To{Type}把类型转换为想要的  </returns>
        public static object ExecuteScalar(MySqlConnection connection, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {

            MySqlCommand cmd = new MySqlCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        ///   <summary>
        ///  将参数集合添加到缓存
        ///   </summary>
        ///   <param name="cacheKey"> 添加到缓存的变量 </param>
        ///   <param name="commandParameters"> 一个将要添加到缓存的sql参数集合 </param>
        public static void CacheParameters(string cacheKey, params MySqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        ///   <summary>
        ///  找回缓存参数集合
        ///   </summary>
        ///   <param name="cacheKey"> 用于找回参数的关键字 </param>
        ///   <returns> 缓存的参数集合 </returns>
        public static MySqlParameter[] GetCachedParameters(string cacheKey)
        {
            MySqlParameter[] cachedParms = (MySqlParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            MySqlParameter[] clonedParms = new MySqlParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (MySqlParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }

        ///   <summary>
        ///  准备执行一个命令
        ///   </summary>
        ///   <param name="cmd"> sql命令 </param>
        ///   <param name="conn"> OleDb连接 </param>
        ///   <param name="trans"> OleDb事务 </param>
        ///   <param name="cmdType"> 命令类型例如 存储过程或者文本 </param>
        ///   <param name="cmdText"> 命令文本,例如:Select * from Products </param>
        ///   <param name="cmdParms"> 执行命令的参数 </param>
        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (MySqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
    }

    public class SvnHelper
    {
        private string userName = "dev_update";

        private string password = "dev_update123";

        private string svnAddress = @"https://gyhyjysvn.chinayasha.com/svn/Public/appupdate/BDSautocad";

        private string localAddress;

        private string localRevitFunctionAddress;

        //private string localCADApplicationAddress;


        public SvnHelper(string checkFilePath, string functionName = "")
        {
            localAddress = checkFilePath;
            localRevitFunctionAddress = !string.IsNullOrEmpty(functionName) ? Path.Combine(localAddress, functionName) : localAddress;
            //localCADApplicationAddress = Path.Combine(localCADAddress, "application");

        }

        /// <summary>
        /// 权限获取
        /// </summary>
        /// <param name="client"></param>
        private void GetPermission(SvnClient client)
        {
            //用户名和密码获取
            client.Authentication.Clear(); //清除predefined处理程序
            client.Authentication.UserNamePasswordHandlers += delegate (object s, SvnUserNamePasswordEventArgs e)
            {
                e.UserName = this.userName;
                e.Password = this.password;
            };
            client.Authentication.SslServerTrustHandlers += delegate (object s, SvnSslServerTrustEventArgs e)
            {
                e.AcceptedFailures = e.Failures;
                e.Save = true;
            };
        }


        /// <summary>
        /// 检出
        /// </summary>
        /// <returns></returns>
        public bool CheckOut(out SvnUpdateResult result)
        {
            bool iRet = false;

            result = null;

            //检查在这台电脑上是否曾经检出过
            if (Directory.Exists(localRevitFunctionAddress))
            {
                try
                {
                    iRet = Update(out result);
                    return iRet;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("更新失败\n");
                }
            }
            else
            {
                Directory.CreateDirectory(localRevitFunctionAddress);
            }
            using (SvnClient client = new SvnClient())
            {
                GetPermission(client);
                try
                {
                    SvnUriTarget target = new SvnUriTarget(svnAddress);
                    client.CheckOut(target, localRevitFunctionAddress, out result);
                    iRet = true;
                }
                catch (SvnException se)
                {
                    iRet = Update(out result);
                    return iRet;
                    //MessageBox.Show(se.Message, "错误提示");
                }
                catch (UriFormatException ufe)
                {
                    //MessageBox.Show(ufe.Message, "错误提示");
                }
            }
            return iRet;
        }


        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updatePath">更新地址</param>
        /// <param name="result">更新结果</param>
        /// <returns>是否更新成功</returns>
        public bool Update(out SvnUpdateResult result)
        {
            bool iRet = false;
            using (SvnClient client = new SvnClient())
            {
                GetPermission(client);
                SvnUpdateArgs updateArgs = new SvnUpdateArgs();
                updateArgs.Depth = SvnDepth.Infinity;
                iRet = client.Update(localAddress, updateArgs, out result);
            }

            return iRet;
        }


        public bool Commit(out SvnCommitResult result)
        {
            bool iRet = false;
            using (SvnClient client = new SvnClient())
            {
                GetPermission(client);
                SvnCommitArgs svnCommitArgs = new SvnCommitArgs();
                svnCommitArgs.Depth = SvnDepth.Infinity;
                svnCommitArgs.LogMessage = "Test";

                var changeList = ChangeFilesOperate();
                iRet = client.Commit(changeList, svnCommitArgs, out result);
            }
            return iRet;
        }


        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="addFilePaths"></param>
        /// <returns></returns>
        public bool Add(List<string> addImageFilePaths, string dllPath)
        {
            bool iRet = true;
            string localFunctionPath = localRevitFunctionAddress;
            if (!Directory.Exists(localFunctionPath))
            {
                Directory.CreateDirectory(localFunctionPath);
                Update(out SvnUpdateResult result);
            }

            //添加图片
            foreach (var path in addImageFilePaths)
            {
                if (string.IsNullOrEmpty(path)) continue;

                string fileName = Path.GetFileName(path);

                string svnLocalFilePath = Path.Combine(localFunctionPath, fileName);

                FileInfo file = new FileInfo(path);

                //拷贝文件到检出svn位置
                if (file.Exists)
                {
                    try
                    {
                        file.CopyTo(svnLocalFilePath);
                    }
                    catch (Exception ex)
                    {
                        iRet = false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(dllPath))
            {
                string localsvnFunctionDllPath = Path.Combine(localFunctionPath, "libdll");

                if (!Directory.Exists(localsvnFunctionDllPath))
                {
                    Directory.CreateDirectory(localsvnFunctionDllPath);
                }

                //上传的dll根目录
                string dllDic = Path.GetDirectoryName(dllPath);

                try
                {
                    CopyDirectory(dllDic, localsvnFunctionDllPath);
                }
                catch (Exception e)
                {

                }
            }

            return iRet;
        }


        /// <summary>
        /// 对所有增加删除修改的文件进行 对应的操作
        /// </summary>
        /// <returns></returns>
        public List<string> ChangeFilesOperate()
        {
            List<string> changeFilePath = new List<string>();

            using (SvnClient client = new SvnClient())
            {
                GetPermission(client);
                Collection<SvnStatusEventArgs> list = new Collection<SvnStatusEventArgs>();
                client.GetStatus(localAddress, out list);
                foreach (SvnStatusEventArgs eventArgs in list)
                {
                    var state = eventArgs.LocalContentStatus;
                    //从eventArgs中获取每个变更文件的相关信息,针对不同状态操作

                    if (state == SvnStatus.Missing)
                    {
                        client.Delete(eventArgs.FullPath);
                    }
                    if (state == SvnStatus.NotVersioned)
                    {
                        client.Add(eventArgs.FullPath);
                    }
                    //修改状态直接提交即可
                    changeFilePath.Add(eventArgs.FullPath);
                }
            }
            return changeFilePath;
        }

        /// <summary>
        /// 复制dll文件中内容
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)
                    {     //判断是否文件夹
                        if (!Directory.Exists(destPath + "\\" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "\\" + i.Name);
                        }
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name);
                    }
                    else
                    {
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

    }
    #endregion
}


