using TMPro;
using UnityEngine;

namespace Jamcat.Ingame.Interface
{
    public class Monitor : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        public void AddText(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }
            else
            {
                Debug.LogWarning("Monitor text component is not assigned.");
            }
        }
    }
}