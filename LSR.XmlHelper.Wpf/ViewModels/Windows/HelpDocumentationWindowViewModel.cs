using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Models;
using LSR.XmlHelper.Wpf.Services;
using LSR.XmlHelper.Wpf.Services.Help;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class HelpDocumentationWindowViewModel : ObservableObject
    {
        private readonly ObservableCollection<HelpTopic> _allTopics;
        private readonly HelpContentService _content;

        private HelpTopic? _selectedTopic;
        private string _searchText = "";

        public HelpDocumentationWindowViewModel(AppearanceService appearance)
        {
            Appearance = appearance;
            _content = new HelpContentService();

            _allTopics = new ObservableCollection<HelpTopic>(_content.GetTopics());

            TopicsView = CollectionViewSource.GetDefaultView(_allTopics);
            TopicsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(HelpTopic.Category)));
            TopicsView.SortDescriptions.Add(new SortDescription(nameof(HelpTopic.CategoryOrder), ListSortDirection.Ascending));
            TopicsView.SortDescriptions.Add(new SortDescription(nameof(HelpTopic.Category), ListSortDirection.Ascending));
            TopicsView.SortDescriptions.Add(new SortDescription(nameof(HelpTopic.TopicOrder), ListSortDirection.Ascending));
            TopicsView.SortDescriptions.Add(new SortDescription(nameof(HelpTopic.Title), ListSortDirection.Ascending));
            TopicsView.Filter = FilterTopic;

            SelectedTopic = _allTopics.FirstOrDefault();
        }

        public AppearanceService Appearance { get; }

        public string AppVersion
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v is null ? "Unknown" : v.ToString(3);
            }
        }

        public ICollectionView TopicsView { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value ?? "";
                OnPropertyChanged();

                TopicsView.Refresh();
                EnsureSelectionIsVisible();
            }
        }

        public HelpTopic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (_selectedTopic == value)
                    return;

                _selectedTopic = value;
                OnPropertyChanged();
            }
        }

        private bool FilterTopic(object obj)
        {
            if (obj is not HelpTopic topic)
                return false;

            var q = (_searchText ?? "").Trim();
            if (q.Length == 0)
                return true;

            return topic.SearchBlob.IndexOf(q, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureSelectionIsVisible()
        {
            if (SelectedTopic is not null && TopicsView.Cast<object>().Contains(SelectedTopic))
                return;

            SelectedTopic = TopicsView.Cast<HelpTopic>().FirstOrDefault();
        }
    }
}
