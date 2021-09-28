using System.Collections;
using System.Collections.Generic;
using Infrastructure;
using UniRx;
using UnityEngine;

namespace ViewModel
{
    [CreateAssetMenu(fileName = "New Country Manager", menuName = "Data/Country Manager")]
    public class CountryManager : ScriptableObject
    {
        public GameObject countryPrefab;
        public CountryData[] countryDataChildren;
        public CountryData currentCountrySelected;
    }
}