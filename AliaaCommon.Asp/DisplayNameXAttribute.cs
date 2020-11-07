using System;
using System.ComponentModel;
using System.Web;

namespace AliaaCommon
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class DisplayNameXAttribute : DisplayNameAttribute
    {
        public DisplayNameXAttribute(string name) : base(name) { }

        private bool fromResource;
        private string resourceName;

        public DisplayNameXAttribute(string resourceName, bool fromResource)
        {
            this.fromResource = fromResource;
            this.resourceName = resourceName;
        }

        public override string DisplayName
        {
            get
            {
                if(fromResource)
                    return (string)HttpContext.GetGlobalResourceObject("Res", resourceName);
                return base.DisplayName;
            }
        }
        
    }
}