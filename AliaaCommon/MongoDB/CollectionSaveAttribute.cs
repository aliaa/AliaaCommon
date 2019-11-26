using System;

namespace AliaaCommon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CollectionSaveAttribute : Attribute
    {
        public bool WriteLog { get; set; }
        public bool NormalizeStrings { get; set; }

        public CollectionSaveAttribute(bool WriteLog = true, bool NormalizeStrings = true)
        {
            this.WriteLog = WriteLog;
            this.NormalizeStrings = NormalizeStrings;
        }
    }
}
