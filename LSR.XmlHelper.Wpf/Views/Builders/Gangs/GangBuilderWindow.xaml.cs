using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Views.Builders
{
    public partial class GangBuilderWindow : Window
    {
        private readonly DispatcherTimer _validationRecheckTimer;
        private INotifyPropertyChanged? _currentVm;
        private string _activeFocusTarget = "";
        private string _activeMessage = "";

        public GangBuilderWindow()
        {
            InitializeComponent();

            _validationRecheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _validationRecheckTimer.Tick += ValidationRecheckTimer_Tick;

            DataContextChanged += GangBuilderWindow_DataContextChanged;
        }

        public void FocusValidationTarget(string focusTarget, string message)
        {
            if (string.IsNullOrWhiteSpace(focusTarget))
                return;

            var element = FindName(focusTarget) as FrameworkElement;
            if (element is null)
                return;

            element.BringIntoView();
            element.Focus();

            if (!string.IsNullOrWhiteSpace(message))
                ShowValidationHint(element, focusTarget, message);
        }

        private void ShowValidationHint(FrameworkElement target, string focusTarget, string message)
        {
            _activeFocusTarget = focusTarget ?? "";
            _activeMessage = message ?? "";

            if (ValidationHintPopupTextBlock is not null)
                ValidationHintPopupTextBlock.Text = _activeMessage;

            if (ValidationHintPopup is not null)
            {
                ValidationHintPopup.PlacementTarget = target;
                ValidationHintPopup.Placement = PlacementMode.Bottom;
                ValidationHintPopup.IsOpen = true;
            }

            StartDebouncedRecheck();
        }

        private void HideValidationHint()
        {
            _activeFocusTarget = "";
            _activeMessage = "";

            if (ValidationHintPopup is not null)
                ValidationHintPopup.IsOpen = false;
        }

        private void ValidationHintCloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideValidationHint();
        }

        private void GangBuilderWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_currentVm is not null)
                _currentVm.PropertyChanged -= Vm_PropertyChanged;

            _currentVm = DataContext as INotifyPropertyChanged;

            if (_currentVm is not null)
                _currentVm.PropertyChanged += Vm_PropertyChanged;

            StartDebouncedRecheck();
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            StartDebouncedRecheck();
        }

        private void StartDebouncedRecheck()
        {
            if (string.IsNullOrWhiteSpace(_activeFocusTarget) || string.IsNullOrWhiteSpace(_activeMessage))
                return;

            _validationRecheckTimer.Stop();
            _validationRecheckTimer.Start();
        }

        private void ValidationRecheckTimer_Tick(object? sender, EventArgs e)
        {
            _validationRecheckTimer.Stop();

            if (string.IsNullOrWhiteSpace(_activeFocusTarget) || string.IsNullOrWhiteSpace(_activeMessage))
                return;

            if (DataContext is not LSR.XmlHelper.Wpf.ViewModels.Windows.GangBuilderWindowViewModel vm)
                return;

            List<LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel> issues;
            try
            {
                issues = vm.GetPreBuildIssues();
            }
            catch
            {
                return;
            }

            var stillExists = issues.Any(i =>
                string.Equals(i.FocusTarget, _activeFocusTarget, StringComparison.Ordinal) &&
                string.Equals(i.Message, _activeMessage, StringComparison.Ordinal));

            if (!stillExists)
                HideValidationHint();
        }

        private void DenVehicleSpawnsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
