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

        _stringBuilder.AppendLine($"Velocity : {playerRb.velocity.XZ().magnitude:0.000}");
        _stringBuilder.AppendLine($"World position : {playerRb.position}");
        _stringBuilder.AppendLine($"Coordinate : {Chunk.GetChunkCoord(playerRb.position)}");

        debugText.SetText(_stringBuilder.ToString());
    }
}
