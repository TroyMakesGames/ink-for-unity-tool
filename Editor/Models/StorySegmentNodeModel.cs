using MobileInkGame.Story.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobileInkGame.Editor.Models
{
    /// <summary>
    /// A node that fits within the story editor graph, holds a story segment.
    /// </summary>
    public sealed class StorySegmentNodeModel : ScriptableObject
    {
        /// <summary>
        /// What to call the title of the window in the story editor.
        /// </summary>
        public const string DEFAULT_WINDOW_NAME = "Story Segement";

        /// <summary>
        /// Path to save the Node segment data assets.
        /// </summary>
        public const string DEFAULT_NODES_PATH = "/Database/Editor/StorySegmentNodes";

        /// <summary>
        /// The width of the node window.
        /// </summary>
        public const float NODE_WIDTH = 220;

        /// <summary>
        /// The height of the node window.
        /// </summary>
        public const float NODE_HEIGHT = 70;

        /// <summary>
        /// The height of the object field within the node.
        /// </summary>
        public const float OBJECT_FIELD_HEIGHT = 40;

        /// <summary>
        /// The font size for the GUIStyle.
        /// </summary>
        public readonly static int FONT_SIZE = 30;

        /// <summary>
        /// Used by the Event manager to detect mouse over node.
        /// </summary>
        [System.NonSerialized]
        public Rect rect;

        [SerializeField, Tooltip("The story this node relates to.")]
        private StoryModel story;
        public StoryModel Story { get { return story; } set { story = value; } }

        [SerializeField, Tooltip("The Story Segment data object that this node represents")]
        private StorySegmentModel storySegment;
        public StorySegmentModel StorySegment { get { return storySegment; } set { storySegment = value; } }

        [SerializeField, Tooltip("The graph location of this node")]
        private Vector2 graphLocation;
        public Vector2 GraphLocation { get { return graphLocation; } set { graphLocation = value; } }
    }
}