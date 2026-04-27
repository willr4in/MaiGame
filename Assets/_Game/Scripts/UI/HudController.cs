using SurfRush.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SurfRush.UI
{
    /// <summary>
    /// Простой HUD: счёт, лучший счёт, жизни (в виде текста "❤ × N").
    /// Подписывается на события GameManager и ScoreSystem, никаких опросов
    /// каждый кадр.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string menuSceneName = "Menu";

        private GameManager _gm;
        private ScoreSystem _ss;

        private void Awake()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void Start()
        {
            _gm = GameManager.Instance;
            _ss = ScoreSystem.Instance;

            if (_gm != null)
            {
                _gm.OnLivesChanged += UpdateLives;
                _gm.OnStateChanged += OnStateChanged;
                UpdateLives(_gm.Lives);
            }
            if (_ss != null)
            {
                _ss.OnScoreChanged += UpdateScore;
                _ss.OnBestScoreChanged += UpdateBestScore;
                UpdateScore(_ss.CurrentScoreInt);
                UpdateBestScore(_ss.BestScore);
            }
        }

        private void OnDestroy()
        {
            if (_gm != null)
            {
                _gm.OnLivesChanged -= UpdateLives;
                _gm.OnStateChanged -= OnStateChanged;
            }
            if (_ss != null)
            {
                _ss.OnScoreChanged -= UpdateScore;
                _ss.OnBestScoreChanged -= UpdateBestScore;
            }
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenu);
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }

        private void UpdateBestScore(int best)
        {
            if (bestScoreText != null) bestScoreText.text = $"Best: {best}";
        }

        private void UpdateLives(int lives)
        {
            if (livesText == null) return;
            string hearts = "";
            for (int i = 0; i < lives; i++) hearts += "♥ ";
            livesText.text = hearts.TrimEnd();
        }

        private void OnStateChanged(GameManager.State s)
        {
            if (s == GameManager.State.GameOver && gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (gameOverScoreText != null && _ss != null)
                    gameOverScoreText.text = $"Score: {_ss.CurrentScoreInt}\nBest: {_ss.BestScore}";
            }
        }

        private void OnRestart()
        {
            if (_gm != null) _gm.Restart();
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
