using System;
using System.Collections.Generic;
using UnityEngine.Localization.Pseudo;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Responsible for providing the list of locales that are currently available to this application.
    /// </summary>
    [Serializable]
    public class LocalesProvider : ILocalesProvider, IPreloadRequired
    {
        // There is a bug with SerializeReference that causes empty instances to not deserialize. This is a workaround while we wait for the fix (case 1183547)
        [SerializeField, HideInInspector]
        int dummyObject;

        List<Locale> m_Locales = new List<Locale>();
        AsyncOperationHandle? m_LoadOperation;
        int m_PseudoLocaleCount;

        /// <summary>
        /// The list of all supported locales.
        /// </summary>
        public List<Locale> Locales
        {
            get
            {
                if (m_LoadOperation == null)
                    Debug.LogError("Locales PreloadOperation has not been initialized, can not return the available locales.");
                return m_Locales;
            }
        }

        /// <summary>
        /// The Locales loading operation. When set to isDone then all locales have been loaded. Can be Null if the operation has not started yet.
        /// </summary>
        public AsyncOperationHandle PreloadOperation
        {
            get
            {
                if (m_LoadOperation == null)
                {
                    if (m_Locales == null)
                        m_Locales = new List<Locale>();

                    m_Locales.Clear();
                    m_PseudoLocaleCount = 0;
                    m_LoadOperation = AddressableAssets.Addressables.LoadAssetsAsync<Locale>(LocalizationSettings.LocaleLabel, AddLocale);
                }

                return m_LoadOperation.Value;
            }
        }

        /// <summary>
        /// Attempt to retrieve a Locale using the identifier.
        /// </summary>
        /// <param name="id"><see cref="LocaleIdentifier"/> to find.</param>
        /// <returns>If no Locale can be found then null is returned.</returns>
        public Locale GetLocale(LocaleIdentifier id)
        {
            foreach (var locale in Locales)
            {
                if (locale.Identifier.Equals(id))
                    return locale;
            }
            return null;
        }

        /// <summary>
        /// Attempt to retrieve a Locale using a Code.
        /// </summary>
        /// <param name="code">If no Locale can be found then null is returned.</param>
        /// <returns></returns>
        public Locale GetLocale(string code)
        {
            foreach (var locale in Locales)
            {
                if (locale.Identifier.Code == code)
                    return locale;
            }
            return null;
        }

        /// <summary>
        /// Attempt to retrieve a Locale using a <see cref="UnityEngine.SystemLanguage"/>.
        /// </summary>
        /// <param name="systemLanguage"></param>
        /// <returns>If no Locale can be found then null is returned.</returns>
        public Locale GetLocale(SystemLanguage systemLanguage)
        {
            return GetLocale(SystemLanguageConverter.GetSystemLanguageCultureCode(systemLanguage));
        }

        /// <summary>
        /// Add a Locale to allow support for a specific language.
        /// </summary>
        /// <param name="locale"></param>
        public void AddLocale(Locale locale)
        {
            bool isPsedudoLocale = locale is PseudoLocale;
            if (!isPsedudoLocale)
            {
                var foundLocale = GetLocale(locale.Identifier);
                if (foundLocale != null && !(foundLocale is PseudoLocale))
                {
                    Debug.LogWarning("Ignoring locale. A locale with the same Id has already been added: " + locale.Identifier);
                    return;
                }
            }

            // Insert PseudoLocale's at the end so they are not returned by GetLocale.
            if (isPsedudoLocale)
            {
                m_PseudoLocaleCount++;
                Locales.Add(locale);
            }
            else if (m_PseudoLocaleCount > 0)
            {
                Locales.Insert(0, locale);
            }
            else
            {
                Locales.Add(locale);
            }
        }

        /// <summary>
        /// Removes support for a specific Locale.
        /// </summary>
        /// <param name="locale">The locale that should be removed if possible.</param>
        /// <returns>true if the locale was removed or false if the locale did not exist.</returns>
        public bool RemoveLocale(Locale locale)
        {
            bool ret = Locales.Remove(locale);
            var settings = LocalizationSettings.GetInstanceDontCreateDefault();
            settings?.OnLocaleRemoved(locale);
            return ret;
        }
    }
}
