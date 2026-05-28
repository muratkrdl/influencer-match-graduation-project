using System.Collections;
using System.Collections.Generic;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Final.Tests.PlayMode.Budget
{
    /// <summary>
    /// PlayMode tests for <see cref="BudgetCategoryInputView"/>'s input-event wiring:
    /// budget end-edit parsing, continue button event, and error visibility.
    /// </summary>
    public sealed class BudgetCategoryInputViewTests
    {
        private readonly List<UnityEngine.Object> m_Owned = new List<UnityEngine.Object>();
        private GameObject m_Object;
        private BudgetCategoryInputView m_View;
        private TMP_InputField m_BudgetInput;
        private Button m_ContinueButton;
        private TMP_Text m_ErrorText;

        [TearDown]
        public void TearDown()
        {
            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
            for (int i = 0; i < m_Owned.Count; i++)
            {
                if (m_Owned[i] != null)
                {
                    Object.DestroyImmediate(m_Owned[i]);
                }
            }
            m_Owned.Clear();
        }

        [UnityTest]
        public IEnumerator BudgetEndEdit_ValidDigits_RaisesBudgetChangedWithParsedValue()
        {
            BuildView();
            decimal? captured = null;
            m_View.BudgetChanged += value => captured = value;

            m_BudgetInput.onEndEdit.Invoke("10000");
            yield return null;

            Assert.IsTrue(captured.HasValue);
            Assert.AreEqual(10_000m, captured.Value);
        }

        [UnityTest]
        public IEnumerator BudgetEndEdit_DotSeparator_RaisesTenThousand_LocaleTrapGuard()
        {
            // Regression guard for the locale fix: "10.000" must yield 10000, not 10.
            BuildView();
            decimal? captured = null;
            m_View.BudgetChanged += value => captured = value;

            m_BudgetInput.onEndEdit.Invoke("10.000");
            yield return null;

            Assert.AreEqual(10_000m, captured.Value);
        }

        [UnityTest]
        public IEnumerator BudgetEndEdit_EmptyString_RaisesZero_AndHidesError()
        {
            BuildView();
            decimal? captured = null;
            m_View.BudgetChanged += value => captured = value;
            m_View.ShowError("previously shown");
            yield return null;

            m_BudgetInput.onEndEdit.Invoke(string.Empty);
            yield return null;

            Assert.AreEqual(0m, captured.Value);
            Assert.IsFalse(m_ErrorText.gameObject.activeSelf, "Error label should hide for an empty input.");
        }

        [UnityTest]
        public IEnumerator ContinueButton_Click_RaisesContinueClicked()
        {
            BuildView();
            bool fired = false;
            m_View.ContinueClicked += () => fired = true;

            m_ContinueButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(fired);
        }

        [UnityTest]
        public IEnumerator ShowError_NonEmpty_ActivatesErrorText()
        {
            BuildView();

            m_View.ShowError("Budget must be a valid number.");
            yield return null;

            Assert.IsTrue(m_ErrorText.gameObject.activeSelf);
            Assert.AreEqual("Budget must be a valid number.", m_ErrorText.text);
        }

        private void BuildView()
        {
            m_Object = new GameObject(nameof(BudgetCategoryInputViewTests));
            m_Object.SetActive(false);

            CanvasGroup canvasGroup = m_Object.AddComponent<CanvasGroup>();
            m_BudgetInput = NewChildWithComponent<TMP_InputField>("BudgetInput");
            TMP_Text budgetLabel = NewChildWithComponent<TextMeshProUGUI>("BudgetLabel");
            GameObject categoryContainer = NewChild("CategoryContainer");

            // Toggle prefab — DisplayCategories instantiates this, but our event-flow tests
            // don't exercise that path. Minimal stub so the SerializeField is non-null.
            GameObject togglePrefabObj = new GameObject("TogglePrefab");
            togglePrefabObj.SetActive(false);
            CategoryToggleView togglePrefab = togglePrefabObj.AddComponent<CategoryToggleView>();
            m_Owned.Add(togglePrefabObj);

            m_ContinueButton = NewChildWithComponent<Button>("ContinueButton");
            TMP_Text continueButtonText = NewChildWithComponent<TextMeshProUGUI>("ContinueLabel");
            m_ErrorText = NewChildWithComponent<TextMeshProUGUI>("ErrorText");
            UISharedConfig sharedConfig = ScriptableObject.CreateInstance<UISharedConfig>();
            m_Owned.Add(sharedConfig);

            BudgetConfig budgetConfig = ScriptableObject.CreateInstance<BudgetConfig>();
            m_Owned.Add(budgetConfig);

            m_View = m_Object.AddComponent<BudgetCategoryInputView>();
            ReflectionHelpers.SetPrivateField(m_View, "m_BudgetInput", m_BudgetInput);
            ReflectionHelpers.SetPrivateField(m_View, "m_BudgetLabel", budgetLabel);
            ReflectionHelpers.SetPrivateField(m_View, "m_CategoryListContainer", categoryContainer.transform);
            ReflectionHelpers.SetPrivateField(m_View, "m_CategoryTogglePrefab", togglePrefab);
            ReflectionHelpers.SetPrivateField(m_View, "m_ContinueButton", m_ContinueButton);
            ReflectionHelpers.SetPrivateField(m_View, "m_ContinueButtonText", continueButtonText);
            ReflectionHelpers.SetPrivateField(m_View, "m_ErrorText", m_ErrorText);
            ReflectionHelpers.SetPrivateField(m_View, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(m_View, "m_SharedConfig", sharedConfig);
            ReflectionHelpers.SetPrivateField(m_View, "m_BudgetConfig", budgetConfig);

            m_Object.SetActive(true);
        }

        private GameObject NewChild(string childName)
        {
            GameObject child = new GameObject(childName, typeof(RectTransform));
            child.transform.SetParent(m_Object.transform);
            return child;
        }

        private T NewChildWithComponent<T>(string childName) where T : Component
        {
            return NewChild(childName).AddComponent<T>();
        }
    }
}
