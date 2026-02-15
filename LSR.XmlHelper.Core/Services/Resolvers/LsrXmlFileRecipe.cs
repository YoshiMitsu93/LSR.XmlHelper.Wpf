using System;

namespace LSR.XmlHelper.Core.Services.Resolvers
{
    public sealed class LsrXmlFileRecipe
    {
        public LsrXmlFileRecipe(string baseFileName, string lsrWildcardWhenConfigEmpty, Func<string, string> fileNameForConfig, bool allowAdditives, string additivePattern)
        {
            BaseFileName = baseFileName;
            LsrWildcardWhenConfigEmpty = lsrWildcardWhenConfigEmpty;
            FileNameForConfig = fileNameForConfig;
            AllowAdditives = allowAdditives;
            AdditivePattern = additivePattern;
        }

        public string BaseFileName { get; }
        public string LsrWildcardWhenConfigEmpty { get; }
        public Func<string, string> FileNameForConfig { get; }
        public bool AllowAdditives { get; }
        public string AdditivePattern { get; }
    }
}
