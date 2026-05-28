using UnityEngine;
using UnityEngine.UI;

namespace Final.InfluencerMatch.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button m_MatchInfluencerButton;

        public event MatchInfluencerClickedHandler MatchInfluencerClicked;

        private void Awake()
        {
            m_MatchInfluencerButton.onClick.AddListener(HandleMatchInfluencerClicked);
        }

        private void OnDestroy()
        {
            m_MatchInfluencerButton.onClick.RemoveListener(HandleMatchInfluencerClicked);
        }

        private void HandleMatchInfluencerClicked()
        {
            MatchInfluencerClicked?.Invoke();
        }

        public delegate void MatchInfluencerClickedHandler();
    }
}
