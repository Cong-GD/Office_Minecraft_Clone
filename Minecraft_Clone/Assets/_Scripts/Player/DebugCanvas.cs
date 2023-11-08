using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Rigidbody playerRb;

    [SerializeField]
    private TextMeshProUGUI debugText;

    private StringBuilder _stringBuilder = new StringBuilder();
    void FixedUpdate()
    {
        _stringBuilder.Clear();

        _stringBuilder.AppendLine($"Player speed: {playerRb.velocity.XZ().magnitude:0.000}");
        _stringBuilder.AppendLine($"Player position: {player.position}");
        _stringBuilder.AppendLine($"Player coord: {Chunk.GetChunkCoord(player.position)}");

        debugText.SetText(_stringBuilder.ToString());
    }
}
