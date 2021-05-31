using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Infrastructure;

namespace ViewModel
{
    [CreateAssetMenu(fileName = "New Country Data", menuName = "Data/Country Data")]
    public class CountryData : ScriptableObject
    {
        public int countryId;
        public StringReactiveProperty countryNick;
        public StringReactiveProperty countryName;

        public Type type;

        [Space]
        public IntReactiveProperty infoDeaths;
        public IntReactiveProperty infoPositives;
        public IntReactiveProperty infoTested;
        
        [Space]
        public BoolReactiveProperty countryFocus;
        public ISubject<StateInformation> OnInformation = new Subject<StateInformation>();
    }

    public enum Type
    {
        Country,
        State
    }
}