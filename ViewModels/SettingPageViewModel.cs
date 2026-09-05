using Avalonia.Media;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using Classcaller.Models;
using Classcaller;
using Classcaller.Helpers;
using Classcaller.Services;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Classcaller.ViewModels;

public class SettingPageViewModel : ReactiveObject
{
    // 基本设置
    private bool _isBreakDisable;
    public bool IsBreakDisable
    {
        get => _isBreakDisable;
        set => this.RaiseAndSetIfChanged(ref _isBreakDisable, value);
    }

    private bool _interruptable;
    public bool Interruptable
    {
        get => _interruptable;
        set => this.RaiseAndSetIfChanged(ref _interruptable, value);
    }

    // 悬浮窗设置
    private bool _isHoverEnable;
    public bool IsHoverEnable
    {
        get => _isHoverEnable;
        set => this.RaiseAndSetIfChanged(ref _isHoverEnable, value);
    }

    private double _hoverScalingFactor;
    public double HoverScalingFactor
    {
        get => _hoverScalingFactor;
        set => this.RaiseAndSetIfChanged(ref _hoverScalingFactor, value);
    }

    private int _hoverLayout;
    public int HoverLayout
    {
        get => _hoverLayout;
        set => this.RaiseAndSetIfChanged(ref _hoverLayout, Math.Clamp(value, 0, 2));
    }

    private int _hoverTheme;
    public int HoverTheme
    {
        get => _hoverTheme;
        set => this.RaiseAndSetIfChanged(ref _hoverTheme, Math.Clamp(value, 0, 1));
    }

