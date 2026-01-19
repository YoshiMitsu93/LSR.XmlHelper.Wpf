using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Models;
using LSR.XmlHelper.Wpf.Services;
using LSR.XmlHelper.Wpf.Services.Appearance;
using LSR.XmlHelper.Wpf.Services.Guides;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class XmlGuidesWindowViewModel : ObservableObject
    {
        private readonly XmlGuideStoreService _store;
        private readonly ObservableCollection<XmlGuide> _allGuides;

        private XmlGuide? _selectedGuide;
        private string _searchText = "";

        private string _title = "";
        private string _category = "";
        private string _summary = "";
        private string _body = "";

        private FlowDocument _guideDocument = new FlowDocument();

        private bool _isDirty;
        private bool _isEditMode;
        private string _guideFontFamily = "";
        private double _guideFontSize;
        private double _guideZoom = 100;

        public XmlGuidesWindowViewModel(AppearanceService appearance, string helperRootFolder)
        {
            Appearance = appearance;

            GuideFontFamilies = new ObservableCollection<string>(
            System.Windows.Media.Fonts.SystemFontFamilies
            .Select(x => x.Source)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

            GuideFontSizes = new ObservableCollection<double>(new[]
            {
                10d, 11d, 12d, 13d, 14d, 15d, 16d, 18d, 20d, 22d, 24d, 28d, 32d
            });

            _guideFontFamily = Appearance.UiFontFamily.Source;
            _guideFontSize = Appearance.UiFontSize;

            _store = new XmlGuideStoreService(helperRootFolder);
            _allGuides = new ObservableCollection<XmlGuide>(_store.LoadAll());

            GuidesView = CollectionViewSource.GetDefaultView(_allGuides);
            GuidesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(XmlGuide.Category)));
            GuidesView.SortDescriptions.Add(new SortDescription(nameof(XmlGuide.Category), ListSortDirection.Ascending));
            GuidesView.SortDescriptions.Add(new SortDescription(nameof(XmlGuide.Title), ListSortDirection.Ascending));
            GuidesView.Filter = FilterGuide;

            NewGuideCommand = new RelayCommand(NewGuide);
            SaveGuideCommand = new RelayCommand(SaveGuide, CanSaveGuide);
            DeleteGuideCommand = new RelayCommand(DeleteGuide, CanDeleteGuide);
            ExportGuideCommand = new RelayCommand(ExportGuide, CanExportGuide);
            ImportGuideCommand = new RelayCommand(ImportGuide);
            DuplicateBuiltInCommand = new RelayCommand(DuplicateGuide, CanDuplicateGuide);
            ToggleEditModeCommand = new RelayCommand(ToggleEditMode, CanToggleEditMode);
            InsertImageCommand = new RelayCommand(InsertImage, CanInsertImage);

            SelectedGuide = _allGuides.FirstOrDefault();
        }

        public AppearanceService Appearance { get; }

        public ICollectionView GuidesView { get; }

        public ObservableCollection<string> GuideFontFamilies { get; }
        public ObservableCollection<double> GuideFontSizes { get; }

        public string GuideFontFamily
        {
            get => _guideFontFamily;
            set
            {
                if (SetProperty(ref _guideFontFamily, value))
                    RebuildGuideDocument();
            }
        }

        public double GuideFontSize
        {
            get => _guideFontSize;
            set
            {
                var v = value;
                if (v < 6)
                    v = 6;

                if (SetProperty(ref _guideFontSize, v))
                    RebuildGuideDocument();
            }
        }

        public double GuideZoom
        {
            get => _guideZoom;
            set
            {
                var v = value;
                if (v < 50)
                    v = 50;
                if (v > 300)
                    v = 300;

                SetProperty(ref _guideZoom, v);
            }
        }

        public RelayCommand NewGuideCommand { get; }
        public RelayCommand SaveGuideCommand { get; }
        public RelayCommand DeleteGuideCommand { get; }
        public RelayCommand ExportGuideCommand { get; }
        public RelayCommand ImportGuideCommand { get; }
        public RelayCommand DuplicateBuiltInCommand { get; }
        public RelayCommand ToggleEditModeCommand { get; }
        public RelayCommand InsertImageCommand { get; }

        public string GuideSourceLabel => SelectedGuide?.IsBuiltIn == true ? "Community guide" : "User guide";

        public string ModeLabel => IsEditMode ? "Edit mode" : "View mode";

        public string ToggleModeButtonText => IsEditMode ? "Switch to View" : "Switch to Edit";

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value ?? "";
                OnPropertyChanged();

                GuidesView.Refresh();
                EnsureSelectionIsVisible();
            }
        }

        public XmlGuide? SelectedGuide
        {
            get => _selectedGuide;
            set
            {
                if (_selectedGuide == value)
                    return;

                _selectedGuide = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GuideSourceLabel));

                LoadEditorFromSelection();
                InvalidateCommands();
            }
        }

        public bool IsBuiltInSelected => SelectedGuide?.IsBuiltIn == true;

        public bool IsEditMode
        {
            get => _isEditMode;
            private set
            {
                if (_isEditMode == value)
                    return;

                _isEditMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModeLabel));
                OnPropertyChanged(nameof(ToggleModeButtonText));
                InvalidateCommands();
            }
        }

        public FlowDocument GuideDocument
        {
            get => _guideDocument;
            private set
            {
                _guideDocument = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value)
                    return;

                _title = value ?? "";
                OnPropertyChanged();
                MarkDirty();
                RebuildGuideDocument();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                if (_category == value)
                    return;

                _category = value ?? "";
                OnPropertyChanged();
                MarkDirty();
                RebuildGuideDocument();
            }
        }

        public string Summary
        {
            get => _summary;
            set
            {
                if (_summary == value)
                    return;

                _summary = value ?? "";
                OnPropertyChanged();
                MarkDirty();
                RebuildGuideDocument();
            }
        }

        public string Body
        {
            get => _body;
            set
            {
                if (_body == value)
                    return;

                _body = value ?? "";
                OnPropertyChanged();
                MarkDirty();
                RebuildGuideDocument();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;
                OnPropertyChanged();
            }
        }

        private bool FilterGuide(object obj)
        {
            if (obj is not XmlGuide g)
                return false;

            var q = (SearchText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q))
                return true;

            return (g.Title ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                || (g.Category ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                || (g.Summary ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                || (g.Body ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureSelectionIsVisible()
        {
            if (SelectedGuide is not null && GuidesView.Cast<object>().Contains(SelectedGuide))
                return;

            SelectedGuide = GuidesView.Cast<XmlGuide>().FirstOrDefault();
        }

        private void LoadEditorFromSelection()
        {
            if (SelectedGuide is null)
            {
                _title = "";
                _category = "";
                _summary = "";
                _body = "";
                IsDirty = false;
                IsEditMode = false;

                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(Summary));
                OnPropertyChanged(nameof(Body));
                OnPropertyChanged(nameof(IsBuiltInSelected));

                RebuildGuideDocument();
                return;
            }

            _title = SelectedGuide.Title ?? "";
            _category = SelectedGuide.Category ?? "";
            _summary = SelectedGuide.Summary ?? "";
            _body = SelectedGuide.Body ?? "";
            IsDirty = false;
            IsEditMode = false;

            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(Body));
            OnPropertyChanged(nameof(IsBuiltInSelected));

            RebuildGuideDocument();
        }

        private void MarkDirty()
        {
            if (!IsEditMode)
                return;

            IsDirty = true;
            InvalidateCommands();
        }

        private void InvalidateCommands()
        {
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            OnPropertyChanged(nameof(IsBuiltInSelected));
        }

        private void NewGuide()
        {
            var g = new XmlGuide
            {
                Title = "New guide",
                Category = "Uncategorized",
                Summary = "",
                Body = "",
                IsBuiltIn = false
            };

            _allGuides.Add(g);
            SelectedGuide = g;

            IsEditMode = true;
            IsDirty = true;
            InvalidateCommands();
        }

        private bool CanSaveGuide()
        {
            if (!IsEditMode)
                return false;

            if (SelectedGuide is null)
                return false;

            if (SelectedGuide.IsBuiltIn)
                return false;

            if (!IsDirty)
                return false;

            return !string.IsNullOrWhiteSpace(Title);
        }

        private void SaveGuide()
        {
            if (SelectedGuide is null)
                return;

            if (SelectedGuide.IsBuiltIn)
                return;

            var title = (Title ?? "").Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                System.Windows.MessageBox.Show("Title is required.", "Save guide", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            SelectedGuide.Title = title;
            SelectedGuide.Category = string.IsNullOrWhiteSpace(Category) ? "Uncategorized" : Category.Trim();
            SelectedGuide.Summary = Summary ?? "";
            SelectedGuide.Body = Body ?? "";
            SelectedGuide.UpdatedUtc = DateTimeOffset.UtcNow;

            _store.SaveUserGuides(_allGuides);

            IsDirty = false;
            GuidesView.Refresh();
            InvalidateCommands();
        }

        private bool CanDeleteGuide() => IsEditMode && SelectedGuide is not null && !SelectedGuide.IsBuiltIn;

        private void DeleteGuide()
        {
            if (SelectedGuide is null)
                return;

            if (SelectedGuide.IsBuiltIn)
                return;

            var ok = System.Windows.MessageBox.Show(
                $"Delete \"{SelectedGuide.Title}\"?",
                "Delete guide",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (ok != System.Windows.MessageBoxResult.Yes)
                return;

            var toRemove = SelectedGuide;
            SelectedGuide = null;

            _allGuides.Remove(toRemove);
            _store.SaveUserGuides(_allGuides);

            GuidesView.Refresh();
            EnsureSelectionIsVisible();

            IsDirty = false;
            IsEditMode = false;
            InvalidateCommands();
        }

        private bool CanExportGuide() => SelectedGuide is not null;

        private void ExportGuide()
        {
            if (SelectedGuide is null)
                return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "LSR Guide Pack (*.lsrguidepack)|*.lsrguidepack|LSR Guide JSON (*.lsrguide.json)|*.lsrguide.json|JSON (*.json)|*.json",
                FileName = MakeSafeFileName(SelectedGuide.Title) + ".lsrguidepack"
            };

            var ok = dialog.ShowDialog();
            if (ok != true)
                return;

            var ext = (Path.GetExtension(dialog.FileName) ?? "").ToLowerInvariant();

            if (ext == ".lsrguidepack")
            {
                var pack = new XmlGuidePackService();
                pack.ExportPack(dialog.FileName, SelectedGuide, _store);

                System.Windows.MessageBox.Show("Guide pack exported.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var payload = new XmlGuide
            {
                Id = SelectedGuide.Id,
                Title = SelectedGuide.Title ?? "",
                Category = SelectedGuide.Category ?? "Uncategorized",
                Summary = SelectedGuide.Summary ?? "",
                Body = SelectedGuide.Body ?? "",
                CreatedUtc = SelectedGuide.CreatedUtc,
                UpdatedUtc = SelectedGuide.UpdatedUtc
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json);

            System.Windows.MessageBox.Show("Guide exported.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ImportGuide()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LSR Guides (*.lsrguidepack;*.lsrguide.json;*.json)|*.lsrguidepack;*.lsrguide.json;*.json"
            };

            var ok = dialog.ShowDialog();
            if (ok != true)
                return;

            try
            {
                var ext = (Path.GetExtension(dialog.FileName) ?? "").ToLowerInvariant();

                XmlGuide guide;
                IReadOnlyDictionary<string, byte[]> images = new Dictionary<string, byte[]>();

                if (ext == ".lsrguidepack")
                {
                    var pack = new XmlGuidePackService();
                    var imported = pack.ImportPack(dialog.FileName);
                    guide = imported.Guide;
                    images = imported.Images;
                }
                else
                {
                    var json = File.ReadAllText(dialog.FileName);
                    guide = JsonSerializer.Deserialize<XmlGuide>(json) ?? new XmlGuide();
                }

                guide = _store.NormalizeImportedGuide(guide);
                guide.IsBuiltIn = false;

                if (_allGuides.Any(g => string.Equals(g.Id, guide.Id, StringComparison.OrdinalIgnoreCase)))
                    guide.Id = Guid.NewGuid().ToString("N");

                foreach (var kv in images)
                    _store.SaveImportedImageToGuide(guide.Id, kv.Key, kv.Value);

                _allGuides.Add(guide);
                _store.SaveUserGuides(_allGuides);

                GuidesView.Refresh();
                SelectedGuide = guide;
                IsEditMode = true;
                IsDirty = false;

                System.Windows.MessageBox.Show("Guide imported.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDuplicateGuide() => SelectedGuide is not null;

        private void DuplicateGuide()
        {
            if (SelectedGuide is null)
                return;

            var src = SelectedGuide;

            var copy = new XmlGuide
            {
                Title = (src.Title ?? "Guide") + " (Copy)",
                Category = src.Category ?? "Uncategorized",
                Summary = src.Summary ?? "",
                Body = src.Body ?? "",
                IsBuiltIn = false
            };

            _allGuides.Add(copy);

            foreach (var file in ExtractImageTokens(src.Body ?? ""))
                _store.CopyImageBetweenGuides(src.Id, copy.Id, file);

            SelectedGuide = copy;

            IsEditMode = true;
            IsDirty = true;
            InvalidateCommands();
        }

        private bool CanToggleEditMode() => SelectedGuide is not null;

        private void ToggleEditMode()
        {
            if (SelectedGuide is null)
                return;

            if (IsEditMode)
            {
                if (IsDirty && !SelectedGuide.IsBuiltIn)
                {
                    var ok = System.Windows.MessageBox.Show(
                        "Discard unsaved changes and exit edit mode?",
                        "Exit edit mode",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Warning);

                    if (ok != System.Windows.MessageBoxResult.Yes)
                        return;

                    LoadEditorFromSelection();
                    return;
                }

                IsEditMode = false;
                IsDirty = false;
                RebuildGuideDocument();
                return;
            }

            if (SelectedGuide.IsBuiltIn)
            {
                var ok = System.Windows.MessageBox.Show(
                    "This guide is built-in (read-only).\n\nClick Yes to duplicate it so you can edit your own copy.",
                    "Edit built-in guide",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Information);

                if (ok != System.Windows.MessageBoxResult.Yes)
                    return;

                DuplicateGuide();
                return;
            }

            IsEditMode = true;
            IsDirty = false;
            InvalidateCommands();
        }

        private bool CanInsertImage() => IsEditMode && SelectedGuide is not null && !SelectedGuide.IsBuiltIn;

        private void InsertImage()
        {
            if (SelectedGuide is null)
                return;

            if (SelectedGuide.IsBuiltIn)
                return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp"
            };

            var ok = dialog.ShowDialog();
            if (ok != true)
                return;

            try
            {
                var fileName = _store.AddImageToGuide(SelectedGuide.Id, dialog.FileName);
                var token = $"[[img:{fileName}]]";

                if (string.IsNullOrWhiteSpace(Body))
                    Body = token;
                else
                    Body = Body.TrimEnd() + Environment.NewLine + Environment.NewLine + token + Environment.NewLine;

                IsDirty = true;
                InvalidateCommands();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Insert image", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        public bool TryPasteClipboardImage(out string token)
        {
            token = "";

            if (SelectedGuide is null)
                return false;

            if (!IsEditMode)
                return false;

            if (!System.Windows.Clipboard.ContainsImage())
                return false;

            var image = System.Windows.Clipboard.GetImage();
            if (image is null)
                return false;

            var fileName = _store.AddPastedImageToGuide(SelectedGuide.Id, image);
            token = "[[img:" + fileName + "]]";
            return true;
        }

        private Paragraph CreateParagraphFromInlineMarkup(string text)
        {
            var p = new Paragraph();
            foreach (var inline in BuildInlines(text))
                p.Inlines.Add(inline);
            return p;
        }

        private System.Collections.Generic.IEnumerable<Inline> BuildInlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield return new Run("");
                yield break;
            }

            var i = 0;
            while (i < text.Length)
            {
                if (!TryFindNextInlineToken(text, i, out var token, out var tokenIndex))
                {
                    yield return new Run(text.Substring(i));
                    yield break;
                }

                if (tokenIndex > i)
                    yield return new Run(text.Substring(i, tokenIndex - i));

                var end = text.IndexOf(token, tokenIndex + token.Length, StringComparison.Ordinal);
                if (end < 0)
                {
                    yield return new Run(text.Substring(tokenIndex));
                    yield break;
                }

                var inner = text.Substring(tokenIndex + token.Length, end - (tokenIndex + token.Length));
                Inline formatted;

                if (token == "**")
                    formatted = new Bold(new Run(inner));
                else if (token == "__")
                    formatted = new Underline(new Run(inner));
                else
                    formatted = new Italic(new Run(inner));

                yield return formatted;

                i = end + token.Length;
            }
        }

        private static bool TryFindNextInlineToken(string text, int startIndex, out string token, out int tokenIndex)
        {
            var bold = text.IndexOf("**", startIndex, StringComparison.Ordinal);
            var underline = text.IndexOf("__", startIndex, StringComparison.Ordinal);
            var italic = text.IndexOf("*", startIndex, StringComparison.Ordinal);

            token = "";
            tokenIndex = -1;

            var best = -1;
            var bestToken = "";

            void Consider(int idx, string t)
            {
                if (idx < 0)
                    return;

                if (best < 0 || idx < best || (idx == best && t.Length > bestToken.Length))
                {
                    best = idx;
                    bestToken = t;
                }
            }

            Consider(bold, "**");
            Consider(underline, "__");
            Consider(italic, "*");

            if (best < 0)
                return false;

            token = bestToken;
            tokenIndex = best;
            return true;
        }
        private void RebuildGuideDocument()
        {
            var doc = new FlowDocument();

            if (!string.IsNullOrWhiteSpace(GuideFontFamily))
                doc.FontFamily = new System.Windows.Media.FontFamily(GuideFontFamily);
            else
                doc.FontFamily = Appearance.UiFontFamily;

            doc.FontSize = GuideFontSize > 0 ? GuideFontSize : Appearance.UiFontSize;

            if (SelectedGuide is null)
            {
                GuideDocument = doc;
                return;
            }

            if (!string.IsNullOrWhiteSpace(Title))
            {
                doc.Blocks.Add(new Paragraph(new Run(Title))
                {
                    FontSize = doc.FontSize + 8,
                    FontWeight = System.Windows.FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 6)
                });
            }

            var meta = BuildMetaLine();
            if (!string.IsNullOrWhiteSpace(meta))
            {
                doc.Blocks.Add(new Paragraph(new Run(meta))
                {
                    FontStyle = System.Windows.FontStyles.Italic,
                    FontSize = Math.Max(8, doc.FontSize - 2),
                    Margin = new Thickness(0, 0, 0, 10)
                });
            }

            if (!string.IsNullOrWhiteSpace(Summary))
            {
                doc.Blocks.Add(new Paragraph(new Run(Summary))
                {
                    Margin = new Thickness(0, 0, 0, 12)
                });
            }

            AppendBodyBlocks(doc);
            GuideDocument = doc;
        }

        private Block? BuildImageBlock(string fileName)
        {
            if (SelectedGuide is null)
                return null;

            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var folder = _store.GetImagesFolder(SelectedGuide.Id);
            var path = Path.Combine(folder, fileName);

            if (!File.Exists(path))
            {
                return new Paragraph(new Run("[Missing image: " + fileName + "]"))
                {
                    FontStyle = System.Windows.FontStyles.Italic,
                    Margin = new Thickness(0, 6, 0, 6)
                };
            }

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();

                var img = new System.Windows.Controls.Image
                {
                    Source = bmp,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    MaxHeight = 520,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };

                var border = new System.Windows.Controls.Border
                {
                    BorderBrush = Appearance.DocumentationWindowControlBorderBrush,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 6, 0, 6),
                    Child = img,
                    Cursor = System.Windows.Input.Cursors.Hand,
                };

                border.MouseLeftButtonUp += (_, __) => OpenImage(path);

                return new BlockUIContainer(border)
                {
                    Margin = new Thickness(0, 8, 0, 8)
                };
            }
            catch
            {
                return new Paragraph(new Run("[Failed to load image: " + fileName + "]"))
                {
                    FontStyle = System.Windows.FontStyles.Italic,
                    Margin = new Thickness(0, 6, 0, 6)
                };
            }
        }

        private void AppendBodyBlocks(FlowDocument doc)
        {
            var body = (Body ?? "").TrimEnd();
            if (string.IsNullOrWhiteSpace(body))
                return;

            var lines = body.Replace("\r\n", "\n").Split('\n');

            var inBullet = false;
            List? bulletList = null;

            foreach (var raw in lines)
            {
                var line = raw ?? "";
                var trimmed = line.Trim();

                if (TryParseImageToken(trimmed, out var fileName))
                {
                    inBullet = false;
                    bulletList = null;

                    var imgBlock = BuildImageBlock(fileName);
                    if (imgBlock is not null)
                        doc.Blocks.Add(imgBlock);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    inBullet = false;
                    bulletList = null;
                    doc.Blocks.Add(new Paragraph(new Run("")) { Margin = new Thickness(0, 0, 0, 8) });
                    continue;
                }

                if (trimmed.StartsWith("# ", StringComparison.Ordinal))
                {
                    inBullet = false;
                    bulletList = null;

                    var h = trimmed.Substring(2).Trim();
                    var p = new Paragraph(new Run(h))
                    {
                        FontSize = doc.FontSize + 2,
                        FontWeight = System.Windows.FontWeights.SemiBold,
                        Margin = new Thickness(0, 12, 0, 6)
                    };
                    doc.Blocks.Add(p);
                    continue;
                }

                if (trimmed.StartsWith("- ", StringComparison.Ordinal))
                {
                    if (!inBullet || bulletList is null)
                    {
                        bulletList = new List
                        {
                            MarkerStyle = TextMarkerStyle.Disc,
                            Margin = new Thickness(18, 0, 0, 8)
                        };
                        doc.Blocks.Add(bulletList);
                        inBullet = true;
                    }

                    var text = trimmed.Substring(2);
                    bulletList.ListItems.Add(new ListItem(CreateParagraphFromInlineMarkup(text)));
                    continue;
                }

                inBullet = false;
                bulletList = null;

                var p2 = CreateParagraphFromInlineMarkup(trimmed);
                p2.Margin = new Thickness(0, 0, 0, 8);
                doc.Blocks.Add(p2);
            }
        }

        private string BuildMetaLine()
        {
            var parts = new StringBuilder();

            var cat = (Category ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(cat))
                parts.Append("Category: ").Append(cat);

            if (SelectedGuide is not null)
            {
                if (parts.Length > 0)
                    parts.Append("  •  ");

                parts.Append(SelectedGuide.IsBuiltIn ? "Community guide" : "My guide");
            }

            return parts.ToString();
        }

        private void AppendBodyBlocks(FlowDocument doc, string body)
        {
            var lines = body.Replace("\r\n", "\n").Split('\n');

            var inBullet = false;
            List? bulletList = null;

            foreach (var raw in lines)
            {
                var line = raw ?? "";
                var trimmed = line.Trim();

                if (TryParseImageToken(trimmed, out var fileName))
                {
                    inBullet = false;
                    bulletList = null;

                    var path = SelectedGuide is null ? null : _store.TryGetImagePath(SelectedGuide.Id, fileName);
                    if (path is null)
                    {
                        doc.Blocks.Add(new Paragraph(new Run($"[Missing image: {fileName}]"))
                        {
                            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0))
                        });
                        continue;
                    }

                    var img = new System.Windows.Controls.Image();
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(path, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    img.Source = bmp;
                    img.MaxWidth = 760;
                    img.Stretch = System.Windows.Media.Stretch.Uniform;

                    var button = new System.Windows.Controls.Button
                    {
                        Background = System.Windows.Media.Brushes.Transparent,
                        BorderThickness = new System.Windows.Thickness(0),
                        Padding = new System.Windows.Thickness(0),
                        Content = img,
                        ToolTip = "Click to view and zoom"
                    };

                    button.Click += (_, _) => OpenImage(path);

                    doc.Blocks.Add(new BlockUIContainer(button) { Margin = new System.Windows.Thickness(0, 6, 0, 6) });
                    continue;
                }

                if (trimmed.StartsWith("# ", StringComparison.Ordinal))
                {
                    inBullet = false;
                    bulletList = null;

                    doc.Blocks.Add(new Paragraph(new Run(trimmed.Substring(2)))
                    {
                        FontSize = 18,
                        FontWeight = System.Windows.FontWeights.SemiBold,
                        Margin = new System.Windows.Thickness(0, 10, 0, 6)
                    });
                    continue;
                }

                if (trimmed.StartsWith("## ", StringComparison.Ordinal))
                {
                    inBullet = false;
                    bulletList = null;

                    doc.Blocks.Add(new Paragraph(new Run(trimmed.Substring(3)))
                    {
                        FontSize = 16,
                        FontWeight = System.Windows.FontWeights.SemiBold,
                        Margin = new System.Windows.Thickness(0, 8, 0, 4)
                    });
                    continue;
                }

                if (trimmed.StartsWith("- ", StringComparison.Ordinal))
                {
                    if (!inBullet)
                    {
                        bulletList = new List { MarkerStyle = TextMarkerStyle.Disc };
                        doc.Blocks.Add(bulletList);
                        inBullet = true;
                    }

                    bulletList!.ListItems.Add(new ListItem(new Paragraph(new Run(trimmed.Substring(2)))));
                    continue;
                }

                inBullet = false;
                bulletList = null;

                doc.Blocks.Add(new Paragraph(new Run(line)));
            }
        }
        private void OpenImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!File.Exists(path))
                return;

            var w = new LSR.XmlHelper.Wpf.Views.GuideImageViewerWindow(path)
            {
                Owner = System.Windows.Application.Current?.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.IsActive)
            };

            w.Show();
            w.Activate();
        }

        private static readonly System.Text.RegularExpressions.Regex ImageTokenRegex =
            new System.Text.RegularExpressions.Regex(@"\[\[img:(?<f>[^\]]+)\]\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

        private static string[] ExtractImageTokens(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Array.Empty<string>();

            var matches = ImageTokenRegex.Matches(body);
            return matches
                .Select(m => (m.Groups["f"].Value ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static bool TryParseImageToken(string text, out string fileName)
        {
            fileName = "";

            if (!text.StartsWith("[[img:", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!text.EndsWith("]]", StringComparison.Ordinal))
                return false;

            var inner = text.Substring(6, text.Length - 8).Trim();
            if (string.IsNullOrWhiteSpace(inner))
                return false;

            fileName = inner;
            return true;
        }

        private static string MakeSafeFileName(string? value)
        {
            var name = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return "Guide";

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");

            name = name.Replace(" ", "_");
            return name.Length > 60 ? name.Substring(0, 60) : name;
        }
    }
}
