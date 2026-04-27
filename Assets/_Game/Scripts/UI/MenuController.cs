using SurfRush.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SurfRush.UI
{
    /// <summary>
    /// Главное меню: Play, переключатель «День/Вечер», Quit.
    /// Выбор времени суток сохраняется в PlayerPrefs и читается главной сценой.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button timeOfDayButton;
        [SerializeField] private TMP_Text timeOfDayLabel;
        [SerializeField] private Button quitButton;
        [Tooltip("Имя главной игровой сцены. Должна быть добавлена в Build Settings.")]
        [SerializeField] private string mainSceneName = "Main";

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (timeOfDayButton != null) timeOfDayButton.onClick.AddListener(OnToggleTime);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            UpdateTimeLabel();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlay);
            if (timeOfDayButton != null) timeOfDayButton.onClick.RemoveListener(OnToggleTime);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
        }

        private void OnPlay()
        {
            SceneManager.LoadScene(mainSceneName);
        }

        private void OnToggleTime()
        {
            GameSettings.TimeOfDay = GameSettings.TimeOfDay == TimeOfDay.Day
                ? TimeOfDay.Evening
                : TimeOfDay.Day;
            UpdateTimeLabel();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void UpdateTimeLabel()
        {
            if (timeOfDayLabel == null) return;
            timeOfDayLabel.text = GameSettings.TimeOfDay == TimeOfDay.Day
                ? "Time: Day"
                : "Time: Evening";
        }
    }
}
