using System;
using System.Collections.Generic;
using Model.Enum.Named;
using UnityEngine;
using System.Linq;
using Player.PlayerActions;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private List<IPlayerAction> actions;
        
        private void Awake()
        {
            actions = Enum.GetValues(typeof(NamedPlayerAction))
                .Cast<NamedPlayerAction>()
                .ToList()
                .Select(PlayerActionFactory.Create)
                .ToList();

            foreach (var action in actions)
            {
                action.Initialize(this);
            }
        }
    }
}