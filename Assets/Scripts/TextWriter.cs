using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextWriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;

    public void showMessage(string text)
    {
        Text.text = text;
    }
}
