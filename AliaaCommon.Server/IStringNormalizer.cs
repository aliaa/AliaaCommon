using EasyMongoNet;

namespace AliaaCommon
{
    public interface IStringNormalizer : IObjectSavePreprocessor
    {
        string NormalizeString(string str);
    }
}
