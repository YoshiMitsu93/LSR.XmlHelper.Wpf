namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class XmlFieldApplyIssue
    {
        public XmlFieldApplyIssue(int personIndex, string fieldName)
        {
            PersonIndex = personIndex;
            FieldName = fieldName ?? "";
        }

        public int PersonIndex { get; }

        public string FieldName { get; }
    }
}
