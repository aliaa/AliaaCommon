using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyMongoNet;

namespace AliaaCommon
{
    public interface IStringNormalizer : IObjectSavePreprocessor
    {
        string NormalizeString(string str);
    }
}
