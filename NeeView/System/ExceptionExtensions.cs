using System;

namespace NeeView
{
    public static class ExceptionExtensions
    {
        public static string ToMessageString(this Exception ex)
        {
            if (ex == null) return "";

            string text = "";

            if (ex.InnerException != null)
            {
                text = ex.InnerException.ToMessageString();
            }

            text += ex.Message + System.Environment.NewLine;

            return text;
        }

        public static string ToStackString(this Exception ex)
        {
            if (ex == null) return "";

            string text = "";

            if (ex.InnerException != null)
            {
                text = ex.InnerException.ToStackString();
            }

            text += $"{ex.GetType()}: {ex.Message}" + System.Environment.NewLine + ex.StackTrace + System.Environment.NewLine;

            return text;
        }
    }
}
