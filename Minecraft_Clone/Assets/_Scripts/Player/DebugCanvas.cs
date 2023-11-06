using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    public Rigidbody playerRb;


    public TextMeshProUGUI debugText;

    private StringBuilder _sb = new StringBuilder();
    void FixedUpdate()
    {
        _sb.Length = 0;
        _sb.Append($"Player speed: {playerRb.velocity.XZ().magnitude}");
        debugText.SetText(_sb);
    }
}
