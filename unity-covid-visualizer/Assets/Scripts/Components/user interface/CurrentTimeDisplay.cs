using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViewModel;
using TMPro;
using UniRx.Triggers;
using UniRx;
using System;

namespace Components
{
    public class CurrentTimeDisplay : MonoBehaviour
    {
        public GameContainer gameContainer;
        public TextMeshProUGUI countryLabel, fontHttpLabel;
        public TextMeshProUGUI dateNowLabel, timeNowLabel;
        
        public void Start()
        {
            countryLabel.text = gameContainer.globalManager.countryData.nameCountry;
            dateNowLabel.text = DateTime.Now.ToShortDateString();
            fontHttpLabel.text = "Fuente: " + gameContainer.globalManager.countryData.fontHttpGlobal;
            
            this.gameObject.AddComponent<ObservableUpdateTrigger>()
                .LateUpdateAsObservable()
                .SampleFrame(60)
                .Subscribe(x => OnLateUpdate())
                .AddTo(this);
        }

        void OnLateUpdate()
        {
            timeNowLabel.text = DateTime.Now.ToShortTimeString().ToString();
        }
    }
}
