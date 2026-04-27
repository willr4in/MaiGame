using UnityEngine;

namespace SurfRush.Core
{
    public enum TimeOfDay { Day = 0, Evening = 1 }

    /// <summary>
    /// Пользовательские настройки, переживающие сессию (PlayerPrefs).
    /// Тонкая обёртка, чтобы все ключи и парсинг enum-ов жили в одном месте.
    /// </summary>
    public static class GameSettings
    {
        private const string KeyTimeOfDay = "SurfRush.TimeOfDay";

        public static TimeOfDay TimeOfDay
        {
            get => (TimeOfDay)PlayerPrefs.GetInt(KeyTimeOfDay, (int)TimeOfDay.Day);
            set
            {
                PlayerPrefs.SetInt(KeyTimeOfDay, (int)value);
                PlayerPrefs.Save();
            }
        }
    }
}
