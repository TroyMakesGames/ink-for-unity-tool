using MobileInkGame.Game.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobileInkGame.Game.Controllers
{
    /// <summary>
    /// Controls the game started, restarting, etc.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField]
        private GameSettingsModel gameSettings;

        private void OnValidate()
        {
            Assert.IsNotNull(gameSettings, "Game Settings reference is null.");
        }


    }
}