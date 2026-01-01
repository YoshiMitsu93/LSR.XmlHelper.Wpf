using System;
using System.Reflection;

namespace LSR.XmlHelper.Wpf.Services.SharedConfigs
{
    public sealed class SettingsCopyService
    {
        public void CopyPublicSettableProperties(object source, object target)
        {
            if (source is null || target is null)
                return;

            var srcType = source.GetType();
            var dstType = target.GetType();

            foreach (var sp in srcType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!sp.CanRead)
                    continue;

                var dp = dstType.GetProperty(sp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (dp is null || !dp.CanWrite)
                    continue;

                var sv = sp.GetValue(source);
                if (sv is null)
                    continue;

                if (dp.PropertyType.IsValueType || dp.PropertyType == typeof(string))
                {
                    if (dp.PropertyType == sp.PropertyType)
                        dp.SetValue(target, sv);
                    continue;
                }

                if (dp.PropertyType != sp.PropertyType)
                    continue;

                var dv = dp.GetValue(target);
                if (dv is null)
                {
                    dp.SetValue(target, sv);
                    continue;
                }

                CopyPublicSettableProperties(sv, dv);
            }
        }
    }
}
