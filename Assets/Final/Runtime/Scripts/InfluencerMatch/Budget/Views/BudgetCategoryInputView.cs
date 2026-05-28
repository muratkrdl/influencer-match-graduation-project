using System.Collections.Generic;
using DG.Tweening;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// Panel view for the budget and category selection screen.
    /// </summary>
    public class BudgetCategoryInputView : UIPanelBase
    {
        [Header("Budget Input")]
        [SerializeField] private TMP_InputField m_BudgetInput;
        [SerializeField] private TMP_Text m_BudgetLabel;

        [Header("Category List")]
        [SerializeField] private Transform m_CategoryListContainer;
        [SerializeField] private CategoryToggleView m_CategoryTogglePrefab;

        [Header("Continue / Feedback")]
        [SerializeField] private Button m_ContinueButton;
        [SerializeField] private TMP_Text m_ContinueButtonText;
        [SerializeField] private TMP_Text m_ErrorText;

        private readonly List<CategoryToggleView> m_SpawnedToggles = new List<CategoryToggleView>();

        private RectTransform m_BudgetInputRect;
        private Vector2 m_BudgetInputOrigin;
        private Tween m_ShakeTween;

        [Inject] private BudgetConfig m_BudgetConfig;

        public event BudgetChangedHandler BudgetChanged;
        public event CategoryToggledHandler CategoryToggled;
        public event ContinueClickedHandler ContinueClicked;

        private void Awake()
        {
            m_BudgetInput.onEndEdit.AddListener(HandleBudgetEndEdit);

            m_ContinueButton.onClick.AddListener(HandleContinueClicked);

            m_BudgetInputRect = (RectTransform)m_BudgetInput.transform;
            m_BudgetInputOrigin = m_BudgetInputRect.anchoredPosition;

            m_BudgetLabel.text = "Budget ($)";
            m_ContinueButtonText.text = "Continue";

            ShowError(null);
        }

        private void OnDestroy()
        {
            m_BudgetInput.onEndEdit.RemoveListener(HandleBudgetEndEdit);
            m_ContinueButton.onClick.RemoveListener(HandleContinueClicked);

            m_ShakeTween?.Kill();
            m_ShakeTween = null;

            ClearSpawnedToggles();
        }

        public void DisplayCategories(IReadOnlyList<CategoryDefinition> categories)
        {
            ClearSpawnedToggles();

            if (categories.Count == 0)
            {
                ShowError("No categories available.");
                return;
            }

            foreach (CategoryDefinition definition in categories)
            {
                CategoryToggleView instance = Instantiate(m_CategoryTogglePrefab, m_CategoryListContainer);
                instance.Bind(definition);
                instance.ToggleChanged += HandleCategoryToggleChanged;
                m_SpawnedToggles.Add(instance);
            }
        }

        public void DisplayBudget(decimal budget)
        {
            string formatted = budget <= 0m ? string.Empty : PriceFormatter.FormatNumeric(budget);
            m_BudgetInput.SetTextWithoutNotify(formatted);
        }

        public void SetContinueEnabled(bool isEnabled)
        {
            m_ContinueButton.interactable = isEnabled;
        }

        public void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                m_ErrorText.gameObject.SetActive(false);
                return;
            }

            m_ErrorText.text = message;
            m_ErrorText.gameObject.SetActive(true);
            PlayBudgetInputShake();
        }

        private void PlayBudgetInputShake()
        {
            m_ShakeTween?.Kill();
            m_BudgetInputRect.anchoredPosition = m_BudgetInputOrigin;
            m_ShakeTween = m_BudgetInputRect
                .DOShakeAnchorPos(
                    m_BudgetConfig.ValidationShakeDuration,
                    new Vector3(m_BudgetConfig.ValidationShakeStrengthX, 0f, 0f),
                    m_BudgetConfig.ValidationShakeVibrato)
                .SetLink(gameObject);
        }

        public void RestoreCategorySelections(IReadOnlyCollection<CategoryId> selectedCategories)
        {
            foreach (CategoryToggleView toggle in m_SpawnedToggles)
            {
                toggle.SetIsOn(false);
            }

            if (selectedCategories.Count == 0)
            {
                return;
            }

            HashSet<CategoryId> set = selectedCategories as HashSet<CategoryId>;
            if (set == null)
            {
                set = new HashSet<CategoryId>();
                foreach (CategoryId id in selectedCategories)
                {
                    set.Add(id);
                }
            }

            foreach (CategoryToggleView toggle in m_SpawnedToggles)
            {
                if (set.Contains(toggle.CategoryId))
                {
                    toggle.SetIsOn(true);
                }
            }
        }

        private void ClearSpawnedToggles()
        {
            foreach (CategoryToggleView toggle in m_SpawnedToggles)
            {
                toggle.ToggleChanged -= HandleCategoryToggleChanged;
                Destroy(toggle.gameObject);
            }
            m_SpawnedToggles.Clear();
        }

        private void HandleBudgetEndEdit(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                ShowError(null);
                BudgetChanged?.Invoke(0m);
                return;
            }

            if (PriceFormatter.TryParse(rawText, out decimal parsed))
            {
                ShowError(null);
                BudgetChanged?.Invoke(parsed);
                m_BudgetInput.SetTextWithoutNotify(parsed > 0m ? PriceFormatter.FormatNumeric(parsed) : string.Empty);
            }
            else
            {
                ShowError("Budget must be a valid number.");
                BudgetChanged?.Invoke(0m);
            }
        }

        private void HandleContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

        private void HandleCategoryToggleChanged(CategoryId id, bool isOn)
        {
            CategoryToggled?.Invoke(id, isOn);
        }

        public delegate void BudgetChangedHandler(decimal budget);
        public delegate void CategoryToggledHandler(CategoryId id, bool isOn);
        public delegate void ContinueClickedHandler();
    }
}
