using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    [SerializeField]
    private Rigidbody playerRb;

    [SerializeField]
    private TextMeshProUGUI debugText;

    private StringBuilder _stringBuilder = new StringBuilder();
    void FixedUpdate()
    {
        _stringBuilder.Clear();

        _stringBuilder.AppendLine($"velocity : {playerRb.velocity.XZ().magnitude:0.000}");
        _stringBuilder.AppendLine($"world position : {playerRb.position}");
        _stringBuilder.AppendLine($"coordinate : {Chunk.GetChunkCoord(playerRb.position)}");

        debugText.SetText(_stringBuilder.ToString());
    }
}
