using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.Extensions
{
    internal static class FormExtensions
    {
        public static void ShowErrorMessage(this Form form, Exception ex)
        {
            MessageBox.Show(form, System.Diagnostics.Debugger.IsAttached ? ex.ToString() : ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
