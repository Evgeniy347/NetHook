using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetHook.UI
{
    public partial class LogConsole : Form
    {
        public LogConsole(Helpers.ConcurrentStreamWriter _consoleStream)
        {
            InitializeComponent();
            ResizeFormHelper.Instance.AddResizeControl(richTextBox_logConsole);

            _consoleStream.SetTextBox(richTextBox_logConsole);
            this.FormClosing += (o, e) => _consoleStream.RemoveTextBox();
        }

        private void LogConsole_Load(object sender, EventArgs e)
        {

        }

        private void LogConsole_SizeChanged(object sender, EventArgs e)
        {
            ResizeFormHelper.Instance.ResizeСhangesForm(this);
        }
    }
}
