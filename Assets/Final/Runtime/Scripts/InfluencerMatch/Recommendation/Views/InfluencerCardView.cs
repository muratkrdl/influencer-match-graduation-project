using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VContainer;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Single recommendation card showing an influencer's avatar, name, follower count, compatibility percent and category chips.
    /// </summary>
    public class InfluencerCardView : MonoBehaviour
    {
        private const int k_ChipPoolDefaultCapacity = 4;
        private const int k_ChipPoolMaxSize = 16;

        [Header("Visual")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private Image m_AvatarImage;

        [Header("Texts")]
        [SerializeField] private TMP_Text m_NameText;
        [SerializeField] private TMP_Text m_EmailText;
        [SerializeField] private TMP_Text m_FollowersText;
        [SerializeField] private TMP_Text m_CompatibilityText;

        [Header("Categories")]
        [SerializeField] private Transform m_ChipsContainer;
        [SerializeField] private CategoryChipView m_CategoryChipPrefab;

        [Header("Interaction")]
        [SerializeField] private Button m_ClickButton;

        [Header("Animation")]
        [SerializeField] private CanvasGroup m_CanvasGroup;

        private readonly List<CategoryChipView> m_SpawnedChips = new List<CategoryChipView>();
        private ObjectPool<CategoryChipView> m_ChipPool;
        private SerializableGuid m_InfluencerId;
        private Tween m_TapTween;

        [Inject] private RecommendationConfig m_Config;

        public event CardClickedHandler CardClicked;

        private void Awake()
        {
            m_ClickButton.onClick.AddListener(HandleClickButtonClicked);
            ApplyEllipsisToProfileTexts();

            m_ChipPool = new ObjectPool<CategoryChipView>(
                createFunc: () => Instantiate(m_CategoryChipPrefab, m_ChipsContainer),
                actionOnGet: chip => chip.gameObject.SetActive(true),
                actionOnRelease: chip => chip.gameObject.SetActive(false),
                actionOnDestroy: chip => { if (chip != null) Destroy(chip.gameObject); },
                collectionCheck: false,
                defaultCapacity: k_ChipPoolDefaultCapacity,
                maxSize: k_ChipPoolMaxSize);
        }

        private void OnDestroy()
        {
            m_ClickButton.onClick.RemoveListener(HandleClickButtonClicked);

            m_TapTween?.Kill();
            m_TapTween = null;

            ReleaseAllChips();
            m_ChipPool?.Dispose();
        }

        public void Bind(ScoredInfluencer scored, CategoryConfig categoryConfig)
        {
            InfluencerData influencer = scored.Influencer;
            m_InfluencerId = influencer.Id;

            BindAvatar(influencer);
            BindTexts(influencer, scored.CompatibilityPercent);
            RebuildChips(influencer, categoryConfig);

            float alpha = scored.IsOverBudget ? m_Config.DisabledAlpha : m_Config.NormalAlpha;
            ApplyAlpha(alpha);
        }

        public void PlayEntranceFade()
        {
            m_CanvasGroup.DOKill();
            m_CanvasGroup.alpha = 0f;
            m_CanvasGroup.DOFade(1f, m_Config.CardFadeInDuration).SetEase(m_Config.CardFadeInEase).SetLink(gameObject);
        }

        private void BindAvatar(InfluencerData influencer)
        {
            m_AvatarImage.sprite = influencer.Avatar;
            m_AvatarImage.enabled = true;
        }

        private void BindTexts(InfluencerData influencer, int compatibilityPercent)
        {
            m_NameText.text = influencer.DisplayName;
            m_EmailText.text = influencer.Email;
            m_FollowersText.text = string.Format(CultureInfo.InvariantCulture, "{0} followers", StringUtils.FormatFollowers(influencer.Followers));
            m_CompatibilityText.text = string.Format(CultureInfo.InvariantCulture, "%{0} match", compatibilityPercent);
        }

        private void RebuildChips(InfluencerData influencer, CategoryConfig categoryConfig)
        {
            ReleaseAllChips();

            IReadOnlyList<CategoryScoreEntry> entries = influencer.CategoryScores;
            if (entries.Count == 0)
            {
                return;
            }

            foreach (CategoryScoreEntry entry in entries)
            {
                if (entry.Score < 1)
                {
                    continue;
                }

                if (!categoryConfig.TryGetDefinition(entry.Category, out CategoryDefinition def))
                {
                    continue;
                }

                CategoryChipView chip = m_ChipPool.Get();
                chip.transform.SetAsLastSibling();
                chip.Bind(def);
                m_SpawnedChips.Add(chip);
            }
        }

        private void ReleaseAllChips()
        {
            foreach (CategoryChipView chip in m_SpawnedChips)
            {
                m_ChipPool.Release(chip);
            }
            m_SpawnedChips.Clear();
        }

        private void ApplyAlpha(float alpha)
        {
            SetGraphicAlpha(m_BackgroundImage, alpha);
            SetGraphicAlpha(m_AvatarImage, alpha);
            SetGraphicAlpha(m_NameText, alpha);
            SetGraphicAlpha(m_EmailText, alpha);
            SetGraphicAlpha(m_FollowersText, alpha);
            SetGraphicAlpha(m_CompatibilityText, alpha);

            foreach (CategoryChipView chip in m_SpawnedChips)
            {
                chip.SetAlpha(alpha);
            }
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            Color c = graphic.color;
            c.a = alpha;
            graphic.color = c;
        }

        private void ApplyEllipsisToProfileTexts()
        {
            m_NameText.overflowMode = TextOverflowModes.Ellipsis;
            m_NameText.textWrappingMode = TextWrappingModes.NoWrap;

            m_EmailText.overflowMode = TextOverflowModes.Ellipsis;
            m_EmailText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private void HandleClickButtonClicked()
        {
            if (m_TapTween != null && m_TapTween.IsActive())
            {
                return;
            }

            SerializableGuid capturedId = m_InfluencerId;
            m_TapTween = transform
                .DOPunchScale(Vector3.one * m_Config.TapPunchScale, m_Config.TapPunchDuration, vibrato: 1, elasticity: m_Config.TapPunchElasticity)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    m_TapTween = null;
                    CardClicked?.Invoke(capturedId);
                });
        }

        public delegate void CardClickedHandler(SerializableGuid influencerId);
    }
}
