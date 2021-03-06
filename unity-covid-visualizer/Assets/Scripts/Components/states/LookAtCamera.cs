using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViewModel;

namespace Components
{
    public class LookAtCamera : MonoBehaviour
    {
        void Start()
        {
            this.gameObject.transform.eulerAngles = new Vector3(
                this.gameObject.transform.eulerAngles.x,
                this.gameObject.transform.eulerAngles.y + 180,
                this.gameObject.transform.eulerAngles.z);
        }
    }
}
