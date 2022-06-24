using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModelReviewFunction.Framework
{
    public abstract class CmdBase : IExternalCommand
    {
        protected Document _document = null;
        protected UIDocument _uiDocument = null;
        protected ExternalCommandData _revit = null;
        protected Autodesk.Revit.Creation.Application _appCreate = null;
        protected UIApplication _uiApp = null;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                System.Diagnostics.Debug.Assert(revit != null);

                _revit = revit;
                _uiDocument = revit.Application.ActiveUIDocument;
                _appCreate = revit.Application.Application.Create;
                _document = _uiDocument != null ? _uiDocument.Document : null;
                _uiApp = revit.Application;

                if (CmdCanExecute())
                {
                    CmdExecuteBefore();

                    CmdExecute();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                //用户取消选择不需要记录到日志文件
                var isOperationCanceled = ex is Autodesk.Revit.Exceptions.OperationCanceledException;

                if (!isOperationCanceled)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine(string.Format("执行 \"{0}\" 时发生错误", CommandName));
                    msg.AppendLine(ex.Message + ex.StackTrace);
                    if (ex.InnerException != null)
                        msg.Append("-->").AppendLine(ex.InnerException.Message);

                    MessageBox.Show(msg.ToString(), "错误消息");
                }

                return Result.Failed;
            }
            finally
            {
                try
                {
                    CmdExecuteEnd();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    _uiDocument = null;
                    _document = null;
                    _revit = null;
                }
            }
        }
        /// <summary>
        /// 判断是否能执行命令
        /// </summary>
        /// <returns></returns>
        protected virtual bool CmdCanExecute()
        {
            return true;
        }
        /// <summary>
        /// 执行命令前的初始化
        /// </summary>
        protected virtual void CmdExecuteBefore() { }
        /// <summary>
        /// 执行命令
        /// </summary>
        protected abstract void CmdExecute();
        /// <summary>
        /// 命令结束时的清理工作
        /// </summary>
        protected virtual void CmdExecuteEnd() { }
        /// <summary>
        /// 获取 执行命令的名称
        /// </summary>
        public abstract string CommandName { get; }

        //public abstract object BtnFunctionEnum { get; }
    }

    public abstract class CmdPostCmdBase : CmdBase
    {
        protected abstract RevitCommandId CommandId { get; }
        protected override void CmdExecute()
        {
            System.Diagnostics.Debug.Assert(CommandId != null && _revit.Application.CanPostCommand(CommandId));

            _revit.Application.PostCommand(CommandId);
        }
    }
}
