using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon
{
    public interface IStringNormalizer
    {
        string NormalizeString(string str);
        void NormalizeStringsInObject(object obj);
    }
}
