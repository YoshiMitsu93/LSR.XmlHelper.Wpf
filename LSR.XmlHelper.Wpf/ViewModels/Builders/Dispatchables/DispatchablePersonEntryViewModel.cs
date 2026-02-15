using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DispatchablePersonEntryViewModel : ObservableObject
    {
        private readonly Dictionary<string, DispatchablePersonFieldViewModel> _byName;

        public DispatchablePersonEntryViewModel(string sourceDebugName, int sourceIndex, IEnumerable<DispatchablePersonFieldViewModel> fields)
        {
            SourceDebugName = sourceDebugName;
            SourceIndex = sourceIndex;

            Fields = new ObservableCollection<DispatchablePersonFieldViewModel>(fields);
            _byName = Fields.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var f in Fields)
                f.PropertyChanged += FieldOnPropertyChanged;
        }

        public string SourceDebugName { get; }

        public int SourceIndex { get; }

        public ObservableCollection<DispatchablePersonFieldViewModel> Fields { get; }

        public string DebugName
        {
            get => GetText("DebugName");
            set
            {
                SetText("DebugName", value);
                OnPropertyChanged(nameof(DebugName));
            }
        }

        public string ModelName
        {
            get => GetText("ModelName");
            set
            {
                SetText("ModelName", value);
                OnPropertyChanged(nameof(ModelName));
            }
        }

        public bool AlwaysHasLongGun
        {
            get => string.Equals(GetText("AlwaysHasLongGun").Trim(), "true", StringComparison.OrdinalIgnoreCase);
            set
            {
                SetText("AlwaysHasLongGun", value ? "true" : "false");
                OnPropertyChanged(nameof(AlwaysHasLongGun));
            }
        }

        public int MinWantedLevelSpawn
        {
            get => GetInt("MinWantedLevelSpawn");
            set
            {
                SetText("MinWantedLevelSpawn", value.ToString(CultureInfo.InvariantCulture));
                OnPropertyChanged(nameof(MinWantedLevelSpawn));
            }
        }

        public int MaxWantedLevelSpawn
        {
            get => GetInt("MaxWantedLevelSpawn");
            set
            {
                SetText("MaxWantedLevelSpawn", value.ToString(CultureInfo.InvariantCulture));
                OnPropertyChanged(nameof(MaxWantedLevelSpawn));
            }
        }

        private void FieldOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DispatchablePersonFieldViewModel f)
                return;

            if (string.Equals(f.Name, "DebugName", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(DebugName));

            if (string.Equals(f.Name, "ModelName", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(ModelName));

            if (string.Equals(f.Name, "AlwaysHasLongGun", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(AlwaysHasLongGun));

            if (string.Equals(f.Name, "MinWantedLevelSpawn", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(MinWantedLevelSpawn));

            if (string.Equals(f.Name, "MaxWantedLevelSpawn", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(MaxWantedLevelSpawn));
        }

        private string GetText(string name)
        {
            if (_byName.TryGetValue(name, out var f))
                return f.Value ?? "";

            var created = new DispatchablePersonFieldViewModel(name, "", false);
            created.PropertyChanged += FieldOnPropertyChanged;

            Fields.Add(created);
            _byName[name] = created;

            return created.Value ?? "";
        }

        private void SetText(string name, string value)
        {
            if (_byName.TryGetValue(name, out var f))
            {
                f.Value = value ?? "";
                return;
            }

            var created = new DispatchablePersonFieldViewModel(name, value ?? "", false);
            created.PropertyChanged += FieldOnPropertyChanged;

            Fields.Add(created);
            _byName[name] = created;
        }

        private int GetInt(string name)
        {
            var txt = GetText(name).Trim();
            if (int.TryParse(txt, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                return v;

            return 0;
        }
    }
}
