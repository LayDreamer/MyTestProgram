using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Framework
{
    public static class RevitUtils
    {
        #region revit元素过滤

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

        #endregion

        #region 元素参数赋值

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

        #endregion

        #region 移除revit弹框提醒

        public class FailureHandleForFunction : IFailuresPreprocessor
        {
            public string ErrorMessage { set; get; }
            public string ErrorSeverity { set; get; }
            public FailureHandleForFunction()
            {
                ErrorMessage = "";
                ErrorSeverity = "";
            }
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                Document doc = failuresAccessor.GetDocument();

                IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
                foreach (FailureMessageAccessor failureMessageAccessor in failureMessages)
                {
                    try
                    {
                        FailureSeverity failureSeverity = failureMessageAccessor.GetSeverity();
                        ErrorSeverity = failureSeverity.ToString();
                        if (failureSeverity == FailureSeverity.Warning)
                        {
                            failureMessageAccessor.GetDefaultResolutionCaption();
                            // 如果是警告，则禁止消息框  
                            failuresAccessor.DeleteWarning(failureMessageAccessor);
                        }
                        else
                        {
                            ErrorMessage += failureMessageAccessor.GetDescriptionText();
                            foreach (ElementId eid in failureMessageAccessor.GetFailingElementIds())
                            {
                                ErrorMessage += doc.GetElement(eid).Name + "ID:" + eid.ToString();
                            }
                            if (failureMessageAccessor.HasResolutions())
                                failuresAccessor.ResolveFailure(failureMessageAccessor);
                            return FailureProcessingResult.ProceedWithCommit;
                        }
                    }
                    catch
                    {
                    }
                }
                return FailureProcessingResult.Continue;
            }
            public static FailureHandleForFunction SetFailureHandle(Transaction transaction)
            {
                FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();

                FailureHandleForFunction failureHandler = new FailureHandleForFunction();

                failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                failureHandlingOptions.SetClearAfterRollback(true);

                transaction.SetFailureHandlingOptions(failureHandlingOptions);
                return failureHandler;
            }
        }

        #endregion

        #region Revit导出

        /// <summary>
        /// 图纸导出DWG
        /// </summary>
        /// <param name="doc"></param>
        public static void ExportToDWG(Document doc)
        {
            var viewSheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().Where(vw =>
                           vw.ViewType == ViewType.DrawingSheet && !vw.IsTemplate);

            // create DWG export options
            DWGExportOptions dwgOptions = new DWGExportOptions
            {
                //MergedViews = true,
                //SharedCoords = true,
                FileVersion = ACADVersion.R2010
            };


            List<ElementId> views = new List<ElementId>();

            foreach (var sheet in viewSheets)
            {
                if (!sheet.IsPlaceholder)
                {
                    views.Add(sheet.Id);
                    if (views.Count > 10)
                    {
                        break;
                    }
                }
            }
            // For Web Deployment
            //doc.Export(@"D:\sheetExporterLocation", "TEST", views, dwgOptions);
            // For Local
            string resPath = Path.Combine(Directory.GetCurrentDirectory(), "ExportFile", "dwg");

            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            doc.Export(resPath, "rvt", views, dwgOptions);
        }

        public static void ExportToDWG(Document doc, List<int> ids, string path, string name = "rvt")
        {
            if (ids.Count == 0)
                return;
            List<ElementId> views = new List<ElementId>();
            ids.ForEach(e => views.Add(new ElementId(e)));

            // create DWG export options
            DWGExportOptions dwgOptions = new DWGExportOptions
            {
                MergedViews = true,
                SharedCoords = true,
                FileVersion = ACADVersion.R2010
            };

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            name += "-" + (doc.GetElement(views.First()) as View).Name;
            doc.Export(path, name, views, dwgOptions);
            doc.Regenerate();
        }

        /// <summary>
        /// 官方api
        /// </summary>
        /// <param name="document"></param>
        /// <param name="view"></param>
        /// <param name="setupName"></param>
        /// <returns></returns>
        public static bool ExportToDWG(Document document, View view, string setupName)
        {
            bool exported = false;
            // Get the predefined setups and use the one with the given name.
            IList<string> setupNames = BaseExportOptions.GetPredefinedSetupNames(document);
            foreach (string name in setupNames)
            {
                if (name.CompareTo(setupName) == 0)
                {
                    // Export using the predefined options
                    DWGExportOptions dwgOptions = DWGExportOptions.GetPredefinedOptions(document, name);

                    // Export the active view
                    ICollection<ElementId> views = new List<ElementId>();
                    views.Add(view.Id);
                    // The document has to be saved already, therefore it has a valid PathName.
                    exported = document.Export(Path.GetDirectoryName(document.PathName),
                        Path.GetFileNameWithoutExtension(document.PathName), views, dwgOptions);
                    break;
                }
            }

            return exported;
        }
        #endregion

    }
}
