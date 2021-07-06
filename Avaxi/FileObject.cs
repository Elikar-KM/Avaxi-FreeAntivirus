using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avaxi
{
    class FileObject
    {
        public string File
        {
            get { return file; }
            set { file = value; }
        }
        private string file;

        public FileObject(string fileName)
        {
            this.file = fileName;
        }
    }
}
