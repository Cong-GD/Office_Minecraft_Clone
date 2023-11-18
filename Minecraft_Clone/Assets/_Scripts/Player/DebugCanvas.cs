using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    [SerializeField]
    private PlayerData_SO playerData;

    [SerializeField]
    private PlayerInteract playerInteract;

    [SerializeField]
    private TextMeshProUGUI debugText;

    private StringBuilder _stringBuilder = new StringBuilder();

    private Rigidbody _playerBody;

    private void Awake()
    {
        _playerBody = playerData.PlayerBody;
        playerInteract = FindAnyObjectByType<PlayerInteract>();
    }

    void FixedUpdate()
    {
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"Velocity : {_playerBody.velocity.XZ().magnitude:0.000}");
        _stringBuilder.AppendLine($"World position : {_playerBody.position}");
        _stringBuilder.AppendLine($"Coordinate : {Chunk.GetChunkCoord(_playerBody.position)}");
        _stringBuilder.AppendLine($"Look at: {Chunk.GetBlock(playerInteract.HitPosition).Data().GetName()}");

        debugText.SetText(_stringBuilder.ToString());
    }
}
