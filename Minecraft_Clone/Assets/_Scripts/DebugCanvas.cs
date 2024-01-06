using Minecraft;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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

    [SerializeField]
    private DayNightSystem dayNightSystem;

    private StringBuilder _sb = new StringBuilder();

    private Rigidbody _playerBody;


    private void Awake()
    {
        _playerBody = playerData.PlayerBody;
    }

    private void Reset()
    {
        playerInteract = FindAnyObjectByType<PlayerInteract>();
        dayNightSystem = FindAnyObjectByType<DayNightSystem>();
    }

    private void FixedUpdate()
    {
        ClearStringBuilder();
        AppendPlayerVelocity();
        AppendPlayerPosition();
        AppendPlayerChunkCoord();
        AppendCurrentBlockLookingAt();
        AppendCurrentDayTime();
        //AppendFrameRate();
        UpdateDebugText();
    }

    private void ClearStringBuilder()
    {
        _sb.Length = 0;
    }

    private void UpdateDebugText()
    {
        debugText.SetText(_sb.ToString());
    }

    private void AppendPlayerVelocity()
    {
        _sb.Append("Velocity : ");
        _sb.Append(MyMath.Decimal(_playerBody.velocity.XZ().magnitude, 3));
        _sb.AppendLine();
    }

    private void AppendPlayerPosition()
    {
        var playerPosition = _playerBody.position;
        _sb.Append("World position : ");
        _sb.Append('(');
        _sb.Append(MyMath.Decimal(playerPosition.x, 2));
        _sb.Append(", ");
        _sb.Append(MyMath.Decimal(playerPosition.y, 2));
        _sb.Append(", ");
        _sb.Append(MyMath.Decimal(playerPosition.z, 2));
        _sb.Append(')');
        _sb.AppendLine();
    }

    private void AppendPlayerChunkCoord()
    {
        var chunkCoord = Chunk.GetChunkCoord(_playerBody.position);
        _sb.Append("Coordinate : ");
        _sb.Append("(");
        _sb.Append(chunkCoord.x);
        _sb.Append(", ");
        _sb.Append(chunkCoord.y);
        _sb.Append(", ");
        _sb.Append(chunkCoord.z);
        _sb.Append(")");
        _sb.AppendLine();
    }

    private void AppendCurrentBlockLookingAt()
    {
        _sb.Append("Looking at: ");
        _sb.Append((playerInteract.IsCastHit ? Chunk.GetBlock(playerInteract.HitPosition) : BlockType.Air).Data().GetName());
        _sb.AppendLine();
    }

    private void AppendCurrentDayTime()
    {
        _sb.Append("Day: ");
        _sb.Append((int)dayNightSystem.TotalDay);
        _sb.Append("\tTime:  ");
        _sb.Append((int)dayNightSystem.CurrentHourInDay);
        _sb.Append(" : ");
        _sb.Append((int)dayNightSystem.CurrentMinuteInHour);
        _sb.AppendLine();
    }

    private void AppendFrameRate()
    {
        _sb.Append("Frame Rate: ");
        _sb.Append((int)(1f / Time.unscaledDeltaTime));
    }
}
