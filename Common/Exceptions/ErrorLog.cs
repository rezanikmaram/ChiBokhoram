using PersianDate.Standard;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Common.Exceptions
{
    public class ErrorLog
    {
        //public ErrorLog(IHostingEnvironment env)
        //{
        //    RootPath = env.ContentRootPath;
        //}
        static string RootPath { get; set; } = Environment.CurrentDirectory;
        //static string _pathLog22 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Refer log\");
        //static string _pathLog = Path.Combine(HttpRuntime.AppDomainAppPath, @"Error log\");
        static string _pathLog = Path.Combine(RootPath, @"Error_log\");
        static string _pathLog2 = Path.Combine(_pathLog, @"log\");


        public static void SaveError(Exception er, string extraMessage = "")
        {
            try
            {
                //var methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                // Get stack trace for the exception with source file information
                var st = new System.Diagnostics.StackTrace(er, true);
                // Get the top stack frame
                //var frame = st.GetFrame(0);
                var frame = st.GetFrames()
                    ?.FirstOrDefault(li => li.GetMethod().Module.Assembly == Assembly.GetExecutingAssembly());

                if (frame == null) frame = st.GetFrame(0);// return;
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                var className = frame.GetMethod().ReflectedType;
                //var namespaceName = className.Namespace;
                var methodName = frame.GetMethod().Name;

                // HttpContext.Current.Server.MapPath("~")
                string fullName = Path.Combine(_pathLog, DateTime.Now.ToFa("yy-M") + ".txt");
                if (!Directory.Exists(_pathLog)) Directory.CreateDirectory(_pathLog);
                var loger = File.AppendText(fullName);
                string baseError = "";
                if (!er.Message.Equals(er.GetBaseException().Message))
                    baseError = "   --   " + er.GetBaseException().Message;

                if (!string.IsNullOrEmpty(extraMessage)) extraMessage += "  - ";
                loger.WriteLine($"{DateTime.Now.ToFa("yyyy/MM/dd hh:mm:ss")}   - {extraMessage}{er.GetAllMessages()}{baseError}  == {className}.{methodName}.{line}");
                loger.Close();
            }
            catch
            {
                //
            }

        }

        public static void SaveLog(string log)
        {
            string fullName = Path.Combine(_pathLog2, DateTime.Now.ToFa("yy-M") + ".txt");
            if (!Directory.Exists(_pathLog2)) Directory.CreateDirectory(_pathLog2);
            var loger = File.AppendText(fullName);
            loger.WriteLine($"{DateTime.Now.ToFa("yyyy/MM/dd hh:mm:ss")}    {log}");
            loger.Close();
        }
    }

    //public interface IErrorLog
    //{
    //    void SaveError(Exception er, string extraMessage = "");
    //    void SaveLog(string log);
    //}
}
