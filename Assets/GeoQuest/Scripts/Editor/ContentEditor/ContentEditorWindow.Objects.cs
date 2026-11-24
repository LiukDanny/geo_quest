using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GeoQuest.Data;
using UnityEditor;
using UnityEngine;

namespace GeoQuest
{
    public partial class ContentEditorWindow
    {
        // Item + rule + prefab from the template, added to the database
        private InteractionRule CreateObject(string objectName)
        {
            var file = string.Concat(objectName.Where(char.IsLetterOrDigit));   // "Apple Tree" -> AppleTree

            var item = CreateAsset<Item>(EditorPaths.Items_Path, file);
            item.Id = MakeId(objectName);
            item.DisplayName = objectName.Trim();
            item.Tags = new List<ItemTag>();                                     // lists are null on a fresh asset

            EditorUtility.SetDirty(item);

            var rule = CreateAsset<InteractionRule>(EditorPaths.Interactions_Path, file);
            rule.Target = item;
            rule.ActionRules = new List<ActionRule>();
            EditorUtility.SetDirty(rule);
            AssetDatabase.SaveAssets();

            item.ItemPrefab = CreatePrefab(file, rule);

            _database.Rules.Add(rule);
            EditorUtility.SetDirty(item);
            EditorUtility.SetDirty(rule);
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            return rule;
        }

        // Id is the save key, so it is unique across all items
        private static string MakeId(string objectName)
        {
            var baseId = Regex.Replace(objectName.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            if (baseId.Length == 0)
                baseId = "object";                                               // name without latin letters

            var used = FindAll<Item>().Select(i => i.Id).ToHashSet();
            var id = baseId;
            for (int n = 2; used.Contains(id); n++)
                id = baseId + "_" + n;
            return id;
        }

        private static GameObject CreatePrefab(string file, InteractionRule rule)
        {
            Directory.CreateDirectory(EditorPaths.Prefabs_Path);
            var path = AssetDatabase.GenerateUniqueAssetPath(EditorPaths.Prefabs_Path + file + ".prefab");
            AssetDatabase.CopyAsset(EditorPaths.ObjectTemplate_Path, path);

            var root = PrefabUtility.LoadPrefabContents(path);
            root.name = file;
            var so = new SerializedObject(root.GetComponentInChildren<Interactable>());
            so.FindProperty("_item").objectReferenceValue = rule;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // The prefab shows the same picture until it is changed by hand
        private static void SyncPrefabSprite(GameObject prefab, Sprite before, Sprite after)
        {
            if (prefab == null)
                return;

            var renderer = prefab.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite != before)
                return;

            renderer.sprite = after;
            if (before == null && after != null)                                 // first picture: fit the tap area
            {
                var box = prefab.GetComponentInChildren<BoxCollider2D>();
                box.size = after.bounds.size;
                box.offset = after.bounds.center;
            }
            PrefabUtility.SavePrefabAsset(prefab);
        }

        private bool ConfirmDelete(InteractionRule rule)
        {
            var item = rule.Target;
            var creators = AllRules().Where(r => r != rule && CreatesItem(r, item)).Select(ObjectLabel).ToList();
            if (creators.Count > 0)
            {
                EditorUtility.DisplayDialog("Cannot delete object",
                    $"'{ObjectLabel(rule)}' is created by effects of: {string.Join(", ", creators)}.\nRemove those effects first.", "OK");
                return false;
            }

            var message = $"'{ObjectLabel(rule)}' will be moved to the recycle bin with its actions and effects"
                          + (UsedElsewhere(rule) ? "." : ", its Item and its prefab (if created by the editor).");

            var levels = FindAll<LevelConfig>().Where(l => l.objects.Any(o => o.itemId == item.Id)).Select(l => l.name).ToList();
            if (levels.Count > 0)
                message += $"\n\nIt is placed on: {string.Join(", ", levels)}. Generate those level configs again afterwards.";

            return EditorUtility.DisplayDialog("Delete object?", message, "Delete", "Cancel");
        }

        private void DeleteObject(InteractionRule rule)
        {
            var item = rule.Target;
            var prefab = item.ItemPrefab;
            bool keepItem = UsedElsewhere(rule);

            _database.Rules.Remove(rule);
            EditorUtility.SetDirty(_database);
            _selected = null;
            _itemSo = null;

            Trash(rule);
            if (!keepItem)
            {
                Trash(item);
                if (prefab != null && AssetDatabase.GetAssetPath(prefab).StartsWith(EditorPaths.Prefabs_Path))
                    Trash(prefab);
            }
            AssetDatabase.SaveAssets();
        }

        private static void Trash(UnityEngine.Object asset) => AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(asset));

        private static IEnumerable<InteractionRule> AllRules() =>
            FindAll<InteractionDatabase>().SelectMany(db => db.Rules).Where(r => r != null);

        private static bool CreatesItem(InteractionRule rule, Item item) =>
            rule.ActionRules.SelectMany(a => a.Effects).Any(e => e.EffectTag == EffectTagEnum.Create && e.Item == item);

        private static bool UsedElsewhere(InteractionRule rule) => AllRules().Any(r => r != rule && r.Target == rule.Target);
    }
}
