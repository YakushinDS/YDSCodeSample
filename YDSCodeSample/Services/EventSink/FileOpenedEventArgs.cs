using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YDSCodeSample.Services.EventSink
{
    public class FileOpenedEventArgs : EventArgs
    {
        public string FilePath { get; }

        public FileOpenedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
