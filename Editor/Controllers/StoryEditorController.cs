using MobileInkGame.Editor.Models;
using MobileInkGame.Game.Models;
using MobileInkGame.Story.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MobileInkGame.Editor.Controllers
{
    /// <summary>
    /// Allows a visualization of story segements in the Unity editor.
    /// </summary>
    public sealed class StoryEditorController : EditorWindow
    {
        private const int MOVE_VIEWPORT_MOUSEBUTTON = 1;
        private const int MOVE_NODE_MOUSEBUTTON = 0;
        private const int EXIT_SELECTION_MOUSEBUTTON = 0;
        private const int CONFIG_WINDOW_ID = -1;
        private const string INK_FILES_PATH = "Assets/Scripts/Ink";
        private static string NODES_PATH => "Assets" + StorySegmentNodeModel.DEFAULT_NODES_PATH;
        private static string FULL_NODES_PATH => Application.dataPath + StorySegmentNodeModel.DEFAULT_NODES_PATH;

        private StoryModel _story;
        private List<StorySegmentNodeModel> _nodes;
        private GUIStyle _guiStyle;
        private List<StorySegmentModel> _drawnSegments;
        private List<int> _usedUids;
        private static Vector2 _viewPoint;
        private bool _draggingViewPoint = false;
        private StorySegmentNodeModel _draggingNode = null;
        private StorySegmentNodeModel _selectingExitFor = null;

        /// <summary>
        /// Opens the editor window, called by Unity when the menu button is pressed.
        /// </summary>
        [MenuItem("Tools/Story Editor")]
        public static void ShowEditor()
        {
            GetWindow((typeof(StoryEditorController)), false, "Story Editor");
            _viewPoint = Vector2.zero;
        }

        private void OnEnable()
        {
            _guiStyle = new GUIStyle
            {
                fontSize = StorySegmentNodeModel.FONT_SIZE,
            };
        }

        private void OnGUI()
        {
            Event e = Event.current;
            BeginWindows();
            ViewpointMovement(e);

            if (_story == null) _story = GetDefaultGameSettingsFromDatabase()?.Story;
            if (_story == null)
            {
                Debug.LogError("Unable to load default story.");
                return;
            }
            if (_nodes == null) _nodes = GetNodesFromDatabase(_story);
            _drawnSegments = new List<StorySegmentModel>();

            DrawHeader();


            DrawNodes(e);

            EndWindows();
            NodeMovement(e);
            NodeExitSelection(e);
        }

        private void ViewpointMovement(Event e)
        {
            if (_draggingNode != null) return;

            if (e.button == MOVE_VIEWPORT_MOUSEBUTTON && e.type == EventType.MouseDown)
            {
                _draggingViewPoint = true;
            }

            if (e.button == MOVE_VIEWPORT_MOUSEBUTTON && e.type == EventType.MouseDrag)
            {
                _viewPoint += e.delta;
                Repaint();
            }

            if (e.button == MOVE_VIEWPORT_MOUSEBUTTON && e.type == EventType.MouseUp)
            {
                _draggingViewPoint = false;
                e.Use();
                Repaint();
            }
        }

        private void NodeMovement(Event e)
        {
            if (_draggingViewPoint)
                return;

            if (e.button == MOVE_NODE_MOUSEBUTTON && e.type == EventType.MouseDown)
            {
                _draggingNode = GetNodeUnderMouse(e);
            }

            if (_draggingNode != null && e.button == MOVE_NODE_MOUSEBUTTON && e.type == EventType.MouseDrag)
            {
                _draggingNode.GraphLocation += e.delta;
                Repaint();
            }

            if (e.button == MOVE_NODE_MOUSEBUTTON && e.type == EventType.MouseUp)
            {
                _draggingNode = null;
                e.Use();
                Repaint();
            }
        }

        private void NodeExitSelection(Event e)
        {
            if (_selectingExitFor == null) return;

            if (e.button == EXIT_SELECTION_MOUSEBUTTON && e.type == EventType.MouseDown)
            {
                StorySegmentNodeModel selectedNode = GetNodeUnderMouse(e);
                if (selectedNode == null)
                {
                    // Create new node.
                    string path = EditorUtility.OpenFilePanel("Create New Story Element", INK_FILES_PATH, "json");

                    if (!string.IsNullOrEmpty(path))
                    {
                        path = path.Replace(Application.dataPath, "Assets");
                        TextAsset jsonFile = (TextAsset) AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));

                        StorySegmentModel storySegment = ScriptableObject.CreateInstance<StorySegmentModel>();
                        storySegment.InkScript = jsonFile;
                        AssetDatabase.CreateAsset(storySegment, AssetDatabase.GenerateUniqueAssetPath("Assets" + StorySegmentModel.DEFAULT_LOCATION + "/StorySegement.asset"));
                        AssetDatabase.SaveAssets();
                        _selectingExitFor.StorySegment.NextSegments.Add(storySegment);

                        StorySegmentNodeModel node = CreateStoryNode(storySegment);
                        node.GraphLocation = e.mousePosition - _viewPoint;
                    }
                }
                else
                {
                    // Join existing node.
                    if (!_selectingExitFor.StorySegment.NextSegments.Contains(selectedNode.StorySegment) && _selectingExitFor != selectedNode)
                    {
                        _selectingExitFor.StorySegment.NextSegments.Add(selectedNode.StorySegment);
                    }
                }

                _selectingExitFor = null;
            }

            if (_selectingExitFor != null)
            {
                DrawConnection(_selectingExitFor.rect, e.mousePosition);
                Repaint();
            }
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Story Editor | Current Story: " + _story.name);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save"))
            {
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button("Clean"))
            {
                // TODO: remove nodes that have a null story segment / aren't connected to anything.
            }
            GUILayout.EndHorizontal();
        }

        private void DrawNodes(Event e)
        {
            _usedUids = new List<int>();
            _usedUids.Add(0);

            if (_story.FirstStorySegment == null)
            {
                Debug.LogError("First story segment can't be null. Please check Data Object.");
                return;
            }

            DrawNode(e, _story.FirstStorySegment, null, null);
        }

        private StorySegmentNodeModel CreateStoryNode(StorySegmentModel storySegment)
        {
            StorySegmentNodeModel node = ScriptableObject.CreateInstance<StorySegmentNodeModel>();
            node.Story = _story;
            node.StorySegment = storySegment;
            _nodes.Add(node);
            AssetDatabase.CreateAsset(node, AssetDatabase.GenerateUniqueAssetPath("Assets" + StorySegmentNodeModel.DEFAULT_NODES_PATH + "/Node.asset"));
            AssetDatabase.SaveAssets();
            return node;
        }

        private void DrawNode(Event e, StorySegmentModel storySegment, Rect? connectingRect, int? exitNumber)
        {
            if (_drawnSegments.Contains(storySegment))
            {
                // This has been drawn before, so just draw the connection.
                if (connectingRect != null)
                {
                    StorySegmentNodeModel n = GetNodeRelatedToStorySegment(storySegment);
                    DrawConnection((Rect)connectingRect, n.rect, (int)exitNumber);
                }
                return;
            }
            EditorUtility.SetDirty(storySegment);

            // Create node if this storysegment doesn't already have one.
            StorySegmentNodeModel node = GetNodeRelatedToStorySegment(storySegment);
            if (node == null)
            {
                node = CreateStoryNode(storySegment);
            }

            // Draw the node on the window. todo: move to StorySegmentNodeExtensions.
            Rect rect = new Rect(node.GraphLocation.x + _viewPoint.x, node.GraphLocation.y + _viewPoint.y, StorySegmentNodeModel.NODE_WIDTH, StorySegmentNodeModel.NODE_HEIGHT);
            if (e.type == EventType.Layout)
            {
                node.rect = rect;
                string title = string.Format("{0}{1}", storySegment.InkScript.name, _story.FirstStorySegment == storySegment ? " (START)" : string.Empty);
                GUILayout.Window(node.GetInstanceID(), rect, (int windowId) =>
                {
                    EditorGUILayout.BeginHorizontal();

                    // First collumn.
                    EditorGUILayout.BeginVertical();
                    node.StorySegment.InkScript = (TextAsset)EditorGUILayout.ObjectField(node.StorySegment.InkScript, typeof(TextAsset), false,
                        GUILayout.Height(StorySegmentNodeModel.OBJECT_FIELD_HEIGHT), GUILayout.Width(StorySegmentNodeModel.NODE_WIDTH * 0.6f)) as TextAsset;

                    if (!_selectingExitFor && GUILayout.Button("Edit Ink Script"))
                    {
                        string path = AssetDatabase.GetAssetPath(node.StorySegment.InkScript).Replace(".json", ".ink");
                        Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                        AssetDatabase.OpenAsset(obj);
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();

                    // Second collumn.
                    EditorGUILayout.BeginVertical();
                    if (storySegment.NextSegments.Count != 0)
                    {
                        for (int i = 0; i < storySegment.NextSegments.Count; i++)
                        {
                            GUILayout.BeginHorizontal("box");
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("            Exit " + i, GUILayout.Width(StorySegmentNodeModel.NODE_WIDTH * 0.4f));
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("x"))
                            {
                                storySegment.NextSegments.RemoveAt(i);
                                _nodes = null;
                                Repaint();
                            }
                            GUILayout.EndHorizontal();

                        }
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("        Finish", GUILayout.Width(StorySegmentNodeModel.NODE_WIDTH * 0.4f));
                        GUILayout.FlexibleSpace();
                    }

                    if (!_selectingExitFor && GUILayout.Button("Add Exit"))
                    {
                        _selectingExitFor = node;
                        Repaint();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();


                }, title/*, _guiStyle*/);
            }

            // Draw connection if previous rect.
            if (e.type == EventType.Repaint && connectingRect != null)
            {
                DrawConnection((Rect)connectingRect, rect, (int)exitNumber);
            }

            _drawnSegments.Add(storySegment);

            // Draw the nodes that flow from this node.
            for (int i = 0; i < storySegment.NextSegments.Count; i++)
            {
                if (storySegment.NextSegments[i] != null)
                {
                    DrawNode(e, storySegment.NextSegments[i], rect, i);
                }
            }
        }

        private GameSettingsModel GetDefaultGameSettingsFromDatabase()
        {
            return (GameSettingsModel)AssetDatabase.LoadAssetAtPath(GameSettingsModel.GAME_SETTINGS_PATH, typeof(GameSettingsModel));
        }

        private List<StorySegmentNodeModel> GetNodesFromDatabase(StoryModel story)
        {
            List<StorySegmentNodeModel> nodes = new List<StorySegmentNodeModel>();

            if (!Directory.Exists(FULL_NODES_PATH))
            {
                Directory.CreateDirectory(FULL_NODES_PATH);
            }

            string[] nodeGuis = AssetDatabase.FindAssets("t:StorySegmentNodeModel", new[] { NODES_PATH });
            for (int i = 0; i < nodeGuis.Length; i++)
            {
                StorySegmentNodeModel node = (StorySegmentNodeModel)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(nodeGuis[i]), typeof(StorySegmentNodeModel));
                if (node.Story == story) nodes.Add(node);
            }

            return nodes;
        }

        private StorySegmentNodeModel GetNodeRelatedToStorySegment(StorySegmentModel storySegment)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].StorySegment == storySegment) return _nodes[i];
            }
            return null;
        }

        private void DrawConnection(Rect start, Rect end, int yOffset = 0)
        {
            Vector3 startPos = new Vector3(start.x + start.width + 30, start.y + 35 + (30 * yOffset), 0);
            Vector3 endPos = new Vector3(end.x, end.y + (end.height / 2), 0);
            DrawConnection(startPos, endPos);
        }

        private void DrawConnection(Rect start, Vector3 endPos)
        {
            Vector3 startPos = new Vector3(start.x + start.width + 20, start.y + 35, 0);
            DrawConnection(startPos, endPos);
        }

        private void DrawConnection(Vector3 startPos, Vector3 endPos)
        {
            Vector3 startTan = startPos + Vector3.right * 30;
            Vector3 endTan = endPos + Vector3.left * 30;
            Color shadowCol = new Color(1, 1, 1, 0.6f);

            for (int i = 0; i < 2; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1 * 5));
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 1);
        }

        private StorySegmentNodeModel GetNodeUnderMouse(Event e)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i] == null || _nodes[i].rect == null)
                {
                    continue;
                }

                if (_nodes[i].rect.Contains(e.mousePosition))
                {
                    return _nodes[i];
                }
            }

            return null;
        }
    }
}