using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoQuest.Data;
using UnityEditor;
using UnityEngine;

namespace GeoQuest
{
    public partial class ContentEditorWindow : EditorWindow
    {
        // Serialized to survive recompiles
        [SerializeField] private InteractionDatabase _database;
        [SerializeField] private InteractionRule _selected;

        private SerializedObject _itemSo;
        private string _search = "";
        private Vector2 _listScroll, _formScroll;

        [MenuItem("Game/Content Editor")]
        public static void Open() => GetWindow<ContentEditorWindow>("Content Editor");

        private void OnEnable()
        {
            if (_database == null)
                _database = FindAll<InteractionDatabase>().FirstOrDefault();
        }

        // Save when the window loses focus
        private void OnLostFocus() => AssetDatabase.SaveAssets();

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _database = (InteractionDatabase)EditorGUILayout.ObjectField("Database", _database, typeof(InteractionDatabase), false);
                if (EditorGUI.EndChangeCheck())
                    _selected = null;

                if (GUILayout.Button("+", GUILayout.Width(24)))
                    CreateAssetPopup.Show<InteractionDatabase>(
                        dbName => CreateAsset<InteractionDatabase>(EditorPaths.Databases_Path, dbName),
                        db => { _database = db; _selected = null; },
                        "location");
            }

