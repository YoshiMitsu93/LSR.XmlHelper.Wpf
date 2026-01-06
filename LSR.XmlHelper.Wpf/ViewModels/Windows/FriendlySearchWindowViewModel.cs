using LSR.XmlHelper.Wpf.Infrastructure;
using System.Windows.Input;
using System;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class FriendlySearchWindowViewModel : ObservableObject
    {
        private readonly Action<string, bool> _findNext;

        private string _query;
        private bool _caseSensitive;

        public FriendlySearchWindowViewModel(Action<string, bool> findNext, string initialQuery = "")
        {
            _findNext = findNext ?? throw new ArgumentNullException(nameof(findNext));
            _query = initialQuery ?? "";
            FindNextCommand = new RelayCommand(FindNext, () => !string.IsNullOrWhiteSpace(Query));
        }

        public RelayCommand FindNextCommand { get; }

        public string Query
        {
            get => _query;
            set
            {
                if (!SetProperty(ref _query, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CaseSensitive
        {
            get => _caseSensitive;
            set => SetProperty(ref _caseSensitive, value);
        }

        private void FindNext()
        {
            _findNext(Query, CaseSensitive);
        }
    }
}