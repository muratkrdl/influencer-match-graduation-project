using System.Collections.Generic;
using System.Globalization;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.InfluencerMatch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// Detail panel UI for a single influencer.
    /// </summary>
    public class InfluencerDetailView : UIPanelBase
    {
        private const int k_ScoreBarPoolDefaultCapacity = 8;
        private const int k_ScoreBarPoolMaxSize = 16;
        private const int k_PlatformIconPoolDefaultCapacity = 4;
        private const int k_PlatformIconPoolMaxSize = 8;

        [Header("Profile")]
        [SerializeField] private Image m_AvatarImage;
        [SerializeField] private TMP_Text m_NameText;
        [SerializeField] private TMP_Text m_HandleText;
        [SerializeField] private TMP_Text m_EmailText;
        [SerializeField] private TMP_Text m_FollowersText;
        [SerializeField] private TMP_Text m_EngagementText;

        [Header("Compatibility")]
        [SerializeField] private TMP_Text m_CompatibilityPercentText;

        [Header("Price")]
        [SerializeField] private TMP_Text m_PriceValueText;

        [Header("Scores")]
        [SerializeField] private Transform m_ScoreBarsContainer;
        [SerializeField] private CategoryScoreBarView m_ScoreBarPrefab;

        [Header("Platforms")]
        [SerializeField] private Transform m_PlatformsContainer;
        [SerializeField] private PlatformIconView m_PlatformIconPrefab;

        [Header("Bio")]
        [SerializeField] private GameObject m_BioSection;
        [SerializeField] private TMP_Text m_BioText;

        [Header("Header")]
        [SerializeField] private Button m_BackButton;
        [SerializeField] private TMP_Text m_TitleText;

        private readonly List<CategoryScoreBarView> m_SpawnedBars = new List<CategoryScoreBarView>();
        private readonly List<PlatformIconView> m_SpawnedPlatforms = new List<PlatformIconView>();
        private readonly HashSet<CategoryId> m_SelectedCategoryLookup = new HashSet<CategoryId>();
        private ObjectPool<CategoryScoreBarView> m_ScoreBarPool;
        private ObjectPool<PlatformIconView> m_PlatformIconPool;

        [Inject] private IObjectResolver m_Container;

        public event BackClickedHandler BackClicked;

        private void Awake()
        {
            m_ScoreBarPool = new ObjectPool<CategoryScoreBarView>(
                createFunc: () => m_Container.Instantiate(m_ScoreBarPrefab, m_ScoreBarsContainer),
                actionOnGet: bar => bar.gameObject.SetActive(true),
                actionOnRelease: bar => bar.gameObject.SetActive(false),
                actionOnDestroy: bar => { if (bar != null) Destroy(bar.gameObject); },
                collectionCheck: false,
                defaultCapacity: k_ScoreBarPoolDefaultCapacity,
                maxSize: k_ScoreBarPoolMaxSize);

            m_PlatformIconPool = new ObjectPool<PlatformIconView>(
                createFunc: () => m_Container.Instantiate(m_PlatformIconPrefab, m_PlatformsContainer),
                actionOnGet: icon => icon.gameObject.SetActive(true),
                actionOnRelease: icon => icon.gameObject.SetActive(false),
                actionOnDestroy: icon => { if (icon != null) Destroy(icon.gameObject); },
                collectionCheck: false,
                defaultCapacity: k_PlatformIconPoolDefaultCapacity,
                maxSize: k_PlatformIconPoolMaxSize);

            m_BackButton.onClick.AddListener(HandleBackButtonClicked);
            ApplyEllipsisToProfileTexts();
        }

        private void OnDestroy()
        {
            m_BackButton.onClick.RemoveListener(HandleBackButtonClicked);

            ClearScoreBars();
            ClearPlatforms();
            m_ScoreBarPool?.Dispose();
            m_PlatformIconPool?.Dispose();
        }

        public void DisplayInfluencer(
            InfluencerData influencer,
            int compatibilityPercent,
            int engagementCount,
            int finalPrice,
            IReadOnlyList<PlatformDefinition> activePlatforms,
            IReadOnlyList<CategoryId> selectedCategories,
            CategoryConfig categoryConfig)
        {
            BindAvatar(influencer);
            BindProfileTexts(influencer);
            BindEngagement(influencer, engagementCount);
            BindCompatibility(compatibilityPercent);
            BindPrice(finalPrice);
            BindBio(influencer);
            BindPlatforms(activePlatforms);
            RebuildScoreBars(influencer, selectedCategories, categoryConfig);
        }

        private void BindPlatforms(IReadOnlyList<PlatformDefinition> activePlatforms)
        {
            ClearPlatforms();

            for (int i = 0; i < activePlatforms.Count; i++)
            {
                PlatformIconView icon = m_PlatformIconPool.Get();
                icon.transform.SetAsLastSibling();
                icon.Bind(activePlatforms[i]);
                m_SpawnedPlatforms.Add(icon);
            }
        }

        private void ClearPlatforms()
        {
            foreach (PlatformIconView icon in m_SpawnedPlatforms)
            {
                m_PlatformIconPool.Release(icon);
            }
            m_SpawnedPlatforms.Clear();
        }

        private void BindEngagement(InfluencerData influencer, int engagementCount)
        {
            string compressed = StringUtils.FormatFollowers(engagementCount);
            int ratePercent = (int)System.Math.Round(influencer.EngagementRate * 100f);
            m_EngagementText.text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} engagements ({1}%)",
                compressed,
                ratePercent);
        }

        private void BindAvatar(InfluencerData influencer)
        {
            m_AvatarImage.sprite = influencer.Avatar;
            m_AvatarImage.enabled = true;
        }

        private void BindProfileTexts(InfluencerData influencer)
        {
            m_NameText.text = influencer.DisplayName;
            m_HandleText.text = influencer.Handle;
            m_EmailText.text = influencer.Email;
            m_FollowersText.text = string.Format(CultureInfo.InvariantCulture, "{0} followers", StringUtils.FormatFollowers(influencer.Followers));
        }

        private void BindCompatibility(int compatibilityPercent)
        {
            int clamped = compatibilityPercent;
            if (clamped < 0)
            {
                clamped = 0;
            }
            else if (clamped > 100)
            {
                clamped = 100;
            }

            m_CompatibilityPercentText.text = "%" + StringUtils.GetNumberString(clamped);
        }

        private void BindPrice(int finalPrice)
        {
            int clamped = finalPrice < 0 ? 0 : finalPrice;
            m_PriceValueText.text = PriceFormatter.Format(clamped);
        }

        private void BindBio(InfluencerData influencer)
        {
            string bio = influencer.ShortBio;
            if (string.IsNullOrWhiteSpace(bio))
            {
                m_BioSection.SetActive(false);
                return;
            }

            m_BioSection.SetActive(true);
            m_BioText.text = bio;
        }

        private void RebuildScoreBars(
            InfluencerData influencer,
            IReadOnlyList<CategoryId> selectedCategories,
            CategoryConfig categoryConfig)
        {
            ClearScoreBars();

            HashSet<CategoryId> selectedSet = RebuildSelectedSet(selectedCategories);

            IReadOnlyList<CategoryDefinition> definitions = categoryConfig.Categories;
            foreach (CategoryDefinition definition in definitions)
            {
                int score = influencer.GetScoreFor(definition.Id);
                bool isSelected = selectedSet.Contains(definition.Id);

                CategoryScoreBarView bar = m_ScoreBarPool.Get();
                bar.transform.SetAsLastSibling();
                bar.Bind(definition, score, isSelected);
                m_SpawnedBars.Add(bar);
            }
        }

        private HashSet<CategoryId> RebuildSelectedSet(IReadOnlyList<CategoryId> selectedCategories)
        {
            m_SelectedCategoryLookup.Clear();
            foreach (CategoryId id in selectedCategories)
            {
                m_SelectedCategoryLookup.Add(id);
            }
            return m_SelectedCategoryLookup;
        }

        private void ClearScoreBars()
        {
            foreach (CategoryScoreBarView bar in m_SpawnedBars)
            {
                m_ScoreBarPool.Release(bar);
            }
            m_SpawnedBars.Clear();
        }

        private void HandleBackButtonClicked()
        {
            BackClicked?.Invoke();
        }

        private void ApplyEllipsisToProfileTexts()
        {
            m_NameText.textWrappingMode = TextWrappingModes.Normal;
            m_NameText.maxVisibleLines = 2;

            m_HandleText.overflowMode = TextOverflowModes.Ellipsis;
            m_HandleText.textWrappingMode = TextWrappingModes.NoWrap;

            m_EmailText.overflowMode = TextOverflowModes.Ellipsis;
            m_EmailText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        public delegate void BackClickedHandler();
    }
}