            if (_database == null)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawObjectList();
                DrawObjectForm();
            }
        }

        private void DrawObjectList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(200)))
            {
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

                foreach (var rule in _database.Rules)
                {
                    if (rule == null)
                        continue;

                    var label = ObjectLabel(rule);
                    if (!label.Contains(_search, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (GUILayout.Toggle(_selected == rule, label, "Button") && _selected != rule)
                        Select(rule);
                }

                if (GUILayout.Button("+ New object"))
                    CreateAssetPopup.Show<InteractionRule>(CreateObject, Select, "object");

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawObjectForm()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (_selected == null)
                {
                    EditorGUILayout.HelpBox("Select an object on the left", MessageType.Info);
                    return;
                }

                if (_selected.Target == null)
                {
                    EditorGUILayout.HelpBox("This object has no Item assigned", MessageType.Warning);
                    return;
                }

                _formScroll = EditorGUILayout.BeginScrollView(_formScroll);
                DrawItem(_selected.Target);
                DrawActions(_selected);

                EditorGUILayout.Space(12);
                if (GUILayout.Button("Delete object", GUILayout.Width(120)) && ConfirmDelete(_selected))
                {
                    DeleteObject(_selected);
                    GUIUtility.ExitGUI();      // the object is gone, stop drawing
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawItem(Item item)
        {
            if (_itemSo == null || _itemSo.targetObject != item)
                _itemSo = new SerializedObject(item);

            _itemSo.Update();

            // Id is the save key, set once at creation
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_itemSo.FindProperty(nameof(Item.Id)));

            EditorGUILayout.PropertyField(_itemSo.FindProperty(nameof(Item.DisplayName)));
            EditorGUILayout.PropertyField(_itemSo.FindProperty(nameof(Item.Description)));

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.PropertyField(_itemSo.FindProperty(nameof(Item.ItemPrefab)), new GUIContent("Prefab"));
                    DrawTags(item);
                    DrawCharge(_itemSo.FindProperty(nameof(Item.Charge)));
                }

                DrawIcon(item, _itemSo.FindProperty(nameof(Item.Icon)));
            }

            _itemSo.ApplyModifiedProperties();
        }

        // -1 = infinite
        private static void DrawCharge(SerializedProperty charge)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent("Charge", "Uses of one object in the backpack. Infinite = never runs out."));
                if (GUILayout.Toggle(charge.intValue < 0, "Infinite", GUILayout.Width(70)))
                    charge.intValue = -1;
                else
                    charge.intValue = Mathf.Max(0, EditorGUILayout.IntField(charge.intValue < 0 ? 1 : charge.intValue));
            }
        }

        // Files from disk: drop on the preview or Import...; Project drags are handled by ObjectField
        private void DrawIcon(Item item, SerializedProperty icon)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(96)))
            {
                EditorGUILayout.LabelField("Icon");
                var preview = EditorGUILayout.GetControlRect(false, 96, GUILayout.Width(96));

                var file = DroppedImage(preview);
                if (file == null && GUILayout.Button("Import..."))
                    file = EditorUtility.OpenFilePanel("Choose image", "", "png,jpg,jpeg");

                // import outside OnGUI
                if (!string.IsNullOrEmpty(file))
                    EditorApplication.delayCall += () => SetIcon(item, ImportIcon(file, item));

                var before = (Sprite)icon.objectReferenceValue;
                icon.objectReferenceValue = EditorGUI.ObjectField(preview, before, typeof(Sprite), false);
                if (icon.objectReferenceValue != before)
                    SyncPrefabSprite(item.ItemPrefab, before, (Sprite)icon.objectReferenceValue);
            }
        }

        private static string DroppedImage(Rect area)
        {
            var e = Event.current;
            bool external = DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0;
            if (!external || !area.Contains(e.mousePosition))
                return null;
            if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform)
                return null;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            e.Use();
            if (e.type != EventType.DragPerform)
                return null;

            DragAndDrop.AcceptDrag();
            var path = DragAndDrop.paths[0];
            return new[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(path).ToLower()) ? path : null;
        }

        private static Sprite ImportIcon(string sourceFile, Item item)
        {
            Directory.CreateDirectory(EditorPaths.ItemIcons_Path);
            var path = EditorPaths.ItemIcons_Path + item.Id + Path.GetExtension(sourceFile);
            File.Copy(sourceFile, path, true);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private void SetIcon(Item item, Sprite sprite)
        {
            Undo.RecordObject(item, "Set icon");
            var before = item.Icon;
            item.Icon = sprite;
            EditorUtility.SetDirty(item);
            SyncPrefabSprite(item.ItemPrefab, before, sprite);
            Repaint();
        }

        // Tags dropdown with check marks, "+" creates a tag
        private void DrawTags(Item item)
        {
            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent("Tags"));
            var dropdown = new Rect(rect.x, rect.y, rect.width - 26, rect.height);
            var plus = new Rect(rect.xMax - 22, rect.y, 22, rect.height);
            var summary = string.Join(", ", item.Tags.Select(t => t.DisplayName));

            if (EditorGUI.DropdownButton(dropdown, new GUIContent(summary), FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                foreach (var tag in FindAll<ItemTag>())
                    menu.AddItem(new GUIContent(tag.DisplayName), item.Tags.Contains(tag), () => ToggleTag(item, tag));
                menu.ShowAsContext();
            }

            if (GUI.Button(plus, "+"))
                CreateAssetPopup.Show<ItemTag>(
                    tagName => FindOrCreateTag(tagName.Trim()),
                    tag => { if (!item.Tags.Contains(tag)) ToggleTag(item, tag); },
                    "tag");
        }

        // Reuse an existing tag: duplicates would break HasTag
        private static ItemTag FindOrCreateTag(string tagName)
        {
            var existing = FindAll<ItemTag>().FirstOrDefault(t => string.Equals(t.DisplayName, tagName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                return existing;

            var tag = CreateAsset<ItemTag>(EditorPaths.ItemTags_Path, tagName);
            tag.DisplayName = tagName;
            EditorUtility.SetDirty(tag);
            return tag;
        }

        private void ToggleTag(Item item, ItemTag tag)
        {
            Undo.RecordObject(item, "Toggle tag");
            if (!item.Tags.Remove(tag))
                item.Tags.Add(tag);
            EditorUtility.SetDirty(item);
            Repaint();
        }

        private static string ObjectLabel(InteractionRule rule) =>
            rule.Target != null && !string.IsNullOrEmpty(rule.Target.DisplayName) ? rule.Target.DisplayName : rule.name;

        // Unique file name: never overwrite an existing asset
        private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
        {
            Directory.CreateDirectory(folder);
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(folder + assetName + ".asset"));
            return asset;
        }

        private static IEnumerable<T> FindAll<T>() where T : UnityEngine.Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
    }
}
