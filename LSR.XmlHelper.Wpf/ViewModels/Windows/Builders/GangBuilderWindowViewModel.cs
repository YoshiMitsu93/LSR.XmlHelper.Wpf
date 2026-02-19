using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services.Parsing;
using LSR.XmlHelper.Wpf.ViewModels.Builders;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class GangBuilderWindowViewModel : ObservableObject
    {
        private const string NewGangVehicleGroupPlaceholder = "__NEW_GANG_VEHICLES__";
        private readonly Services.AppearanceService _appearance;
        private readonly string _rootFolderPath;
        private string _packName = "MyGangPack";
        private string _newGangId = "";
        private string _newGangFullName = "";
        private string _cloneFromGangId = "";
        private string _selectedEditGangVehicleGroupId = "";
        private string _minimumRep = "";
        private string _maximumRep = "";
        private string _startingRep = "";
        private string _hostileRepLevel = "";
        private string _neutralRepLevel = "";
        private string _friendlyRepLevel = "";
        private string _memberOfferRepLevel = "";
        private string _hitSquadRep = "";
        private string _pickupPaymentMin = "";
        private string _pickupPaymentMax = "";
        private string _theftPaymentMin = "";
        private string _theftPaymentMax = "";
        private string _hitPaymentMin = "";
        private string _hitPaymentMax = "";
        private string _deliveryPaymentMin = "";
        private string _deliveryPaymentMax = "";
        private string _wheelmanPaymentMin = "";
        private string _wheelmanPaymentMax = "";
        private string _impoundTheftPaymentMin = "";
        private string _impoundTheftPaymentMax = "";
        private string _bodyDisposalPaymentMin = "";
        private string _bodyDisposalPaymentMax = "";
        private string _copHitPaymentMin = "";
        private string _copHitPaymentMax = "";
        private string _ambushPaymentMin = "";
        private string _ambushPaymentMax = "";
        private string _briberyPaymentMin = "";
        private string _briberyPaymentMax = "";
        private string _arsonPaymentMin = "";
        private string _arsonPaymentMax = "";
        private string _fightPercentage = "";
        private string _fightPolicePercentage = "";
        private string _alwaysFightPolicePercentage = "";
        private string _drugDealerPercentage = "";
        private string _ambientMemberMoneyMin = "";
        private string _ambientMemberMoneyMax = "";
        private string _dealerMemberMoneyMin = "";
        private string _dealerMemberMoneyMax = "";
        private string _costToPayoffGangScalar = "";
        private string _percentageTrustingOfPlayer = "";
        private string _percentageWithLongGuns = "";
        private string _percentageWithSidearms = "";
        private string _percentageWithMelee = "";
        private string _vehicleSpawnPercentage = "";
        private string _pedestrianSpawnPercentageAroundDen = "";
        private string _memberKickUpDays = "";
        private string _memberKickUpAmount = "";
        private string _memberKickUpMissLimit = "";
        private readonly ObservableCollection<LoanParameterEntryViewModel> _loanParameters = new ObservableCollection<LoanParameterEntryViewModel>();
        private LoanParameterEntryViewModel? _selectedLoanParameter;
        private HashSet<string> _editVehicleModelsOriginal = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _includeTerritories = true;
        private bool _includeTerritoryMenus = true;
        private bool _cloneTerritoryMenusIntoPack = true;
        private string _territoryDealerMenuContainerId = "";
        private string _territoryCustomerMenuContainerId = "";
        private bool _territoryMenuContainersHasMultipleValues;
        private string _territoryMenuContainersMultipleValuesText = "";
        private bool _territoryCurrentSetupHasData;
        private string _territoryCurrentSetupText = "";
        private bool _includePeople = true;
        private bool _includeVehicles = true;
        private bool _includeDens = true;
        private bool _includeDealerMenus = true;
        private bool _includeWeapons = true;
        private bool _includeRelationships = true;
        private bool _includeZones = true;
        private bool _createNewDen = true;
        private bool _keepSourceDenTypeName;
        private string _newDenName = "";
        private bool _isDenNameAutoFilled = true;
        private bool _isSettingDenNameFromFullName;
        private string _newDenX = "";
        private string _newDenY = "";
        private string _newDenZ = "";
        private string _newDenHeading = "";
        private string _denMenuId = "";
        private string _denBannerImagePath = "";
        private bool _generateDenInventoryMenu = true;
        private string _selectedDenInventoryItemName = "";
        private DenInventoryMenuItemViewModel? _selectedDenInventoryItem;
        private string _selectedDenInventoryCategory = "All";
        private string _denInventorySearchText = "";
        private readonly Dictionary<string, string> _modItemCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool _newDenIsBlipEnabled = true;
        private string _newDenMapIcon = "378";
        private string _newDenMapIconColorString = "White";
        private string _newDenMapIconScale = "0.5";
        private string _newDenMapIconRadius = "1";
        private string _newDenMapOpenIconAlpha = "1";
        private string _newDenMapClosedIconAlpha = "0.25";
        private string _gangColorPrefix = "";
        private string _gangColorString = "";
        private bool _isUpdatingGangColor;
        private string _lastBuiltVehicleGroupId = "";
        private readonly ObservableCollection<ViewModels.Builders.PossiblePedSpawnViewModel> _possiblePedSpawns = new();
        private readonly ObservableCollection<ViewModels.Builders.PossibleVehicleSpawnViewModel> _possibleVehicleSpawns = new();
        private readonly ObservableCollection<ViewModels.Builders.DispatchableVehicleModelOptionViewModel> _dispatchableVehicleModelOptions = new();
        private ViewModels.Builders.DispatchableVehicleModelOptionViewModel? _selectedDispatchableVehicleModelOption;
        private readonly ObservableCollection<ViewModels.Builders.CustomDispatchableVehicleModelViewModel> _customDispatchableVehicleModelsToAdd = new();
        private ViewModels.Builders.CustomDispatchableVehicleModelViewModel? _selectedCustomDispatchableVehicleModelToAdd;
        private string _vehicleModelPickerText = "";
        private readonly ObservableCollection<ViewModels.Builders.DispatchableVehicleVariantOptionViewModel> _dispatchableVehicleVariantOptions = new();
        private ViewModels.Builders.DispatchableVehicleVariantOptionViewModel? _selectedDispatchableVehicleVariantOption;
        private ViewModels.Builders.PossiblePedSpawnViewModel? _selectedPossiblePedSpawn;
        private readonly ObservableCollection<ViewModels.Builders.TaskRequirementOptionViewModel> _denPedTaskRequirementOptions = new();
        private readonly ObservableCollection<ViewModels.Builders.DispatchableVehicleGroupOptionViewModel> _denVehicleGroupOptions = new();
        private ViewModels.Builders.PossibleVehicleSpawnViewModel? _selectedPossibleVehicleSpawn;
        private bool _cloneDenPedSpawnsFromSource;
        private readonly LSR.XmlHelper.Core.Services.Validation.WeaponModelValidationService _weaponModelValidationService = new LSR.XmlHelper.Core.Services.Validation.WeaponModelValidationService();
        private readonly Services.Parsing.SmartCoordinatePasteParser _smartCoordinatePasteParser = new Services.Parsing.SmartCoordinatePasteParser();
        private readonly Services.Parsing.SmartRequiredVariationPasteParser _smartRequiredVariationPasteParser = new Services.Parsing.SmartRequiredVariationPasteParser();
        private Models.BlipSpriteOption? _selectedCommonBlipSprite;
        private string? _selectedCommonBlipColor;
        private bool _useSourceGangPeopleGroup;
        private DispatchablePeopleGroupOptionViewModel? _selectedDispatchablePeopleGroup;
        private DispatchablePersonEntryViewModel? _selectedDispatchablePersonEntry;
        private DispatchablePersonFieldViewModel? _selectedDispatchablePersonField;
        private string _dispatchablePersonFieldSearchText = "";
        private System.ComponentModel.ICollectionView? _dispatchablePersonFieldsView;
        private string _buildSummaryText = "";
        private bool _hasBuildSummary;


        public GangBuilderWindowViewModel(Services.AppearanceService appearance, string rootFolderPath)
        {
            _appearance = appearance;
            _rootFolderPath = rootFolderPath;

            Tasks = new ObservableCollection<GangBuilderTaskViewModel>();
            BuildPackCommand = new RelayCommand(BuildPack, CanBuildPack);
            RefreshGangsCommand = new RelayCommand(RefreshGangs);
            RefreshZonesCommand = new RelayCommand(RefreshZones);
            SuggestNewGangIdCommand = new RelayCommand(SuggestNewGangId);
            RefreshShopMenuGroupsCommand = new RelayCommand(RefreshShopMenuGroups);
            ChooseDenBannerImageCommand = new RelayCommand(ChooseDenBannerImage);
            RefreshIssuableWeaponsGroupsCommand = new RelayCommand(RefreshIssuableWeaponsGroups);
            RefreshDispatchablePeopleGroupsCommand = new RelayCommand(RefreshDispatchablePeopleGroups);
            AddDispatchablePersonEntryCommand = new RelayCommand(AddDispatchablePersonEntry);
            RemoveSelectedDispatchablePersonEntryCommand = new NotifyRelayCommand(RemoveSelectedDispatchablePersonEntry, HasSelectedDispatchablePersonEntry);
            DuplicateSelectedDispatchablePersonEntryCommand = new NotifyRelayCommand(DuplicateSelectedDispatchablePersonEntry, HasSelectedDispatchablePersonEntry);
            ResetDispatchablePeopleEntriesCommand = new NotifyRelayCommand(ResetDispatchablePeopleEntries, CanResetDispatchablePeopleEntries);
            OpenBuildOutputFileCommand = new RelayCommandOfT<string>(OpenBuildOutputFile);
            OpenUrlCommand = new RelayCommandOfT<string>(OpenUrl);
            AddLoanParameterCommand = new RelayCommand(AddLoanParameter);
            DuplicateSelectedLoanParameterCommand = new NotifyRelayCommand(DuplicateSelectedLoanParameter, HasSelectedLoanParameter);
            RemoveSelectedLoanParameterCommand = new NotifyRelayCommand(RemoveSelectedLoanParameter, HasSelectedLoanParameter);
            ResetLoanParametersCommand = new RelayCommand(ResetLoanParameters);
            AddDenPedSpawnRowCommand = new RelayCommand(AddDenPedSpawnRow);
            RemoveDenPedSpawnRowCommand = new NotifyRelayCommand(RemoveDenPedSpawnRow, HasSelectedDenPedSpawnRow);
            DuplicateDenPedSpawnRowCommand = new NotifyRelayCommand(DuplicateDenPedSpawnRow, HasSelectedDenPedSpawnRow);
            SmartPasteDenEntranceCoordsCommand = new RelayCommand(SmartPasteDenEntranceCoords);
            SmartPasteDenPedSpawnCoordsCommand = new NotifyRelayCommand(SmartPasteSelectedDenPedSpawnCoords, HasSelectedDenPedSpawnRow);
            AddDenVehicleSpawnRowCommand = new RelayCommand(AddDenVehicleSpawnRow);
            RemoveDenVehicleSpawnRowCommand = new NotifyRelayCommand(RemoveDenVehicleSpawnRow, HasSelectedDenVehicleSpawnRow);
            DuplicateDenVehicleSpawnRowCommand = new NotifyRelayCommand(DuplicateDenVehicleSpawnRow, HasSelectedDenVehicleSpawnRow);
            SmartPasteDenVehicleSpawnCoordsCommand = new NotifyRelayCommand(SmartPasteSelectedDenVehicleSpawnCoords, HasSelectedDenVehicleSpawnRow);
            SmartPasteRequiredVariationCommand = new NotifyRelayCommand(SmartPasteRequiredVariationFromClipboard, CanSmartPasteRequiredVariation);
            AddDenInventoryItemCommand = new RelayCommand(AddDenInventoryItem);
            DenInventoryItemsView = System.Windows.Data.CollectionViewSource.GetDefaultView(DenInventoryItems);
            DenInventoryItemsView.Filter = DenInventoryItemsFilter;
            RemoveDenInventoryItemCommand = new NotifyRelayCommand(RemoveDenInventoryItem, HasSelectedDenInventoryItem);
            AddCustomDispatchableVehicleModelCommand = new NotifyRelayCommand(AddCustomDispatchableVehicleModel, CanAddCustomDispatchableVehicleModel);
            RemoveSelectedCustomDispatchableVehicleModelCommand = new NotifyRelayCommand(RemoveSelectedCustomDispatchableVehicleModel, CanRemoveSelectedCustomDispatchableVehicleModel);
            AddCustomTerritoryCommand = new NotifyRelayCommand(AddCustomTerritory, CanAddCustomTerritory);
            RemoveSelectedCustomTerritoryCommand = new NotifyRelayCommand(RemoveSelectedCustomTerritory, CanRemoveSelectedCustomTerritory);
            SmartPasteCustomTerritoryBoundariesCommand = new RelayCommand(SmartPasteCustomTerritoryBoundaries);
            DealerMenuGroupItemsEditor = new LSR.XmlHelper.Wpf.ViewModels.Builders.DealerMenuGroupItemsEditorViewModel(_rootFolderPath);

            SelectedZones.CollectionChanged += (_, _) => { UpdateZoneOverlapWarning(); OnPropertyChanged(nameof(SelectedZonesSummary)); RefreshTerritoryCurrentSetup(); RefreshTaskState(); };
            SelectedEnemyGangs.CollectionChanged += (_, _) => RefreshTaskState();

            var isDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

            if (!isDesignMode)
            {
                RefreshGangs();
                RefreshZones();
                RefreshDispatchablePeopleGroups();
                RefreshDispatchableVehicleGroups();
                RefreshDispatchableVehicleModels();
                RefreshShopMenuGroups();
                RefreshDenInventoryItemNames();
                RefreshIssuableWeaponsGroups();
            }

            InitializeTasks();

            foreach (var opt in ViewModels.Builders.TaskRequirementOptionViewModel.CreateDefaults())
                _denPedTaskRequirementOptions.Add(opt);

            if (!isDesignMode)
            {
                LoadBlipAndColorReferences();
                ApplyCloneAwareDefaults();
            }

            RefreshTaskState();

        }

        public LSR.XmlHelper.Wpf.ViewModels.Builders.DealerMenuGroupItemsEditorViewModel DealerMenuGroupItemsEditor { get; }
        public Services.AppearanceService Appearance => _appearance;

        public string RootFolderPath => _rootFolderPath;

        public ObservableCollection<GangBuilderTaskViewModel> Tasks { get; }

        public ICommand BuildPackCommand { get; }
        public ICommand OpenBuildOutputFileCommand { get; }
        public ICommand RefreshDispatchablePeopleGroupsCommand { get; }
        public ICommand AddDispatchablePersonEntryCommand { get; }
        public NotifyRelayCommand RemoveSelectedDispatchablePersonEntryCommand { get; }
        public NotifyRelayCommand DuplicateSelectedDispatchablePersonEntryCommand { get; }
        public NotifyRelayCommand ResetDispatchablePeopleEntriesCommand { get; }
        public ICommand OpenUrlCommand { get; }
        public ObservableCollection<BuildOutputFileViewModel> BuildOutputFiles { get; } = new ObservableCollection<BuildOutputFileViewModel>();
        public ObservableCollection<ViewModels.Builders.PossiblePedSpawnViewModel> PossiblePedSpawns => _possiblePedSpawns;
        public ObservableCollection<ViewModels.Builders.PossibleVehicleSpawnViewModel> PossibleVehicleSpawns => _possibleVehicleSpawns;
        public ObservableCollection<ViewModels.Builders.TaskRequirementOptionViewModel> DenPedTaskRequirementOptions => _denPedTaskRequirementOptions;
        public ObservableCollection<ViewModels.Builders.DispatchableVehicleGroupOptionViewModel> DenVehicleGroupOptions => _denVehicleGroupOptions;
        public ObservableCollection<ViewModels.Builders.DispatchableVehicleModelOptionViewModel> DispatchableVehicleModelOptions => _dispatchableVehicleModelOptions;
        public ViewModels.Builders.DispatchableVehicleModelOptionViewModel? SelectedDispatchableVehicleModelOption
        {
            get => _selectedDispatchableVehicleModelOption;
            set
            {
                if (SetProperty(ref _selectedDispatchableVehicleModelOption, value))
                {
                    if (value is not null)
                        VehicleModelPickerText = value.ModelName;
                }
            }
        }
        public ObservableCollection<ViewModels.Builders.CustomDispatchableVehicleModelViewModel> CustomDispatchableVehicleModelsToAdd => _customDispatchableVehicleModelsToAdd;
        public ObservableCollection<ViewModels.Builders.DispatchableVehicleVariantOptionViewModel> DispatchableVehicleVariantOptions => _dispatchableVehicleVariantOptions;

        public ViewModels.Builders.DispatchableVehicleVariantOptionViewModel? SelectedDispatchableVehicleVariantOption
        {
            get => _selectedDispatchableVehicleVariantOption;
            set => SetProperty(ref _selectedDispatchableVehicleVariantOption, value);
        }
        public ViewModels.Builders.CustomDispatchableVehicleModelViewModel? SelectedCustomDispatchableVehicleModelToAdd
        {
            get => _selectedCustomDispatchableVehicleModelToAdd;
            set
            {
                SetProperty(ref _selectedCustomDispatchableVehicleModelToAdd, value);
                RemoveSelectedCustomDispatchableVehicleModelCommand.RaiseCanExecuteChanged();
            }
        }

        public string VehicleModelPickerText
        {
            get => _vehicleModelPickerText;
            set
            {
                if (SetProperty(ref _vehicleModelPickerText, value))
                {
                    RefreshDispatchableVehicleVariantsForModel((_vehicleModelPickerText ?? "").Trim());
                    AddCustomDispatchableVehicleModelCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public ICommand AddLoanParameterCommand { get; }
        public NotifyRelayCommand DuplicateSelectedLoanParameterCommand { get; }
        public NotifyRelayCommand RemoveSelectedLoanParameterCommand { get; }
        public ICommand ResetLoanParametersCommand { get; }
        public ICommand AddDenPedSpawnRowCommand { get; }
        public NotifyRelayCommand RemoveDenPedSpawnRowCommand { get; }
        public NotifyRelayCommand DuplicateDenPedSpawnRowCommand { get; }
        public ICommand SmartPasteDenEntranceCoordsCommand { get; }
        public NotifyRelayCommand SmartPasteDenPedSpawnCoordsCommand { get; }
        public ICommand AddDenVehicleSpawnRowCommand { get; }
        public NotifyRelayCommand AddCustomDispatchableVehicleModelCommand { get; }
        public NotifyRelayCommand RemoveSelectedCustomDispatchableVehicleModelCommand { get; }
        public NotifyRelayCommand RemoveDenVehicleSpawnRowCommand { get; }
        public NotifyRelayCommand DuplicateDenVehicleSpawnRowCommand { get; }
        public NotifyRelayCommand SmartPasteDenVehicleSpawnCoordsCommand { get; }
        public NotifyRelayCommand SmartPasteRequiredVariationCommand { get; }

        public ViewModels.Builders.PossiblePedSpawnViewModel? SelectedPossiblePedSpawn
        {
            get => _selectedPossiblePedSpawn;
            set
            {
                if (SetProperty(ref _selectedPossiblePedSpawn, value))
                {
                    RemoveDenPedSpawnRowCommand.RaiseCanExecuteChanged();
                    DuplicateDenPedSpawnRowCommand.RaiseCanExecuteChanged();
                    SmartPasteDenPedSpawnCoordsCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public ViewModels.Builders.PossibleVehicleSpawnViewModel? SelectedPossibleVehicleSpawn
        {
            get => _selectedPossibleVehicleSpawn;
            set
            {
                if (SetProperty(ref _selectedPossibleVehicleSpawn, value))
                {
                    RemoveDenVehicleSpawnRowCommand.RaiseCanExecuteChanged();
                    DuplicateDenVehicleSpawnRowCommand.RaiseCanExecuteChanged();
                    SmartPasteDenVehicleSpawnCoordsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string BuildSummaryText
        {
            get => _buildSummaryText;
            set => SetProperty(ref _buildSummaryText, value);
        }

        public bool HasBuildSummary
        {
            get => _hasBuildSummary;
            set => SetProperty(ref _hasBuildSummary, value);
        }

        public string PackName
        {
            get => _packName;
            set
            {
                if (!SetProperty(ref _packName, value))
                    return;

                RefreshTaskState();
            }
        }
      
        public string NewGangId
        {
            get => _newGangId;
            set
            {
                if (!SetProperty(ref _newGangId, value))
                    return;

                UpdateNewGangVehicleGroupOptionCount();
                RefreshTaskState();
            }
        }

        public string NewGangFullName
        {
            get => _newGangFullName;
            set
            {
                if (!SetProperty(ref _newGangFullName, value))
                    return;

                if (CreateNewDen && !IsEditExistingGang && _isDenNameAutoFilled)
                {
                    _isSettingDenNameFromFullName = true;
                    NewDenName = value;
                    _isSettingDenNameFromFullName = false;
                }

                RefreshTaskState();
            }
        }

        public string CloneFromGangId
        {
            get => _cloneFromGangId;
            set
            {
                if (!SetProperty(ref _cloneFromGangId, value))
                    return;

                RefreshSourceEnemyGangs();

                RefreshPossiblePedSpawnsFromClone();
                OnPropertyChanged(nameof(HasCloneSourceGang));
                OnPropertyChanged(nameof(ShowDenMenuIdAdvanced));
                ApplyCloneAwareDefaults();
                RefreshTaskState();
            }
        }
        public bool CloneDenPedSpawnsFromSource
        {
            get => _cloneDenPedSpawnsFromSource;
            set
            {
                if (!SetProperty(ref _cloneDenPedSpawnsFromSource, value))
                    return;

                RefreshPossiblePedSpawnsFromClone();
                RefreshTaskState();
            }
        }

        public bool IncludeZones
        {
            get => _includeZones;
            set
            {
                if (!SetProperty(ref _includeZones, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool IncludeTerritories
        {
            get => _includeTerritories;
            set
            {
                if (!SetProperty(ref _includeTerritories, value))
                    return;

                RefreshTaskState();
            }
        }
        public bool IncludeTerritoryMenus
        {
            get => _includeTerritoryMenus;
            set
            {
                if (!SetProperty(ref _includeTerritoryMenus, value))
                    return;

                RefreshTaskState();
            }
        }


        public bool CloneTerritoryMenusIntoPack
        {
            get => _cloneTerritoryMenusIntoPack;
            set
            {
                if (!SetProperty(ref _cloneTerritoryMenusIntoPack, value))
                    return;

                RefreshTaskState();
            }
        }

        public string TerritoryDealerMenuContainerId
        {
            get => _territoryDealerMenuContainerId;
            set
            {
                if (!SetProperty(ref _territoryDealerMenuContainerId, value))
                    return;

                RefreshTaskState();
            }
        }

        public string TerritoryCustomerMenuContainerId
        {
            get => _territoryCustomerMenuContainerId;
            set
            {
                if (!SetProperty(ref _territoryCustomerMenuContainerId, value))
                    return;

                RefreshTaskState();
            }
        }
        public bool TerritoryMenuContainersHasMultipleValues
        {
            get => _territoryMenuContainersHasMultipleValues;
            set => SetProperty(ref _territoryMenuContainersHasMultipleValues, value);
        }

        public string TerritoryMenuContainersMultipleValuesText
        {
            get => _territoryMenuContainersMultipleValuesText;
            set => SetProperty(ref _territoryMenuContainersMultipleValuesText, value);
        }
        public bool TerritoryCurrentSetupHasData
        {
            get => _territoryCurrentSetupHasData;
            set => SetProperty(ref _territoryCurrentSetupHasData, value);
        }

        public string TerritoryCurrentSetupText
        {
            get => _territoryCurrentSetupText;
            set => SetProperty(ref _territoryCurrentSetupText, value);
        }

        public bool IncludePeople
        {
            get => _includePeople;
            set
            {
                if (!SetProperty(ref _includePeople, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool UseSourceGangPeopleGroup
        {
            get => _useSourceGangPeopleGroup;
            set
            {
                if (!SetProperty(ref _useSourceGangPeopleGroup, value))
                    return;

                RefreshTaskState();
                LoadDispatchablePeopleEntries();
                ResetDispatchablePeopleEntriesCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<DispatchablePeopleGroupOptionViewModel> DispatchablePeopleGroups { get; } = new ObservableCollection<DispatchablePeopleGroupOptionViewModel>();

        public DispatchablePeopleGroupOptionViewModel? SelectedDispatchablePeopleGroup
        {
            get => _selectedDispatchablePeopleGroup;
            set
            {
                if (!SetProperty(ref _selectedDispatchablePeopleGroup, value))
                    return;

                RefreshTaskState();
                LoadDispatchablePeopleEntries();
                ResetDispatchablePeopleEntriesCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<DispatchablePersonEntryViewModel> DispatchablePeopleEntries { get; } = new ObservableCollection<DispatchablePersonEntryViewModel>();

        public DispatchablePersonEntryViewModel? SelectedDispatchablePersonEntry
        {
            get => _selectedDispatchablePersonEntry;
            set
            {
                if (!SetProperty(ref _selectedDispatchablePersonEntry, value))
                    return;

                SelectedDispatchablePersonField = _selectedDispatchablePersonEntry?.Fields.FirstOrDefault();
                UpdateDispatchablePersonFieldsView();
                RemoveSelectedDispatchablePersonEntryCommand.RaiseCanExecuteChanged();
                DuplicateSelectedDispatchablePersonEntryCommand.RaiseCanExecuteChanged();
            }
        }

        public DispatchablePersonFieldViewModel? SelectedDispatchablePersonField
        {
            get => _selectedDispatchablePersonField;
            set
            {
                var old = _selectedDispatchablePersonField;

                if (SetProperty(ref _selectedDispatchablePersonField, value))
                {
                    if (old is not null)
                        old.PropertyChanged -= SelectedDispatchablePersonFieldOnPropertyChanged;

                    if (_selectedDispatchablePersonField is not null)
                        _selectedDispatchablePersonField.PropertyChanged += SelectedDispatchablePersonFieldOnPropertyChanged;

                    OnPropertyChanged(nameof(ShowSmartPasteRequiredVariation));
                    SmartPasteRequiredVariationCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ShowSmartPasteRequiredVariation
        {
            get => string.Equals(SelectedDispatchablePersonField?.Name ?? "", "RequiredVariation", StringComparison.OrdinalIgnoreCase);
        }

        public string DispatchablePersonFieldSearchText
        {
            get => _dispatchablePersonFieldSearchText;
            set
            {
                if (!SetProperty(ref _dispatchablePersonFieldSearchText, value))
                    return;

                DispatchablePersonFieldsView?.Refresh();
            }
        }

        public System.ComponentModel.ICollectionView? DispatchablePersonFieldsView
        {
            get => _dispatchablePersonFieldsView;
            private set => SetProperty(ref _dispatchablePersonFieldsView, value);
        }

        public bool IncludeVehicles
        {
            get => _includeVehicles;
            set
            {
                if (!SetProperty(ref _includeVehicles, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool IncludeDens
        {
            get => _includeDens;
            set
            {
                if (!SetProperty(ref _includeDens, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool CreateNewDen
        {
            get => _createNewDen;
            set
            {
                if (!SetProperty(ref _createNewDen, value))
                    return;

                RefreshTaskState();
            }
        }
        public bool KeepSourceDenTypeName
        {
            get => _keepSourceDenTypeName;
            set
            {
                if (!SetProperty(ref _keepSourceDenTypeName, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenName
        {
            get => _newDenName;
            set
            {
                var old = _newDenName;

                if (!SetProperty(ref _newDenName, value))
                    return;

                var newValue = _newDenName ?? "";

                if (!string.IsNullOrWhiteSpace(old) && !string.IsNullOrWhiteSpace(newValue))
                    UpdateDenNameOnSpawnRows(old, newValue);

                if (!_isSettingDenNameFromFullName)
                    _isDenNameAutoFilled = string.IsNullOrWhiteSpace(value);

                RefreshTaskState();
            }
        }

        private void UpdateDenNameOnSpawnRows(string oldName, string newName)
        {
            oldName = (oldName ?? "").Trim();
            newName = (newName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
                return;

            foreach (var vm in _possiblePedSpawns)
            {
                if (vm is null)
                    continue;

                if (string.Equals((vm.DenName ?? "").Trim(), oldName, StringComparison.OrdinalIgnoreCase))
                    vm.DenName = newName;
            }

            foreach (var vm in _possibleVehicleSpawns)
            {
                if (vm is null)
                    continue;

                if (string.Equals((vm.DenName ?? "").Trim(), oldName, StringComparison.OrdinalIgnoreCase))
                    vm.DenName = newName;
            }
        }

        public string NewDenX
        {
            get => _newDenX;
            set
            {
                if (!SetProperty(ref _newDenX, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenY
        {
            get => _newDenY;
            set
            {
                if (!SetProperty(ref _newDenY, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenZ
        {
            get => _newDenZ;
            set
            {
                if (!SetProperty(ref _newDenZ, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenHeading
        {
            get => _newDenHeading;
            set
            {
                if (!SetProperty(ref _newDenHeading, value))
                    return;

                RefreshTaskState();
            }
        }
        public string DenMenuId
        {
            get => _denMenuId;
            set
            {
                if (!SetProperty(ref _denMenuId, value))
                    return;

                RefreshTaskState();
            }
        }

        public string DenBannerImagePath
        {
            get => _denBannerImagePath;
            set
            {
                if (!SetProperty(ref _denBannerImagePath, value))
                    return;

                RefreshTaskState();
            }
        }
        public bool GenerateDenInventoryMenu
        {
            get => _generateDenInventoryMenu;
            set
            {
                if (!SetProperty(ref _generateDenInventoryMenu, value))
                    return;

                OnPropertyChanged(nameof(ShowDenMenuIdAdvanced));
                RefreshTaskState();
            }
        }
        public string SelectedEditGangVehicleGroupId
        {
            get => _selectedEditGangVehicleGroupId;
            set
            {
                if (!SetProperty(ref _selectedEditGangVehicleGroupId, value))
                    return;

                SelectedEditGangVehicleModelsPreview.Clear();

                if (!string.IsNullOrWhiteSpace(_rootFolderPath) && !string.IsNullOrWhiteSpace(_selectedEditGangVehicleGroupId))
                {
                    var vehiclePreview = new LSR.XmlHelper.Core.Services.Reading.DispatchableVehicleGroupModelsReadService();
                    foreach (var model in (IsEditExistingGang ? vehiclePreview.GetModelsForGroupIdResolved(_rootFolderPath, _selectedEditGangVehicleGroupId) : vehiclePreview.GetModelsForGroupId(_rootFolderPath, _selectedEditGangVehicleGroupId)))
                        SelectedEditGangVehicleModelsPreview.Add(model);
                }

                if (IsEditExistingGang)
                {
                    _editVehicleModelsOriginal = SelectedEditGangVehicleModelsPreview
                        .Select(x => (x ?? "").Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    CustomDispatchableVehicleModelsToAdd.Clear();
                    foreach (var model in _editVehicleModelsOriginal.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                        CustomDispatchableVehicleModelsToAdd.Add(new ViewModels.Builders.CustomDispatchableVehicleModelViewModel(model, "", ""));

                    SelectedCustomDispatchableVehicleModelToAdd = CustomDispatchableVehicleModelsToAdd.FirstOrDefault();
                }

                RefreshTaskState();
            }
        }

        public bool HasCloneSourceGang
        {
            get => !string.IsNullOrWhiteSpace(CloneFromGangId);
        }

        public bool ShowDenMenuIdAdvanced
        {
            get => HasCloneSourceGang && !GenerateDenInventoryMenu;
        }

        public ObservableCollection<string> AvailableDenInventoryItemNames { get; } = new ObservableCollection<string>();
        public System.ComponentModel.ICollectionView? AvailableDenInventoryItemNamesView { get; private set; }

        public string SelectedDenInventoryItemName
        {
            get => _selectedDenInventoryItemName;
            set => SetProperty(ref _selectedDenInventoryItemName, value);
        }

        public ObservableCollection<DenInventoryMenuItemViewModel> DenInventoryItems { get; } = new ObservableCollection<DenInventoryMenuItemViewModel>();
        public System.ComponentModel.ICollectionView DenInventoryItemsView { get; private set; }
        public ObservableCollection<string> SelectedEditGangVehicleModelsPreview { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedEditGangDenInventoryItemsPreview { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedEditGangDealerMenuPreview { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> DenInventoryCategories { get; } = new ObservableCollection<string>
        {
            "All",
            "Drugs",
            "Weapons",
            "Food",
            "Tools",
            "Equipment",
            "Other"
        };

        public string SelectedDenInventoryCategory
        {
            get => _selectedDenInventoryCategory;
            set
            {
                if (!SetProperty(ref _selectedDenInventoryCategory, value))
                    return;

                DenInventoryItemsView?.Refresh();
                AvailableDenInventoryItemNamesView?.Refresh();
            }
        }

        public string DenInventorySearchText
        {
            get => _denInventorySearchText;
            set
            {
                if (!SetProperty(ref _denInventorySearchText, value))
                    return;

                DenInventoryItemsView?.Refresh();
                AvailableDenInventoryItemNamesView?.Refresh();
            }
        }

        public DenInventoryMenuItemViewModel? SelectedDenInventoryItem
        {
            get => _selectedDenInventoryItem;
            set
            {
                if (!SetProperty(ref _selectedDenInventoryItem, value))
                    return;

                RemoveDenInventoryItemCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddDenInventoryItemCommand { get; }
        public NotifyRelayCommand RemoveDenInventoryItemCommand { get; }

        public bool NewDenIsBlipEnabled
        {
            get => _newDenIsBlipEnabled;
            set
            {
                if (!SetProperty(ref _newDenIsBlipEnabled, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenMapIcon
        {
            get => _newDenMapIcon;
            set
            {
                if (!SetProperty(ref _newDenMapIcon, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenMapIconColorString
        {
            get => _newDenMapIconColorString;
            set
            {
                if (!SetProperty(ref _newDenMapIconColorString, value))
                    return;

                RefreshTaskState();
            }
        }
        public string GangColorPrefix
        {
            get => _gangColorPrefix;
            set
            {
                if (!SetProperty(ref _gangColorPrefix, value))
                    return;

                if (!_isUpdatingGangColor && GangColorNameByPrefixMap.TryGetValue((_gangColorPrefix ?? "").Trim(), out var name))
                {
                    _isUpdatingGangColor = true;
                    GangColorString = name;
                    _isUpdatingGangColor = false;
                }

                RefreshTaskState();
            }
        }

        public string GangColorString
        {
            get => _gangColorString;
            set
            {
                if (!SetProperty(ref _gangColorString, value))
                    return;

                if (!_isUpdatingGangColor && GangColorPrefixMap.TryGetValue((_gangColorString ?? "").Trim(), out var prefix))
                {
                    _isUpdatingGangColor = true;
                    GangColorPrefix = prefix;
                    _isUpdatingGangColor = false;
                }

                RefreshTaskState();
            }
        }
        public string MinimumRep { get => _minimumRep; set { if (SetProperty(ref _minimumRep, value)) RefreshTaskState(); } }
        public string MaximumRep { get => _maximumRep; set { if (SetProperty(ref _maximumRep, value)) RefreshTaskState(); } }
        public string StartingRep { get => _startingRep; set { if (SetProperty(ref _startingRep, value)) RefreshTaskState(); } }
        public string HostileRepLevel { get => _hostileRepLevel; set { if (SetProperty(ref _hostileRepLevel, value)) RefreshTaskState(); } }
        public string NeutralRepLevel { get => _neutralRepLevel; set { if (SetProperty(ref _neutralRepLevel, value)) RefreshTaskState(); } }
        public string FriendlyRepLevel { get => _friendlyRepLevel; set { if (SetProperty(ref _friendlyRepLevel, value)) RefreshTaskState(); } }
        public string MemberOfferRepLevel { get => _memberOfferRepLevel; set { if (SetProperty(ref _memberOfferRepLevel, value)) RefreshTaskState(); } }
        public string HitSquadRep { get => _hitSquadRep; set { if (SetProperty(ref _hitSquadRep, value)) RefreshTaskState(); } }

        public string PickupPaymentMin { get => _pickupPaymentMin; set { if (SetProperty(ref _pickupPaymentMin, value)) RefreshTaskState(); } }
        public string PickupPaymentMax { get => _pickupPaymentMax; set { if (SetProperty(ref _pickupPaymentMax, value)) RefreshTaskState(); } }
        public string TheftPaymentMin { get => _theftPaymentMin; set { if (SetProperty(ref _theftPaymentMin, value)) RefreshTaskState(); } }
        public string TheftPaymentMax { get => _theftPaymentMax; set { if (SetProperty(ref _theftPaymentMax, value)) RefreshTaskState(); } }
        public string HitPaymentMin { get => _hitPaymentMin; set { if (SetProperty(ref _hitPaymentMin, value)) RefreshTaskState(); } }
        public string HitPaymentMax { get => _hitPaymentMax; set { if (SetProperty(ref _hitPaymentMax, value)) RefreshTaskState(); } }
        public string DeliveryPaymentMin { get => _deliveryPaymentMin; set { if (SetProperty(ref _deliveryPaymentMin, value)) RefreshTaskState(); } }
        public string DeliveryPaymentMax { get => _deliveryPaymentMax; set { if (SetProperty(ref _deliveryPaymentMax, value)) RefreshTaskState(); } }
        public string WheelmanPaymentMin { get => _wheelmanPaymentMin; set { if (SetProperty(ref _wheelmanPaymentMin, value)) RefreshTaskState(); } }
        public string WheelmanPaymentMax { get => _wheelmanPaymentMax; set { if (SetProperty(ref _wheelmanPaymentMax, value)) RefreshTaskState(); } }
        public string ImpoundTheftPaymentMin { get => _impoundTheftPaymentMin; set { if (SetProperty(ref _impoundTheftPaymentMin, value)) RefreshTaskState(); } }
        public string ImpoundTheftPaymentMax { get => _impoundTheftPaymentMax; set { if (SetProperty(ref _impoundTheftPaymentMax, value)) RefreshTaskState(); } }
        public string BodyDisposalPaymentMin { get => _bodyDisposalPaymentMin; set { if (SetProperty(ref _bodyDisposalPaymentMin, value)) RefreshTaskState(); } }
        public string BodyDisposalPaymentMax { get => _bodyDisposalPaymentMax; set { if (SetProperty(ref _bodyDisposalPaymentMax, value)) RefreshTaskState(); } }
        public string CopHitPaymentMin { get => _copHitPaymentMin; set { if (SetProperty(ref _copHitPaymentMin, value)) RefreshTaskState(); } }
        public string CopHitPaymentMax { get => _copHitPaymentMax; set { if (SetProperty(ref _copHitPaymentMax, value)) RefreshTaskState(); } }
        public string AmbushPaymentMin { get => _ambushPaymentMin; set { if (SetProperty(ref _ambushPaymentMin, value)) RefreshTaskState(); } }
        public string AmbushPaymentMax { get => _ambushPaymentMax; set { if (SetProperty(ref _ambushPaymentMax, value)) RefreshTaskState(); } }
        public string BriberyPaymentMin { get => _briberyPaymentMin; set { if (SetProperty(ref _briberyPaymentMin, value)) RefreshTaskState(); } }
        public string BriberyPaymentMax { get => _briberyPaymentMax; set { if (SetProperty(ref _briberyPaymentMax, value)) RefreshTaskState(); } }
        public string ArsonPaymentMin { get => _arsonPaymentMin; set { if (SetProperty(ref _arsonPaymentMin, value)) RefreshTaskState(); } }
        public string ArsonPaymentMax { get => _arsonPaymentMax; set { if (SetProperty(ref _arsonPaymentMax, value)) RefreshTaskState(); } }

        public string FightPercentage { get => _fightPercentage; set { if (SetProperty(ref _fightPercentage, value)) RefreshTaskState(); } }
        public string FightPolicePercentage { get => _fightPolicePercentage; set { if (SetProperty(ref _fightPolicePercentage, value)) RefreshTaskState(); } }
        public string AlwaysFightPolicePercentage { get => _alwaysFightPolicePercentage; set { if (SetProperty(ref _alwaysFightPolicePercentage, value)) RefreshTaskState(); } }
        public string DrugDealerPercentage { get => _drugDealerPercentage; set { if (SetProperty(ref _drugDealerPercentage, value)) RefreshTaskState(); } }

        public string AmbientMemberMoneyMin { get => _ambientMemberMoneyMin; set { if (SetProperty(ref _ambientMemberMoneyMin, value)) RefreshTaskState(); } }
        public string AmbientMemberMoneyMax { get => _ambientMemberMoneyMax; set { if (SetProperty(ref _ambientMemberMoneyMax, value)) RefreshTaskState(); } }
        public string DealerMemberMoneyMin { get => _dealerMemberMoneyMin; set { if (SetProperty(ref _dealerMemberMoneyMin, value)) RefreshTaskState(); } }
        public string DealerMemberMoneyMax { get => _dealerMemberMoneyMax; set { if (SetProperty(ref _dealerMemberMoneyMax, value)) RefreshTaskState(); } }
        public string CostToPayoffGangScalar { get => _costToPayoffGangScalar; set { if (SetProperty(ref _costToPayoffGangScalar, value)) RefreshTaskState(); } }

        public string PercentageTrustingOfPlayer { get => _percentageTrustingOfPlayer; set { if (SetProperty(ref _percentageTrustingOfPlayer, value)) RefreshTaskState(); } }
        public string PercentageWithLongGuns { get => _percentageWithLongGuns; set { if (SetProperty(ref _percentageWithLongGuns, value)) RefreshTaskState(); } }
        public string PercentageWithSidearms { get => _percentageWithSidearms; set { if (SetProperty(ref _percentageWithSidearms, value)) RefreshTaskState(); } }
        public string PercentageWithMelee { get => _percentageWithMelee; set { if (SetProperty(ref _percentageWithMelee, value)) RefreshTaskState(); } }
        public string VehicleSpawnPercentage { get => _vehicleSpawnPercentage; set { if (SetProperty(ref _vehicleSpawnPercentage, value)) RefreshTaskState(); } }
        public string PedestrianSpawnPercentageAroundDen { get => _pedestrianSpawnPercentageAroundDen; set { if (SetProperty(ref _pedestrianSpawnPercentageAroundDen, value)) RefreshTaskState(); } }

        public string MemberKickUpDays { get => _memberKickUpDays; set { if (SetProperty(ref _memberKickUpDays, value)) RefreshTaskState(); } }
        public string MemberKickUpAmount { get => _memberKickUpAmount; set { if (SetProperty(ref _memberKickUpAmount, value)) RefreshTaskState(); } }
        public string MemberKickUpMissLimit { get => _memberKickUpMissLimit; set { if (SetProperty(ref _memberKickUpMissLimit, value)) RefreshTaskState(); } }

        public ObservableCollection<LoanParameterEntryViewModel> LoanParameters => _loanParameters;

        public LoanParameterEntryViewModel? SelectedLoanParameter
        {
            get => _selectedLoanParameter;
            set
            {
                if (!SetProperty(ref _selectedLoanParameter, value))
                    return;

                DuplicateSelectedLoanParameterCommand.RaiseCanExecuteChanged();
                RemoveSelectedLoanParameterCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewDenMapIconScale
        {
            get => _newDenMapIconScale;
            set
            {
                if (!SetProperty(ref _newDenMapIconScale, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenMapIconRadius
        {
            get => _newDenMapIconRadius;
            set
            {
                if (!SetProperty(ref _newDenMapIconRadius, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenMapOpenIconAlpha
        {
            get => _newDenMapOpenIconAlpha;
            set
            {
                if (!SetProperty(ref _newDenMapOpenIconAlpha, value))
                    return;

                RefreshTaskState();
            }
        }

        public string NewDenMapClosedIconAlpha
        {
            get => _newDenMapClosedIconAlpha;
            set
            {
                if (!SetProperty(ref _newDenMapClosedIconAlpha, value))
                    return;

                RefreshTaskState();
            }
        }

        public ObservableCollection<Models.BlipSpriteOption> CommonBlipSprites { get; } = new ObservableCollection<Models.BlipSpriteOption>();

        public ObservableCollection<string> CommonBlipColors { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CommonTextColorPrefixes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> GangColorNames { get; } = new ObservableCollection<string>();


        public Models.BlipSpriteOption? SelectedCommonBlipSprite
        {
            get => _selectedCommonBlipSprite;
            set
            {
                if (!SetProperty(ref _selectedCommonBlipSprite, value))
                    return;

                if (value is not null)
                    NewDenMapIcon = value.Value;
            }
        }

        public string? SelectedCommonBlipColor
        {
            get => _selectedCommonBlipColor;
            set
            {
                if (!SetProperty(ref _selectedCommonBlipColor, value))
                    return;

                if (!string.IsNullOrWhiteSpace(value))
                    NewDenMapIconColorString = value;
            }
        }

        public bool IncludeDealerMenus
        {
            get => _includeDealerMenus;
            set
            {
                if (!SetProperty(ref _includeDealerMenus, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool IncludeWeapons
        {
            get => _includeWeapons;
            set
            {
                if (!SetProperty(ref _includeWeapons, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool IncludeRelationships
        {
            get => _includeRelationships;
            set
            {
                if (!SetProperty(ref _includeRelationships, value))
                    return;

                RefreshTaskState();
            }
        }

        private void InitializeTasks()
        {
            Tasks.Add(new GangBuilderTaskViewModel("Choose pack name", "Creates additive files like Gangs+_PackName.xml and can be deleted to uninstall."));
            Tasks.Add(new GangBuilderTaskViewModel("Set new gang ID", "This becomes Gangs.Gang.ID and is the anchor used by territories and dens."));
            Tasks.Add(new GangBuilderTaskViewModel("Set gang display name", "Full name shown in-game and in logs."));
            Tasks.Add(new GangBuilderTaskViewModel("Choose a clone source gang ID", "Recommended. Copies a working gang and updates IDs and links safely."));
            Tasks.Add(new GangBuilderTaskViewModel("Select Zones", "Pick which Zones your gang owns. This is used to generate GangTerritories so the gang spawns in those areas."));
            Tasks.Add(new GangBuilderTaskViewModel("Pre-build validation", "Stops the build if critical inputs are missing or conflicting (duplicate Gang ID, missing dealer group, missing zones, invalid dens, missing people override group)."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update Gangs entry", "Creates the Gang record and links PeopleGroupID, VehicleGroupID, weapons IDs, and dealer group."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update DispatchablePeople group", "Defines which peds can spawn as members."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update DispatchableVehicles group", "Defines which vehicles can spawn for the gang."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update GangTerritories", "Assigns Zones to the gang so it spawns in the world."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update Territory menus", "Optional. Updates DealerMenuContainerID and CustomerMenuContainerID for the selected Zones, and can clone the referenced ShopMenuGroups into ShopMenus+_PackName.xml."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update Gang Dens", "Adds hangouts/den locations and links them to the gang."));
            Tasks.Add(new GangBuilderTaskViewModel("Assign Dealer Menu Group", "Links the gang to an existing or cloned ShopMenuGroup."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update ShopMenus", "Optional. Clones the dealer ShopMenuGroup (and referenced menus) into ShopMenus+_PackName.xml so the pack is self-contained."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Assign Weapons Loadouts", "Links melee/sidearm/long gun sets to the gang."));
            Tasks.Add(new GangBuilderTaskViewModel("Create/Update IssuableWeapons", "Optional. Clones selected IssuableWeapons groups into IssuableWeapons+_PackName.xml so the pack is self-contained."));
            Tasks.Add(new GangBuilderTaskViewModel("Configure Relationships", "Optional hostility/friendliness links to other gangs, agencies, or groups."));
            Tasks.Add(new GangBuilderTaskViewModel("Write additive XML files", "Outputs Gangs+_, DispatchablePeople+_, DispatchableVehicles+_, GangTerritories+_, Locations+_, ShopMenus+_, IssuableWeapons+_ as needed."));
            Tasks.Add(new GangBuilderTaskViewModel("Show summary + next steps", "Explains what was created and what’s missing, with a checklist."));
            Tasks.Add(new GangBuilderTaskViewModel("Export pack folder", "Copies your generated +_PackName.xml files into GangPacks\\PackName (plus a README) so it can be shared or zipped."));
        }

        private void RefreshTaskState()
        {
            SetTask("Choose pack name", !string.IsNullOrWhiteSpace(PackName), string.IsNullOrWhiteSpace(PackName) ? "Missing" : "Ready");
            SetTask("Set new gang ID", !string.IsNullOrWhiteSpace(NewGangId), string.IsNullOrWhiteSpace(NewGangId) ? "Missing" : "Ready");
            SetTask("Set gang display name", !string.IsNullOrWhiteSpace(NewGangFullName), string.IsNullOrWhiteSpace(NewGangFullName) ? "Missing" : "Ready");
            SetTask("Choose a clone source gang ID", !string.IsNullOrWhiteSpace(CloneFromGangId), string.IsNullOrWhiteSpace(CloneFromGangId) ? "Recommended" : "Ready");
            SetTask("Create/Update Gangs entry", HasCoreInputs(), HasCoreInputs() ? "Planned" : "Blocked");
            SetTask("Pre-build validation", HasCoreInputs(), HasCoreInputs() ? "Planned" : "Blocked");
            if (!IncludePeople)
            {
                SetTask("Create/Update DispatchablePeople group", false, "Skipped");
            }
            else if (!HasCoreInputs())
            {
                SetTask("Create/Update DispatchablePeople group", false, "Blocked");
            }
            else if (!UseSourceGangPeopleGroup && SelectedDispatchablePeopleGroup is null)
            {
                SetTask("Create/Update DispatchablePeople group", false, "Missing group");
            }
            else
            {
                SetTask("Create/Update DispatchablePeople group", true, "Planned");
            }

            SetTask("Create/Update DispatchableVehicles group", IncludeVehicles && HasCoreInputs(), IncludeVehicles ? (HasCoreInputs() ? "Planned" : "Blocked") : "Skipped");
            SetTask("Select Zones", IncludeZones && IncludeTerritories && SelectedZones.Count > 0, IncludeZones && IncludeTerritories ? (SelectedZones.Count > 0 ? "Ready" : "Missing") : "Skipped");
            SetTask("Create/Update GangTerritories", IncludeTerritories && IncludeZones && HasCoreInputs() && SelectedZones.Count > 0, IncludeTerritories && IncludeZones ? (HasCoreInputs() ? (SelectedZones.Count > 0 ? "Planned" : "Blocked") : "Blocked") : "Skipped");
            SetTask("Create/Update Territory menus", IncludeTerritoryMenus && IncludeTerritories && IncludeZones && HasCoreInputs() && SelectedZones.Count > 0 && !string.IsNullOrWhiteSpace(TerritoryDealerMenuContainerId) && !string.IsNullOrWhiteSpace(TerritoryCustomerMenuContainerId), IncludeTerritoryMenus ? (HasCoreInputs() ? (SelectedZones.Count > 0 ? (!string.IsNullOrWhiteSpace(TerritoryDealerMenuContainerId) && !string.IsNullOrWhiteSpace(TerritoryCustomerMenuContainerId) ? "Planned" : "Missing IDs") : "Missing zones") : "Blocked") : "Skipped");

            if (!IncludeDens)
            {
                SetTask("Create/Update Gang Dens", false, "Skipped");
            }
            else if (!HasCoreInputs())
            {
                SetTask("Create/Update Gang Dens", false, "Blocked");
            }
            else if (!CreateNewDen)
            {
                SetTask("Create/Update Gang Dens", true, "Planned");
            }
            else
            {
                var warning = GetNewDenWarning(NewDenName, NewDenX, NewDenY, NewDenZ, NewDenHeading);
                if (!string.IsNullOrWhiteSpace(warning))
                    SetTask("Create/Update Gang Dens", false, warning);
                else
                    SetTask("Create/Update Gang Dens", true, "Planned");
            }

            SetTask("Assign Dealer Menu Group", IncludeDealerMenus && HasCoreInputs(), IncludeDealerMenus ? (HasCoreInputs() ? "Planned" : "Blocked") : "Skipped");
            SetTask("Create/Update ShopMenus", IncludeDealerMenus && CloneDealerMenusIntoPack && HasCoreInputs(), IncludeDealerMenus ? (CloneDealerMenusIntoPack ? (HasCoreInputs() ? "Planned" : "Blocked") : "Skipped") : "Skipped");
            SetTask("Create/Assign Weapons Loadouts", IncludeWeapons && HasCoreInputs(), IncludeWeapons ? (HasCoreInputs() ? "Planned" : "Blocked") : "Skipped");

            if (!IncludeWeapons)
            {
                SetTask("Create/Update IssuableWeapons", false, "Skipped");
            }
            else if (!CloneWeaponsIntoPack)
            {
                SetTask("Create/Update IssuableWeapons", false, "Skipped");
            }
            else if (UseSourceGangWeaponsLoadouts)
            {
                SetTask("Create/Update IssuableWeapons", false, "Disabled (using source loadouts)");
            }
            else
            {
                var warning = GetWeaponsCloneWarning(false, MeleeWeaponsId, SideArmsId, LongGunsId);
                if (!string.IsNullOrWhiteSpace(warning))
                    SetTask("Create/Update IssuableWeapons", false, warning);
                else
                    SetTask("Create/Update IssuableWeapons", HasCoreInputs(), HasCoreInputs() ? "Planned" : "Blocked");
            }

            SetTask("Configure Relationships", IncludeRelationships && HasCoreInputs(), IncludeRelationships ? (HasCoreInputs() ? "Planned" : "Blocked") : "Skipped");
            SetTask("Write additive XML files", HasCoreInputs(), HasCoreInputs() ? "Planned" : "Blocked");
            SetTask("Show summary + next steps", HasCoreInputs(), HasCoreInputs() ? "Planned" : "Blocked");
            SetTask("Export pack folder", HasCoreInputs(), HasCoreInputs() ? "Planned" : "Blocked");

            CommandManager.InvalidateRequerySuggested();
        }

        private bool HasCoreInputs()
        {
            return !string.IsNullOrWhiteSpace(PackName)
                && !string.IsNullOrWhiteSpace(NewGangId)
                && !string.IsNullOrWhiteSpace(NewGangFullName);
        }

        private void SetTask(string title, bool complete, string status)
        {
            var task = Tasks.FirstOrDefault(t => string.Equals(t.Title, title, StringComparison.OrdinalIgnoreCase));
            if (task is null)
                return;

            task.IsComplete = complete;
            task.Status = status;
            task.IsRequired = true;
        }
        public ObservableCollection<GangOptionViewModel> ExistingGangs { get; } = new ObservableCollection<GangOptionViewModel>();

        private GangOptionViewModel? _selectedCloneGang;

        public GangOptionViewModel? SelectedCloneGang
        {
            get => _selectedCloneGang;
            set
            {
                if (!SetProperty(ref _selectedCloneGang, value))
                    return;

                CloneFromGangId = _selectedCloneGang?.Id ?? "";

                if (!IsEditExistingGang)
                    LoadSelectedGangAdvancedSettings(CloneFromGangId);
            }
        }

        public ICommand RefreshGangsCommand { get; }

        private bool _isEditExistingGang;

        public bool IsEditExistingGang
        {
            get => _isEditExistingGang;
            set
            {
                if (!SetProperty(ref _isEditExistingGang, value))
                    return;

                OnPropertyChanged(nameof(ShowEditGangPicker));
                OnPropertyChanged(nameof(ShowCloneGangPicker));

                if (value)
                    IncludeTerritories = true;

                ApplyEditGangSelection();
                RefreshTaskState();
            }
        }

        public bool ShowEditGangPicker => IsEditExistingGang;

        public bool ShowCloneGangPicker => !IsEditExistingGang;

        private GangOptionViewModel? _selectedEditGang;

        private string _editModeResolvedSourcesWarning = "";

        public GangOptionViewModel? SelectedEditGang
        {
            get => _selectedEditGang;
            set
            {
                if (!SetProperty(ref _selectedEditGang, value))
                    return;

                ApplyEditGangSelection();
            }
        }
        public string EditModeResolvedSourcesWarning
        {
            get => _editModeResolvedSourcesWarning;
            set => SetProperty(ref _editModeResolvedSourcesWarning, value);
        }

        public ICommand SuggestNewGangIdCommand { get; }

        private void RefreshGangs()
        {
            var previousId = SelectedCloneGang?.Id ?? CloneFromGangId;

            ExistingGangs.Clear();

            var catalog = new LSR.XmlHelper.Core.Services.Builders.GangCatalogService();
            var gangs = catalog.GetGangs(_rootFolderPath);

            foreach (var g in gangs)
                ExistingGangs.Add(new GangOptionViewModel(g.Id, g.FullName));

            if (ExistingGangs.Count == 0)
            {
                SelectedCloneGang = null;
                CloneFromGangId = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(previousId))
            {
                var match = ExistingGangs.FirstOrDefault(x => string.Equals(x.Id, previousId, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                {
                    SelectedCloneGang = match;
                    return;
                }
            }

            SelectedCloneGang = ExistingGangs[0];
        }

        private void SuggestNewGangId()
        {
            var existingIds = ExistingGangs.Select(g => g.Id).ToArray();

            var suggester = new LSR.XmlHelper.Core.Services.Builders.GangIdSuggestionService();
            NewGangId = suggester.Suggest(PackName, NewGangFullName, existingIds);
        }

        private void ApplyEditGangSelection()
        {
            if (!IsEditExistingGang)
                return;

            if (SelectedEditGang is null)
                return;

            var gangId = SelectedEditGang.Id;

            NewGangId = gangId;
            NewGangFullName = SelectedEditGang.FullName;
            CloneFromGangId = gangId;
            IncludeZones = true;
            IncludeTerritories = true;
            IncludeTerritoryMenus = true;
            IncludeRelationships = true;
            IncludeDens = true;

            LoadSelectedGangZones(gangId);
            LoadSelectedGangCustomTerritories();
            LoadSelectedGangTerritoryMenuContainers();
            LoadSelectedGangReferences(gangId);
            LoadSelectedGangDenDetails(gangId);
            LoadSelectedGangDenSpawns(gangId);
            LoadSelectedGangEnemyGangs(gangId);
            LoadSelectedGangAdvancedSettings(gangId);
            var info = new LSR.XmlHelper.Wpf.Services.Editing.EditModeResolvedSourcesInfoService();
            EditModeResolvedSourcesWarning = info.Build(_rootFolderPath, gangId);
        }

        private void LoadSelectedGangZones(string gangId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;
                var lookup = new LSR.XmlHelper.Core.Services.Builders.GangTerritoryZoneLookupService();
                var zoneNamesRaw = lookup.GetZoneInternalNamesForGang(_rootFolderPath, gangId).ToArray();

                var zoneCatalog = new LSR.XmlHelper.Core.Services.Builders.ZoneCatalogService();
                var catalogZones = zoneCatalog.GetZones(_rootFolderPath);

                var displayToInternal = catalogZones
                    .GroupBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().InternalGameName, StringComparer.OrdinalIgnoreCase);

                var normalizedZoneNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var zn in zoneNamesRaw)
                {
                    if (displayToInternal.TryGetValue(zn, out var internalName))
                        normalizedZoneNames.Add(internalName);
                    else
                        normalizedZoneNames.Add(zn);
                }

                RefreshZones();

                SelectedZones.Clear();

                foreach (var z in Zones.Where(z => normalizedZoneNames.Contains(z.InternalGameName)))
                    SelectedZones.Add(z);

                UpdateZoneOverlapWarning();
            }
            catch
            {
            }
        }

        private void LoadSelectedGangCustomTerritories()
        {
            try
            {
                CustomTerritoriesToAdd.Clear();
                SelectedCustomTerritoryToAdd = null;

                if (!IsEditExistingGang)
                    return;

                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                if (SelectedZones.Count == 0)
                    return;

                var zoneNames = SelectedZones
                    .Select(z => z.InternalGameName)
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (zoneNames.Length == 0)
                    return;

                var lookup = new LSR.XmlHelper.Core.Services.Builders.Zones.ZoneDefinitionLookupService();

                foreach (var zn in zoneNames)
                {
                    if (!lookup.TryGetZoneDefinition(_rootFolderPath, zn, out var def))
                        continue;

                    CustomTerritoriesToAdd.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.Zones.CustomTerritoryToAddViewModel(def));
                }

                SelectedCustomTerritoryToAdd = CustomTerritoriesToAdd.FirstOrDefault();
            }
            catch
            {
            }
        }

        private void LoadSelectedGangTerritoryMenuContainers()
        {
            try
            {
                TerritoryDealerMenuContainerId = "";
                TerritoryCustomerMenuContainerId = "";
                TerritoryMenuContainersHasMultipleValues = false;
                TerritoryMenuContainersMultipleValuesText = "";

                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                if (SelectedZones.Count == 0)
                    return;

                var zoneInternalNames = SelectedZones
                    .Select(x => x.InternalGameName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (zoneInternalNames.Length == 0)
                    return;

                var lookup = new LSR.XmlHelper.Core.Services.Builders.ZoneMenuContainersLookupService();
                var results = lookup.GetZoneMenuContainers(_rootFolderPath, zoneInternalNames);

                var dealerIds = results
                    .Select(x => x.DealerMenuContainerId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var customerIds = results
                    .Select(x => x.CustomerMenuContainerId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (dealerIds.Length > 0)
                    TerritoryDealerMenuContainerId = dealerIds[0];

                if (customerIds.Length > 0)
                    TerritoryCustomerMenuContainerId = customerIds[0];

                if (dealerIds.Length > 1 || customerIds.Length > 1)
                {
                    TerritoryMenuContainersHasMultipleValues = true;

                    var dealerText = dealerIds.Length > 1 ? string.Join(", ", dealerIds) : "";
                    var customerText = customerIds.Length > 1 ? string.Join(", ", customerIds) : "";

                    if (!string.IsNullOrWhiteSpace(dealerText) && !string.IsNullOrWhiteSpace(customerText))
                        TerritoryMenuContainersMultipleValuesText = "Multiple values across selected zones. Dealers: " + dealerText + " | Customers: " + customerText;
                    else if (!string.IsNullOrWhiteSpace(dealerText))
                        TerritoryMenuContainersMultipleValuesText = "Multiple dealer values across selected zones: " + dealerText;
                    else if (!string.IsNullOrWhiteSpace(customerText))
                        TerritoryMenuContainersMultipleValuesText = "Multiple customer values across selected zones: " + customerText;
                }
            }
            catch
            {
            }
        }

        private void LoadSelectedGangDenSpawns(string gangId)
        {
            try
            {
                _possiblePedSpawns.Clear();
                _possibleVehicleSpawns.Clear();

                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                var denLookup = new LSR.XmlHelper.Core.Services.Builders.GangDenLookupService();
                var dens = denLookup.GetGangDens(_rootFolderPath, gangId);

                foreach (var den in dens)
                {
                    var pedSpawns = LSR.XmlHelper.Core.Services.Parsing.PossiblePedSpawnParser.ParseGangDen(den);
                    foreach (var spawn in pedSpawns)
                    {
                        _possiblePedSpawns.Add(new ViewModels.Builders.PossiblePedSpawnViewModel
                        {
                            DenName = spawn.DenName,
                            X = spawn.X,
                            Y = spawn.Y,
                            Z = spawn.Z,
                            Heading = spawn.Heading,
                            Percentage = spawn.Percentage,
                            TaskRequirements = spawn.TaskRequirements,
                            MinHourSpawn = spawn.MinHourSpawn,
                            MaxHourSpawn = spawn.MaxHourSpawn,
                            MinWantedLevelSpawn = spawn.MinWantedLevelSpawn,
                            MaxWantedLevelSpawn = spawn.MaxWantedLevelSpawn,
                            LongGunAlwaysEquipped = spawn.LongGunAlwaysEquipped,
                            SourceElement = spawn.SourceElement
                        });
                    }

                    var vehicleSpawns = LSR.XmlHelper.Core.Services.Parsing.PossibleVehicleSpawnParser.ParseGangDen(den);
                    foreach (var v in vehicleSpawns)
                    {
                        _possibleVehicleSpawns.Add(new ViewModels.Builders.PossibleVehicleSpawnViewModel
                        {
                            DenName = v.DenName ?? "",
                            X = v.X,
                            Y = v.Y,
                            Z = v.Z,
                            Heading = v.Heading,
                            Percentage = v.Percentage,
                            TaskRequirements = v.TaskRequirements,
                            MinHourSpawn = v.MinHourSpawn,
                            MaxHourSpawn = v.MaxHourSpawn,
                            MinWantedLevelSpawn = v.MinWantedLevelSpawn,
                            MaxWantedLevelSpawn = v.MaxWantedLevelSpawn,
                            RequiredVehicleGroup = string.IsNullOrWhiteSpace(v.RequiredVehicleGroup) && IsEditExistingGang ? SelectedEditGangVehicleGroupId : v.RequiredVehicleGroup,
                            ForceVehicleGroup = v.ForceVehicleGroup,
                            AllowAirVehicle = v.AllowAirVehicle,
                            AllowBoat = v.AllowBoat,
                            SourceElement = v.SourceElement
                        });
                    }
                }
            }
            catch
            {
            }
        }

        public ObservableCollection<ShopMenuGroupOptionViewModel> ShopMenuGroups { get; } = new ObservableCollection<ShopMenuGroupOptionViewModel>();
        public ObservableCollection<ShopMenuGroupOptionViewModel> TerritoryDealerMenuGroups { get; } = new ObservableCollection<ShopMenuGroupOptionViewModel>();
        public ObservableCollection<ShopMenuGroupOptionViewModel> TerritoryCustomerMenuGroups { get; } = new ObservableCollection<ShopMenuGroupOptionViewModel>();
        public ObservableCollection<ShopMenuOptionViewModel> ShopMenusForSelectedGroup { get; } = new ObservableCollection<ShopMenuOptionViewModel>();

        private ShopMenuOptionViewModel? _selectedDenShopMenu;

        public ShopMenuOptionViewModel? SelectedDenShopMenu
        {
            get => _selectedDenShopMenu;
            set
            {
                if (!SetProperty(ref _selectedDenShopMenu, value))
                    return;

                if (_selectedDenShopMenu is not null)
                    DenMenuId = _selectedDenShopMenu.Id;

                RefreshTaskState();
            }
        }

        private ShopMenuGroupOptionViewModel? _selectedShopMenuGroup;

        public ShopMenuGroupOptionViewModel? SelectedShopMenuGroup
        {
            get => _selectedShopMenuGroup;
            set
            {
                if (!SetProperty(ref _selectedShopMenuGroup, value))
                    return;

                if (_selectedShopMenuGroup is not null)
                    ManualDealerMenuGroupId = _selectedShopMenuGroup.Id;
                RefreshShopMenusForSelectedGroup();

                RefreshTaskState();
            }
        }

        private bool _useSourceGangDealerMenuGroup;

        public bool UseSourceGangDealerMenuGroup
        {
            get => _useSourceGangDealerMenuGroup;
            set
            {
                if (!SetProperty(ref _useSourceGangDealerMenuGroup, value))
                    return;

                RefreshTaskState();
            }
        }

        private string _manualDealerMenuGroupId = "";

        public string ManualDealerMenuGroupId
        {
            get => _manualDealerMenuGroupId;
            set
            {
                if (!SetProperty(ref _manualDealerMenuGroupId, value))
                    return;

                RefreshTaskState();

                if (IsEditExistingGang)
                    DealerMenuGroupItemsEditor.IsEnabled = true;
                DealerMenuGroupItemsEditor.LoadGroup(_manualDealerMenuGroupId);
                LoadSelectedGangDealerMenuPreview(_manualDealerMenuGroupId);
            }
        }

        private bool _showCustomerMenus;

        private bool _cloneDealerMenusIntoPack = true;

        private bool _cloneWeaponsIntoPack = true;
        public bool CloneDealerMenusIntoPack
        {
            get => _cloneDealerMenusIntoPack;
            set
            {
                if (!SetProperty(ref _cloneDealerMenusIntoPack, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool CloneWeaponsIntoPack
        {
            get => _cloneWeaponsIntoPack;
            set
            {
                if (!SetProperty(ref _cloneWeaponsIntoPack, value))
                    return;

                RefreshTaskState();
            }
        }

        public bool ShowCustomerMenus
        {
            get => _showCustomerMenus;
            set
            {
                if (!SetProperty(ref _showCustomerMenus, value))
                    return;

                RefreshShopMenuGroups();
            }
        }

        public ICommand RefreshShopMenuGroupsCommand { get; }
        public ICommand ChooseDenBannerImageCommand { get; }
        private void LoadSelectedGangReferences(string gangId)
        {
            try
            {
                var reader = new LSR.XmlHelper.Core.Services.Builders.GangEditSnapshotReadService();
                var snapshot = reader.TryGet(_rootFolderPath, gangId);

                if (snapshot is null)
                    return;

                if (!string.IsNullOrWhiteSpace(snapshot.FullName))
                    NewGangFullName = snapshot.FullName;

                if (!string.IsNullOrWhiteSpace(snapshot.ColorString))
                    GangColorString = snapshot.ColorString;
                else if (!string.IsNullOrWhiteSpace(snapshot.ColorPrefix))
                    GangColorPrefix = snapshot.ColorPrefix;

                IncludePeople = true;

                RefreshDispatchablePeopleGroups();

                if (!string.IsNullOrWhiteSpace(snapshot.PeopleGroupId))
                {
                    var peopleMatch = DispatchablePeopleGroups.FirstOrDefault(x => string.Equals(x.Id, snapshot.PeopleGroupId, StringComparison.OrdinalIgnoreCase));
                    if (peopleMatch is not null)
                        SelectedDispatchablePeopleGroup = peopleMatch;
                }

                UseSourceGangPeopleGroup = false;
                IncludeVehicles = true;
                _lastBuiltVehicleGroupId = snapshot.VehicleGroupId;
                SelectedEditGangVehicleGroupId = _lastBuiltVehicleGroupId;

                SelectedEditGangVehicleModelsPreview.Clear();

                var vehiclePreview = new LSR.XmlHelper.Core.Services.Reading.DispatchableVehicleGroupModelsReadService();
                foreach (var model in vehiclePreview.GetModelsForGroupIdResolved(_rootFolderPath, SelectedEditGangVehicleGroupId))
                    SelectedEditGangVehicleModelsPreview.Add(model);

                if (!string.IsNullOrWhiteSpace(snapshot.DealerMenuGroupId))
                {
                    IncludeDealerMenus = true;
                    UseSourceGangDealerMenuGroup = false;
                    ManualDealerMenuGroupId = snapshot.DealerMenuGroupId;

                    LoadSelectedGangDealerMenuPreview(snapshot.DealerMenuGroupId);
                }

                RefreshDispatchablePeopleGroups();
                RefreshDispatchableVehicleGroups();
                RefreshShopMenuGroups();
                if (!string.IsNullOrWhiteSpace(snapshot.DealerMenuGroupId))
                {
                    var dealerMatch = ShopMenuGroups.FirstOrDefault(x => string.Equals(x.Id, snapshot.DealerMenuGroupId, StringComparison.OrdinalIgnoreCase));
                    if (dealerMatch is not null)
                        SelectedShopMenuGroup = dealerMatch;
                }
                RefreshIssuableWeaponsGroups();

                if (!string.IsNullOrWhiteSpace(snapshot.PeopleGroupId))
                {
                    var peopleMatch = DispatchablePeopleGroups.FirstOrDefault(x => string.Equals(x.Id, snapshot.PeopleGroupId, StringComparison.OrdinalIgnoreCase));
                    if (peopleMatch is not null)
                        SelectedDispatchablePeopleGroup = peopleMatch;
                }

                if (!string.IsNullOrWhiteSpace(snapshot.VehicleGroupId))
                {
                    RefreshDispatchableVehicleGroups();
                }

                if (!string.IsNullOrWhiteSpace(snapshot.MeleeWeaponsId))
                {
                    var meleeMatch = MeleeWeaponsGroups.FirstOrDefault(x => string.Equals(x.Id, snapshot.MeleeWeaponsId, StringComparison.OrdinalIgnoreCase));
                    if (meleeMatch is not null)
                        SelectedMeleeWeaponsGroup = meleeMatch;
                }

                if (!string.IsNullOrWhiteSpace(snapshot.SideArmsId))
                {
                    var sideMatch = SidearmWeaponsGroups.FirstOrDefault(x => string.Equals(x.Id, snapshot.SideArmsId, StringComparison.OrdinalIgnoreCase));
                    if (sideMatch is not null)
                        SelectedSideArmsGroup = sideMatch;
                }

                if (!string.IsNullOrWhiteSpace(snapshot.LongGunsId))
                {
                    var longMatch = LongGunWeaponsGroups.FirstOrDefault(x => string.Equals(x.Id, snapshot.LongGunsId, StringComparison.OrdinalIgnoreCase));
                    if (longMatch is not null)
                        SelectedLongGunsGroup = longMatch;
                }
            }
            catch
            {
            }
        }
        private void LoadSelectedGangDenDetails(string gangId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                var denLookup = new LSR.XmlHelper.Core.Services.Builders.GangDenLookupService();
                var dens = denLookup.GetGangDens(_rootFolderPath, gangId);

                var den = dens.FirstOrDefault();
                if (den is null)
                    return;

                var denName = ((string?)den.Element("Name") ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(denName))
                    NewDenName = denName;

                var entrance = den.Element("EntrancePosition");
                if (entrance is not null)
                {
                    NewDenX = ((string?)entrance.Element("X") ?? "").Trim();
                    NewDenY = ((string?)entrance.Element("Y") ?? "").Trim();
                    NewDenZ = ((string?)entrance.Element("Z") ?? "").Trim();
                }

                NewDenHeading = ((string?)den.Element("EntranceHeading") ?? "").Trim();

                DenBannerImagePath = ((string?)den.Element("BannerImagePath") ?? "").Trim();
                DenMenuId = ((string?)den.Element("MenuID") ?? "").Trim();

                if (!string.IsNullOrWhiteSpace(DenMenuId))
                {
                    var menuMatch = ShopMenusForSelectedGroup.FirstOrDefault(x => string.Equals(x.Id, DenMenuId, StringComparison.OrdinalIgnoreCase));
                    if (menuMatch is null)
                    {
                        RefreshShopMenusForSelectedGroup();
                        menuMatch = ShopMenusForSelectedGroup.FirstOrDefault(x => string.Equals(x.Id, DenMenuId, StringComparison.OrdinalIgnoreCase));
                    }

                    if (menuMatch is not null)
                        SelectedDenShopMenu = menuMatch;
                }

                LoadSelectedGangDenInventoryPreview(DenMenuId);
                LoadSelectedGangDenInventoryEditable(DenMenuId);
                LoadSelectedGangDenInventoryPreview(DenMenuId);
            }
            catch
            {
            }
        }

        private void LoadSelectedGangEnemyGangs(string gangId)
        {
            try
            {
                UseSourceGangEnemyGangs = false;

                SelectedEnemyGangs.Clear();

                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
                var path = resolver.ResolveGangFile(_rootFolderPath, gangId);

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return;

                var doc = XDocument.Load(path, LoadOptions.None);

                var gang = doc
                    .Descendants("Gang")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase));

                if (gang is null)
                    return;

                var enemyIds = gang
                    .Element("EnemyGangs")?
                    .Elements()
                    .Select(e => ((string?)e.Element("GangID") ?? (string?)e.Value ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray() ?? Array.Empty<string>();

                foreach (var enemyId in enemyIds)
                {
                    var match = ExistingGangs.FirstOrDefault(x => string.Equals(x.Id, enemyId, StringComparison.OrdinalIgnoreCase));
                    if (match is not null)
                        SelectedEnemyGangs.Add(match);
                }
            }
            catch
            {
            }
        }

        private void RefreshDispatchablePeopleGroups()
        {
            var previousId = SelectedDispatchablePeopleGroup?.Id;

            DispatchablePeopleGroups.Clear();

            var catalog = new LSR.XmlHelper.Core.Services.Builders.DispatchablePeopleGroupCatalogService();
            var groups = catalog.GetGroups(_rootFolderPath);

            foreach (var g in groups)
                DispatchablePeopleGroups.Add(new DispatchablePeopleGroupOptionViewModel(g.Id, g.Count));

            if (!string.IsNullOrWhiteSpace(previousId))
            {
                var match = DispatchablePeopleGroups.FirstOrDefault(x => string.Equals(x.Id, previousId, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    SelectedDispatchablePeopleGroup = match;
            }

            RefreshTaskState();
        }

        void RefreshDispatchableVehicleGroups()
        {
            DenVehicleGroupOptions.Clear();
            DenVehicleGroupOptions.Add(new ViewModels.Builders.DispatchableVehicleGroupOptionViewModel(NewGangVehicleGroupPlaceholder, CustomDispatchableVehicleModelsToAdd.Count, GetNewGangVehicleGroupDisplayName()));

            var catalog = new LSR.XmlHelper.Core.Services.Builders.DispatchableVehicleGroupCatalogService();
            var groups = catalog.GetGroups(_rootFolderPath);

            foreach (var g in groups)
                DenVehicleGroupOptions.Add(new ViewModels.Builders.DispatchableVehicleGroupOptionViewModel(g.Id, g.Count));
        }

        void UpdateNewGangVehicleGroupOptionCount()
        {
            for (var i = 0; i < DenVehicleGroupOptions.Count; i++)
            {
                var opt = DenVehicleGroupOptions[i];
                if (!string.Equals(opt.Id, NewGangVehicleGroupPlaceholder, StringComparison.OrdinalIgnoreCase))
                    continue;

                var updated = new ViewModels.Builders.DispatchableVehicleGroupOptionViewModel(NewGangVehicleGroupPlaceholder, CustomDispatchableVehicleModelsToAdd.Count, GetNewGangVehicleGroupDisplayName());
                DenVehicleGroupOptions[i] = updated;
                return;
            }
        }
        private string GetNewGangVehicleGroupDisplayName()
        {
            var id = (NewGangId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(id))
                return "NewGang_Vehicles";

            return $"{id}_Vehicles";
        }

        void RefreshDispatchableVehicleModels()
        {
            DispatchableVehicleModelOptions.Clear();

            var catalog = new LSR.XmlHelper.Core.Services.Builders.DispatchableVehicleModelCatalogService();
            var models = catalog.GetModels(_rootFolderPath);

            foreach (var m in models)
                DispatchableVehicleModelOptions.Add(new ViewModels.Builders.DispatchableVehicleModelOptionViewModel(m.ModelName, m.Count));
        }

        void RefreshDispatchableVehicleVariantsForModel(string modelName)
        {
            DispatchableVehicleVariantOptions.Clear();
            SelectedDispatchableVehicleVariantOption = null;

            if (string.IsNullOrWhiteSpace(modelName))
                return;

            var catalog = new LSR.XmlHelper.Core.Services.Builders.DispatchableVehicleVariantCatalogService();
            var variants = catalog.GetVariantsForModel(_rootFolderPath, modelName);

            foreach (var v in variants)
                DispatchableVehicleVariantOptions.Add(new ViewModels.Builders.DispatchableVehicleVariantOptionViewModel(v.VariantKey, v.DisplayText));

            SelectedDispatchableVehicleVariantOption = DispatchableVehicleVariantOptions.FirstOrDefault();
        }

        private void LoadDispatchablePeopleEntries()
        {
            DispatchablePeopleEntries.Clear();
            SelectedDispatchablePersonEntry = null;
            SelectedDispatchablePersonField = null;

            if (UseSourceGangPeopleGroup && !IsEditExistingGang)
                return;

            if (SelectedDispatchablePeopleGroup is null)
                return;

            var reader = new LSR.XmlHelper.Core.Services.Builders.DispatchablePeopleGroupReadService();
            var group = reader.TryReadGroup(_rootFolderPath, SelectedDispatchablePeopleGroup.Id);

            if (group is null)
                return;

            var people = group.Descendants("DispatchablePerson").ToList();
            for (var i = 0; i < people.Count; i++)
            {
                var p = people[i];
                var debugName = ((string?)p.Element("DebugName") ?? "").Trim();

                var fields = new List<DispatchablePersonFieldViewModel>();
                foreach (var child in p.Elements())
                {
                    var name = child.Name.LocalName;
                    var isXml = child.HasElements;
                    var value = isXml ? child.ToString(SaveOptions.DisableFormatting) : (child.Value ?? "");
                    fields.Add(new DispatchablePersonFieldViewModel(name, value, isXml));
                }

                var entry = new DispatchablePersonEntryViewModel(debugName, i, fields);
                DispatchablePeopleEntries.Add(entry);
            }

            SelectedDispatchablePersonEntry = DispatchablePeopleEntries.FirstOrDefault();
            SelectedDispatchablePersonField = SelectedDispatchablePersonEntry?.Fields.FirstOrDefault();
            UpdateDispatchablePersonFieldsView();
            ResetDispatchablePeopleEntriesCommand.RaiseCanExecuteChanged();
        }

        private void RefreshShopMenuGroups()
        {
            var previousId = SelectedShopMenuGroup?.Id ?? ManualDealerMenuGroupId;

            ShopMenuGroups.Clear();
            TerritoryDealerMenuGroups.Clear();
            TerritoryCustomerMenuGroups.Clear();

            var catalog = new LSR.XmlHelper.Core.Services.Builders.ShopMenuGroupCatalogService();
            var groups = catalog.GetShopMenuGroups(_rootFolderPath);

            foreach (var g in groups)
            {
                var text = (g.Id + " " + g.Name).ToLowerInvariant();
                var isCustomer = text.Contains("customermenu") || text.Contains("customer") || text.Contains("cust");
                var isDealer = text.Contains("dealermenu") || text.Contains("dealer");

                if (isDealer)
                    TerritoryDealerMenuGroups.Add(new ShopMenuGroupOptionViewModel(g.Id, g.Name));

                if (isCustomer)
                    TerritoryCustomerMenuGroups.Add(new ShopMenuGroupOptionViewModel(g.Id, g.Name));

                if (!ShowCustomerMenus && isCustomer)
                    continue;

                ShopMenuGroups.Add(new ShopMenuGroupOptionViewModel(g.Id, g.Name));
            }

            if (!string.IsNullOrWhiteSpace(previousId))
            {
                var match = ShopMenuGroups.FirstOrDefault(x => string.Equals(x.Id, previousId, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    SelectedShopMenuGroup = match;
            }
        }

        private void RefreshShopMenusForSelectedGroup()
        {
            ShopMenusForSelectedGroup.Clear();
            SelectedDenShopMenu = null;

            var groupId = SelectedShopMenuGroup?.Id ?? ManualDealerMenuGroupId;
            if (string.IsNullOrWhiteSpace(groupId))
                return;

            var catalog = new LSR.XmlHelper.Core.Services.Builders.ShopMenuCatalogService();
            var menus = catalog.GetShopMenusForGroup(_rootFolderPath, groupId);

            foreach (var m in menus)
                ShopMenusForSelectedGroup.Add(new ShopMenuOptionViewModel(m.Id, m.Name));
        }
        public ObservableCollection<LSR.XmlHelper.Wpf.ViewModels.Builders.Zones.CustomTerritoryToAddViewModel> CustomTerritoriesToAdd { get; } = new ObservableCollection<LSR.XmlHelper.Wpf.ViewModels.Builders.Zones.CustomTerritoryToAddViewModel>();

        private LSR.XmlHelper.Wpf.ViewModels.Builders.Zones.CustomTerritoryToAddViewModel? _selectedCustomTerritoryToAdd;
        public LSR.XmlHelper.Wpf.ViewModels.Builders.Zones.CustomTerritoryToAddViewModel? SelectedCustomTerritoryToAdd
        {
            get => _selectedCustomTerritoryToAdd;
            set
            {
                if (!SetProperty(ref _selectedCustomTerritoryToAdd, value))
                    return;

                if (value is null)
                    return;

                LoadCustomTerritoryEditorFromDefinition(value.Definition);
            }
        }

        private string _customTerritoryInternalGameName = "";
        public string CustomTerritoryInternalGameName
        {
            get => _customTerritoryInternalGameName;
            set => SetProperty(ref _customTerritoryInternalGameName, value);
        }

        private string _customTerritoryDisplayName = "";
        public string CustomTerritoryDisplayName
        {
            get => _customTerritoryDisplayName;
            set => SetProperty(ref _customTerritoryDisplayName, value);
        }

        private string _customTerritoryCountyId = "";
        public string CustomTerritoryCountyId
        {
            get => _customTerritoryCountyId;
            set => SetProperty(ref _customTerritoryCountyId, value);
        }

        private string _customTerritoryState = "";
        public string CustomTerritoryState
        {
            get => _customTerritoryState;
            set => SetProperty(ref _customTerritoryState, value);
        }

        private string _customTerritoryEconomy = "";
        public string CustomTerritoryEconomy
        {
            get => _customTerritoryEconomy;
            set => SetProperty(ref _customTerritoryEconomy, value);
        }

        private string _customTerritoryType = "";
        public string CustomTerritoryType
        {
            get => _customTerritoryType;
            set => SetProperty(ref _customTerritoryType, value);
        }

        private bool _customTerritoryIsRestrictedDuringWanted;
        public bool CustomTerritoryIsRestrictedDuringWanted
        {
            get => _customTerritoryIsRestrictedDuringWanted;
            set => SetProperty(ref _customTerritoryIsRestrictedDuringWanted, value);
        }

        private bool _customTerritoryIsSpecificLocation = true;
        public bool CustomTerritoryIsSpecificLocation
        {
            get => _customTerritoryIsSpecificLocation;
            set => SetProperty(ref _customTerritoryIsSpecificLocation, value);
        }

        private string _customTerritoryBoundariesText = "";
        public string CustomTerritoryBoundariesText
        {
            get => _customTerritoryBoundariesText;
            set => SetProperty(ref _customTerritoryBoundariesText, value);
        }

        public NotifyRelayCommand AddCustomTerritoryCommand { get; }
        public NotifyRelayCommand RemoveSelectedCustomTerritoryCommand { get; }
        public RelayCommand SmartPasteCustomTerritoryBoundariesCommand { get; }
        public ObservableCollection<string> CustomTerritoryCountyIdOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CustomTerritoryStateIdOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CustomTerritoryEconomyOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CustomTerritoryTypeOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<ZoneOptionViewModel> Zones { get; } = new ObservableCollection<ZoneOptionViewModel>();
        private bool _useSourceGangWeaponsLoadouts;

        public bool UseSourceGangWeaponsLoadouts
        {
            get => _useSourceGangWeaponsLoadouts;
            set
            {
                if (!SetProperty(ref _useSourceGangWeaponsLoadouts, value))
                    return;

                RefreshTaskState();
            }
        }

        private string _sourceMeleeWeaponsId = "";

        public string SourceMeleeWeaponsId
        {
            get => _sourceMeleeWeaponsId;
            set => SetProperty(ref _sourceMeleeWeaponsId, value);
        }

        private string _sourceSideArmsId = "";

        public string SourceSideArmsId
        {
            get => _sourceSideArmsId;
            set => SetProperty(ref _sourceSideArmsId, value);
        }

        private string _sourceLongGunsId = "";

        public string SourceLongGunsId
        {
            get => _sourceLongGunsId;
            set => SetProperty(ref _sourceLongGunsId, value);
        }

        public ObservableCollection<IssuableWeaponsGroupOptionViewModel> MeleeWeaponsGroups { get; } = new ObservableCollection<IssuableWeaponsGroupOptionViewModel>();
        public ObservableCollection<IssuableWeaponsGroupOptionViewModel> SidearmWeaponsGroups { get; } = new ObservableCollection<IssuableWeaponsGroupOptionViewModel>();
        public ObservableCollection<IssuableWeaponsGroupOptionViewModel> LongGunWeaponsGroups { get; } = new ObservableCollection<IssuableWeaponsGroupOptionViewModel>();

        private IssuableWeaponsGroupOptionViewModel? _selectedMeleeWeaponsGroup;

        public IssuableWeaponsGroupOptionViewModel? SelectedMeleeWeaponsGroup
        {
            get => _selectedMeleeWeaponsGroup;
            set
            {
                if (!SetProperty(ref _selectedMeleeWeaponsGroup, value))
                    return;

                MeleeWeaponsId = _selectedMeleeWeaponsGroup?.Id ?? "";
                RefreshTaskState();
            }
        }

        private IssuableWeaponsGroupOptionViewModel? _selectedSideArmsGroup;

        public IssuableWeaponsGroupOptionViewModel? SelectedSideArmsGroup
        {
            get => _selectedSideArmsGroup;
            set
            {
                if (!SetProperty(ref _selectedSideArmsGroup, value))
                    return;

                SideArmsId = _selectedSideArmsGroup?.Id ?? "";
                RefreshTaskState();
            }
        }

        private IssuableWeaponsGroupOptionViewModel? _selectedLongGunsGroup;

        public IssuableWeaponsGroupOptionViewModel? SelectedLongGunsGroup
        {
            get => _selectedLongGunsGroup;
            set
            {
                if (!SetProperty(ref _selectedLongGunsGroup, value))
                    return;

                LongGunsId = _selectedLongGunsGroup?.Id ?? "";
                RefreshTaskState();
            }
        }

        private string _meleeWeaponsId = "";

        public string MeleeWeaponsId
        {
            get => _meleeWeaponsId;
            set
            {
                if (!SetProperty(ref _meleeWeaponsId, value))
                    return;

                RefreshTaskState();
            }
        }

        private string _sideArmsId = "";

        public string SideArmsId
        {
            get => _sideArmsId;
            set
            {
                if (!SetProperty(ref _sideArmsId, value))
                    return;

                RefreshTaskState();
            }
        }

        private string _longGunsId = "";

        public string LongGunsId
        {
            get => _longGunsId;
            set
            {
                if (!SetProperty(ref _longGunsId, value))
                    return;

                RefreshTaskState();
            }
        }

        public ICommand RefreshIssuableWeaponsGroupsCommand { get; }

        private bool _useSourceGangEnemyGangs;

        public bool UseSourceGangEnemyGangs
        {
            get => _useSourceGangEnemyGangs;
            set
            {
                if (!SetProperty(ref _useSourceGangEnemyGangs, value))
                    return;

                RefreshTaskState();
            }
        }

        private string _sourceEnemyGangsSummary = "None";

        public string SourceEnemyGangsSummary
        {
            get => _sourceEnemyGangsSummary;
            set => SetProperty(ref _sourceEnemyGangsSummary, value);
        }

        public ObservableCollection<GangOptionViewModel> SelectedEnemyGangs { get; } = new ObservableCollection<GangOptionViewModel>();

        private void RefreshSourceEnemyGangs()
        {
            var relCatalog = new LSR.XmlHelper.Core.Services.Builders.GangRelationshipsCatalogService();
            var enemyIds = relCatalog.GetEnemyGangIds(_rootFolderPath, CloneFromGangId);

            SourceEnemyGangsSummary = enemyIds.Count == 0 ? "None" : string.Join(", ", enemyIds);

            if (UseSourceGangEnemyGangs)
            {
                var selectedSet = enemyIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

                SelectedEnemyGangs.Clear();

                foreach (var g in ExistingGangs.Where(g => selectedSet.Contains(g.Id)))
                    SelectedEnemyGangs.Add(g);
            }

            RefreshTaskState();
        }

        private void RefreshIssuableWeaponsGroups()
        {
            var previousMelee = SelectedMeleeWeaponsGroup?.Id ?? MeleeWeaponsId;
            var previousSide = SelectedSideArmsGroup?.Id ?? SideArmsId;
            var previousLong = SelectedLongGunsGroup?.Id ?? LongGunsId;

            MeleeWeaponsGroups.Clear();
            SidearmWeaponsGroups.Clear();
            LongGunWeaponsGroups.Clear();

            var catalog = new LSR.XmlHelper.Core.Services.Builders.IssuableWeaponsGroupCatalogService();
            var groups = catalog.GetGroups(_rootFolderPath)
                .Select(g => new IssuableWeaponsGroupOptionViewModel(g.Id, g.Name))
                .ToList();

            static bool ContainsAny(string value, params string[] tokens)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                foreach (var t in tokens)
                {
                    if (value.Contains(t, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }

            foreach (var g in groups)
            {
                var id = g.Id ?? "";
                var name = g.Name ?? "";

                if (ContainsAny(id, "Melee") || ContainsAny(name, "Melee"))
                    MeleeWeaponsGroups.Add(g);

                if (ContainsAny(id, "Sidearm", "SideArms", "Handgun", "Pistol") || ContainsAny(name, "Sidearm", "SideArms", "Handgun", "Pistol"))
                    SidearmWeaponsGroups.Add(g);

                if (ContainsAny(id, "LongGun", "LongGuns", "Rifle", "Shotgun", "SMG") || ContainsAny(name, "LongGun", "LongGuns", "Rifle", "Shotgun", "SMG"))
                    LongGunWeaponsGroups.Add(g);
            }

            SelectedMeleeWeaponsGroup = MeleeWeaponsGroups.FirstOrDefault(x => string.Equals(x.Id, previousMelee, StringComparison.OrdinalIgnoreCase));
            SelectedSideArmsGroup = SidearmWeaponsGroups.FirstOrDefault(x => string.Equals(x.Id, previousSide, StringComparison.OrdinalIgnoreCase));
            SelectedLongGunsGroup = LongGunWeaponsGroups.FirstOrDefault(x => string.Equals(x.Id, previousLong, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdateZoneOverlapWarning()
        {
            var overlapping = SelectedZones
                .Where(z => !string.IsNullOrWhiteSpace(z.UsedBy) && z.UsedBy.Contains(",", StringComparison.Ordinal))
                .OrderBy(z => z.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (overlapping.Length == 0)
            {
                ShowZoneOverlapWarning = false;
                ZoneOverlapWarningText = "";
                return;
            }

            var parts = overlapping
                .Select(z => $"{z.DisplayName} ({z.InternalGameName}) used by: {z.UsedBy}")
                .ToArray();

            ShowZoneOverlapWarning = true;
            ZoneOverlapWarningText = "Warning: Some selected zones are already used by multiple gangs:\r\n" + string.Join("\r\n", parts);
        }

        public ObservableCollection<ZoneOptionViewModel> SelectedZones { get; } = new ObservableCollection<ZoneOptionViewModel>();

        public string SelectedZonesSummary
        {
            get
            {
                if (SelectedZones.Count == 0)
                    return "Selected zones: (none)";

                var names = SelectedZones
                    .Select(z => (z.DisplayText ?? "").Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .OrderBy(z => z, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (names.Length == 0)
                    return "Selected zones: (none)";

                return "Selected zones: " + string.Join(", ", names);
            }
        }

        public ICommand RefreshZonesCommand { get; }

        private bool _showZoneOverlapWarning;

        public bool ShowZoneOverlapWarning
        {
            get => _showZoneOverlapWarning;
            set => SetProperty(ref _showZoneOverlapWarning, value);
        }

        private string _zoneOverlapWarningText = "";

        public string ZoneOverlapWarningText
        {
            get => _zoneOverlapWarningText;
            set => SetProperty(ref _zoneOverlapWarningText, value);
        }

        private void RefreshZones()
        {
            Zones.Clear();

            var usedByCatalog = new LSR.XmlHelper.Core.Services.Builders.ZoneUsageCatalogService();
            var usedBy = usedByCatalog.GetZoneUsedByDisplay(_rootFolderPath);

            var catalog = new LSR.XmlHelper.Core.Services.Builders.ZoneCatalogService();
            var zones = catalog.GetZones(_rootFolderPath);

            var territoryService = new LSR.XmlHelper.Core.Services.Builders.TerritoryMenuZoneLabelService();
            var drugInfo = territoryService.GetZoneDrugSummary(
                _rootFolderPath,
                zones.Select(x => x.InternalGameName).ToArray());

            foreach (var z in zones)
            {
                usedBy.TryGetValue(z.InternalGameName, out var usedByText);
                drugInfo.TryGetValue(z.InternalGameName, out var drugs);

                Zones.Add(
                    new ZoneOptionViewModel(
                        z.InternalGameName,
                        z.DisplayName,
                        usedByText ?? "",
                        drugs.Dealers ?? "",
                        drugs.Customers ?? ""));
            }

            var selected = SelectedZones
                .Select(x => x.InternalGameName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            SelectedZones.Clear();

            foreach (var z in Zones.Where(z => selected.Contains(z.InternalGameName)))
                SelectedZones.Add(z);

            var refValues = new LSR.XmlHelper.Core.Services.Builders.Zones.ZonesReferenceValuesCatalogService();
            var (countyIds, stateIds, economies, types) = refValues.GetOptions(_rootFolderPath);

            CustomTerritoryCountyIdOptions.Clear();
            foreach (var s in countyIds)
                CustomTerritoryCountyIdOptions.Add(s);

            CustomTerritoryStateIdOptions.Clear();
            foreach (var s in stateIds)
                CustomTerritoryStateIdOptions.Add(s);

            CustomTerritoryEconomyOptions.Clear();
            foreach (var s in economies)
                CustomTerritoryEconomyOptions.Add(s);

            CustomTerritoryTypeOptions.Clear();
            foreach (var s in types)
                CustomTerritoryTypeOptions.Add(s);

            UpdateZoneOverlapWarning();
            RefreshTerritoryCurrentSetup();
            RefreshTaskState();
        }

        private static void SetOrUpdateGangField(System.Xml.Linq.XElement gangNode, string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var existing = gangNode.Element(fieldName);
            if (existing is null)
                gangNode.Add(new System.Xml.Linq.XElement(fieldName, value));
            else
                existing.Value = value;
        }
        private static void SetEnemyGangs(System.Xml.Linq.XElement gangNode, IReadOnlyCollection<string> enemyGangIds)
        {
            var existing = gangNode.Element("EnemyGangs");
            existing?.Remove();

            var container = new System.Xml.Linq.XElement("EnemyGangs");

            foreach (var id in enemyGangIds)
                container.Add(new System.Xml.Linq.XElement("string", id));

            gangNode.Add(container);
        }

        private bool CanBuildPack()
        {
            return HasCoreInputs();
        }
       
        private void BuildPack()
        {
            var preBuildOk = ValidateBeforeBuild(out var preBuildIssues);
            if (!preBuildOk)
            {
                if (!IsEditExistingGang)
                {
                    var duplicateIdIssue = preBuildIssues.FirstOrDefault(x =>
                        x.FocusTarget == "NewGangIdTextBox"
                        && x.Message.StartsWith("NewGangId already exists:", StringComparison.OrdinalIgnoreCase));

                    if (duplicateIdIssue is not null && preBuildIssues.Count == 1)
                    {
                        var choice = System.Windows.MessageBox.Show(
                            duplicateIdIssue.Message + "\n\nCreate the gang anyway?",
                            "Gang Builder",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (choice == MessageBoxResult.Yes)
                            preBuildOk = true;
                    }
                }

                if (!preBuildOk)
                {
                    SetTask("Pre-build validation", false, "Blocked");

                    var window = System.Windows.Application.Current.Windows.OfType<LSR.XmlHelper.Wpf.Views.Builders.GangBuilderWindow>().FirstOrDefault();
                    var dialog = new LSR.XmlHelper.Wpf.Views.Dialogs.PreBuildValidationWindow
                    {
                        Owner = window,
                        DataContext = preBuildIssues
                    };

                    var dialogResult = dialog.ShowDialog();
                    if (dialogResult == true && window is not null)
                        window.FocusValidationTarget(dialog.RequestedFocusTarget, dialog.RequestedMessage);

                    return;
                }
            }

            SetTask("Pre-build validation", true, "Done");
            
            if (IsEditExistingGang)
            {
                BuildPack_EditMode();
                return;
            }

            var builder = new LSR.XmlHelper.Core.Services.Builders.GangPackBuilderService();
            var (ok, message, result) = builder.BuildCloneFirst(_rootFolderPath, PackName, NewGangId, NewGangFullName, CloneFromGangId);

            if (!ok || result is null)
            {
                System.Windows.MessageBox.Show(message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _lastBuiltVehicleGroupId = result.VehicleGroupId;
            var removedInvalidDenWeaponItems = new List<string>();
            foreach (var vm in PossibleVehicleSpawns)
            {
                if (string.Equals((vm.RequiredVehicleGroup ?? "").Trim(), NewGangVehicleGroupPlaceholder, StringComparison.OrdinalIgnoreCase))
                    vm.RequiredVehicleGroup = result.VehicleGroupId;
            }

            RefreshDispatchableVehicleGroups();
            var missingCustomVehicleModels = new List<string>();
            var backedUpFiles = new List<string>();
            var editedFiles = new List<string>();
            var xmlService = new LSR.XmlHelper.Core.Services.XmlDocumentService();
            var gangsPath = Path.Combine(_rootFolderPath, $"Gangs+_{PackName}.xml");
            var peoplePath = Path.Combine(_rootFolderPath, $"DispatchablePeople+_{PackName}.xml");
            var vehiclesPath = Path.Combine(_rootFolderPath, $"DispatchableVehicles+_{PackName}.xml");
            var shopMenusWritten = false;
            var zonesWritten = false;
            var derivedDenMenuId = "";
            var issuableWeaponsWritten = false;
            var peopleDocToWrite = result.PeopleDoc;

            if (IncludePeople && !UseSourceGangPeopleGroup)
            {
                if (SelectedDispatchablePeopleGroup is null)
                {
                    SetTask("Create/Update DispatchablePeople group", false, "Missing group");
                    System.Windows.MessageBox.Show("People is enabled, but no DispatchablePeople group was selected.", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var cloner = new LSR.XmlHelper.Core.Services.Builders.DispatchablePeopleGroupCloneService();
                var (peopleOk, peopleMessage, peopleDoc) = cloner.CloneToNewId(_rootFolderPath, SelectedDispatchablePeopleGroup.Id, result.PeopleGroupId);

                if (!peopleOk || peopleDoc is null)
                {
                    SetTask("Create/Update DispatchablePeople group", false, "Blocked");
                    System.Windows.MessageBox.Show(peopleMessage, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                peopleDocToWrite = peopleDoc;

                if (IncludePeople && !UseSourceGangPeopleGroup && DispatchablePeopleEntries.Count > 0)
                {
                    var applier = new LSR.XmlHelper.Wpf.Services.Editing.DispatchablePeopleGroupEditsApplyService();
                    var applyResult = applier.Apply(peopleDocToWrite, result.PeopleGroupId, DispatchablePeopleEntries);

                    if (applyResult.XmlIssues.Count > 0)
                    {
                        var issuesText = string.Join("\r\n- ", applyResult.XmlIssues.Select(x => $"Person index {x.PersonIndex}: {x.FieldName}"));
                        System.Windows.MessageBox.Show(
                            "Some fields contained invalid XML and were not saved:\r\n\r\n- " + issuesText,
                            "Gang Builder",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }

            if (IncludeDealerMenus)
            {
                var finalDealerGroupId = "";

                if (UseSourceGangDealerMenuGroup && !string.IsNullOrWhiteSpace(result.SourceDealerMenuGroupId))
                    finalDealerGroupId = result.SourceDealerMenuGroupId;
                else if (!string.IsNullOrWhiteSpace(ManualDealerMenuGroupId))
                    finalDealerGroupId = ManualDealerMenuGroupId;

                var gangNode = result.GangsDoc.Descendants("Gang").FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(finalDealerGroupId) && gangNode is not null)
                {
                    SetOrUpdateGangField(gangNode, "DealerMenuGroup", finalDealerGroupId);
                    SetTask("Assign Dealer Menu Group", true, "Done");

                    if (CloneDealerMenusIntoPack)
                    {
                        var cloner = new LSR.XmlHelper.Core.Services.Builders.ShopMenusCloneBuilderService();
                        var desiredId = $"{finalDealerGroupId}_{PackName}";
                        var (shopMenusDoc, clonedGroupId, clonedMenusCount) = cloner.CloneDealerGroup(_rootFolderPath, finalDealerGroupId, desiredId, PackName);
                        var editedMenus = DealerMenuGroupItemsEditor.GetMenuEditsForSave();

                        if (editedMenus.Count > 0)
                        {
                            var applier = new LSR.XmlHelper.Core.Services.Builders.ShopMenuGroupMenuItemsApplyService();

                            foreach (var edit in editedMenus)
                                applier.ApplyItemsToGroupMenuIndex(shopMenusDoc, clonedGroupId, edit.MenuIndex, edit.Items);
                        }

                        if (!string.IsNullOrWhiteSpace(clonedGroupId))
                        {
                            SetOrUpdateGangField(gangNode, "DealerMenuGroup", clonedGroupId);

                            if (string.IsNullOrWhiteSpace(derivedDenMenuId) && TryGetFirstShopMenuId(shopMenusDoc, out var firstMenuId))
                                derivedDenMenuId = firstMenuId;

                            var shopMenusPath = Path.Combine(_rootFolderPath, $"ShopMenus+_{PackName}.xml");

                            var shopMenusDocToWrite = shopMenusDoc;

                            if (File.Exists(shopMenusPath))
                            {
                                try
                                {
                                    var existing = XDocument.Load(shopMenusPath, LoadOptions.None);
                                    var merger = new LSR.XmlHelper.Core.Services.Builders.ShopMenusMergeService();
                                    shopMenusDocToWrite = merger.MergeNew(existing, shopMenusDoc);
                                }
                                catch
                                {
                                    shopMenusDocToWrite = shopMenusDoc;
                                }
                            }

                            xmlService.SaveToFile(shopMenusPath, xmlService.Format(shopMenusDocToWrite.ToString()));

                            shopMenusWritten = true;

                            SetTask("Create/Update ShopMenus", true, clonedMenusCount > 0 ? "Done" : "Done");
                        }
                        else
                        {
                            SetTask("Create/Update ShopMenus", false, "None found");
                        }
                    }
                    else
                    {
                        SetTask("Create/Update ShopMenus", false, "Skipped");
                    }
                }
                else
                {
                    SetTask("Assign Dealer Menu Group", false, "Skipped");
                    SetTask("Create/Update ShopMenus", false, "Skipped");
                }
            }
            else
            {
                SetTask("Assign Dealer Menu Group", false, "Skipped");
                SetTask("Create/Update ShopMenus", false, "Skipped");
            }

            if (GenerateDenInventoryMenu && DenInventoryItems.Count > 0 && !string.IsNullOrWhiteSpace(PackName))
            {
                var shopMenusPath = Path.Combine(_rootFolderPath, $"ShopMenus+_{PackName}.xml");
                XDocument? existing = null;

                if (File.Exists(shopMenusPath))
                {
                    try
                    {
                        existing = XDocument.Load(shopMenusPath, LoadOptions.None);
                    }
                    catch
                    {
                        existing = null;
                    }
                }

                var denInventoryBuilder = new LSR.XmlHelper.Core.Services.Builders.DenInventoryShopMenuBuilderService();
                var groupId = $"{PackName}_DenInventoryGroup";
                var menuId = $"{PackName}_DenInventory";
                var groupName = $"{PackName} Den Inventory";
                var menuName = $"{PackName} Den Inventory";

                var models = DenInventoryItems.Select(x => x.ToModel()).Where(x => !string.IsNullOrWhiteSpace(x.ModItemName)).ToList();
                removedInvalidDenWeaponItems = _weaponModelValidationService.RemoveInvalidWeaponItems(_rootFolderPath, models);
                var merged = denInventoryBuilder.CreateOrMergeInto(existing, groupId, groupName, menuId, menuName, models);

                xmlService.SaveToFile(shopMenusPath, xmlService.Format(merged.ToString()));
                shopMenusWritten = true;

                if (string.IsNullOrWhiteSpace(derivedDenMenuId))
                    derivedDenMenuId = menuId;
            }


            if (IncludeWeapons)
            {
                var meleeId = "";
                var sideArmsId = "";
                var longGunsId = "";

                if (UseSourceGangWeaponsLoadouts)
                {
                    meleeId = result.SourceMeleeWeaponsId;
                    sideArmsId = result.SourceSideArmsId;
                    longGunsId = result.SourceLongGunsId;

                    SourceMeleeWeaponsId = meleeId;
                    SourceSideArmsId = sideArmsId;
                    SourceLongGunsId = longGunsId;
                }
                else
                {
                    meleeId = MeleeWeaponsId;
                    sideArmsId = SideArmsId;
                    longGunsId = LongGunsId;
                }

                var gangNode = result.GangsDoc.Descendants("Gang").FirstOrDefault();
                if (gangNode is not null)
                {
                    if (CloneWeaponsIntoPack)
                    {
                        var weaponsCloneWarning = GetWeaponsCloneWarning(UseSourceGangWeaponsLoadouts, meleeId, sideArmsId, longGunsId);

                        if (UseSourceGangWeaponsLoadouts)
                        {
                            SetTask("Create/Update IssuableWeapons", false, weaponsCloneWarning);
                        }
                        else
                        {
                            var desiredIdsBySourceId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                            if (!string.IsNullOrWhiteSpace(meleeId))
                                desiredIdsBySourceId[meleeId] = $"{meleeId}_{PackName}";

                            if (!string.IsNullOrWhiteSpace(sideArmsId))
                                desiredIdsBySourceId[sideArmsId] = $"{sideArmsId}_{PackName}";

                            if (!string.IsNullOrWhiteSpace(longGunsId))
                                desiredIdsBySourceId[longGunsId] = $"{longGunsId}_{PackName}";

                            var cloner = new LSR.XmlHelper.Core.Services.Builders.IssuableWeaponsCloneBuilderService();
                            var (weaponsDoc, clonedIdsBySourceId, clonedGroupsCount) = cloner.CloneGroups(_rootFolderPath, desiredIdsBySourceId);

                            if (clonedIdsBySourceId.Count > 0)
                            {
                                if (!string.IsNullOrWhiteSpace(meleeId) && clonedIdsBySourceId.TryGetValue(meleeId, out var clonedMeleeId))
                                    meleeId = clonedMeleeId;

                                if (!string.IsNullOrWhiteSpace(sideArmsId) && clonedIdsBySourceId.TryGetValue(sideArmsId, out var clonedSideArmsId))
                                    sideArmsId = clonedSideArmsId;

                                if (!string.IsNullOrWhiteSpace(longGunsId) && clonedIdsBySourceId.TryGetValue(longGunsId, out var clonedLongGunsId))
                                    longGunsId = clonedLongGunsId;

                                var weaponsPath = Path.Combine(_rootFolderPath, $"IssuableWeapons+_{PackName}.xml");

                                var weaponsDocToWrite = weaponsDoc;

                                if (File.Exists(weaponsPath))
                                {
                                    try
                                    {
                                        var existing = XDocument.Load(weaponsPath, LoadOptions.None);
                                        var merger = new LSR.XmlHelper.Core.Services.Editing.XmlArrayReplaceByKeyService();
                                        weaponsDocToWrite = merger.MergeReplace(existing, weaponsDoc, "IssuableWeaponsGroup", "IssuableWeaponsID");
                                    }
                                    catch
                                    {
                                        weaponsDocToWrite = weaponsDoc;
                                    }
                                }

                                xmlService.SaveToFile(weaponsPath, xmlService.Format(weaponsDocToWrite.ToString()));

                                issuableWeaponsWritten = true;

                                var status = string.IsNullOrWhiteSpace(weaponsCloneWarning) ? "Done" : $"Done ({weaponsCloneWarning})";
                                SetTask("Create/Update IssuableWeapons", true, status);
                            }
                            else
                            {
                                var status = string.IsNullOrWhiteSpace(weaponsCloneWarning) ? "None found" : weaponsCloneWarning;
                                SetTask("Create/Update IssuableWeapons", false, status);
                            }
                        }
                    }
                    else
                    {
                        SetTask("Create/Update IssuableWeapons", false, "Skipped");
                    }

                    SetOrUpdateGangField(gangNode, "MeleeWeaponsID", meleeId);
                    SetOrUpdateGangField(gangNode, "SideArmsID", sideArmsId);
                    SetOrUpdateGangField(gangNode, "LongGunsID", longGunsId);

                    SetTask("Create/Assign Weapons Loadouts", true, "Done");
                }
                else
                {
                    SetTask("Create/Assign Weapons Loadouts", false, "Skipped");
                    SetTask("Create/Update IssuableWeapons", false, "Skipped");
                }
            }
            else
            {
                SetTask("Create/Assign Weapons Loadouts", false, "Skipped");
                SetTask("Create/Update IssuableWeapons", false, "Skipped");
            }

            if (IncludeRelationships)
            {
                var gangNode = result.GangsDoc.Descendants("Gang").FirstOrDefault();
                if (gangNode is not null)
                {
                    if (UseSourceGangEnemyGangs)
                    {
                        var currentEnemies = gangNode.Element("EnemyGangs")?.Elements().Select(e => (e.Value ?? "").Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                            ?? Array.Empty<string>();

                        SourceEnemyGangsSummary = currentEnemies.Length == 0 ? "None" : string.Join(", ", currentEnemies);

                        SetTask("Configure Relationships", true, "Done");
                    }
                    else
                    {
                        var enemyIds = SelectedEnemyGangs
                            .Select(x => x.Id)
                            .Where(v => !string.IsNullOrWhiteSpace(v))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        SetEnemyGangs(gangNode, enemyIds);

                        SourceEnemyGangsSummary = enemyIds.Length == 0 ? "None" : string.Join(", ", enemyIds);

                        SetTask("Configure Relationships", true, "Done");
                    }
                }
                else
                {
                    SetTask("Configure Relationships", false, "Skipped");
                }
            }
            else
            {
                SetTask("Configure Relationships", false, "Skipped");
            }

            var advancedGangNode = result.GangsDoc.Descendants("Gang").FirstOrDefault();
            if (advancedGangNode is not null)
                ApplyAdvancedGangSettingsToGangNode(advancedGangNode);

            var gangsDocToWrite = result.GangsDoc;

            if (File.Exists(gangsPath))
            {
                try
                {
                    var existing = XDocument.Load(gangsPath, LoadOptions.None);
                    var merger = new LSR.XmlHelper.Core.Services.Editing.XmlArrayReplaceByKeyService();
                    gangsDocToWrite = merger.MergeReplace(existing, result.GangsDoc, "Gang", "ID");
                }
                catch
                {
                    gangsDocToWrite = result.GangsDoc;
                }
            }

            ApplyGangColorToResult(gangsDocToWrite);

            xmlService.SaveToFile(gangsPath, xmlService.Format(gangsDocToWrite.ToString()));

            editedFiles.Add(gangsPath);

            ApplyDispatchablePeopleEdits(peopleDocToWrite);

            var peopleDocFinal = peopleDocToWrite;

            if (File.Exists(peoplePath))
            {
                try
                {
                    var existing = XDocument.Load(peoplePath, LoadOptions.None);
                    var merger = new LSR.XmlHelper.Core.Services.Editing.XmlArrayReplaceByKeyService();
                    peopleDocFinal = merger.MergeReplace(existing, peopleDocToWrite, "DispatchablePersonGroup", "DispatchablePersonGroupID");
                }
                catch
                {
                    peopleDocFinal = peopleDocToWrite;
                }
            }

            xmlService.SaveToFile(peoplePath, xmlService.Format(peopleDocFinal.ToString()));

            if (IncludeVehicles)
            {
                var requiredGroups = PossibleVehicleSpawns
                    .Where(x => x.ForceVehicleGroup && !string.IsNullOrWhiteSpace(x.RequiredVehicleGroup))
                    .Select(x => x.RequiredVehicleGroup.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (requiredGroups.Count > 0)
                {
                    var augmenter = new LSR.XmlHelper.Core.Services.Builders.GangDispatchableVehiclesAugmenterService();
                    augmenter.AugmentWithVehicleGroups(_rootFolderPath, result.VehiclesDoc, result.VehicleGroupId, requiredGroups);
                }

                var selections = new List<(string ModelName, string VariantKey, int? OverridePrimaryColorId, int? OverrideSecondaryColorId, IReadOnlyList<int> OverrideLiveries)>();

                foreach (var item in CustomDispatchableVehicleModelsToAdd)
                {
                    var model = (item.ModelName ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(model))
                        continue;

                    int? pri = null;
                    if (item.TryGetOverridePrimaryColorId(out var priValue))
                        pri = priValue;

                    int? sec = null;
                    if (item.TryGetOverrideSecondaryColorId(out var secValue))
                        sec = secValue;

                    var liveries = item.GetOverrideLiveryIds();

                    selections.Add((model, item.VariantKey ?? "", pri, sec, liveries));
                }

                if (selections.Count > 0)
                {
                    var adder = new LSR.XmlHelper.Core.Services.Builders.GangDispatchableVehicleModelsAdderService();
                    var missing = adder.AddSelections(_rootFolderPath, result.VehiclesDoc, result.VehicleGroupId, selections);
                    missingCustomVehicleModels.AddRange(missing);
                }
            }

            var vehiclesWrittenPath = vehiclesPath;
            var vehiclesDocToWrite = result.VehiclesDoc;

            if (File.Exists(vehiclesPath))
            {
                try
                {
                    var existing = XDocument.Load(vehiclesPath, LoadOptions.None);
                    var merger = new LSR.XmlHelper.Core.Services.Editing.XmlArrayReplaceByKeyService();
                    vehiclesDocToWrite = merger.MergeReplace(existing, result.VehiclesDoc, "DispatchableVehicleGroup", "DispatchableVehicleGroupID");
                }
                catch
                {
                    vehiclesDocToWrite = result.VehiclesDoc;
                }
            }

            xmlService.SaveToFile(vehiclesPath, xmlService.Format(vehiclesDocToWrite.ToString()));

            var densWritten = false;

            if (IncludeTerritories && IncludeZones && SelectedZones.Count > 0)
            {
                var zoneNames = SelectedZones.Select(z => z.InternalGameName).ToArray();
                var territoriesPath = Path.Combine(_rootFolderPath, $"GangTerritories+_{PackName}.xml");

                var territoriesBuilder = new LSR.XmlHelper.Core.Services.Builders.GangTerritoriesBuilderService();
                XDocument territoriesDoc;

                if (File.Exists(territoriesPath))
                {
                    try
                    {
                        territoriesDoc = XDocument.Load(territoriesPath, LoadOptions.None);
                    }
                    catch
                    {
                        territoriesDoc = territoriesBuilder.Build(_rootFolderPath, result.GangId, Array.Empty<string>());
                    }

                    if (territoriesDoc.Root is null)
                        territoriesDoc = territoriesBuilder.Build(_rootFolderPath, result.GangId, Array.Empty<string>());

                    var replacer = new LSR.XmlHelper.Core.Services.Editing.GangTerritoriesReplaceService();
                    var replaced = replacer.ReplaceForGang(territoriesDoc, result.GangId, zoneNames);

                    if (!replaced)
                        territoriesDoc = territoriesBuilder.Build(_rootFolderPath, result.GangId, zoneNames);
                }
                else
                {
                    territoriesDoc = territoriesBuilder.Build(_rootFolderPath, result.GangId, zoneNames);
                }

                xmlService.SaveToFile(territoriesPath, xmlService.Format(territoriesDoc.ToString()));

                SetTask("Create/Update GangTerritories", true, "Done");
            }
            else
            {
                SetTask("Create/Update GangTerritories", false, "Skipped");
            }

            if (IncludeTerritoryMenus && IncludeTerritories && IncludeZones && SelectedZones.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(TerritoryDealerMenuContainerId) || string.IsNullOrWhiteSpace(TerritoryCustomerMenuContainerId))
                {
                    SetTask("Create/Update Territory menus", false, "Missing IDs");
                }
                else
                {
                    var upserter = new LSR.XmlHelper.Core.Services.Builders.Zones.ZonesUpsertService();
                    var updatedFiles = upserter.UpsertZonesIntoWinnerFile(
                        _rootFolderPath,
                        SelectedZones.Select(z => z.InternalGameName).ToArray(),
                        CustomTerritoriesToAdd.Select(x => x.Definition).ToArray(),
                        TerritoryDealerMenuContainerId,
                        TerritoryCustomerMenuContainerId,
                        !IsEditExistingGang);

                    zonesWritten = updatedFiles.Count > 0;

                    if (CloneTerritoryMenusIntoPack)
                    {
                        var cloner = new LSR.XmlHelper.Core.Services.Builders.ShopMenusCloneBuilderService();
                        var merger = new LSR.XmlHelper.Core.Services.Builders.ShopMenusMergeService();

                        var shopMenusPath = Path.Combine(_rootFolderPath, $"ShopMenus+_{PackName}.xml");

                        XDocument? existingShopMenus = null;
                        if (File.Exists(shopMenusPath))
                        {
                            try
                            {
                                existingShopMenus = XDocument.Load(shopMenusPath, LoadOptions.None);
                            }
                            catch
                            {
                                existingShopMenus = null;
                            }
                        }

                        var (dealerDoc, dealerClonedGroupId, _) = cloner.CloneDealerGroup(_rootFolderPath, TerritoryDealerMenuContainerId, $"{TerritoryDealerMenuContainerId}_{PackName}", PackName);
                        var (customerDoc, customerClonedGroupId, _) = cloner.CloneDealerGroup(_rootFolderPath, TerritoryCustomerMenuContainerId, $"{TerritoryCustomerMenuContainerId}_{PackName}", PackName);

                        var mergedShopMenus = existingShopMenus ?? dealerDoc;
                        if (existingShopMenus is null)
                            mergedShopMenus = merger.MergeNew(mergedShopMenus, dealerDoc);

                        mergedShopMenus = merger.MergeNew(mergedShopMenus, customerDoc);

                        xmlService.SaveToFile(shopMenusPath, xmlService.Format(mergedShopMenus.ToString()));
                        shopMenusWritten = true;
                    }

                    SetTask("Create/Update Territory menus", true, "Done");
                }
            }
            else
            {
                SetTask("Create/Update Territory menus", false, "Skipped");
            }

            SetTask("Create/Update Gangs entry", true, "Done");

            if (IncludeDens)
            {
                var densBuilder = new LSR.XmlHelper.Core.Services.Builders.GangDensBuilderService();
                var generatedDenMenuId = "";
                var isGeneratedDenMenu = false;

                if (GenerateDenInventoryMenu && DenInventoryItems.Count > 0)
                {
                    generatedDenMenuId = $"{PackName}_DenInventory";
                    isGeneratedDenMenu = true;
                }

                if (!string.IsNullOrWhiteSpace(generatedDenMenuId))
                    DenMenuId = generatedDenMenuId;

                var denMenuId = (DenMenuId ?? "").Trim();
                if (string.IsNullOrWhiteSpace(denMenuId) && !string.IsNullOrWhiteSpace(derivedDenMenuId))
                    denMenuId = derivedDenMenuId;

                if (!isGeneratedDenMenu && CloneDealerMenusIntoPack && !string.IsNullOrWhiteSpace(denMenuId) && !string.IsNullOrWhiteSpace(PackName))
                {
                    var suffix = "_" + PackName.Trim();
                    if (!denMenuId.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        denMenuId = denMenuId + suffix;
                }

                var denBannerImagePath = (DenBannerImagePath ?? "").Trim();

                if (CreateNewDen)
                {
                    var warning = GetNewDenWarning(NewDenName, NewDenX, NewDenY, NewDenZ, NewDenHeading);
                    if (!string.IsNullOrWhiteSpace(warning))
                    {
                        SetTask("Create/Update Gang Dens", false, warning);
                    }
                    else if (!TryGetNewDenValues(NewDenX, NewDenY, NewDenZ, NewDenHeading, out var x, out var y, out var z, out var heading))
                    {
                        SetTask("Create/Update Gang Dens", false, "Blocked");
                    }
                    else
                    {
                        var blipSettings = new LSR.XmlHelper.Core.Models.GangDenBlipSettings
                        {
                            IsBlipEnabled = NewDenIsBlipEnabled,
                            MapIcon = NewDenMapIcon,
                            MapIconColorString = NewDenMapIconColorString,
                            MapIconScale = NewDenMapIconScale,
                            MapIconRadius = NewDenMapIconRadius,
                            MapOpenIconAlpha = NewDenMapOpenIconAlpha,
                            MapClosedIconAlpha = NewDenMapClosedIconAlpha
                        };

                        var (densDoc, createdCount) = densBuilder.BuildNewDen(_rootFolderPath, result.GangId, NewDenName, x, y, z, heading, blipSettings, denMenuId, denBannerImagePath);
                        ApplyDenPossiblePedSpawnEdits(densDoc, result.GangId);
                        ApplyDenPossibleVehicleSpawnEdits(densDoc, result.GangId);

                        var densPath = Path.Combine(_rootFolderPath, $"Locations+_{PackName}.xml");

                        var densDocToWrite = densDoc;

                        if (File.Exists(densPath))
                        {
                            try
                            {
                                var existing = XDocument.Load(densPath, LoadOptions.None);
                                var merger = new LSR.XmlHelper.Core.Services.Editing.GangDensReplaceService();
                                densDocToWrite = merger.MergeReplaceForGang(existing, densDoc, result.GangId);
                            }
                            catch
                            {
                                densDocToWrite = densDoc;
                            }
                        }

                        xmlService.SaveToFile(densPath, xmlService.Format(densDocToWrite.ToString()));

                        densWritten = true;

                        SetTask("Create/Update Gang Dens", createdCount > 0, createdCount > 0 ? "Done" : "Skipped");
                    }
                }
                else
                {
                    var (densDoc, clonedCount) = densBuilder.BuildClone(_rootFolderPath, CloneFromGangId, result.GangId, KeepSourceDenTypeName, denMenuId, denBannerImagePath);
                    ApplyDenPossiblePedSpawnEdits(densDoc, result.GangId);
                    ApplyDenPossibleVehicleSpawnEdits(densDoc, result.GangId);

                    var densPath = Path.Combine(_rootFolderPath, $"Locations+_{PackName}.xml");

                    var densDocToWrite = densDoc;

                    if (File.Exists(densPath))
                    {
                        try
                        {
                            var existing = XDocument.Load(densPath, LoadOptions.None);
                            var merger = new LSR.XmlHelper.Core.Services.Editing.GangDensReplaceService();
                            densDocToWrite = merger.MergeReplaceForGang(existing, densDoc, result.GangId);
                        }
                        catch
                        {
                            densDocToWrite = densDoc;
                        }
                    }

                    xmlService.SaveToFile(densPath, xmlService.Format(densDocToWrite.ToString()));

                    densWritten = true;

                    SetTask("Create/Update Gang Dens", clonedCount > 0, clonedCount > 0 ? "Done" : "None found");
                }
            }
            else
            {
                SetTask("Create/Update Gang Dens", false, "Skipped");
            }

            SetTask("Create/Update DispatchablePeople group", IncludePeople && (UseSourceGangPeopleGroup || SelectedDispatchablePeopleGroup is not null), !IncludePeople ? "Skipped" : (UseSourceGangPeopleGroup || SelectedDispatchablePeopleGroup is not null ? "Done" : "Blocked"));
            SetTask("Create/Update DispatchableVehicles group", IncludeVehicles, IncludeVehicles ? "Done" : "Skipped");
            SetTask("Write additive XML files", true, "Done");

            var missingOutputFiles = new System.Collections.Generic.List<string>();
            var createdFiles = new System.Collections.Generic.List<string>();

            BuildOutputFiles.Clear();

            void AddOutputIfExists(string fullPath)
            {
                if (string.IsNullOrWhiteSpace(fullPath))
                    return;

                if (!File.Exists(fullPath))
                {
                    missingOutputFiles.Add(fullPath);
                    return;
                }

                createdFiles.Add(Path.GetFileName(fullPath));
                BuildOutputFiles.Add(new BuildOutputFileViewModel(Path.GetFileName(fullPath), fullPath));
            }

            AddOutputIfExists(gangsPath);
            AddOutputIfExists(peoplePath);
            AddOutputIfExists(vehiclesPath);

            if (IncludeTerritories && IncludeZones && SelectedZones.Count > 0)
            {
                var territoriesPath = Path.Combine(_rootFolderPath, $"GangTerritories+_{PackName}.xml");
                AddOutputIfExists(territoriesPath);
            }

            if (zonesWritten)
            {
                var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
                var resolved = resolver.ResolveZones(_rootFolderPath, "Default");
                if (!string.IsNullOrWhiteSpace(resolved.BasePath))
                    AddOutputIfExists(resolved.BasePath);
            }

            if (shopMenusWritten)
            {
                var shopMenusPath = Path.Combine(_rootFolderPath, $"ShopMenus+_{PackName}.xml");
                AddOutputIfExists(shopMenusPath);
            }

            if (issuableWeaponsWritten)
            {
                var weaponsPath = Path.Combine(_rootFolderPath, $"IssuableWeapons+_{PackName}.xml");
                AddOutputIfExists(weaponsPath);
            }

            if (densWritten)
            {
                var densPath = Path.Combine(_rootFolderPath, $"Locations+_{PackName}.xml");
                AddOutputIfExists(densPath);

                var bannerRel = (DenBannerImagePath ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(bannerRel))
                {
                    bannerRel = bannerRel.Replace("/", "\\").TrimStart('\\');

                    if (bannerRel.StartsWith("images\\", StringComparison.OrdinalIgnoreCase))
                        bannerRel = bannerRel.Substring("images\\".Length);

                    var bannerFullPath = Path.Combine(_rootFolderPath, "images", bannerRel);
                    AddOutputIfExists(bannerFullPath);
                }
            }

            var gangNodeForSummary = result.GangsDoc.Descendants("Gang").FirstOrDefault();
            var finalDealerMenuGroupId = ((string?)gangNodeForSummary?.Element("DealerMenuGroupID") ?? "").Trim();
            var finalMeleeWeaponsId = ((string?)gangNodeForSummary?.Element("MeleeWeaponsID") ?? "").Trim();
            var finalSideArmsId = ((string?)gangNodeForSummary?.Element("SideArmsID") ?? "").Trim();
            var finalLongGunsId = ((string?)gangNodeForSummary?.Element("LongGunsID") ?? "").Trim();

            BuildSummaryText =
            $"New IDs:\r\n" +
            $"GangID: {result.GangId}\r\n" +
            $"PeopleGroupID: {result.PeopleGroupId}\r\n" +
            $"VehicleGroupID: {result.VehicleGroupId}\r\n" +
            $"\r\n" +
            $"Final references written into the Gang:\r\n" +
            $"DealerMenuGroupID: {finalDealerMenuGroupId}\r\n" +
            $"MeleeWeaponsID: {finalMeleeWeaponsId}\r\n" +
            $"SideArmsID: {finalSideArmsId}\r\n" +
            $"LongGunsID: {finalLongGunsId}\r\n" +
            $"\r\n" +
            $"Files created:\r\n{string.Join("\r\n", BuildOutputFiles.Select(f => f.FileName))}" +
            (backedUpFiles.Count > 0
                ? $"\r\n\r\nBackups created:\r\n{string.Join("\r\n", backedUpFiles)}"
                : "") +
            (editedFiles.Count > 0
                ? $"\r\n\r\nMain XML files edited:\r\n{string.Join("\r\n", editedFiles)}"
                : "") +
            (removedInvalidDenWeaponItems.Count > 0
                ? $"\r\n\r\nDen inventory: removed invalid weapon items (missing ModelName in Weapons.xml):\r\n{string.Join("\r\n", removedInvalidDenWeaponItems)}"
                : "") +
            (missingCustomVehicleModels.Count > 0
                ? $"\r\n\r\nVehicles: could not add these ModelName entries (not found in any DispatchableVehicles*.xml and no template vehicle existed in the cloned gang group):\r\n{string.Join("\r\n", missingCustomVehicleModels)}"
                : "");



            HasBuildSummary = true;

            SetTask("Show summary + next steps", true, "Done");

            System.Windows.MessageBox.Show(
                $"Created additive files:\r\n\r\n{string.Join("\r\n", createdFiles)}\r\n\r\nMissing (NOT written):\r\n\r\n{(missingOutputFiles.Count == 0 ? "(none)" : string.Join("\r\n", missingOutputFiles))}\r\n\r\nNew IDs:\r\nGangID: {result.GangId}\r\nPeopleGroupID: {result.PeopleGroupId}\r\nVehicleGroupID: {result.VehicleGroupId}\r\n\r\nSee the Build Summary panel for details + Open buttons.", "Gang Builder",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        private void BuildPack_EditMode()
        {
            if (SelectedEditGang is null)
            {
                System.Windows.MessageBox.Show("No gang is selected to edit.", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_rootFolderPath) || !Directory.Exists(_rootFolderPath))
            {
                System.Windows.MessageBox.Show("Root folder is not set or does not exist.", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gangId = (SelectedEditGang.Id ?? "").Trim();
            if (string.IsNullOrWhiteSpace(gangId))
            {
                System.Windows.MessageBox.Show("Selected gang has an empty ID.", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
            var gangsPath = resolver.ResolveGangFile(_rootFolderPath, gangId) ?? Path.Combine(_rootFolderPath, "Gangs.xml");
            if (!File.Exists(gangsPath))
            {
                System.Windows.MessageBox.Show("No gangs config file was found to edit (Gangs.xml or any Gangs+_*.xml).", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var peoplePath = Path.Combine(_rootFolderPath, "DispatchablePeople.xml");
            var vehiclesPath = Path.Combine(_rootFolderPath, "DispatchableVehicles.xml");
            var territoriesPath = Path.Combine(_rootFolderPath, "GangTerritories.xml");
            var locationsPath = Path.Combine(_rootFolderPath, "Locations.xml");
            var shopMenusPath = Path.Combine(_rootFolderPath, "ShopMenus.xml");

            var willEditRelationships = IncludeRelationships && !UseSourceGangEnemyGangs;
            var willEditDen = IncludeDens && (CreateNewDen || IsEditExistingGang);
            var willEditDenInventory = IncludeDens && GenerateDenInventoryMenu && DenInventoryItems.Count > 0 && !string.IsNullOrWhiteSpace(DenMenuId);
            var willEditDealerMenus = IncludeDealerMenus && !UseSourceGangDealerMenuGroup;
            var willEditTerritories = IncludeTerritories && IncludeZones;
            var willEditPeople = IncludePeople && !UseSourceGangPeopleGroup && SelectedDispatchablePeopleGroup is not null && DispatchablePeopleEntries.Count > 0;
            var vehicleGroupIdForEdits = (SelectedEditGangVehicleGroupId ?? "").Trim();

            var currentVehicleModels = CustomDispatchableVehicleModelsToAdd
                .Select(x => (x.ModelName ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var willEditVehicles = IncludeVehicles
                && !string.IsNullOrWhiteSpace(vehicleGroupIdForEdits)
                && (!IsEditExistingGang
                    ? currentVehicleModels.Count > 0
                    : currentVehicleModels.Except(_editVehicleModelsOriginal).Any() || _editVehicleModelsOriginal.Except(currentVehicleModels).Any());

            if (willEditPeople && SelectedDispatchablePeopleGroup is not null)
            {
                var resolvedPeople = resolver.ResolveDispatchablePeopleFile(_rootFolderPath, (SelectedDispatchablePeopleGroup.Id ?? "").Trim());
                if (!string.IsNullOrWhiteSpace(resolvedPeople))
                    peoplePath = resolvedPeople;
            }

            if (willEditVehicles)
            {
                var resolvedVehicles = resolver.ResolveDispatchableVehiclesFile(_rootFolderPath, (SelectedEditGangVehicleGroupId ?? "").Trim());
                if (!string.IsNullOrWhiteSpace(resolvedVehicles))
                    vehiclesPath = resolvedVehicles;
            }

            if (willEditDen)
            {
                string? resolvedLocations;

                if (!CreateNewDen)
                {
                    var trimmedDenName = (NewDenName ?? "").Trim();

                    resolvedLocations =
                        !string.IsNullOrWhiteSpace(trimmedDenName)
                            ? resolver.ResolveLocationsFileForGangDen(_rootFolderPath, gangId, trimmedDenName)
                            : resolver.ResolveLocationsFileForGangDens(_rootFolderPath, gangId);
                }
                else
                {
                    resolvedLocations = resolver.ResolveLocationsFileForGangDens(_rootFolderPath, gangId);
                }

                if (!string.IsNullOrWhiteSpace(resolvedLocations))
                    locationsPath = resolvedLocations;
            }

            if (willEditDenInventory || willEditDealerMenus)
            {
                var resolvedShopMenus = resolver.ResolveShopMenusFile(_rootFolderPath, (DenMenuId ?? "").Trim(), (TerritoryDealerMenuContainerId ?? "").Trim(), (TerritoryCustomerMenuContainerId ?? "").Trim());
                if (!string.IsNullOrWhiteSpace(resolvedShopMenus))
                    shopMenusPath = resolvedShopMenus;
            }

            var filesEdited = new List<string>();
            var backedUpFiles = new List<string>();
            var changes = new List<string>();

            var pendingWrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var filesToEdit = new List<string>();

            filesToEdit.Add(gangsPath);

            if (willEditPeople && File.Exists(peoplePath))
                filesToEdit.Add(peoplePath);

            if (willEditVehicles && File.Exists(vehiclesPath))
                filesToEdit.Add(vehiclesPath);

            if (willEditTerritories)
            {
                var lookup = new LSR.XmlHelper.Core.Services.Builders.GangTerritoryZoneLookupService();

                var zonesBefore = lookup
                    .GetZoneInternalNamesForGang(_rootFolderPath, gangId)
                    .Select(z => (z ?? "").Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var zonesAfter = SelectedZones
                    .Select(z => (z.InternalGameName ?? "").Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var impactedZones = new HashSet<string>(zonesBefore, StringComparer.OrdinalIgnoreCase);
                foreach (var z in zonesAfter)
                    impactedZones.Add(z);

                if (impactedZones.Count > 0)
                {
                    var winnerResolver = new LSR.XmlHelper.Core.Services.Editing.GangTerritoriesWinnerFileByZoneResolverService();
                    var resolution = winnerResolver.Resolve(_rootFolderPath, "Default");

                    var territoryFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var zone in impactedZones)
                    {
                        var winnerPath = resolution.WinnerFileByZone.TryGetValue(zone, out var p) ? p : resolution.FallbackPath;

                        if (string.IsNullOrWhiteSpace(winnerPath))
                            continue;

                        if (File.Exists(winnerPath))
                            territoryFiles.Add(winnerPath);
                    }

                    foreach (var f in territoryFiles)
                        filesToEdit.Add(f);
                }
            }

            if (willEditDen && File.Exists(locationsPath))
                filesToEdit.Add(locationsPath);

            if ((willEditDenInventory || willEditDealerMenus) && File.Exists(shopMenusPath))
                filesToEdit.Add(shopMenusPath);

            var owner = System.Windows.Application.Current.Windows
                .OfType<LSR.XmlHelper.Wpf.Views.Builders.GangBuilderWindow>()
                .FirstOrDefault();

            var prompt = new LSR.XmlHelper.Wpf.Views.Dialogs.EditModeBackupPromptWindow(
                filesToEdit
                    .Select(Path.GetFileName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray())
            {
                Owner = owner
            };

            var promptResult = prompt.ShowDialog();
            if (promptResult != true)
                return;

            if (prompt.Action == LSR.XmlHelper.Wpf.Views.Dialogs.EditModeBackupPromptAction.Backup)
            {
                var root = new LSR.XmlHelper.Core.Services.XmlHelperRootService();
                var backupRequest = new LSR.XmlHelper.Wpf.Services.XmlBackupRequestService();

                foreach (var fileToBackup in filesToEdit.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!File.Exists(fileToBackup))
                        continue;

                    var backupFolder = root.GetOrCreateSubfolder(fileToBackup, "BackupXMLs");

                    var before = Directory.Exists(backupFolder)
                        ? Directory.GetFiles(backupFolder, "*.xml").ToHashSet(StringComparer.OrdinalIgnoreCase)
                        : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (!backupRequest.TryBackup(fileToBackup, out var backupError))
                    {
                        System.Windows.MessageBox.Show(
                            "Backup failed:\r\n" + (backupError ?? "Unknown error") + "\r\n\r\nEdit was cancelled to keep your files safe.",
                            "Gang Builder",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    var after = Directory.Exists(backupFolder)
                        ? Directory.GetFiles(backupFolder, "*.xml")
                        : Array.Empty<string>();

                    var createdBackup = after
                        .Where(x => !before.Contains(x))
                        .OrderByDescending(File.GetLastWriteTime)
                        .FirstOrDefault();

                    backedUpFiles.Add(createdBackup ?? backupFolder);
                }
            }

            var xmlService = new LSR.XmlHelper.Core.Services.XmlDocumentService();

            XDocument gangsDoc;
            try
            {
                gangsDoc = XDocument.Load(gangsPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to load " + Path.GetFileName(gangsPath) + ":\r\n" + ex.Message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gangsBefore = gangsDoc.ToString(SaveOptions.DisableFormatting);
            var gangNode = gangsDoc
                .Descendants("Gang")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase));

            if (gangNode is null)
            {
                System.Windows.MessageBox.Show($"Could not find Gang with ID '{gangId}' in {Path.GetFileName(gangsPath)}.", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newFullName = (NewGangFullName ?? "").Trim();

            var oldFullName = ((string?)gangNode.Element("FullName") ?? "").Trim();
            var oldShortName = ((string?)gangNode.Element("ShortName") ?? "").Trim();
            var oldContactName = ((string?)gangNode.Element("ContactName") ?? "").Trim();
            var oldMemberName = ((string?)gangNode.Element("MemberName") ?? "").Trim();

            if (!string.Equals(oldFullName, newFullName, StringComparison.Ordinal))
                changes.Add($"FullName: '{oldFullName}' -> '{newFullName}'");
            SetOrUpdateGangField(gangNode, "FullName", newFullName);

            if (gangNode.Element("ShortName") is not null)
            {
                if (!string.Equals(oldShortName, newFullName, StringComparison.Ordinal))
                    changes.Add($"ShortName: '{oldShortName}' -> '{newFullName}'");
                SetOrUpdateGangField(gangNode, "ShortName", newFullName);
            }

            if (!string.Equals(oldContactName, newFullName, StringComparison.Ordinal))
                changes.Add($"ContactName: '{oldContactName}' -> '{newFullName}'");
            SetOrUpdateGangField(gangNode, "ContactName", newFullName);

            var newMemberName = $"{newFullName} Member";
            if (!string.Equals(oldMemberName, newMemberName, StringComparison.Ordinal))
                changes.Add($"MemberName: '{oldMemberName}' -> '{newMemberName}'");
            SetOrUpdateGangField(gangNode, "MemberName", newMemberName);

            var desiredColorString = (GangColorString ?? "").Trim();
            var desiredColorPrefix = (GangColorPrefix ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(desiredColorString) || !string.IsNullOrWhiteSpace(desiredColorPrefix))
            {
                var oldColorString = ((string?)gangNode.Element("ColorString") ?? "").Trim();
                var oldColorPrefix = ((string?)gangNode.Element("ColorPrefix") ?? "").Trim();

                if (!string.Equals(oldColorString, desiredColorString, StringComparison.Ordinal))
                    changes.Add($"ColorString: '{oldColorString}' -> '{desiredColorString}'");

                if (!string.Equals(oldColorPrefix, desiredColorPrefix, StringComparison.Ordinal))
                    changes.Add($"ColorPrefix: '{oldColorPrefix}' -> '{desiredColorPrefix}'");

                SetOrUpdateGangField(gangNode, "ColorString", desiredColorString);
                SetOrUpdateGangField(gangNode, "ColorPrefix", desiredColorPrefix);
                ApplyAdvancedGangSettingsToGangNode(gangNode);
            }

            if (IncludePeople && !UseSourceGangPeopleGroup && SelectedDispatchablePeopleGroup is not null)
            {
                var desiredPeopleGroupId = (SelectedDispatchablePeopleGroup.Id ?? "").Trim();

                var peopleField = gangNode.Element("PeopleGroupID") is not null
                    ? "PeopleGroupID"
                    : gangNode.Element("PersonnelID") is not null
                        ? "PersonnelID"
                        : "PeopleGroupID";

                var oldPeopleGroupId = ((string?)gangNode.Element(peopleField) ?? "").Trim();

                if (!string.Equals(oldPeopleGroupId, desiredPeopleGroupId, StringComparison.Ordinal))
                    changes.Add($"PeopleGroupID: '{oldPeopleGroupId}' -> '{desiredPeopleGroupId}'");

                SetOrUpdateGangField(gangNode, peopleField, desiredPeopleGroupId);
            }

            if (IncludeVehicles)
            {
                var desiredVehicleGroupId = (SelectedEditGangVehicleGroupId ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(desiredVehicleGroupId))
                {
                    var vehicleField = gangNode.Element("VehicleGroupID") is not null
                     ? "VehicleGroupID"
                     : gangNode.Element("VehiclesID") is not null
                         ? "VehiclesID"
                         : "VehicleGroupID";

                    var oldVehicleGroupId = ((string?)gangNode.Element(vehicleField) ?? "").Trim();

                    if (!string.Equals(oldVehicleGroupId, desiredVehicleGroupId, StringComparison.Ordinal))
                        changes.Add($"VehicleGroupID: '{oldVehicleGroupId}' -> '{desiredVehicleGroupId}'");

                    SetOrUpdateGangField(gangNode, vehicleField, desiredVehicleGroupId);
                }
            }

            if (IncludeDealerMenus && !UseSourceGangDealerMenuGroup)
            {
                var desiredDealerGroupId = (SelectedShopMenuGroup?.Id ?? ManualDealerMenuGroupId ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(desiredDealerGroupId))
                {
                    var dealerField = gangNode.Element("DealerMenuGroupID") is not null ? "DealerMenuGroupID" : "DealerMenuGroup";
                    var oldDealerId = ((string?)gangNode.Element(dealerField) ?? "").Trim();

                    if (!string.Equals(oldDealerId, desiredDealerGroupId, StringComparison.Ordinal))
                        changes.Add($"{dealerField}: '{oldDealerId}' -> '{desiredDealerGroupId}'");

                    SetOrUpdateGangField(gangNode, dealerField, desiredDealerGroupId);
                }
            }

            if (!UseSourceGangWeaponsLoadouts)
            {
                var desiredMeleeId = (SelectedMeleeWeaponsGroup?.Id ?? "").Trim();
                var desiredSideArmsId = (SelectedSideArmsGroup?.Id ?? "").Trim();
                var desiredLongGunsId = (SelectedLongGunsGroup?.Id ?? "").Trim();

                var oldMelee = ((string?)gangNode.Element("MeleeWeaponsID") ?? "").Trim();
                var oldSide = ((string?)gangNode.Element("SideArmsID") ?? "").Trim();
                var oldLong = ((string?)gangNode.Element("LongGunsID") ?? "").Trim();

                if (!string.Equals(oldMelee, desiredMeleeId, StringComparison.Ordinal))
                    changes.Add($"MeleeWeaponsID: '{oldMelee}' -> '{desiredMeleeId}'");
                if (!string.Equals(oldSide, desiredSideArmsId, StringComparison.Ordinal))
                    changes.Add($"SideArmsID: '{oldSide}' -> '{desiredSideArmsId}'");
                if (!string.Equals(oldLong, desiredLongGunsId, StringComparison.Ordinal))
                    changes.Add($"LongGunsID: '{oldLong}' -> '{desiredLongGunsId}'");

                SetOrUpdateGangField(gangNode, "MeleeWeaponsID", desiredMeleeId);
                SetOrUpdateGangField(gangNode, "SideArmsID", desiredSideArmsId);
                SetOrUpdateGangField(gangNode, "LongGunsID", desiredLongGunsId);
            }

            if (willEditRelationships)
            {
                var enemyIds = SelectedEnemyGangs
                    .Select(x => x.Id)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var oldEnemies = gangNode.Element("EnemyGangs")?.Elements()
                    .Select(e => (e.Value ?? "").Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToArray() ?? Array.Empty<string>();

                var oldEnemiesText = oldEnemies.Length == 0 ? "None" : string.Join(", ", oldEnemies);
                var newEnemiesText = enemyIds.Length == 0 ? "None" : string.Join(", ", enemyIds);

                if (!string.Equals(oldEnemiesText, newEnemiesText, StringComparison.Ordinal))
                    changes.Add($"EnemyGangs: '{oldEnemiesText}' -> '{newEnemiesText}'");

                SetEnemyGangs(gangNode, enemyIds);
            }

            var gangsAfter = gangsDoc.ToString(SaveOptions.DisableFormatting);
            if (!string.Equals(gangsBefore, gangsAfter, StringComparison.Ordinal))
                pendingWrites[gangsPath] = xmlService.Format(gangsDoc.ToString());

            if (willEditTerritories)
            {
                var lookup = new LSR.XmlHelper.Core.Services.Builders.GangTerritoryZoneLookupService();

                var zonesBefore = lookup
                    .GetZoneInternalNamesForGang(_rootFolderPath, gangId)
                    .Select(z => (z ?? "").Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var zonesAfter = SelectedZones
                    .Select(z => (z.InternalGameName ?? "").Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var impactedZones = new HashSet<string>(zonesBefore, StringComparer.OrdinalIgnoreCase);
                foreach (var z in zonesAfter)
                    impactedZones.Add(z);

                if (impactedZones.Count > 0)
                {
                    var winnerResolver = new LSR.XmlHelper.Core.Services.Editing.GangTerritoriesWinnerFileByZoneResolverService();
                    var resolution = winnerResolver.Resolve(_rootFolderPath, "Default");

                    var docsByPath = new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);
                    var beforeByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var zone in impactedZones)
                    {
                        var winnerPath = resolution.WinnerFileByZone.TryGetValue(zone, out var p) ? p : resolution.FallbackPath;

                        if (string.IsNullOrWhiteSpace(winnerPath))
                            continue;

                        if (!File.Exists(winnerPath))
                        {
                            System.Windows.MessageBox.Show(
                                "No GangTerritories config file was found to edit for zone '" + zone + "'.\r\nExpected winner file:\r\n" + winnerPath,
                                "Gang Builder",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        if (!docsByPath.TryGetValue(winnerPath, out var doc))
                        {
                            try
                            {
                                doc = XDocument.Load(winnerPath, LoadOptions.None);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(
                                    "Failed to load " + Path.GetFileName(winnerPath) + ":\r\n" + ex.Message,
                                    "Gang Builder",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            docsByPath[winnerPath] = doc;
                            beforeByPath[winnerPath] = doc.ToString(SaveOptions.DisableFormatting);
                        }

                        var shouldOwn = zonesAfter.Contains(zone);

                        var updater = new LSR.XmlHelper.Core.Services.Editing.GangTerritoryZoneOwnershipUpdaterService();
                        updater.Apply(doc, zone, gangId, shouldOwn);
                    }

                    foreach (var kvp in docsByPath)
                    {
                        var path = kvp.Key;
                        var doc = kvp.Value;

                        var before = beforeByPath.TryGetValue(path, out var b) ? b : "";
                        var after = doc.ToString(SaveOptions.DisableFormatting);

                        if (!string.Equals(before, after, StringComparison.Ordinal))
                        {
                            pendingWrites[path] = xmlService.Format(doc.ToString());

                            var territoriesSummary = new LSR.XmlHelper.Wpf.Services.Editing.GangTerritoriesChangeSummaryService()
                                .Summarize(before, after, gangId);

                            foreach (var s in territoriesSummary)
                                changes.Add(s);
                        }
                    }
                }
            }

            if (willEditDen && File.Exists(locationsPath))
            {
                XDocument locationsDoc;

                try
                {
                    locationsDoc = XDocument.Load(locationsPath, LoadOptions.None);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to load " + Path.GetFileName(locationsPath) + ":\r\n" + ex.Message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var before = locationsDoc.ToString(SaveOptions.DisableFormatting);

                var updater = new LSR.XmlHelper.Core.Services.Editing.GangDenBasicFieldsUpdaterService();
                updater.Apply(
                    locationsDoc,
                    gangId,
                    (NewDenName ?? "").Trim(),
                    (NewDenX ?? "").Trim(),
                    (NewDenY ?? "").Trim(),
                    (NewDenZ ?? "").Trim(),
                    (NewDenHeading ?? "").Trim(),
                    (DenMenuId ?? "").Trim(),
                    (DenBannerImagePath ?? "").Trim());

                ApplyDenPossiblePedSpawnEdits(locationsDoc, gangId);
                ApplyDenPossibleVehicleSpawnEdits(locationsDoc, gangId);

                var after = locationsDoc.ToString(SaveOptions.DisableFormatting);
                var updated = !string.Equals(before, after, StringComparison.Ordinal);

                if (updated)
                {
                    pendingWrites[locationsPath] = xmlService.Format(locationsDoc.ToString());
                    var denSummary = new LSR.XmlHelper.Wpf.Services.Editing.LocationsDenDetailedChangeSummaryService()
                        .Summarize(before, after, gangId);

                    foreach (var s in denSummary)
                        changes.Add(s);
                }
            }

            if (willEditDenInventory && File.Exists(shopMenusPath))
            {
                XDocument shopDoc;

                try
                {
                    shopDoc = XDocument.Load(shopMenusPath, LoadOptions.None);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to load " + Path.GetFileName(shopMenusPath) + ":\r\n" + ex.Message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var beforeShopMenus = shopDoc.ToString(SaveOptions.DisableFormatting);

                var items = DenInventoryItems
                    .Select(x => x.ToModel())
                    .Where(x => !string.IsNullOrWhiteSpace(x.ModItemName))
                    .ToList();

                var builder = new LSR.XmlHelper.Core.Services.Builders.DenInventoryShopMenuBuilderService();
                var updatedDoc = builder.CreateOrMergeInto(
                    shopDoc,
                    groupId: "",
                    groupName: "",
                    menuId: (DenMenuId ?? "").Trim(),
                    menuName: (NewDenName ?? "Gang Den").Trim(),
                    items: items);

                var afterShopMenus = updatedDoc.ToString(SaveOptions.DisableFormatting);

                pendingWrites[shopMenusPath] = xmlService.Format(updatedDoc.ToString());

                var denInventorySummary = new LSR.XmlHelper.Wpf.Services.Editing.ShopMenusDenInventoryChangeSummaryService()
                    .Summarize(beforeShopMenus, afterShopMenus, (DenMenuId ?? "").Trim());

                foreach (var s in denInventorySummary)
                    changes.Add(s);
            }

            if (IncludeDealerMenus && !UseSourceGangDealerMenuGroup && File.Exists(shopMenusPath))
            {
                var dealerGroupId = (SelectedShopMenuGroup?.Id ?? ManualDealerMenuGroupId ?? "").Trim();
                var dealerEdits = DealerMenuGroupItemsEditor.GetMenuEditsForSave();

                if (!string.IsNullOrWhiteSpace(dealerGroupId) && dealerEdits.Count > 0)
                {
                    XDocument shopDoc2;

                    try
                    {
                        shopDoc2 = XDocument.Load(shopMenusPath, LoadOptions.None);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Failed to load " + Path.GetFileName(shopMenusPath) + ":\r\n" + ex.Message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var before2 = shopDoc2.ToString(SaveOptions.DisableFormatting);

                    var applier2 = new LSR.XmlHelper.Core.Services.Builders.ShopMenuGroupMenuItemsApplyService();

                    foreach (var edit in dealerEdits)
                        applier2.ApplyItemsToGroupMenuIndex(shopDoc2, dealerGroupId, edit.MenuIndex, edit.Items);

                    var after2 = shopDoc2.ToString(SaveOptions.DisableFormatting);

                    if (!string.Equals(before2, after2, StringComparison.Ordinal))
                    {
                        pendingWrites[shopMenusPath] = xmlService.Format(shopDoc2.ToString());
                        var dealerSummary = new LSR.XmlHelper.Wpf.Services.Editing.ShopMenusDealerGroupChangeSummaryService()
                        .Summarize(before2, after2, dealerGroupId);

                        foreach (var s in dealerSummary)
                            changes.Add(s);
                    }
                }
            }

            if (willEditPeople && File.Exists(peoplePath))
            {
                XDocument peopleDoc;

                try
                {
                    peopleDoc = XDocument.Load(peoplePath, LoadOptions.None);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to load " + Path.GetFileName(peoplePath) + ":\r\n" + ex.Message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var groupId = (SelectedDispatchablePeopleGroup?.Id ?? "").Trim();
                var beforePeople = peopleDoc.ToString(SaveOptions.DisableFormatting);

                var applier = new LSR.XmlHelper.Wpf.Services.Editing.DispatchablePeopleGroupEditsApplyService();
                var peopleApplyResult = applier.Apply(peopleDoc, groupId, DispatchablePeopleEntries);
                var updated = peopleApplyResult.Updated;

                if (peopleApplyResult.XmlIssues.Count > 0)
                {
                    var issuesText = string.Join("\r\n- ", peopleApplyResult.XmlIssues.Select(x => $"Person index {x.PersonIndex}: {x.FieldName}"));
                    System.Windows.MessageBox.Show(
                        "Some fields contained invalid XML and were not saved:\r\n\r\n- " + issuesText,
                        "Gang Builder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                if (updated)
                {
                    pendingWrites[peoplePath] = xmlService.Format(peopleDoc.ToString());

                    var afterPeople = peopleDoc.ToString(SaveOptions.DisableFormatting);
                    var peopleSummary = new LSR.XmlHelper.Wpf.Services.Editing.DispatchablePeopleChangeSummaryService()
                        .Summarize(beforePeople, afterPeople, groupId);

                    foreach (var s in peopleSummary)
                        changes.Add(s);
                }

                if (IncludeVehicles && File.Exists(vehiclesPath))
                {
                    var vehicleGroupId = (SelectedEditGangVehicleGroupId ?? "").Trim();

                    if (!string.IsNullOrWhiteSpace(vehicleGroupId))
                    {
                        var currentModels = CustomDispatchableVehicleModelsToAdd
                            .Select(x => (x.ModelName ?? "").Trim())
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        var removedModels = new List<string>();
                        var addedViewModels = new List<ViewModels.Builders.CustomDispatchableVehicleModelViewModel>();

                        if (IsEditExistingGang)
                        {
                            removedModels = _editVehicleModelsOriginal
                                .Where(x => !currentModels.Contains(x))
                                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                                .ToList();

                            var addedModels = currentModels
                                .Where(x => !_editVehicleModelsOriginal.Contains(x))
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            addedViewModels = CustomDispatchableVehicleModelsToAdd
                                .Where(x => addedModels.Contains(((x.ModelName ?? "").Trim())))
                                .ToList();
                        }
                        else
                        {
                            addedViewModels = CustomDispatchableVehicleModelsToAdd.ToList();
                        }

                        if (removedModels.Count > 0 || addedViewModels.Count > 0)
                        {
                            XDocument vehiclesDoc;

                            try
                            {
                                vehiclesDoc = XDocument.Load(vehiclesPath, LoadOptions.None);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show("Failed to load " + Path.GetFileName(vehiclesPath) + ":\r\n" + ex.Message, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            var vehiclesBefore = vehiclesDoc.ToString(SaveOptions.DisableFormatting);
                            var vehiclesXmlUpdated = false;

                            if (removedModels.Count > 0)
                            {
                                var remover = new LSR.XmlHelper.Wpf.Services.Editing.DispatchableVehicleGroupModelsRemoveService();
                                vehiclesXmlUpdated = remover.RemoveModels(vehiclesDoc, vehicleGroupId, removedModels) || vehiclesXmlUpdated;
                            }

                            IReadOnlyList<string> missingModels = Array.Empty<string>();
                            if (addedViewModels.Count > 0)
                            {
                                var vehicleApplier = new LSR.XmlHelper.Wpf.Services.Editing.DispatchableVehicleGroupEditsApplyService();
                                var (vehiclesUpdated, missing) = vehicleApplier.Apply(_rootFolderPath, vehiclesDoc, vehicleGroupId, addedViewModels);
                                updated = vehiclesUpdated || updated;
                                missingModels = missing;
                            }

                            if (updated)
                            {
                                pendingWrites[vehiclesPath] = xmlService.Format(vehiclesDoc.ToString());

                                var afterVehicles = vehiclesDoc.ToString(SaveOptions.DisableFormatting);

                                var vehiclesSummary = new LSR.XmlHelper.Wpf.Services.Editing.DispatchableVehiclesChangeSummaryService()
                                    .Summarize(vehiclesBefore, afterVehicles, vehicleGroupId, addedViewModels, removedModels);

                                foreach (var s in vehiclesSummary)
                                    changes.Add(s);

                                if (missingModels.Count > 0)
                                    changes.Add($"DispatchableVehicles: missing {missingModels.Count} model(s) (not added)");
                            }

                            if (missingModels.Count > 0)
                            {
                                System.Windows.MessageBox.Show(
                                    "Some custom vehicle models could not be found in the source XMLs and were not added:\r\n\r\n- " + string.Join("\r\n- ", missingModels),
                                    "Gang Builder",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                        }
                    }
                }

            if (pendingWrites.Count == 0)
                {
                    System.Windows.MessageBox.Show("No changes detected.", "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (pendingWrites.Count > 0)
                {
                    var tx = new LSR.XmlHelper.Wpf.Services.Editing.EditModeSaveTransactionService();
                    if (!tx.TryCommit(pendingWrites, out var commitError))
                    {
                        System.Windows.MessageBox.Show("Edit save failed:\r\n" + commitError, "Gang Builder", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    foreach (var p in pendingWrites.Keys)
                        filesEdited.Add(p);
                }
                if (pendingWrites.Count > 0)
                {
                    var validator = new LSR.XmlHelper.Wpf.Services.Editing.EditModePostSaveValidationService();

                    var expectedZones = SelectedZones
                        .Select(z => (z.InternalGameName ?? "").Trim())
                        .Where(z => !string.IsNullOrWhiteSpace(z))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var dealerGroupId = (SelectedShopMenuGroup?.Id ?? ManualDealerMenuGroupId ?? "").Trim();
                    var dealerEdits = DealerMenuGroupItemsEditor.GetMenuEditsForSave();

                    var issues = validator.Validate(
                        _rootFolderPath,
                        gangId,
                        pendingWrites.Keys.ToList(),
                        willEditPeople,
                        (SelectedDispatchablePeopleGroup?.Id ?? "").Trim(),
                        DispatchablePeopleEntries,
                        IncludeVehicles && !string.IsNullOrWhiteSpace((SelectedEditGangVehicleGroupId ?? "").Trim()),
                        (SelectedEditGangVehicleGroupId ?? "").Trim(),
                        willEditTerritories,
                        expectedZones,
                        willEditDen,
                        (NewDenName ?? "").Trim(),
                        _possiblePedSpawns,
                        _possibleVehicleSpawns,
                        willEditDenInventory,
                        (DenMenuId ?? "").Trim(),
                        DenInventoryItems,
                        willEditDealerMenus,
                        dealerGroupId,
                        dealerEdits,
                        willEditVehicles,
                        (SelectedEditGangVehicleGroupId ?? "").Trim(),
                        CustomDispatchableVehicleModelsToAdd);

                    if (issues.Count > 0)
                    {
                        var dialogOwner = System.Windows.Application.Current.Windows
                            .OfType<LSR.XmlHelper.Wpf.Views.Builders.GangBuilderWindow>()
                            .FirstOrDefault();

                        var dialog = new LSR.XmlHelper.Wpf.Views.Dialogs.LargeTextDialogWindow(
                            "Post-save validation found mismatches",
                            "- " + string.Join("\r\n- ", issues))
                        {
                            Owner = dialogOwner
                        };

                        dialog.ShowDialog();
                    }
                }

                var filesSection = "Edited files:\r\n" + string.Join("\r\n", filesEdited.Distinct(StringComparer.OrdinalIgnoreCase).Select(x => "- " + x));
                var backupsSection = backedUpFiles.Count > 0
                    ? "\r\n\r\nBackups created:\r\n" + string.Join("\r\n", backedUpFiles.Distinct(StringComparer.OrdinalIgnoreCase).Select(x => "- " + x))
                    : "\r\n\r\nBackups created:\r\n- (none)";

                var changesSection = changes.Count > 0
                    ? "\r\n\r\nChanges applied:\r\n" + string.Join("\r\n", changes.Select(x => "- " + x))
                    : "\r\n\r\nChanges applied:\r\n- (none)";

                BuildSummaryText =
                    "Edit mode summary:\r\n\r\n" +
                    filesSection +
                    backupsSection +
                    changesSection;

                HasBuildSummary = true;
                SetTask("Show summary + next steps", true, "Done");

                System.Windows.MessageBox.Show(
                    "Edit complete.\r\n\r\nSee the Build Summary panel for details.",
                    "Gang Builder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private static string GetWeaponsCloneWarning(bool useSourceGangWeaponsLoadouts, string meleeId, string sideArmsId, string longGunsId)
        {
            if (useSourceGangWeaponsLoadouts)
                return "Disabled (using source loadouts)";

            var missing = new System.Collections.Generic.List<string>();

            if (string.IsNullOrWhiteSpace(meleeId))
                missing.Add("MeleeWeaponsID");

            if (string.IsNullOrWhiteSpace(sideArmsId))
                missing.Add("SideArmsID");

            if (string.IsNullOrWhiteSpace(longGunsId))
                missing.Add("LongGunsID");

            if (missing.Count == 0)
                return "";

            if (missing.Count == 3)
                return "Warning: No weapon group IDs selected";

            return $"Warning: Missing {string.Join(", ", missing)}";
        }


        private static string GetNewDenWarning(string name, string x, string y, string z, string heading)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Warning: Missing Den Name";

            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(x))
                missing.Add("X");

            if (string.IsNullOrWhiteSpace(y))
                missing.Add("Y");

            if (string.IsNullOrWhiteSpace(z))
                missing.Add("Z");

            if (string.IsNullOrWhiteSpace(heading))
                missing.Add("Heading");

            if (missing.Count > 0)
                return $"Warning: Missing {string.Join(", ", missing)}";

            if (!TryGetNewDenValues(x, y, z, heading, out _, out _, out _, out _))
                return "Warning: Invalid number format";

            return "";
        }

        private static bool TryGetNewDenValues(string x, string y, string z, string heading, out double parsedX, out double parsedY, out double parsedZ, out double parsedHeading)
        {
            parsedX = 0;
            parsedY = 0;
            parsedZ = 0;
            parsedHeading = 0;

            var style = NumberStyles.Float | NumberStyles.AllowThousands;
            var culture = CultureInfo.InvariantCulture;

            if (!double.TryParse(x, style, culture, out parsedX))
                return false;

            if (!double.TryParse(y, style, culture, out parsedY))
                return false;

            if (!double.TryParse(z, style, culture, out parsedZ))
                return false;

            if (!double.TryParse(heading, style, culture, out parsedHeading))
                return false;

            return true;
        }
        private void ApplyDispatchablePeopleEdits(XDocument peopleDoc)
        {
            if (UseSourceGangPeopleGroup)
                return;

            if (DispatchablePeopleEntries.Count == 0)
                return;

            var group = peopleDoc.Descendants("DispatchablePersonGroup").FirstOrDefault();
            if (group is null)
                return;

            var people = group.Descendants("DispatchablePerson").ToList();
            if (people.Count == 0)
                return;

            foreach (var entry in DispatchablePeopleEntries)
            {
                XElement? person = null;

                if (!string.IsNullOrWhiteSpace(entry.SourceDebugName))
                {
                    person = people.FirstOrDefault(p => string.Equals(((string?)p.Element("DebugName") ?? "").Trim(), entry.SourceDebugName, StringComparison.OrdinalIgnoreCase));
                }
                else if (entry.SourceIndex >= 0 && entry.SourceIndex < people.Count)
                {
                    person = people[entry.SourceIndex];
                }

                if (person is null)
                    continue;

                foreach (var field in entry.Fields)
                {
                    if (string.IsNullOrWhiteSpace(field.Name))
                        continue;

                    if (string.IsNullOrWhiteSpace(field.Value))
                        continue;

                    if (field.IsXml)
                    {
                        try
                        {
                            var parsed = XElement.Parse(field.Value, LoadOptions.None);
                            if (!string.Equals(parsed.Name.LocalName, field.Name, StringComparison.OrdinalIgnoreCase))
                                continue;

                            var existing = person.Element(field.Name);
                            if (existing is not null)
                                existing.ReplaceWith(parsed);
                            else
                                person.Add(parsed);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        SetOrCreate(person, field.Name, field.Value);
                    }
                }
            }
        }
        private void UpdateDispatchablePersonFieldsView()
        {
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(SelectedDispatchablePersonEntry?.Fields);

            if (view is null)
            {
                DispatchablePersonFieldsView = null;
                return;
            }

            view.Filter = item =>
            {
                if (item is not DispatchablePersonFieldViewModel field)
                    return false;

                var q = (DispatchablePersonFieldSearchText ?? "").Trim();
                if (string.IsNullOrWhiteSpace(q))
                    return true;

                return field.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            DispatchablePersonFieldsView = view;
            view.Refresh();
        }
        private bool ValidateBeforeBuild(out List<LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel> issues)
        {
            issues = new List<LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel>();

            if (!HasCoreInputs())
            {
                issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                    "Missing core inputs: Pack Name, New Gang ID, New Gang Full Name, or Clone From Gang ID.",
                    "NewGangIdTextBox"));
            }

            if (!IsEditExistingGang && !string.IsNullOrWhiteSpace(NewGangId))
            {
                var lookup = new LSR.XmlHelper.Core.Services.Builders.GangIdLookupService();
                if (lookup.TryFindGangId(_rootFolderPath, NewGangId, out var foundIn))
                {
                    issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                        $"NewGangId already exists: '{NewGangId}' (found in {foundIn}). Pick a unique ID.",
                        "NewGangIdTextBox"));
                }
            }

            if (IncludeDealerMenus)
            {
                var finalDealerGroupId = "";

                if (UseSourceGangDealerMenuGroup)
                {
                    var sourceLookup = new LSR.XmlHelper.Core.Services.Builders.GangDealerMenuGroupLookupService();
                    finalDealerGroupId = sourceLookup.GetDealerMenuGroupId(_rootFolderPath, CloneFromGangId);
                }
                else
                {
                    finalDealerGroupId = ManualDealerMenuGroupId;
                }

                if (string.IsNullOrWhiteSpace(finalDealerGroupId))
                {
                    issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                        "Dealer menus are enabled, but no Dealer Menu Group was found/selected. If using source, the source gang must have DealerMenuGroupID. If manual, you must enter one.",
                        "ManualDealerMenuGroupTextBox"));
                }
            }

            if (IncludeTerritories && IncludeZones && SelectedZones.Count == 0)
            {
                issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                    "Territories are enabled, but no zones are selected.",
                    "ZonesListBox"));
            }

            if (IncludeDens)
            {
                if (CreateNewDen)
                {
                    var denWarning = GetNewDenWarning(NewDenName, NewDenX, NewDenY, NewDenZ, NewDenHeading);
                    if (!string.IsNullOrWhiteSpace(denWarning))
                    {
                        issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                            $"Den creation is enabled, but the new den inputs are not valid: {denWarning}",
                            "NewDenNameTextBox"));
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CloneFromGangId))
                    {
                        issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                            "Dens are enabled but Clone From Gang ID is blank (needed to clone dens).",
                            "NewGangIdTextBox"));
                    }
                }
            }

            AddDenPedSpawnValidationIssues(issues);

            if (IncludePeople && !UseSourceGangPeopleGroup && SelectedDispatchablePeopleGroup is null)
            {
                issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                    "People is enabled and override is on, but no DispatchablePeople group was selected.",
                    "DispatchablePeopleGroupComboBox"));
            }

            if (IncludePeople && !UseSourceGangPeopleGroup && SelectedDispatchablePeopleGroup is not null && DispatchablePeopleEntries.Count == 0)
            {
                issues.Add(new LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel(
                    "People is enabled and override is on, but the selected group has no peds. Add at least one ped.",
                    "DispatchablePeopleGroupComboBox"));
            }

            return issues.Count == 0;
        }
        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }
        public List<LSR.XmlHelper.Wpf.ViewModels.Builders.PreBuildValidationIssueViewModel> GetPreBuildIssues()
        {
            ValidateBeforeBuild(out var issues);
            return issues;
        }
        private void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private void ApplyDenPossiblePedSpawnEdits(System.Xml.Linq.XDocument densDoc, string newGangId)
        {
            if (densDoc is null)
                return;

            if (string.IsNullOrWhiteSpace(newGangId))
                return;

            if (_possiblePedSpawns.Count == 0)
                return;

            var denNameLookup = densDoc
     .Descendants("GangDen")
     .Where(d => string.Equals(d.Element("AssignedAssociationID")?.Value ?? "", newGangId, StringComparison.OrdinalIgnoreCase))
     .Select(d => new
     {
         Name = (d.Element("Name")?.Value ?? "").Trim(),
         FullName = (d.Element("FullName")?.Value ?? "").Trim()
     })
     .Where(x => !string.IsNullOrWhiteSpace(x.Name))
     .ToList();

            var models = new System.Collections.Generic.List<LSR.XmlHelper.Core.Models.PossiblePedSpawnModel>();

            foreach (var vm in _possiblePedSpawns)
            {
                var denName = (vm.DenName ?? "").Trim();

                var direct = denNameLookup.FirstOrDefault(x => string.Equals(x.Name, denName, StringComparison.OrdinalIgnoreCase));
                if (direct is null)
                {
                    var byFullName = denNameLookup.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FullName) && string.Equals(x.FullName, denName, StringComparison.OrdinalIgnoreCase));
                    if (byFullName is not null)
                        denName = byFullName.Name;
                }

                if (direct is null)
                {
                    var safehouseSuffix = " Safehouse";
                    if (denName.EndsWith(safehouseSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        var trimmed = denName.Substring(0, denName.Length - safehouseSuffix.Length).Trim();
                        var byTrimmed = denNameLookup.FirstOrDefault(x => string.Equals(x.Name, trimmed, StringComparison.OrdinalIgnoreCase));
                        if (byTrimmed is not null)
                            denName = byTrimmed.Name;
                    }
                }

                models.Add(new LSR.XmlHelper.Core.Models.PossiblePedSpawnModel
                {
                    DenName = denName,
                    X = vm.X,
                    Y = vm.Y,
                    Z = vm.Z,
                    Heading = vm.Heading,
                    Percentage = vm.Percentage,
                    TaskRequirements = vm.TaskRequirements,
                    MinHourSpawn = vm.MinHourSpawn,
                    MaxHourSpawn = vm.MaxHourSpawn,
                    MinWantedLevelSpawn = vm.MinWantedLevelSpawn,
                    MaxWantedLevelSpawn = vm.MaxWantedLevelSpawn,
                    LongGunAlwaysEquipped = vm.LongGunAlwaysEquipped,
                    SourceElement = vm.SourceElement is null ? null : new System.Xml.Linq.XElement(vm.SourceElement)
                });
            }
            var updater = new LSR.XmlHelper.Core.Services.Builders.GangDenPossiblePedSpawnsUpdaterService();
            updater.Apply(densDoc, newGangId, models);
        }
        private string ResolveDenVehicleGroupId(string rawValue, string builtVehicleGroupId)
        {
            var value = (rawValue ?? "").Trim();

            if (string.IsNullOrWhiteSpace(value))
                return value;

            if (string.Equals(value, NewGangVehicleGroupPlaceholder, StringComparison.OrdinalIgnoreCase))
                return (builtVehicleGroupId ?? "").Trim();

            return value;
        }

        private void ApplyDenPossibleVehicleSpawnEdits(System.Xml.Linq.XDocument densDoc, string newGangId)
        {
            if (densDoc.Root is null)
                return;

            if (string.IsNullOrWhiteSpace(newGangId))
                return;

            var models = new System.Collections.Generic.List<LSR.XmlHelper.Core.Models.PossibleVehicleSpawnModel>();

            foreach (var vm in _possibleVehicleSpawns)
            {
                models.Add(new LSR.XmlHelper.Core.Models.PossibleVehicleSpawnModel
                {
                    DenName = vm.DenName,
                    X = vm.X,
                    Y = vm.Y,
                    Z = vm.Z,
                    Heading = vm.Heading,
                    Percentage = vm.Percentage,
                    TaskRequirements = vm.TaskRequirements,
                    MinHourSpawn = vm.MinHourSpawn,
                    MaxHourSpawn = vm.MaxHourSpawn,
                    MinWantedLevelSpawn = vm.MinWantedLevelSpawn,
                    MaxWantedLevelSpawn = vm.MaxWantedLevelSpawn,
                    RequiredVehicleGroup = ResolveDenVehicleGroupId(vm.RequiredVehicleGroup, _lastBuiltVehicleGroupId),
                    ForceVehicleGroup = vm.ForceVehicleGroup,
                    AllowAirVehicle = vm.AllowAirVehicle,
                    AllowBoat = vm.AllowBoat,
                    SourceElement = vm.SourceElement is null ? null : new System.Xml.Linq.XElement(vm.SourceElement)
                });
            }

            var updater = new LSR.XmlHelper.Core.Services.Builders.GangDenPossibleVehicleSpawnsUpdaterService();
            updater.Apply(densDoc, newGangId, models);
        }

        private bool HasSelectedDenPedSpawnRow() => SelectedPossiblePedSpawn is not null;

        private void AddDenPedSpawnRow()
        {
            var denName = BuildDefaultDenPedSpawnDenName();

            var vm = new ViewModels.Builders.PossiblePedSpawnViewModel
            {
                DenName = denName,
                X = ParseDoubleInvariant(NewDenX),
                Y = ParseDoubleInvariant(NewDenY),
                Z = ParseDoubleInvariant(NewDenZ),
                Heading = ParseDoubleInvariant(NewDenHeading),
                Percentage = 35,
                TaskRequirements = "None",
                MinHourSpawn = 0,
                MaxHourSpawn = 24,
                MinWantedLevelSpawn = 0,
                MaxWantedLevelSpawn = 3,
                LongGunAlwaysEquipped = false
            };

            _possiblePedSpawns.Add(vm);
            SelectedPossiblePedSpawn = vm;
        }

        private void DuplicateDenPedSpawnRow()
        {
            if (SelectedPossiblePedSpawn is null)
                return;

            var src = SelectedPossiblePedSpawn;

            var vm = new ViewModels.Builders.PossiblePedSpawnViewModel
            {
                DenName = src.DenName,
                X = src.X,
                Y = src.Y,
                Z = src.Z,
                Heading = src.Heading,
                Percentage = src.Percentage,
                TaskRequirements = src.TaskRequirements,
                MinHourSpawn = src.MinHourSpawn,
                MaxHourSpawn = src.MaxHourSpawn,
                MinWantedLevelSpawn = src.MinWantedLevelSpawn,
                MaxWantedLevelSpawn = src.MaxWantedLevelSpawn,
                LongGunAlwaysEquipped = src.LongGunAlwaysEquipped
            };

            _possiblePedSpawns.Add(vm);
            SelectedPossiblePedSpawn = vm;
        }

        private void RemoveDenPedSpawnRow()
        {
            if (SelectedPossiblePedSpawn is null)
                return;

            var idx = _possiblePedSpawns.IndexOf(SelectedPossiblePedSpawn);
            if (idx < 0)
                return;

            _possiblePedSpawns.RemoveAt(idx);

            if (_possiblePedSpawns.Count == 0)
            {
                SelectedPossiblePedSpawn = null;
                return;
            }

            if (idx >= _possiblePedSpawns.Count)
                idx = _possiblePedSpawns.Count - 1;

            SelectedPossiblePedSpawn = _possiblePedSpawns[idx];
        }
        private bool HasSelectedDenVehicleSpawnRow() => SelectedPossibleVehicleSpawn is not null;

        private void AddDenVehicleSpawnRow()
        {
            var denName = BuildDefaultDenPedSpawnDenName();

            var vm = new ViewModels.Builders.PossibleVehicleSpawnViewModel
            {
                DenName = denName,
                X = ParseDoubleInvariant(NewDenX),
                Y = ParseDoubleInvariant(NewDenY),
                Z = ParseDoubleInvariant(NewDenZ),
                Heading = ParseDoubleInvariant(NewDenHeading),
                Percentage = 35,
                TaskRequirements = "None",
                MinHourSpawn = 0,
                MaxHourSpawn = 24,
                MinWantedLevelSpawn = 0,
                MaxWantedLevelSpawn = 3,
                RequiredVehicleGroup = "",
                ForceVehicleGroup = true,
                AllowAirVehicle = false,
                AllowBoat = false
            };

            _possibleVehicleSpawns.Add(vm);
            SelectedPossibleVehicleSpawn = vm;
        }

        private void DuplicateDenVehicleSpawnRow()
        {
            if (SelectedPossibleVehicleSpawn is null)
                return;

            var src = SelectedPossibleVehicleSpawn;

            var vm = new ViewModels.Builders.PossibleVehicleSpawnViewModel
            {
                DenName = src.DenName,
                X = src.X,
                Y = src.Y,
                Z = src.Z,
                Heading = src.Heading,
                Percentage = src.Percentage,
                TaskRequirements = src.TaskRequirements,
                MinHourSpawn = src.MinHourSpawn,
                MaxHourSpawn = src.MaxHourSpawn,
                MinWantedLevelSpawn = src.MinWantedLevelSpawn,
                MaxWantedLevelSpawn = src.MaxWantedLevelSpawn,
                RequiredVehicleGroup = src.RequiredVehicleGroup,
                ForceVehicleGroup = src.ForceVehicleGroup,
                AllowAirVehicle = src.AllowAirVehicle,
                AllowBoat = src.AllowBoat
            };

            _possibleVehicleSpawns.Add(vm);
            SelectedPossibleVehicleSpawn = vm;
        }

        private void RemoveDenVehicleSpawnRow()
        {
            if (SelectedPossibleVehicleSpawn is null)
                return;

            var idx = _possibleVehicleSpawns.IndexOf(SelectedPossibleVehicleSpawn);
            if (idx < 0)
                return;

            _possibleVehicleSpawns.RemoveAt(idx);

            if (_possibleVehicleSpawns.Count == 0)
            {
                SelectedPossibleVehicleSpawn = null;
                return;
            }

            if (idx >= _possibleVehicleSpawns.Count)
                idx = _possibleVehicleSpawns.Count - 1;

            SelectedPossibleVehicleSpawn = _possibleVehicleSpawns[idx];
        }

        private string BuildDefaultDenPedSpawnDenName()
        {
            var baseName = string.IsNullOrWhiteSpace(NewDenName) ? "Gang" : NewDenName.Trim();
            return baseName;
        }

        private void AddDenPedSpawnValidationIssues(List<ViewModels.Builders.PreBuildValidationIssueViewModel> issues)
        {
            if (_possiblePedSpawns.Count == 0)
                return;

            for (var i = 0; i < _possiblePedSpawns.Count; i++)
            {
                var row = _possiblePedSpawns[i];

                if (row.Percentage < 0 || row.Percentage > 100)
                {
                    issues.Add(new ViewModels.Builders.PreBuildValidationIssueViewModel(
                        $"Den ped spawn row {i + 1}: Percentage must be 0–100.",
                        "DenPedSpawnsGrid"));
                }

                if (row.MinHourSpawn < 0 || row.MinHourSpawn > 24)
                {
                    issues.Add(new ViewModels.Builders.PreBuildValidationIssueViewModel(
                        $"Den ped spawn row {i + 1}: Min Hour must be 0–24.",
                        "DenPedSpawnsGrid"));
                }

                if (row.MaxHourSpawn < 0 || row.MaxHourSpawn > 24)
                {
                    issues.Add(new ViewModels.Builders.PreBuildValidationIssueViewModel(
                        $"Den ped spawn row {i + 1}: Max Hour must be 0–24.",
                        "DenPedSpawnsGrid"));
                }

                if (row.MinHourSpawn > row.MaxHourSpawn)
                {
                    issues.Add(new ViewModels.Builders.PreBuildValidationIssueViewModel(
                        $"Den ped spawn row {i + 1}: Min Hour cannot be greater than Max Hour.",
                        "DenPedSpawnsGrid"));
                }
            }
        }

        private static bool TryParseInt(string? value, out int result)
        {
            return int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result);
        }
        private static double ParseDoubleInvariant(string? value)
        {
            return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;
        }

        private static readonly IReadOnlyDictionary<string, string> GangColorPrefixMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Red"] = "~r~",
                ["Green"] = "~g~",
                ["Blue"] = "~b~",
                ["Yellow"] = "~y~",
                ["Purple"] = "~p~",
                ["Orange"] = "~o~",
                ["White"] = "~w~",
                ["Gray"] = "~c~",
                ["Black"] = "~u~",
                ["DarkGray"] = "~m~",
                ["Pink"] = "~q~"
            };

        private static readonly IReadOnlyDictionary<string, string> GangColorNameByPrefixMap =
            GangColorPrefixMap
                .GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Key, StringComparer.OrdinalIgnoreCase);

        private void LoadBlipAndColorReferences()
        {
            CommonBlipColors.Clear();
            CommonTextColorPrefixes.Clear();
            GangColorNames.Clear();

            foreach (var kc in Enum.GetValues<System.Drawing.KnownColor>())
            {
                var c = System.Drawing.Color.FromKnownColor(kc);
                if (!c.IsSystemColor)
                    CommonBlipColors.Add(c.Name);
            }

            var ordered = CommonBlipColors.OrderBy(s => s).ToList();
            CommonBlipColors.Clear();
            foreach (var name in ordered)
                CommonBlipColors.Add(name);

            var gangNames = GangColorPrefixMap.Keys.OrderBy(s => s).ToList();
            foreach (var n in gangNames)
                GangColorNames.Add(n);

            CommonTextColorPrefixes.Add("~s~");
            foreach (var p in GangColorPrefixMap.Values.Distinct(StringComparer.OrdinalIgnoreCase))
                CommonTextColorPrefixes.Add(p);

            var prefixes = new[]
            {
                "~s~",
                "~r~",
                "~g~",
                "~b~",
                "~y~",
                "~p~",
                "~o~",
                "~w~",
                "~h~",
                "~c~",
                "~u~",
                "~m~",
                "~q~"
            };

            foreach (var p in prefixes)
                CommonTextColorPrefixes.Add(p);

            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            System.Threading.Tasks.Task.Run(() =>
            {
                var blips = Services.Blips.BlipSpriteReferenceProvider.LoadAllBlips();
                return blips;
            }).ContinueWith(t =>
            {
                if (t.IsFaulted || t.Result is null)
                    return;

                void Apply()
                {
                    CommonBlipSprites.Clear();
                    foreach (var b in t.Result)
                        CommonBlipSprites.Add(b);
                }

                if (dispatcher is null)
                {
                    Apply();
                    return;
                }

                dispatcher.Invoke(Apply);
            });
        }
        private static bool TryGetFirstShopMenuId(XDocument shopMenusDoc, out string menuId)
        {
            menuId = "";

            if (shopMenusDoc.Root is null)
                return false;

            var id = shopMenusDoc
                .Descendants("ShopMenu")
                .Select(x => (string?)x.Element("ID"))
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

            if (string.IsNullOrWhiteSpace(id))
                return false;

            menuId = id.Trim();
            return true;
        }
        private void RefreshDenInventoryItemNames()
        {
            AvailableDenInventoryItemNames.Clear();
            _modItemCategories.Clear();

            if (AvailableDenInventoryItemNamesView is null)
            {
                AvailableDenInventoryItemNamesView = System.Windows.Data.CollectionViewSource.GetDefaultView(AvailableDenInventoryItemNames);
                AvailableDenInventoryItemNamesView.Filter = AvailableDenInventoryItemNamesFilter;
            }
            else
            {
                AvailableDenInventoryItemNamesView.Filter = AvailableDenInventoryItemNamesFilter;
            }

            var catalog = new LSR.XmlHelper.Core.Services.Builders.ModItemCategoryCatalogService();
            var items = catalog.GetAllItemsWithCategories(_rootFolderPath);

            foreach (var item in items)
            {
                if (!_modItemCategories.ContainsKey(item.Name))
                    _modItemCategories[item.Name] = item.Category;

                AvailableDenInventoryItemNames.Add(item.Name);
            }

            if (AvailableDenInventoryItemNames.Count == 0)
            {
                var rootShopMenus = new LSR.XmlHelper.Core.Services.Builders.RootShopMenusItemNameCatalogService();
                var names = rootShopMenus.GetDistinctModItemNames(_rootFolderPath);

                foreach (var name in names)
                {
                    if (!_modItemCategories.ContainsKey(name))
                        _modItemCategories[name] = "Other";

                    AvailableDenInventoryItemNames.Add(name);
                }
            }

            AvailableDenInventoryItemNamesView.Refresh();
        }

        private bool AvailableDenInventoryItemNamesFilter(object obj)
        {
            if (obj is not string name)
                return false;

            var search = (DenInventorySearchText ?? "").Trim();
            if (search.Length > 0)
            {
                if (name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            var cat = (SelectedDenInventoryCategory ?? "All").Trim();
            if (!string.Equals(cat, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (!_modItemCategories.TryGetValue(name, out var itemCat))
                    itemCat = "Other";

                if (!string.Equals(itemCat, cat, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private bool DenInventoryItemsFilter(object obj)
        {
            if (obj is not DenInventoryMenuItemViewModel item)
                return false;

            var search = (DenInventorySearchText ?? "").Trim();
            if (search.Length > 0)
            {
                var name = item.ModItemName ?? "";
                if (name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            var cat = (SelectedDenInventoryCategory ?? "All").Trim();
            if (!string.Equals(cat, "All", StringComparison.OrdinalIgnoreCase))
            {
                var itemCat = (item.Category ?? "All").Trim();
                if (string.Equals(itemCat, "All", StringComparison.OrdinalIgnoreCase))
                {
                    var key = (item.ModItemName ?? "").Trim();
                    if (!_modItemCategories.TryGetValue(key, out itemCat))
                        itemCat = "Other";
                }

                if (!string.Equals(itemCat, cat, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private void AddDenInventoryItem()
        {
            var itemName = (SelectedDenInventoryItemName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(itemName))
                return;

            var resolvedCategory = (SelectedDenInventoryCategory ?? "All").Trim();
            if (string.Equals(resolvedCategory, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (_modItemCategories.TryGetValue(itemName, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
                    resolvedCategory = mapped;
            }

            if (string.IsNullOrWhiteSpace(resolvedCategory))
                resolvedCategory = "Other";

            var isDuplicate = DenInventoryItems.Any(x => string.Equals((x.ModItemName ?? "").Trim(), itemName, StringComparison.OrdinalIgnoreCase));

            var buyPrice = 100;
            var sellPrice = 50;

            var priceLookup = new LSR.XmlHelper.Core.Services.Builders.ShopMenuItemPriceLookupService();
            if (priceLookup.TryGetFirstPrices(_rootFolderPath, itemName, out var foundBuy, out var foundSell))
            {
                buyPrice = foundBuy;
                sellPrice = foundSell;
            }

            var added = new DenInventoryMenuItemViewModel
            {
                ModItemName = itemName,
                PurchasePrice = buyPrice,
                SalesPrice = isDuplicate ? -1 : sellPrice,
                MinimumPurchaseAmount = 1,
                MaximumPurchaseAmount = 10,
                PurchaseIncrement = 1,
                NumberOfItemsToSellToPlayer = -1,
                NumberOfItemsToPurchaseFromPlayer = -1,
                IsIllicilt = false,
                IsFree = false,
                SubPrice = 1,
                SubAmount = 30,
                NumberOfItemsSoldToPlayer = 0,
                NumberOfItemsPurchasedByPlayer = 0,
                Category = resolvedCategory
            };

            DenInventoryItems.Add(added);
            SelectedDenInventoryItem = added;

            DenInventoryItemsView?.Refresh();
        }

        private bool HasSelectedDenInventoryItem()
        {
            return SelectedDenInventoryItem is not null;
        }

        private void RemoveDenInventoryItem()
        {
            if (SelectedDenInventoryItem is null)
                return;

            DenInventoryItems.Remove(SelectedDenInventoryItem);
            SelectedDenInventoryItem = DenInventoryItems.FirstOrDefault();

            RefreshTaskState();
        }

        private void ChooseDenBannerImage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
                Title = "Choose gang banner image"
            };

            var ok = dlg.ShowDialog();
            if (ok != true)
                return;

            var sourcePath = dlg.FileName;
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return;

            var gangsFolder = Path.Combine(_rootFolderPath, "images", "gangs");
            Directory.CreateDirectory(gangsFolder);

            var fileName = Path.GetFileName(sourcePath);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var destPath = Path.Combine(gangsFolder, fileName);

            var sourceFull = Path.GetFullPath(sourcePath);
            var destFull = Path.GetFullPath(destPath);

            if (!string.Equals(sourceFull, destFull, StringComparison.OrdinalIgnoreCase))
                File.Copy(sourcePath, destPath, true);

            DenBannerImagePath = $"gangs\\\\{fileName}";
        }
        private void RefreshPossiblePedSpawnsFromClone()
        {
            _possiblePedSpawns.Clear();

            if (!CloneDenPedSpawnsFromSource)
                return;

            if (string.IsNullOrWhiteSpace(CloneFromGangId))
                return;

            var denLookup = new LSR.XmlHelper.Core.Services.Builders.GangDenLookupService();
            var dens = denLookup.GetGangDens(_rootFolderPath, CloneFromGangId);

            foreach (var den in dens)
            {
                var spawns = LSR.XmlHelper.Core.Services.Parsing.PossiblePedSpawnParser.ParseGangDen(den);

                foreach (var spawn in spawns)
                {
                    _possiblePedSpawns.Add(new ViewModels.Builders.PossiblePedSpawnViewModel
                    {
                        DenName = spawn.DenName,
                        X = spawn.X,
                        Y = spawn.Y,
                        Z = spawn.Z,
                        Heading = spawn.Heading,
                        Percentage = spawn.Percentage,
                        TaskRequirements = spawn.TaskRequirements,
                        MinHourSpawn = spawn.MinHourSpawn,
                        MaxHourSpawn = spawn.MaxHourSpawn,
                        MinWantedLevelSpawn = spawn.MinWantedLevelSpawn,
                        MaxWantedLevelSpawn = spawn.MaxWantedLevelSpawn,
                        LongGunAlwaysEquipped = spawn.LongGunAlwaysEquipped,
                        SourceElement = spawn.SourceElement
                    });
                }
            }
        }
        void ApplyCloneAwareDefaults()
        {
            GenerateDenInventoryMenu = true;

            CloneDenPedSpawnsFromSource = false;

            UseSourceGangPeopleGroup = false;

            UseSourceGangDealerMenuGroup = false;
            ShowCustomerMenus = false;
            CloneDealerMenusIntoPack = true;

            UseSourceGangWeaponsLoadouts = false;
            CloneWeaponsIntoPack = true;

            UseSourceGangEnemyGangs = false;

            RefreshSourceEnemyGangs();
        }

        private void OpenBuildOutputFile(string? fullPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullPath))
                    return;

                if (!File.Exists(fullPath))
                    return;

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private void SmartPasteDenEntranceCoords()
        {
            if (!TryGetClipboardXyzHeading(out var x, out var y, out var z, out var heading))
            {
                System.Windows.MessageBox.Show("Smart paste could not find 4 numbers to use as X, Y, Z, Heading.", "Smart paste", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewDenX = x.ToString("0.######", CultureInfo.InvariantCulture);
            NewDenY = y.ToString("0.######", CultureInfo.InvariantCulture);
            NewDenZ = z.ToString("0.######", CultureInfo.InvariantCulture);
            NewDenHeading = heading.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private void SmartPasteSelectedDenPedSpawnCoords()
        {
            if (SelectedPossiblePedSpawn is null)
                return;

            if (!TryGetClipboardXyzHeading(out var x, out var y, out var z, out var heading))
            {
                System.Windows.MessageBox.Show("Smart paste could not find 4 numbers to use as X, Y, Z, Heading.", "Smart paste", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedPossiblePedSpawn.X = x;
            SelectedPossiblePedSpawn.Y = y;
            SelectedPossiblePedSpawn.Z = z;
            SelectedPossiblePedSpawn.Heading = heading;
        }
        private void SmartPasteSelectedDenVehicleSpawnCoords()
        {
            if (SelectedPossibleVehicleSpawn is null)
                return;

            if (!TryGetClipboardXyzHeading(out var x, out var y, out var z, out var heading))
            {
                System.Windows.MessageBox.Show("Smart paste could not find 4 numbers to use as X, Y, Z, Heading.", "Smart paste", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedPossibleVehicleSpawn.X = x;
            SelectedPossibleVehicleSpawn.Y = y;
            SelectedPossibleVehicleSpawn.Z = z;
            SelectedPossibleVehicleSpawn.Heading = heading;
        }

        private bool CanSmartPasteRequiredVariation()
        {
            return string.Equals(SelectedDispatchablePersonField?.Name ?? "", "RequiredVariation", StringComparison.OrdinalIgnoreCase);
        }

        private void SmartPasteRequiredVariationFromClipboard()
        {
            if (!CanSmartPasteRequiredVariation())
                return;

            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : "";

            if (!_smartRequiredVariationPasteParser.TryGetRequiredVariationXml(text, out var requiredVariationXml))
            {
                System.Windows.MessageBox.Show("Smart paste could not find RequiredVariation, PedVariation, or SavedOutfit XML in the clipboard.", "Smart paste", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDispatchablePersonField is null)
                return;

            SelectedDispatchablePersonField.Value = requiredVariationXml;
        }
        private bool CanAddCustomDispatchableVehicleModel()
        {
            return IncludeVehicles && !string.IsNullOrWhiteSpace(VehicleModelPickerText);
        }

        private void AddCustomDispatchableVehicleModel()
        {
            var model = (VehicleModelPickerText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(model))
                return;

            var exists = CustomDispatchableVehicleModelsToAdd.Any(x => string.Equals((x.ModelName ?? "").Trim(), model, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return;

            var variantKey = SelectedDispatchableVehicleVariantOption?.VariantKey ?? "";
            var variantDisplay = SelectedDispatchableVehicleVariantOption?.DisplayText ?? "";

            var vm = new ViewModels.Builders.CustomDispatchableVehicleModelViewModel(model, variantKey, variantDisplay);
            CustomDispatchableVehicleModelsToAdd.Add(vm);
            SelectedCustomDispatchableVehicleModelToAdd = vm;

            VehicleModelPickerText = "";
            SelectedDispatchableVehicleVariantOption = null;
            UpdateNewGangVehicleGroupOptionCount();
            RefreshTaskState();

            AddCustomDispatchableVehicleModelCommand.RaiseCanExecuteChanged();
            RemoveSelectedCustomDispatchableVehicleModelCommand.RaiseCanExecuteChanged();
        }

        private bool CanRemoveSelectedCustomDispatchableVehicleModel()
        {
            return IncludeVehicles && SelectedCustomDispatchableVehicleModelToAdd is not null;
        }

        private void RemoveSelectedCustomDispatchableVehicleModel()
        {
            if (SelectedCustomDispatchableVehicleModelToAdd is null)
                return;

            var toRemove = SelectedCustomDispatchableVehicleModelToAdd;
            CustomDispatchableVehicleModelsToAdd.Remove(toRemove);

            SelectedCustomDispatchableVehicleModelToAdd = CustomDispatchableVehicleModelsToAdd.LastOrDefault();
            UpdateNewGangVehicleGroupOptionCount();
            RefreshTaskState();

            AddCustomDispatchableVehicleModelCommand.RaiseCanExecuteChanged();
            RemoveSelectedCustomDispatchableVehicleModelCommand.RaiseCanExecuteChanged();
        }
        private void LoadSelectedGangDealerMenuPreview(string dealerMenuGroupId)
        {
            SelectedEditGangDealerMenuPreview.Clear();

            if (string.IsNullOrWhiteSpace(dealerMenuGroupId))
                return;

            var preview = new LSR.XmlHelper.Core.Services.Reading.ShopMenuDetailedPreviewReadService();
            foreach (var line in preview.GetShopMenuGroupItemDetailLines(_rootFolderPath, dealerMenuGroupId))
                SelectedEditGangDealerMenuPreview.Add(line);
        }

        private void LoadSelectedGangDenInventoryPreview(string denMenuId)
        {
            SelectedEditGangDenInventoryItemsPreview.Clear();

            if (string.IsNullOrWhiteSpace(denMenuId))
                return;

            var preview = new LSR.XmlHelper.Core.Services.Reading.ShopMenuDetailedPreviewReadService();
            foreach (var line in preview.GetShopMenuItemDetailLinesForMenuId(_rootFolderPath, denMenuId))
                SelectedEditGangDenInventoryItemsPreview.Add(line);
        }
        private void LoadSelectedGangDenInventoryEditable(string denMenuId)
        {
            DenInventoryItems.Clear();

            var menuId = (denMenuId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(menuId))
                return;

            GenerateDenInventoryMenu = true;

            var reader = new LSR.XmlHelper.Core.Services.Reading.ShopMenuDenInventoryReadService();
            var items = reader.GetDenInventoryItemsForMenuId(_rootFolderPath, menuId);

            if (items.Count == 0)
                return;

            if (_modItemCategories.Count == 0)
                RefreshDenInventoryItemNames();

            foreach (var item in items)
            {
                var resolvedCategory = "Other";

                var name = (item.ModItemName ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (_modItemCategories.TryGetValue(name, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
                        resolvedCategory = mapped.Trim();
                }

                DenInventoryItems.Add(new DenInventoryMenuItemViewModel
                {
                    ModItemName = item.ModItemName ?? "",
                    PurchasePrice = item.PurchasePrice,
                    SalesPrice = item.SalesPrice,
                    MinimumPurchaseAmount = item.MinimumPurchaseAmount,
                    MaximumPurchaseAmount = item.MaximumPurchaseAmount,
                    PurchaseIncrement = item.PurchaseIncrement,
                    NumberOfItemsToSellToPlayer = item.NumberOfItemsToSellToPlayer,
                    NumberOfItemsToPurchaseFromPlayer = item.NumberOfItemsToPurchaseFromPlayer,
                    IsIllicilt = item.IsIllicilt,
                    IsFree = item.IsFree,
                    SubPrice = item.SubPrice,
                    SubAmount = item.SubAmount,
                    NumberOfItemsSoldToPlayer = item.NumberOfItemsSoldToPlayer,
                    NumberOfItemsPurchasedByPlayer = item.NumberOfItemsPurchasedByPlayer,
                    Category = resolvedCategory
                });
            }

            SelectedDenInventoryItem = DenInventoryItems.FirstOrDefault();
            DenInventoryItemsView?.Refresh();
            RefreshTaskState();
        }

        private void SmartPasteCustomTerritoryBoundaries()
        {
            string text;
            try
            {
                text = System.Windows.Clipboard.GetText();
            }
            catch
            {
                System.Windows.MessageBox.Show("Unable to read clipboard text.", "Gang Builder");
                return;
            }

            var parser = new LSR.XmlHelper.Wpf.Services.Parsing.SmartVector2PasteParser();
            if (!parser.TryParseManyXy(text, out var points))
            {
                System.Windows.MessageBox.Show("Clipboard did not contain 3+ X,Y points. You can paste multiple lines. Each line can be any format as long as it contains numbers for X and Y.", "Gang Builder");
                return;
            }

            var formatted = string.Join("\r\n", points.Select(p => $"{p.X.ToString("0.################", CultureInfo.InvariantCulture)},{p.Y.ToString("0.################", CultureInfo.InvariantCulture)}"));
            CustomTerritoryBoundariesText = formatted;
        }
        private void LoadCustomTerritoryEditorFromDefinition(LSR.XmlHelper.Core.Services.Builders.Zones.ZoneDefinition def)
        {
            if (def is null)
                return;

            CustomTerritoryInternalGameName = def.InternalGameName ?? "";
            CustomTerritoryDisplayName = def.DisplayName ?? "";
            CustomTerritoryCountyId = def.CountyID ?? "";
            CustomTerritoryState = def.StateID ?? "";
            CustomTerritoryEconomy = def.Economy ?? "";
            CustomTerritoryType = def.Type ?? "";
            CustomTerritoryIsRestrictedDuringWanted = def.IsRestrictedDuringWanted;
            CustomTerritoryIsSpecificLocation = def.IsSpecificLocation;

            var boundaries = def.Boundaries ?? Array.Empty<LSR.XmlHelper.Core.Services.Builders.Zones.ZoneBoundaryPoint>();
            var formatted = string.Join("\r\n", boundaries.Select(p => $"{p.X.ToString("0.################", CultureInfo.InvariantCulture)},{p.Y.ToString("0.################", CultureInfo.InvariantCulture)}"));
            CustomTerritoryBoundariesText = formatted;

            AddCustomTerritoryCommand.RaiseCanExecuteChanged();
            RemoveSelectedCustomTerritoryCommand.RaiseCanExecuteChanged();
        }

        private bool TryParseCustomTerritoryBoundaries(string boundariesText, out List<LSR.XmlHelper.Core.Services.Builders.Zones.ZoneBoundaryPoint> points)
        {
            points = new List<LSR.XmlHelper.Core.Services.Builders.Zones.ZoneBoundaryPoint>();

            var raw = boundariesText ?? "";
            var lines = raw.Replace("\r\n", "\n").Split('\n');

            foreach (var ln in lines)
            {
                var line = (ln ?? "").Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    return false;

                if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                    return false;

                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                    return false;

                points.Add(new LSR.XmlHelper.Core.Services.Builders.Zones.ZoneBoundaryPoint(x, y));
            }

            return points.Count >= 3;
        }

        private bool CanAddCustomTerritory()
        {
            return IncludeZones;
        }

        private void AddCustomTerritory()
        {
            var internalName = (CustomTerritoryInternalGameName ?? "").Trim();
            var displayName = (CustomTerritoryDisplayName ?? "").Trim();
            var countyId = (CustomTerritoryCountyId ?? "").Trim();
            var stateId = (CustomTerritoryState ?? "").Trim();
            var economy = (CustomTerritoryEconomy ?? "").Trim();
            var type = (CustomTerritoryType ?? "").Trim();

            if (string.IsNullOrWhiteSpace(internalName) || string.IsNullOrWhiteSpace(displayName))
            {
                System.Windows.MessageBox.Show("Custom territory requires InternalGameName and DisplayName.", "Gang Builder");
                return;
            }

            if (string.IsNullOrWhiteSpace(countyId) || string.IsNullOrWhiteSpace(stateId))
            {
                System.Windows.MessageBox.Show("Custom territory requires CountyID and StateID.", "Gang Builder");
                return;
            }

            if (string.IsNullOrWhiteSpace(economy) || string.IsNullOrWhiteSpace(type))
            {
                System.Windows.MessageBox.Show("Custom territory requires Economy and Type.", "Gang Builder");
                return;
            }

            if (!TryParseCustomTerritoryBoundaries(CustomTerritoryBoundariesText, out var points))
            {
                System.Windows.MessageBox.Show("Custom territory Boundaries must be 3+ lines of X,Y points using invariant decimals like: -2491.46,1955.868", "Gang Builder");
                return;
            }

            var def = new LSR.XmlHelper.Core.Services.Builders.Zones.ZoneDefinition(
                internalName,
                displayName,
                countyId,
                stateId,
                CustomTerritoryIsRestrictedDuringWanted,
                CustomTerritoryIsSpecificLocation,
                economy,
                type,
                points);


            var vm = new LSR.XmlHelper.Wpf.ViewModels.Builders.Zones.CustomTerritoryToAddViewModel(def);
            CustomTerritoriesToAdd.Add(vm);
            SelectedCustomTerritoryToAdd = vm;

            var zoneOption = new ZoneOptionViewModel(internalName, displayName, "", "", "");
            Zones.Add(zoneOption);
            SelectedZones.Add(zoneOption);

            CustomTerritoryInternalGameName = "";
            CustomTerritoryDisplayName = "";
            CustomTerritoryCountyId = "";
            CustomTerritoryState = "";
            CustomTerritoryEconomy = "";
            CustomTerritoryType = "";
            CustomTerritoryIsRestrictedDuringWanted = false;
            CustomTerritoryIsSpecificLocation = true;
            CustomTerritoryBoundariesText = "";

            AddCustomTerritoryCommand.RaiseCanExecuteChanged();
            RemoveSelectedCustomTerritoryCommand.RaiseCanExecuteChanged();
            RefreshTaskState();
        }

        private bool CanRemoveSelectedCustomTerritory()
        {
            return IncludeZones && SelectedCustomTerritoryToAdd is not null;
        }

        private void RemoveSelectedCustomTerritory()
        {
            if (SelectedCustomTerritoryToAdd is null)
                return;

            var toRemove = SelectedCustomTerritoryToAdd;
            CustomTerritoriesToAdd.Remove(toRemove);

            SelectedCustomTerritoryToAdd = CustomTerritoriesToAdd.LastOrDefault();

            AddCustomTerritoryCommand.RaiseCanExecuteChanged();
            RemoveSelectedCustomTerritoryCommand.RaiseCanExecuteChanged();
            RefreshTaskState();
        }

        private void RefreshTerritoryCurrentSetup()
        {
            try
            {
                TerritoryCurrentSetupText = "";
                TerritoryCurrentSetupHasData = false;

                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                if (SelectedZones.Count == 0)
                    return;

                var zoneInternalNames = SelectedZones
                    .Select(x => x.InternalGameName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (zoneInternalNames.Length == 0)
                    return;

                var summary = new LSR.XmlHelper.Core.Services.Builders.TerritoryMenuSetupSummaryService();
                var text = summary.Build(_rootFolderPath, zoneInternalNames);

                TerritoryCurrentSetupText = text ?? "";
                TerritoryCurrentSetupHasData = !string.IsNullOrWhiteSpace(TerritoryCurrentSetupText);
            }
            catch
            {
                TerritoryCurrentSetupText = "";
                TerritoryCurrentSetupHasData = false;
            }
        }
        private void ResetDispatchablePeopleEntries()
        {
            LoadDispatchablePeopleEntries();
            ResetDispatchablePeopleEntriesCommand.RaiseCanExecuteChanged();
        }

        private bool CanResetDispatchablePeopleEntries()
        {
            if (UseSourceGangPeopleGroup && !IsEditExistingGang)
                return false;

            return SelectedDispatchablePeopleGroup is not null;
        }

        private void AddDispatchablePersonEntry()
        {
            if (UseSourceGangPeopleGroup && !IsEditExistingGang)
                return;

            var template = SelectedDispatchablePersonEntry ?? DispatchablePeopleEntries.FirstOrDefault();
            if (template is null)
            {
                LoadDispatchablePeopleEntries();
                template = SelectedDispatchablePersonEntry ?? DispatchablePeopleEntries.FirstOrDefault();
            }

            if (template is null)
                return;

            var cloner = new LSR.XmlHelper.Wpf.Services.Builders.Dispatchables.DispatchablePersonEntryCloneService();
            var suggested = cloner.SuggestNextDebugName(DispatchablePeopleEntries, template.DebugName);
            var created = cloner.Clone(template, suggested);
            DispatchablePeopleEntries.Add(created);
            SelectedDispatchablePersonEntry = created;

            RemoveSelectedDispatchablePersonEntryCommand.RaiseCanExecuteChanged();
            DuplicateSelectedDispatchablePersonEntryCommand.RaiseCanExecuteChanged();
        }

        private void DuplicateSelectedDispatchablePersonEntry()
        {
            if (!HasSelectedDispatchablePersonEntry())
                return;

            if (SelectedDispatchablePersonEntry is null)
                return;

            var index = DispatchablePeopleEntries.IndexOf(SelectedDispatchablePersonEntry);
            if (index < 0)
                index = DispatchablePeopleEntries.Count - 1;

            var cloner = new LSR.XmlHelper.Wpf.Services.Builders.Dispatchables.DispatchablePersonEntryCloneService();
            var suggested = cloner.SuggestNextDebugName(DispatchablePeopleEntries, SelectedDispatchablePersonEntry.DebugName);
            var created = cloner.Clone(SelectedDispatchablePersonEntry, suggested);

            if (index + 1 >= DispatchablePeopleEntries.Count)
                DispatchablePeopleEntries.Add(created);
            else
                DispatchablePeopleEntries.Insert(index + 1, created);

            SelectedDispatchablePersonEntry = created;
        }

        private void RemoveSelectedDispatchablePersonEntry()
        {
            if (!HasSelectedDispatchablePersonEntry())
                return;

            if (SelectedDispatchablePersonEntry is null)
                return;

            var index = DispatchablePeopleEntries.IndexOf(SelectedDispatchablePersonEntry);
            if (index < 0)
                return;

            DispatchablePeopleEntries.RemoveAt(index);

            if (DispatchablePeopleEntries.Count == 0)
            {
                SelectedDispatchablePersonEntry = null;
                SelectedDispatchablePersonField = null;
                UpdateDispatchablePersonFieldsView();
            }
            else
            {
                var nextIndex = Math.Min(index, DispatchablePeopleEntries.Count - 1);
                SelectedDispatchablePersonEntry = DispatchablePeopleEntries[nextIndex];
            }

            RemoveSelectedDispatchablePersonEntryCommand.RaiseCanExecuteChanged();
            DuplicateSelectedDispatchablePersonEntryCommand.RaiseCanExecuteChanged();
        }

        private bool HasSelectedDispatchablePersonEntry()
        {
            if (UseSourceGangPeopleGroup && !IsEditExistingGang)
                return false;

            return SelectedDispatchablePersonEntry is not null;
        }
        private void SelectedDispatchablePersonFieldOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not DispatchablePersonFieldViewModel field)
                return;

            if (!string.Equals(e.PropertyName ?? "", nameof(DispatchablePersonFieldViewModel.Value), StringComparison.OrdinalIgnoreCase))
                return;

            if (!string.Equals(field.Name ?? "", "RequiredVariation", StringComparison.OrdinalIgnoreCase))
                return;

            ApplyAppearanceLockFromRequiredVariation();
        }

        private void ApplyAppearanceLockFromRequiredVariation()
        {
            if (SelectedDispatchablePersonEntry is null)
                return;

            SetSelectedDispatchablePersonFieldValue("RandomizeHead", "false");
            SetSelectedDispatchablePersonFieldValue("OptionalPropChance", "0");
            SetSelectedDispatchablePersonFieldValue("OptionalComponentChance", "0");
            SetSelectedDispatchablePersonFieldValue("OptionalComponents", "");
            SetSelectedDispatchablePersonFieldValue("OptionalProps", "");
            SetSelectedDispatchablePersonFieldValue("OptionalAppliedOverlayLogic", "");
        }

        private void SetSelectedDispatchablePersonFieldValue(string fieldName, string value)
        {
            if (SelectedDispatchablePersonEntry is null)
                return;

            var field = SelectedDispatchablePersonEntry.Fields.FirstOrDefault(x => string.Equals(x.Name ?? "", fieldName, StringComparison.OrdinalIgnoreCase));
            if (field is null)
                return;

            field.Value = value;
        }

        private bool HasSelectedLoanParameter() => SelectedLoanParameter is not null;

        private void AddLoanParameter()
        {
            var vm = new LoanParameterEntryViewModel();
            _loanParameters.Add(vm);
            SelectedLoanParameter = vm;
        }

        private void DuplicateSelectedLoanParameter()
        {
            if (SelectedLoanParameter is null)
                return;

            var clone = SelectedLoanParameter.Clone();
            _loanParameters.Add(clone);
            SelectedLoanParameter = clone;
        }

        private void RemoveSelectedLoanParameter()
        {
            if (SelectedLoanParameter is null)
                return;

            var toRemove = SelectedLoanParameter;
            SelectedLoanParameter = null;
            _loanParameters.Remove(toRemove);
        }

        private void ResetLoanParameters()
        {
            SelectedLoanParameter = null;
            _loanParameters.Clear();
        }

        private void LoadSelectedGangAdvancedSettings(string gangId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rootFolderPath))
                    return;

                if (string.IsNullOrWhiteSpace(gangId))
                    return;

                var reader = new LSR.XmlHelper.Core.Services.Builders.GangEditSnapshotReadService();
                var snapshot = reader.TryGet(_rootFolderPath, gangId);

                if (snapshot is null)
                    return;

                MinimumRep = snapshot.MinimumRep;
                MaximumRep = snapshot.MaximumRep;
                StartingRep = snapshot.StartingRep;
                HostileRepLevel = snapshot.HostileRepLevel;
                NeutralRepLevel = snapshot.NeutralRepLevel;
                FriendlyRepLevel = snapshot.FriendlyRepLevel;
                MemberOfferRepLevel = snapshot.MemberOfferRepLevel;
                HitSquadRep = snapshot.HitSquadRep;

                PickupPaymentMin = snapshot.PickupPaymentMin;
                PickupPaymentMax = snapshot.PickupPaymentMax;
                TheftPaymentMin = snapshot.TheftPaymentMin;
                TheftPaymentMax = snapshot.TheftPaymentMax;
                HitPaymentMin = snapshot.HitPaymentMin;
                HitPaymentMax = snapshot.HitPaymentMax;
                DeliveryPaymentMin = snapshot.DeliveryPaymentMin;
                DeliveryPaymentMax = snapshot.DeliveryPaymentMax;
                WheelmanPaymentMin = snapshot.WheelmanPaymentMin;
                WheelmanPaymentMax = snapshot.WheelmanPaymentMax;
                ImpoundTheftPaymentMin = snapshot.ImpoundTheftPaymentMin;
                ImpoundTheftPaymentMax = snapshot.ImpoundTheftPaymentMax;
                BodyDisposalPaymentMin = snapshot.BodyDisposalPaymentMin;
                BodyDisposalPaymentMax = snapshot.BodyDisposalPaymentMax;
                CopHitPaymentMin = snapshot.CopHitPaymentMin;
                CopHitPaymentMax = snapshot.CopHitPaymentMax;
                AmbushPaymentMin = snapshot.AmbushPaymentMin;
                AmbushPaymentMax = snapshot.AmbushPaymentMax;
                BriberyPaymentMin = snapshot.BriberyPaymentMin;
                BriberyPaymentMax = snapshot.BriberyPaymentMax;
                ArsonPaymentMin = snapshot.ArsonPaymentMin;
                ArsonPaymentMax = snapshot.ArsonPaymentMax;

                FightPercentage = snapshot.FightPercentage;
                FightPolicePercentage = snapshot.FightPolicePercentage;
                AlwaysFightPolicePercentage = snapshot.AlwaysFightPolicePercentage;
                DrugDealerPercentage = snapshot.DrugDealerPercentage;

                AmbientMemberMoneyMin = snapshot.AmbientMemberMoneyMin;
                AmbientMemberMoneyMax = snapshot.AmbientMemberMoneyMax;
                DealerMemberMoneyMin = snapshot.DealerMemberMoneyMin;
                DealerMemberMoneyMax = snapshot.DealerMemberMoneyMax;
                CostToPayoffGangScalar = snapshot.CostToPayoffGangScalar;

                PercentageTrustingOfPlayer = snapshot.PercentageTrustingOfPlayer;
                PercentageWithLongGuns = snapshot.PercentageWithLongGuns;
                PercentageWithSidearms = snapshot.PercentageWithSidearms;
                PercentageWithMelee = snapshot.PercentageWithMelee;
                VehicleSpawnPercentage = snapshot.VehicleSpawnPercentage;
                PedestrianSpawnPercentageAroundDen = snapshot.PedestrianSpawnPercentageAroundDen;

                MemberKickUpDays = snapshot.MemberKickUpDays;
                MemberKickUpAmount = snapshot.MemberKickUpAmount;
                MemberKickUpMissLimit = snapshot.MemberKickUpMissLimit;

                _loanParameters.Clear();
                SelectedLoanParameter = null;

                foreach (var lp in snapshot.LoanParameters)
                {
                    _loanParameters.Add(new LoanParameterEntryViewModel(
                        lp.ResepectLevel,
                        lp.Rate,
                        lp.MaxPeriods,
                        lp.MinAmount,
                        lp.MaxAmount));
                }
            }
            catch
            {
            }
        }

        private void ApplyGangColorToResult(XDocument gangsDoc)
        {
            if (gangsDoc is null)
                return;

            var gangId = (NewGangId ?? "").Trim();

            var gangNode = string.IsNullOrWhiteSpace(gangId)
                ? gangsDoc.Descendants("Gang").FirstOrDefault()
                : gangsDoc.Descendants("Gang").FirstOrDefault(x => string.Equals((x.Element("ID")?.Value ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase));

            if (gangNode is null)
                return;

            var prefix = (GangColorPrefix ?? "").Trim();
            var color = (GangColorString ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(prefix))
                SetOrCreate(gangNode, "ColorPrefix", prefix);

            if (!string.IsNullOrWhiteSpace(color))
                SetOrCreate(gangNode, "ColorString", color);
        }

        private void ApplyAdvancedGangSettingsToGangNode(System.Xml.Linq.XElement gangNode)
        {
            if (gangNode is null)
                return;

            ApplyAdvancedGangScalarFieldsToGangNode(gangNode);
            ApplyAdvancedGangLoanParametersToGangNode(gangNode);
        }

        private void ApplyAdvancedGangScalarFieldsToGangNode(System.Xml.Linq.XElement gangNode)
        {
            void Apply(string fieldName, string value)
            {
                var trimmed = (value ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    SetOrUpdateGangField(gangNode, fieldName, trimmed);
            }

            Apply("MinimumRep", MinimumRep);
            Apply("MaximumRep", MaximumRep);
            Apply("StartingRep", StartingRep);
            Apply("HostileRepLevel", HostileRepLevel);
            Apply("NeutralRepLevel", NeutralRepLevel);
            Apply("FriendlyRepLevel", FriendlyRepLevel);
            Apply("MemberOfferRepLevel", MemberOfferRepLevel);
            Apply("HitSquadRep", HitSquadRep);

            Apply("PickupPaymentMin", PickupPaymentMin);
            Apply("PickupPaymentMax", PickupPaymentMax);
            Apply("TheftPaymentMin", TheftPaymentMin);
            Apply("TheftPaymentMax", TheftPaymentMax);
            Apply("HitPaymentMin", HitPaymentMin);
            Apply("HitPaymentMax", HitPaymentMax);
            Apply("DeliveryPaymentMin", DeliveryPaymentMin);
            Apply("DeliveryPaymentMax", DeliveryPaymentMax);
            Apply("WheelmanPaymentMin", WheelmanPaymentMin);
            Apply("WheelmanPaymentMax", WheelmanPaymentMax);
            Apply("ImpoundTheftPaymentMin", ImpoundTheftPaymentMin);
            Apply("ImpoundTheftPaymentMax", ImpoundTheftPaymentMax);
            Apply("BodyDisposalPaymentMin", BodyDisposalPaymentMin);
            Apply("BodyDisposalPaymentMax", BodyDisposalPaymentMax);
            Apply("CopHitPaymentMin", CopHitPaymentMin);
            Apply("CopHitPaymentMax", CopHitPaymentMax);
            Apply("AmbushPaymentMin", AmbushPaymentMin);
            Apply("AmbushPaymentMax", AmbushPaymentMax);
            Apply("BriberyPaymentMin", BriberyPaymentMin);
            Apply("BriberyPaymentMax", BriberyPaymentMax);
            Apply("ArsonPaymentMin", ArsonPaymentMin);
            Apply("ArsonPaymentMax", ArsonPaymentMax);

            Apply("FightPercentage", FightPercentage);
            Apply("FightPolicePercentage", FightPolicePercentage);
            Apply("AlwaysFightPolicePercentage", AlwaysFightPolicePercentage);
            Apply("DrugDealerPercentage", DrugDealerPercentage);

            Apply("AmbientMemberMoneyMin", AmbientMemberMoneyMin);
            Apply("AmbientMemberMoneyMax", AmbientMemberMoneyMax);
            Apply("DealerMemberMoneyMin", DealerMemberMoneyMin);
            Apply("DealerMemberMoneyMax", DealerMemberMoneyMax);
            Apply("CostToPayoffGangScalar", CostToPayoffGangScalar);

            Apply("PercentageTrustingOfPlayer", PercentageTrustingOfPlayer);
            Apply("PercentageWithLongGuns", PercentageWithLongGuns);
            Apply("PercentageWithSidearms", PercentageWithSidearms);
            Apply("PercentageWithMelee", PercentageWithMelee);
            Apply("VehicleSpawnPercentage", VehicleSpawnPercentage);
            Apply("PedestrianSpawnPercentageAroundDen", PedestrianSpawnPercentageAroundDen);

            Apply("MemberKickUpDays", MemberKickUpDays);
            Apply("MemberKickUpAmount", MemberKickUpAmount);
            Apply("MemberKickUpMissLimit", MemberKickUpMissLimit);
        }

        private void ApplyAdvancedGangLoanParametersToGangNode(System.Xml.Linq.XElement gangNode)
        {
            if (gangNode is null)
                return;

            var loanParametersRoot = gangNode.Element("LoanParameters");
            if (loanParametersRoot is null)
            {
                loanParametersRoot = new System.Xml.Linq.XElement("LoanParameters");
                gangNode.Add(loanParametersRoot);
            }

            var loanParameterList = loanParametersRoot.Element("LoanParamterList");
            if (loanParameterList is null)
            {
                loanParameterList = new System.Xml.Linq.XElement("LoanParamterList");
                loanParametersRoot.Add(loanParameterList);
            }

            loanParameterList.Elements("LoanParameter").Remove();

            foreach (var row in LoanParameters)
            {
                var resepectLevel = (row?.ResepectLevel ?? "").Trim();
                var rate = (row?.Rate ?? "").Trim();
                var maxPeriods = (row?.MaxPeriods ?? "").Trim();
                var minAmount = (row?.MinAmount ?? "").Trim();
                var maxAmount = (row?.MaxAmount ?? "").Trim();

                if (string.IsNullOrWhiteSpace(resepectLevel)
                    && string.IsNullOrWhiteSpace(rate)
                    && string.IsNullOrWhiteSpace(maxPeriods)
                    && string.IsNullOrWhiteSpace(minAmount)
                    && string.IsNullOrWhiteSpace(maxAmount))
                    continue;

                var node = new System.Xml.Linq.XElement("LoanParameter",
                    new System.Xml.Linq.XElement("ResepectLevel", resepectLevel),
                    new System.Xml.Linq.XElement("Rate", rate),
                    new System.Xml.Linq.XElement("MaxPeriods", maxPeriods),
                    new System.Xml.Linq.XElement("MinAmount", minAmount),
                    new System.Xml.Linq.XElement("MaxAmount", maxAmount));

                loanParameterList.Add(node);
            }
        }

        private bool TryGetClipboardXyzHeading(out double x, out double y, out double z, out double heading)
        {
            x = 0;
            y = 0;
            z = 0;
            heading = 0;

            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : "";
            return _smartCoordinatePasteParser.TryParseFirstXyzHeading(text, out x, out y, out z, out heading);
        }
    }
}

