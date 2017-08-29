using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AliaaCommon
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class DisplayNameXAttribute : Attribute
    {
        public DisplayNameXAttribute(string name)
        {
            this.DisplayName = name;
        }

        public DisplayNameXAttribute(bool fromResource, string resourceName)
        {
            if (fromResource)
                DisplayName = (string)HttpContext.GetGlobalResourceObject("Res", resourceName);
        }

        public string DisplayName { get; private set; }
        
    }
}