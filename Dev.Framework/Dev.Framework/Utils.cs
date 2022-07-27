using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Framework
{
    public static class Utils
    {
        #region 内容转换

        public static DataTable WorksheetToTable(ExcelWorksheet worksheet, int rowStart, int ColEnd)
        {
            int rows = worksheet.Dimension.End.Row;
            int cols = worksheet.Dimension.End.Column;
            DataTable dt = new DataTable(worksheet.Name);
            DataRow dr = null;
            bool isStop = false;
            ///行
            for (int i = 1; i < rows; i++)
            {
                if (i >= rowStart && !isStop)
                {
                    dr = dt.Rows.Add();
                }
                ///列
                for (int j = 1; j < cols; j++)
                {
                    if (i == rowStart - 1)
                    {
                        string value = GetString(worksheet.Cells[i, j].Value);
                        if (!dt.Columns.Contains(value))
                        {
                            dt.Columns.Add(value);
                        }
                    }
                    else
                    {
                        if (i < rowStart || j > ColEnd)
                        {
                            continue;
                        }
                        string value = GetString(worksheet.Cells[i, j].Value);
                        if (j == 1 && string.IsNullOrEmpty(value))
                        {
                            isStop = true;
                            dr.Delete();
                        }
                        dr[j - 1] = value;

                        //if (addColumns != null || addColumns.Count() != 0)
                        //{
                        //    foreach (var addItem in addColumns)
                        //    {
                        //        int columnIndex = addItem.Key;
                        //        string columnName = addItem.Value;
                        //        int currentColIndex = dt.Columns.IndexOf(columnName);
                        //        dr[currentColIndex] = GetString(worksheet.Cells[i, columnIndex].Value);
                        //    }
                        //}
                    }
                }
            }
            return dt;
        }

        static string GetString(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return "";
                }
                else
                {
                    return obj.ToString();
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// ToList<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ToModelList<T>(this DataTable dt) where T : new()
        {
            // 定义集合    
            List<T> ts = new List<T>();

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = null, tempDescription = null;

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    // 检查DataTable是否包含此列    
                    tempName = pi.Name;
                    tempDescription = pi == null ? null : ((DescriptionAttribute)Attribute.GetCustomAttribute(pi, typeof(DescriptionAttribute)))?.Description;
                    string column = tempDescription ?? tempName;

                    if (dt.Columns.Contains(column))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite)
                            continue;

                        object value = dr[column];

                        try
                        {
                            if (value != DBNull.Value)
                            {
                                if (pi.PropertyType.ToString().Contains("System.Nullable"))
                                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(pi.PropertyType));
                                else
                                    value = Convert.ChangeType(value, pi.PropertyType);
                                pi.SetValue(t, value, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                    }
                }
                ts.Add(t);
            }
            return ts;
        }


        /// <summary>
        /// ToDataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> array)
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

        #endregion

        #region 打开文件
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="filePathAndName">文件的路径和名称（比如：C:\Users\Administrator\test.txt）</param>
        /// <param name="isWaitFileClose">是否等待文件关闭（true：表示等待）</param>

        public static void OpenFile(string filePathAndName, bool isWaitFileClose = true)
        {
            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo(filePathAndName);
            process.StartInfo = psi;

            process.StartInfo.UseShellExecute = true;

            try
            {
                process.Start();

                //等待打开的程序关闭
                if (isWaitFileClose)
                {
                    process.WaitForExit();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                process?.Close();

            }
        }

        #endregion

        #region 保存文件
        /// <summary>
        /// 重名文件自动添加新文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetNewPathForDupes(string path)
        {
            string newFullPath = path.Trim();
            if (File.Exists(path))
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
                } while (File.Exists(newFullPath));
            }
            return newFullPath;
        }

        #endregion
    }
}