    // 点名设置
    private int _notifyMethod;
    public int NotifyMethod
    {
        get => _notifyMethod;
        set
        {
            int notifyMethod = value & 0b11;
            if (_notifyMethod == notifyMethod)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _notifyMethod, notifyMethod);
            this.RaisePropertyChanged(nameof(IsClassIslandNotificationEnabled));
            this.RaisePropertyChanged(nameof(IsClasscallerNotificationEnabled));
        }
    }

    public bool IsClassIslandNotificationEnabled
    {
        get => (NotifyMethod & 0b01) != 0;
        set => NotifyMethod = value ? NotifyMethod | 0b01 : NotifyMethod & ~0b01;
    }

    public bool IsClasscallerNotificationEnabled
    {
        get => (NotifyMethod & 0b10) != 0;
        set => NotifyMethod = value ? NotifyMethod | 0b10 : NotifyMethod & ~0b10;
    }

    private int _showerTheme;
    public int ShowerTheme
    {
        get => _showerTheme;
        set => this.RaiseAndSetIfChanged(ref _showerTheme, Math.Clamp(value, 0, 1));
    }

    private float _baseTime = 2.0f;
    public float BaseTime
    {
        get => _baseTime;
        set => this.RaiseAndSetIfChanged(ref _baseTime, value);
    }

    private float _additionalTime = 1.0f;
    public float AdditionalTime
    {
        get => _additionalTime;
        set => this.RaiseAndSetIfChanged(ref _additionalTime, value);
    }

    // TTS 设置
    private TtsProvider _provider;
    public TtsProvider Provider
    {
        get => _provider;
        set => this.RaiseAndSetIfChanged(ref _provider, value);
    }

    public IReadOnlyList<TtsProvider> TtsProviders { get; } = Enum.GetValues<TtsProvider>();

    private string _beforeText = string.Empty;
    public string BeforeText
    {
        get => _beforeText;
        set => this.RaiseAndSetIfChanged(ref _beforeText, value);
    }

    private string _afterText = string.Empty;
    public string AfterText
    {
        get => _afterText;
        set => this.RaiseAndSetIfChanged(ref _afterText, value);
    }

    private string _exampleText = "{学生姓名}";
    public string ExampleText
    {
        get => _exampleText;
        set => this.RaiseAndSetIfChanged(ref _exampleText, value);
    }

    // 算法设置（自定义点名算法参数）
    private double _halfRecoveryDistance;
    public double HalfRecoveryDistance
    {
        get => _halfRecoveryDistance;
        set => this.RaiseAndSetIfChanged(ref _halfRecoveryDistance, value);
    }

    private double _curvePower;
    public double CurvePower
    {
        get => _curvePower;
        set => this.RaiseAndSetIfChanged(ref _curvePower, value);
    }

    private double _gamma;
    public double Gamma
    {
        get => _gamma;
        set => this.RaiseAndSetIfChanged(ref _gamma, value);
    }

    private double _rMin;
    public double RMin
    {
        get => _rMin;
        set => this.RaiseAndSetIfChanged(ref _rMin, value);
    }

    private double _rMax;
    public double RMax
    {
        get => _rMax;
        set => this.RaiseAndSetIfChanged(ref _rMax, value);
    }

    // 外观设置（草稿：编辑后点「保存修改」才生效）
    private Color _accentColorDraft = Color.Parse("#0078D4");
    public Color AccentColorDraft
    {
        get => _accentColorDraft;
        set => this.RaiseAndSetIfChanged(ref _accentColorDraft, value);
    }

    // 结果文字色（HasResultTextColorDraft=false 表示自动按背景亮度选黑白）
    private bool _hasResultTextColorDraft;
    public bool HasResultTextColorDraft
    {
        get => _hasResultTextColorDraft;
        set => this.RaiseAndSetIfChanged(ref _hasResultTextColorDraft, value);
    }

    private Color _resultTextColorDraft = Colors.Black;
    public Color ResultTextColorDraft
    {
        get => _resultTextColorDraft;
        set => this.RaiseAndSetIfChanged(ref _resultTextColorDraft, value);
    }

    // 结果背景色（HasResultBackgroundDraft=false 表示透明）
    private bool _hasResultBackgroundDraft;
    public bool HasResultBackgroundDraft
    {
        get => _hasResultBackgroundDraft;
        set => this.RaiseAndSetIfChanged(ref _hasResultBackgroundDraft, value);
    }

    private Color _resultBackgroundDraft = Colors.Transparent;
    public Color ResultBackgroundDraft
    {
        get => _resultBackgroundDraft;
        set => this.RaiseAndSetIfChanged(ref _resultBackgroundDraft, value);
    }

    private string _hoverTextDraft = string.Empty;
    public string HoverTextDraft
    {
        get => _hoverTextDraft;
        set => this.RaiseAndSetIfChanged(ref _hoverTextDraft, value);
    }

    private string _hoverImagePathDraft = string.Empty;
    public string HoverImagePathDraft
    {
        get => _hoverImagePathDraft;
        set => this.RaiseAndSetIfChanged(ref _hoverImagePathDraft, value);
    }

    private string _resultImagePathDraft = string.Empty;
    public string ResultImagePathDraft
    {
        get => _resultImagePathDraft;
        set => this.RaiseAndSetIfChanged(ref _resultImagePathDraft, value);
    }

    private string _fontFamilyDraft = "HarmonyOS Sans SC";
    public string FontFamilyDraft
    {
        get => _fontFamilyDraft;
        set => this.RaiseAndSetIfChanged(ref _fontFamilyDraft, value);
    }

    private double _resultFontSizeDraft = 60;
    public double ResultFontSizeDraft
    {
        get => _resultFontSizeDraft;
        set => this.RaiseAndSetIfChanged(ref _resultFontSizeDraft, value);
    }

    /// <summary>系统字体名列表，供「字体」下拉框选择。</summary>
    public IReadOnlyList<string> FontFamilies { get; } = GetSystemFontFamilies();

    /// <summary>图片字段可选的内置选项（无 + 内置图标）。</summary>
    public IReadOnlyList<ImageOption> ImageOptions => BuiltinImages.Options;

    // 安全设置（双密码：查看密码 + 修改密码，逻辑互不关联）
    private bool _isViewPasswordEnabled;
    public bool IsViewPasswordEnabled
    {
        get => _isViewPasswordEnabled;
        set => this.RaiseAndSetIfChanged(ref _isViewPasswordEnabled, value);
    }

    private bool _isEditPasswordEnabled;
    public bool IsEditPasswordEnabled
    {
        get => _isEditPasswordEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEditPasswordEnabled, value);
    }

    private bool _isViewUnlocked = true;
    public bool IsViewUnlocked
    {
        get => _isViewUnlocked;
        private set
        {
            if (this.RaiseAndSetIfChanged(ref _isViewUnlocked, value))
            {
                this.RaisePropertyChanged(nameof(IsViewLocked));
            }
        }
    }

    /// <summary>查看密码是否未通过（进入设置页时锁定）。</summary>
    public bool IsViewLocked => !IsViewUnlocked;

    private bool _isEditUnlocked = true;
    public bool IsEditUnlocked
    {
        get => _isEditUnlocked;
        private set
        {
            if (this.RaiseAndSetIfChanged(ref _isEditUnlocked, value))
            {
                this.RaisePropertyChanged(nameof(IsEditLocked));
            }
        }
    }

    /// <summary>修改密码是否未通过（配置项处于只读状态）。</summary>
    public bool IsEditLocked => !IsEditUnlocked;

    // 档案设置
    private ObservableCollection<ProfileItemViewModel> _profileItems = new();
    public ObservableCollection<ProfileItemViewModel> ProfileItems
    {
        get => _profileItems;
        private set => this.RaiseAndSetIfChanged(ref _profileItems, value);
    }

    private ProfileItemViewModel? _selectedProfile;
    public ProfileItemViewModel? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProfile, value);
            if (value is null || value.ProfileId == Settings.Instance.Profile.DefaultProfile)
            {
                return;
            }

            var previousProfile = Settings.Instance.Profile.DefaultProfile;
            try
            {
                ProfileRuntimeService.Reload(value.ProfileId);
                Settings.Instance.Profile.DefaultProfile = value.ProfileId;
                ReloadProfiles();
            }
            catch
            {
                Settings.Instance.Profile.DefaultProfile = previousProfile;
                ReloadProfiles();
                throw;
            }
        }
    }

    private bool _isPreferProfile;
    public bool IsPreferProfile
    {
        get => _isPreferProfile;
        set => this.RaiseAndSetIfChanged(ref _isPreferProfile, value);
    }

    private IReadOnlyList<SubjectItemViewModel> _subjectItems = [];
    private ObservableCollection<ProfilePreferenceItemViewModel> _profilePreferenceItems = new();
    public ObservableCollection<ProfilePreferenceItemViewModel> ProfilePreferenceItems
    {
        get => _profilePreferenceItems;
        private set => this.RaiseAndSetIfChanged(ref _profilePreferenceItems, value);
    }

    public ProfileService ProfileService { get; }
    private ProfileRuntimeService ProfileRuntimeService { get; }
    private IProfileService? ClassIslandProfileService { get; }

    public SettingPageViewModel()
    {
        ProfileService = IAppHost.GetService<ProfileService>();
        ProfileRuntimeService = IAppHost.GetService<ProfileRuntimeService>();
        ClassIslandProfileService = IAppHost.TryGetService<IProfileService>();

        IsBreakDisable = Settings.Instance.General.BreakDisable;
        Interruptable = Settings.Instance.General.Interruptable;
        IsHoverEnable = Settings.Instance.Hover.IsEnable;
        HoverScalingFactor = Settings.Instance.Hover.ScalingFactor;
        HoverLayout = Settings.Instance.Hover.HoverLayout;
        HoverTheme = Settings.Instance.Hover.HoverTheme;
        NotifyMethod = Settings.Instance.Call.NotifyMethod;
        ShowerTheme = Settings.Instance.Call.ShowerTheme;
        BaseTime = Settings.Instance.Call.BaseTime;
        AdditionalTime = Settings.Instance.Call.AdditionalTime;
        Provider = Settings.Instance.TTS.Provider;
        BeforeText = Settings.Instance.TTS.BeforeText;
        AfterText = Settings.Instance.TTS.AfterText;
        IsPreferProfile = Settings.Instance.Profile.IsPreferProfile;
        ExampleText = $"{BeforeText}{{学生姓名}}{AfterText}";
        HalfRecoveryDistance = Settings.Instance.Algorithm.HalfRecoveryDistance;
        CurvePower = Settings.Instance.Algorithm.CurvePower;
        Gamma = Settings.Instance.Algorithm.Gamma;
        RMin = Settings.Instance.Algorithm.RMin;
        RMax = Settings.Instance.Algorithm.RMax;
        LoadAppearanceDraft();
        IsViewPasswordEnabled = Settings.Instance.Security.IsViewPasswordEnabled;
        IsEditPasswordEnabled = Settings.Instance.Security.IsEditPasswordEnabled;
        IsViewUnlocked = !IsViewPasswordEnabled;
        IsEditUnlocked = !IsEditPasswordEnabled;
        ReloadProfiles();

        this.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IsBreakDisable))
            {
                Settings.Instance.General.BreakDisable = IsBreakDisable;
            }
            else if (args.PropertyName == nameof(Interruptable))
            {
                Settings.Instance.General.Interruptable = Interruptable;
            }
            else if (args.PropertyName == nameof(IsHoverEnable))
            {
                Settings.Instance.Hover.IsEnable = IsHoverEnable;
            }
            else if (args.PropertyName == nameof(HoverScalingFactor))
            {
                Settings.Instance.Hover.ScalingFactor = HoverScalingFactor;
            }
            else if (args.PropertyName == nameof(HoverLayout))
            {
                Settings.Instance.Hover.HoverLayout = HoverLayout;
            }
            else if (args.PropertyName == nameof(HoverTheme))
            {
                Settings.Instance.Hover.HoverTheme = HoverTheme;
            }
            else if (args.PropertyName == nameof(NotifyMethod))
            {
                Settings.Instance.Call.NotifyMethod = NotifyMethod;
            }
            else if (args.PropertyName == nameof(ShowerTheme))
            {
                Settings.Instance.Call.ShowerTheme = ShowerTheme;
            }
            else if (args.PropertyName == nameof(BaseTime))
            {
                Settings.Instance.Call.BaseTime = BaseTime;
            }
            else if (args.PropertyName == nameof(AdditionalTime))
            {
                Settings.Instance.Call.AdditionalTime = AdditionalTime;
            }
            else if (args.PropertyName == nameof(BeforeText))
            {
                Settings.Instance.TTS.BeforeText = BeforeText;
                ExampleText = $"{BeforeText}{{学生姓名}}{AfterText}";
            }
            else if (args.PropertyName == nameof(Provider))
            {
                Settings.Instance.TTS.Provider = Provider;
            }
            else if (args.PropertyName == nameof(AfterText))
            {
                Settings.Instance.TTS.AfterText = AfterText;
                ExampleText = $"{BeforeText}{{学生姓名}}{AfterText}";
            }
            else if (args.PropertyName == nameof(IsPreferProfile))
            {
                Settings.Instance.Profile.IsPreferProfile = IsPreferProfile;
            }
            else if (args.PropertyName == nameof(HalfRecoveryDistance))
            {
                Settings.Instance.Algorithm.HalfRecoveryDistance = HalfRecoveryDistance;
            }
            else if (args.PropertyName == nameof(CurvePower))
            {
                Settings.Instance.Algorithm.CurvePower = CurvePower;
            }
            else if (args.PropertyName == nameof(Gamma))
            {
                Settings.Instance.Algorithm.Gamma = Gamma;
            }
            else if (args.PropertyName == nameof(RMin))
            {
                Settings.Instance.Algorithm.RMin = RMin;
            }
            else if (args.PropertyName == nameof(RMax))
            {
                Settings.Instance.Algorithm.RMax = RMax;
            }
            else if (args.PropertyName == nameof(IsViewPasswordEnabled))
            {
                Settings.Instance.Security.IsViewPasswordEnabled = IsViewPasswordEnabled;
                if (!IsViewPasswordEnabled)
                {
                    IsViewUnlocked = true;
                }
            }
            else if (args.PropertyName == nameof(IsEditPasswordEnabled))
            {
                Settings.Instance.Security.IsEditPasswordEnabled = IsEditPasswordEnabled;
                if (!IsEditPasswordEnabled)
                {
                    IsEditUnlocked = true;
                }
            }
        };
    }

    /// <summary>尝试用查看密码解锁设置页（通过后可查看全部配置）。</summary>
    public bool TryUnlockView(string password)
    {
        if (Settings.VerifyPassword(password, Settings.Instance.Security.ViewPasswordHash))
        {
            IsViewUnlocked = true;
            return true;
        }
        return false;
    }

    /// <summary>尝试用修改密码解锁配置编辑。</summary>
    public bool TryUnlockEdit(string password)
    {
        if (Settings.VerifyPassword(password, Settings.Instance.Security.EditPasswordHash))
        {
            IsEditUnlocked = true;
            return true;
        }
        return false;
    }

    /// <summary>设置（或修改）查看密码。</summary>
    public void SetViewPassword(string newPassword)
    {
        Settings.Instance.Security.ViewPasswordHash = Settings.HashPassword(newPassword);
        IsViewPasswordEnabled = true;
        IsViewUnlocked = true;
    }

    /// <summary>设置（或修改）修改密码。</summary>
    public void SetEditPassword(string newPassword)
    {
        Settings.Instance.Security.EditPasswordHash = Settings.HashPassword(newPassword);
        IsEditPasswordEnabled = true;
        IsEditUnlocked = true;
    }

    /// <summary>从已保存设置加载外观草稿。</summary>
    private void LoadAppearanceDraft()
    {
        var a = Settings.Instance.Appearance;
        AccentColorDraft = ParseColor(a.AccentColor) ?? Color.Parse("#0078D4");
        HasResultTextColorDraft = !string.IsNullOrWhiteSpace(a.ResultTextColor);
        ResultTextColorDraft = ParseColor(a.ResultTextColor) ?? Colors.Black;
        HasResultBackgroundDraft = !string.IsNullOrWhiteSpace(a.ResultBackground);
        ResultBackgroundDraft = ParseColor(a.ResultBackground) ?? Colors.Transparent;
        HoverTextDraft = a.HoverText;
        HoverImagePathDraft = a.HoverImagePath;
        ResultImagePathDraft = a.ResultImagePath;
        FontFamilyDraft = a.FontFamily;
        ResultFontSizeDraft = a.ResultFontSize;
    }

    /// <summary>把外观草稿提交到设置并持久化（点「保存修改」时调用）。</summary>
    public void SaveAppearance()
    {
        var a = Settings.Instance.Appearance;
        a.AccentColor = ColorToHex(AccentColorDraft);
        a.ResultTextColor = HasResultTextColorDraft ? ColorToHex(ResultTextColorDraft) : string.Empty;
        a.ResultBackground = HasResultBackgroundDraft ? ColorToHex(ResultBackgroundDraft) : string.Empty;
        a.HoverText = HoverTextDraft ?? string.Empty;
        a.HoverImagePath = HoverImagePathDraft ?? string.Empty;
        a.ResultImagePath = ResultImagePathDraft ?? string.Empty;
        a.FontFamily = FontFamilyDraft ?? string.Empty;
        a.ResultFontSize = ResultFontSizeDraft > 0 ? ResultFontSizeDraft : 60;
    }

    private static Color? ParseColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        return Color.TryParse(hex, out var color) ? color : null;
    }

    private static string ColorToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static IReadOnlyList<string> GetSystemFontFamilies()
    {
        try
        {
            return FontManager.Current.SystemFonts
                .Select(font => font.Name)
                .Distinct()
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch
        {
            return ["HarmonyOS Sans SC", "Microsoft YaHei UI", "Segoe UI"];
        }
    }

    public void ReloadProfiles()
    {
        ProfileItems = new ObservableCollection<ProfileItemViewModel>(Settings.Instance.Profile.ProfileList
            .OrderBy(profile => profile.Value)
            .Select(profile => new ProfileItemViewModel(profile.Key, profile.Value,
                profile.Key != Settings.Instance.Profile.DefaultProfile)));
        SelectedProfile = ProfileItems.FirstOrDefault(profile => profile.ProfileId == Settings.Instance.Profile.DefaultProfile);

        ReloadSubjects();
        ReloadProfilePreferenceItems();
    }

    public void AddProfilePreferenceRule()
    {
        var assignedSubjectIds = ProfilePreferenceItems
            .Where(item => item.IsRule)
            .Select(item => item.SubjectId)
            .ToHashSet();
        var subject = _subjectItems.FirstOrDefault(item => !assignedSubjectIds.Contains(item.SubjectId));
        if (subject is null || Settings.Instance.Profile.DefaultProfile == Guid.Empty)
        {
            return;
        }

        ProfilePreferenceItems.Add(ProfilePreferenceItemViewModel.CreateRule(subject.SubjectId,
            Settings.Instance.Profile.DefaultProfile, SynchronizeProfilePreferenceRules));
        SynchronizeProfilePreferenceRules();
    }

    public void RemoveProfilePreferenceRule(ProfilePreferenceItemViewModel item)
    {
        if (!item.IsRule)
        {
            return;
        }

        ProfilePreferenceItems.Remove(item);
        SynchronizeProfilePreferenceRules();
    }

    public int RemoveProfilePreferenceRulesForProfile(Guid profileId)
    {
        var rules = ProfilePreferenceItems
            .Where(item => item.IsRule && item.ProfileId == profileId)
            .ToList();
        foreach (var rule in rules)
        {
            ProfilePreferenceItems.Remove(rule);
        }

        if (rules.Count > 0)
        {
            SynchronizeProfilePreferenceRules();
        }

        return rules.Count;
    }

    public int GetProfilePreferenceRuleCount(Guid profileId)
    {
        return ProfilePreferenceItems.Count(item => item.IsRule && item.ProfileId == profileId);
    }

    private void ReloadSubjects()
    {
        object? classIslandProfile = ClassIslandProfileService?.Profile;
        _subjectItems = ClassIslandSubjectHelper.GetSubjects(classIslandProfile)
            .OrderBy(subject => subject.Name)
            .Select(subject => new SubjectItemViewModel(subject.SubjectId, subject.Name))
            .ToList();
    }

    private void ReloadProfilePreferenceItems()
    {
        var items = new ObservableCollection<ProfilePreferenceItemViewModel>
        {
            ProfilePreferenceItemViewModel.CreateAddAction()
        };
        foreach (var preference in Settings.Instance.Profile.ProfilePrefer.OrderBy(item => GetSubjectName(item.Key)))
        {
            items.Add(ProfilePreferenceItemViewModel.CreateRule(preference.Key, preference.Value,
                SynchronizeProfilePreferenceRules));
        }

        ProfilePreferenceItems = items;
        RefreshProfilePreferenceRuleOptions();
    }

    private void SynchronizeProfilePreferenceRules()
    {
        var preferences = new Dictionary<Guid, Guid>();
        foreach (var rule in ProfilePreferenceItems.Where(item => item.IsRule))
        {
            if (rule.SubjectId != Guid.Empty && rule.ProfileId != Guid.Empty)
            {
                preferences.TryAdd(rule.SubjectId, rule.ProfileId);
            }
        }

        Settings.Instance.Profile.ProfilePrefer = preferences;
        RefreshProfilePreferenceRuleOptions();
    }

    private void RefreshProfilePreferenceRuleOptions()
    {
        var rules = ProfilePreferenceItems.Where(item => item.IsRule).ToList();
        var assignedSubjectIds = rules.Select(item => item.SubjectId).ToHashSet();
        foreach (var rule in rules)
        {
            var subjects = _subjectItems.ToList();
            if (subjects.All(subject => subject.SubjectId != rule.SubjectId))
            {
                subjects.Add(new SubjectItemViewModel(rule.SubjectId, "未知科目"));
            }

            rule.AvailableSubjects = subjects
                .Where(subject => subject.SubjectId == rule.SubjectId || !assignedSubjectIds.Contains(subject.SubjectId))
                .OrderBy(subject => subject.Name)
                .ToList();
            rule.AvailableProfiles = ProfileItems.ToList();
        }

        var addAction = ProfilePreferenceItems.FirstOrDefault(item => item.IsAddAction);
        if (addAction is not null)
        {
            addAction.CanAddRule = ProfileItems.Count > 0 &&
                                   _subjectItems.Any(subject => !assignedSubjectIds.Contains(subject.SubjectId));
        }
    }

    private string GetSubjectName(Guid subjectId)
    {
        return _subjectItems.FirstOrDefault(subject => subject.SubjectId == subjectId)?.Name ?? "未知科目";
    }

    public sealed class ProfileItemViewModel
    {
        public Guid ProfileId { get; }
        public string Name { get; }
        public bool CanDelete { get; }

        public ProfileItemViewModel(Guid profileId, string name, bool canDelete)
        {
            ProfileId = profileId;
            Name = name;
            CanDelete = canDelete;
        }
    }

    public sealed class SubjectItemViewModel
    {
        public Guid SubjectId { get; }
        public string Name { get; }

        public SubjectItemViewModel(Guid subjectId, string name)
        {
            SubjectId = subjectId;
            Name = name;
        }
    }

    public sealed class ProfilePreferenceItemViewModel : ReactiveObject
    {
        private readonly Action? _onRuleChanged;
        private Guid _subjectId;
        private Guid _profileId;
        private bool _canAddRule;
        private IReadOnlyList<SubjectItemViewModel> _availableSubjects = [];
        private IReadOnlyList<ProfileItemViewModel> _availableProfiles = [];

        private ProfilePreferenceItemViewModel(bool isAddAction, Action? onRuleChanged = null)
        {
            IsAddAction = isAddAction;
            _onRuleChanged = onRuleChanged;
        }

        public bool IsAddAction { get; }
        public bool IsRule => !IsAddAction;

        public bool CanAddRule
        {
            get => _canAddRule;
            internal set => this.RaiseAndSetIfChanged(ref _canAddRule, value);
        }

        public Guid SubjectId
        {
            get => _subjectId;
            set
            {
                if (_subjectId == value)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _subjectId, value);
                _onRuleChanged?.Invoke();
            }
        }

        public Guid ProfileId
        {
            get => _profileId;
            set
            {
                if (_profileId == value)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _profileId, value);
                _onRuleChanged?.Invoke();
            }
        }

        public IReadOnlyList<SubjectItemViewModel> AvailableSubjects
        {
            get => _availableSubjects;
            internal set => this.RaiseAndSetIfChanged(ref _availableSubjects, value);
        }

        public IReadOnlyList<ProfileItemViewModel> AvailableProfiles
        {
            get => _availableProfiles;
            internal set => this.RaiseAndSetIfChanged(ref _availableProfiles, value);
        }

        public static ProfilePreferenceItemViewModel CreateAddAction()
        {
            return new ProfilePreferenceItemViewModel(isAddAction: true);
        }

        public static ProfilePreferenceItemViewModel CreateRule(Guid subjectId, Guid profileId, Action onRuleChanged)
        {
            return new ProfilePreferenceItemViewModel(isAddAction: false, onRuleChanged)
            {
                _subjectId = subjectId,
                _profileId = profileId
            };
        }
    }
}
