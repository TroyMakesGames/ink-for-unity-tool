using MobileInkGame.Story.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobileInkGame.Game.Models
{
    /// <summary>
    /// Global game settings to be used by controllers.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "DataObject/Game/GameSettings")]
    public sealed class GameSettingsModel : ScriptableObject
    {
        /// <summary>
        /// The path from Assets to the location of the game settings asset.
        /// </summary>
        public const string GAME_SETTINGS_PATH = "Assets/Database/Game/GameSettings.asset";

        [SerializeField, Tooltip("The story that plays when the game starts.")]
        private StoryModel story;
        public StoryModel Story { get { return story; } }

        private void OnValidate()
        {
            Assert.IsNotNull(story, "Story reference is null.");
        }
    }
}