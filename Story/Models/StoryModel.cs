using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobileInkGame.Story.Models
{
    /// <summary>
    /// A story is a collection of segments.
    /// </summary>
    [CreateAssetMenu(fileName = "Story", menuName = "DataObject/Story/Story")]
    public sealed class StoryModel : ScriptableObject
    {
        [SerializeField, Tooltip("The first segment that is activated when the story starts.")]
        private StorySegmentModel firstStorySegment;
        public StorySegmentModel FirstStorySegment { get { return firstStorySegment; } }

        private void OnValidate()
        {
            Assert.IsNotNull(firstStorySegment, "First story segment reference is not null.");
        }
    }
}