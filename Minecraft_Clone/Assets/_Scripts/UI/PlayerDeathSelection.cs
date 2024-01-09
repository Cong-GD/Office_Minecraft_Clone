using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public class PlayerDeathSelection : MonoBehaviour
    {
        [SerializeField]
        private Canvas myCanvas;

        [SerializeField]
        private Health playerHealth;

        [SerializeField]
        private PlayerData_SO playerData;

        private void OnEnable()
        {
            myCanvas.enabled = true;
        }

        private void OnDisable()
        {
            myCanvas.enabled = false;
        }

        public void Respawn()
        {
            playerHealth.FullHeal();
            playerData.PlayerBody.position = playerData.checkPoint;
            myCanvas.enabled = false;
        }
    }
}
