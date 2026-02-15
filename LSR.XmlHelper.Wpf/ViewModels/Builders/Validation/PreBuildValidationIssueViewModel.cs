namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class PreBuildValidationIssueViewModel
    {
        public PreBuildValidationIssueViewModel(string message, string focusTarget)
        {
            Message = message;
            FocusTarget = focusTarget;
        }

        public string Message { get; }

        public string FocusTarget { get; }
    }
}
