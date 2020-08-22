using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobileInkGame.Story.Models
{
    /// <summary>
    /// A segment contains data about an Ink script, and which ink scripts this one leads to.
    /// </summary>
    [CreateAssetMenu(fileName = "StorySegment", menuName = "DataObject/Story/StorySegment")]
    public sealed class StorySegmentModel : ScriptableObject
    {
        /// <summary>
        /// Path to save the Node segment data assets.
        /// </summary>
        public const string DEFAULT_LOCATION = "/Database/Story/StorySegments";

        [SerializeField, Tooltip("The JSON version of the Ink Script.")]
        private TextAsset inkScript;
        public TextAsset InkScript { get { return inkScript; } set { inkScript = value; } }

        [SerializeField, Tooltip("The next potential segments that ")]
        private List<StorySegmentModel> nextSegments = new List<StorySegmentModel>();
        public List<StorySegmentModel> NextSegments { get { return nextSegments; } }

        private void OnValidate()
        {
            Assert.IsNotNull(inkScript, "Ink Script reference is null.");
        }
    }
}