using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CollectionSaveAttribute : Attribute
    {
        public bool WriteLog { get; set; }
        public bool UnifyChars { get; set; }
        public bool UnifyNumbers { get; set; }

        public CollectionSaveAttribute(bool WriteLog = true, bool UnifyChars = true, bool UnifyNumbers = false)
        {
            this.WriteLog = WriteLog;
            this.UnifyChars = UnifyChars;
            this.UnifyNumbers = UnifyNumbers;
        }
    }
}
