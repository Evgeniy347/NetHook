using NetHook.Cores.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NetHook.UI.Helpers
{
    public class ConcurrentStreamWriter : StreamWriter
    {
        private readonly ConcurrentQueue<string> _stringQueue = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private readonly List<string> _allString = new List<string>();
        private Boolean _disposing;
        private RichTextBox _textBox;
        public RichTextBox TextBox => _textBox;
        private bool _isNewSetTextBox;

        public ConcurrentStreamWriter(Stream stream)
            : base(stream)
        {
            CreateQueueListener();
        }

        public ConcurrentStreamWriter()
            : base(new MemoryStream())
        {
            Console.SetOut(this);
            CreateQueueListener();
        }

        public void SetTextBox(RichTextBox richTextBox)
        {
            _isNewSetTextBox = true;
            _textBox = richTextBox;
            _resetEvent.Set();
        }

        public void RemoveTextBox()
        {
            _textBox = null;
        }

        public override void WriteLine()
        {
            base.WriteLine();
            _stringQueue.Enqueue(Environment.NewLine);
            _resetEvent.Set();
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            _stringQueue.Enqueue(String.Format("{0}\n", value));
            _resetEvent.Set();
        }

        public override void Write(string value)
        {
            base.Write(value);
            _stringQueue.Enqueue(value);
            _resetEvent.Set();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _disposing = disposing;
        }

        private void CreateQueueListener()
        {
            Thread thread = new Thread(() =>
            {
                while (!_disposing)
                {
                    if (_stringQueue.Count > 0)
                    {
                        string value = string.Empty;
                        if (_stringQueue.TryDequeue(out value))
                        {
                            _allString.Add(value);
                            if (_textBox != null && _textBox.IsHandleCreated)
                                WriteTextBoxCheckThread(value);
                        }
                    }

                    if (_isNewSetTextBox && (_textBox?.IsHandleCreated ?? false))
                        WriteTextBoxCheckThread();

                    _resetEvent.WaitOne();
                }
            });

            thread.Name = "TextBoxStreamWriter";
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void WriteTextBoxCheckThread(string value = null)
        {
            try
            {
                if (_textBox.InvokeRequired)
                {
                    _textBox.Invoke(new Action(() => WriteTextBox(value)));
                }
                else
                {
                    WriteTextBox(value);
                }
            }
            catch { }
        }

        private void WriteTextBox(string value = null)
        {
            if (_isNewSetTextBox)
            {
                _textBox.AppendText(_allString.JoinString(string.Empty));
                _textBox.ScrollToCaret();
                _isNewSetTextBox = false;
            }
            else
            {
                _textBox.AppendText(value);
                _textBox.ScrollToCaret();
            }
        }
    }
}
