using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Parsing
{
    public sealed class SmartRequiredVariationPasteParser
    {
        private static readonly Regex RequiredVariationRegex = new Regex(@"<RequiredVariation\b[\s\S]*?</RequiredVariation>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PedVariationRegex = new Regex(@"<PedVariation\b[\s\S]*?</PedVariation>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SavedOutfitRegex = new Regex(@"<SavedOutfit\b[\s\S]*?</SavedOutfit>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool TryGetRequiredVariationXml(string input, out string requiredVariationXml)
        {
            requiredVariationXml = "";

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var requiredMatch = RequiredVariationRegex.Match(input);
            if (requiredMatch.Success)
            {
                requiredVariationXml = NormalizeXml(requiredMatch.Value);
                return !string.IsNullOrWhiteSpace(requiredVariationXml);
            }

            var pedVarMatch = PedVariationRegex.Match(input);
            if (pedVarMatch.Success)
            {
                requiredVariationXml = ConvertPedVariationXmlToRequiredVariation(pedVarMatch.Value);
                return !string.IsNullOrWhiteSpace(requiredVariationXml);
            }

            var savedMatch = SavedOutfitRegex.Match(input);
            if (savedMatch.Success)
            {
                var pedInside = PedVariationRegex.Match(savedMatch.Value);
                if (pedInside.Success)
                {
                    requiredVariationXml = ConvertPedVariationXmlToRequiredVariation(pedInside.Value);
                    return !string.IsNullOrWhiteSpace(requiredVariationXml);
                }
            }

            return false;
        }

        private static string ConvertPedVariationXmlToRequiredVariation(string pedVariationXml)
        {
            try
            {
                var doc = XDocument.Parse(pedVariationXml, LoadOptions.PreserveWhitespace);
                if (doc.Root is null)
                    return "";

                var wrapper = new XElement("RequiredVariation");

                foreach (var node in doc.Root.Nodes())
                {
                    wrapper.Add(node);
                }

                return NormalizeXml(wrapper.ToString(SaveOptions.DisableFormatting));
            }
            catch
            {
                return "";
            }
        }

        private static string NormalizeXml(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
                return doc.Root is null ? "" : doc.Root.ToString(SaveOptions.DisableFormatting);
            }
            catch
            {
                return "";
            }
        }
    }
}
