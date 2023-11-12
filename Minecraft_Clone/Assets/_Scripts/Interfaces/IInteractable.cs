using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Minecraft
{
    public interface IInteractable
    {
        void Interact(Vector3Int worldPosition);
    }
}
