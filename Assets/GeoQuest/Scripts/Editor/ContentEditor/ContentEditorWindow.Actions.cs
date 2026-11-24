using System;
using System.Collections.Generic;
using System.Linq;
using GeoQuest.Data;
using UnityEditor;
using UnityEngine;

namespace GeoQuest
{
    public partial class ContentEditorWindow
    {
        private void Select(InteractionRule rule)
        {
            MakeOwned(rule);
            _selected = rule;
            Repaint();
        }

        private void DrawActions(InteractionRule rule)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            ActionRule removed = null;
            foreach (var action in rule.ActionRules)
                if (DrawAction(rule, action))
                    removed = action;           // removed after the loop

            if (removed != null)
                RemoveAction(rule, removed);

            if (GUILayout.Button("+ Action"))
                ShowAddActionMenu(rule);
        }

        // true when X is pressed
        private bool DrawAction(InteractionRule rule, ActionRule action)
        {
            bool remove;
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(ActionTitle(action), ActionHeader);
                    remove = GUILayout.Button("X", GUILayout.Width(22));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(16);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        DrawConditions(action);

                        var so = new SerializedObject(action);
                        so.Update();
                        EditorGUILayout.PropertyField(so.FindProperty(nameof(ActionRule.Time)), new GUIContent("Time", "Seconds the character spends on the action before the effects happen. 0 = instantly."));
                        EditorGUILayout.PropertyField(so.FindProperty(nameof(ActionRule.SpendsCharge)), new GUIContent("Spends a charge", "Takes one charge from the dragged item when the action runs. The action fails if the item has no charges left."));
                        so.ApplyModifiedProperties();

                        DrawEffects(rule, action);
                    }
                }
            }
            return remove;
        }

        private static GUIStyle _actionHeader;
        private static GUIStyle ActionHeader => _actionHeader ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };

        private void ShowAddActionMenu(InteractionRule rule)
        {
            var menu = new GenericMenu();
            foreach (var tag in FindAll<ActionTag>().Where(t => !t.isCharacterAction || rule.ActionRules.All(a => a.Action != t)))
                menu.AddItem(new GUIContent(ActionName(tag)), false, () => AddAction(rule, tag));
            menu.ShowAsContext();
        }

        private void AddAction(InteractionRule rule, ActionTag tag)
        {
            var action = AddSub<ActionRule>(rule, tag.name);
            action.Action = tag;
            action.Conditions = new List<ConditionDefinition>();   // lists are null on a fresh asset
            action.Effects = new List<EffectRule>();

            Undo.RecordObject(rule, "Add action");
            rule.ActionRules.Add(action);
            EditorUtility.SetDirty(rule);
            Repaint();
        }

        private static void RemoveAction(InteractionRule rule, ActionRule action)
        {
            rule.ActionRules.Remove(action);
            EditorUtility.SetDirty(rule);
            foreach (var effect in action.Effects)
                DestroySub(effect);
            DestroySub(action);
        }

        private void DrawConditions(ActionRule action)
        {
            EditorGUILayout.LabelField("Conditions", EditorStyles.miniBoldLabel);

            int removeAt = -1;
            for (int i = 0; i < action.Conditions.Count; i++)
            {
                var index = i;
                using (new EditorGUILayout.HorizontalScope())
                {
                    switch (action.Conditions[i])
                    {
                        case HasTagCondition has:
                            EditorGUILayout.PrefixLabel("Used item has tag");
                            if (EditorGUILayout.DropdownButton(new GUIContent(has.Tag.DisplayName), FocusType.Keyboard))
                                ShowTagMenu(tag => SetCondition(action, index, FindOrCreateHasTag(tag)));
                            break;
                        case IsItemCondition isItem:
                            EditorGUILayout.PrefixLabel("Used item is");
                            if (EditorGUILayout.DropdownButton(new GUIContent(ItemLabel(isItem.Item)), FocusType.Keyboard))
                                ShowItemMenu(item => SetCondition(action, index, FindOrCreateIsItem(item)));
                            break;
                        default:
                            EditorGUILayout.LabelField(action.Conditions[i].name);
                            break;
                    }

                    if (GUILayout.Button("X", GUILayout.Width(22)))
                        removeAt = i;
                }
            }

            if (removeAt >= 0)
            {
                action.Conditions.RemoveAt(removeAt);
                EditorUtility.SetDirty(action);
            }

            if (GUILayout.Button("+ Condition"))
                ShowAddConditionMenu(action);
        }

        private void ShowTagMenu(Action<ItemTag> onPick)
        {
            var menu = new GenericMenu();
            foreach (var tag in FindAll<ItemTag>())
                menu.AddItem(new GUIContent(tag.DisplayName), false, () => onPick(tag));
            menu.ShowAsContext();
        }

        private void SetCondition(ActionRule action, int index, ConditionDefinition condition)   // index -1 appends
        {
            Undo.RecordObject(action, "Set condition");
            if (index < 0)
                action.Conditions.Add(condition);
            else
                action.Conditions[index] = condition;
            EditorUtility.SetDirty(action);
            Repaint();
        }

        // One condition asset per tag
        private static HasTagCondition FindOrCreateHasTag(ItemTag tag)
        {
            var existing = FindAll<HasTagCondition>().FirstOrDefault(c => c.Tag == tag);
            if (existing != null)
                return existing;

            var condition = CreateAsset<HasTagCondition>(EditorPaths.Conditions_Path, "Has" + tag.DisplayName);
            condition.Tag = tag;
            EditorUtility.SetDirty(condition);
            return condition;
        }

        // "+ Condition": by tag or by a specific object
        private void ShowAddConditionMenu(ActionRule action)
        {
            var menu = new GenericMenu();
            foreach (var tag in FindAll<ItemTag>())
                menu.AddItem(new GUIContent("Used item has tag/" + tag.DisplayName), false,
                    () => SetCondition(action, -1, FindOrCreateHasTag(tag)));
            foreach (var item in DatabaseItems())
                menu.AddItem(new GUIContent("Used item is/" + ItemLabel(item)), false,
                    () => SetCondition(action, -1, FindOrCreateIsItem(item)));
            menu.ShowAsContext();
        }

        private void ShowItemMenu(Action<Item> onPick)
        {
            var menu = new GenericMenu();
            foreach (var item in DatabaseItems())
                menu.AddItem(new GUIContent(ItemLabel(item)), false, () => onPick(item));
            menu.ShowAsContext();
        }

        private static IsItemCondition FindOrCreateIsItem(Item item)
        {
            var existing = FindAll<IsItemCondition>().FirstOrDefault(c => c.Item == item);
            if (existing != null)
                return existing;

            var condition = CreateAsset<IsItemCondition>(EditorPaths.Conditions_Path, "Is" + item.name);
            condition.Item = item;
            EditorUtility.SetDirty(condition);
            return condition;
        }

        private IEnumerable<Item> DatabaseItems() => _database.Rules.Where(r => r != null && r.Target != null).Select(r => r.Target);

        private static string ItemLabel(Item item) => string.IsNullOrEmpty(item.DisplayName) ? item.name : item.DisplayName;

        private void DrawEffects(InteractionRule rule, ActionRule action)
        {
            EditorGUILayout.LabelField("Effects", EditorStyles.miniBoldLabel);

            EffectRule removed = null;
            foreach (var effect in action.Effects)
            {
                using (new EditorGUILayout.VerticalScope("helpbox"))
                {
                    var so = new SerializedObject(effect);
                    so.Update();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.EffectTag)), new GUIContent("Type"));
                        if (GUILayout.Button("X", GUILayout.Width(22)))
                            removed = effect;
                    }

                    switch (effect.EffectTag)
                    {
                        case EffectTagEnum.Create:
                            DrawItemPopup(so.FindProperty(nameof(EffectRule.Item)), "Creates");
                            EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.Place)), new GUIContent("Where"));
                            break;
                        case EffectTagEnum.AddToInventory:
                            EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.Subject)),
                                new GUIContent("Adds", "This: the object is picked up and removed from the scene. Other: the chosen object is added, the scene is unchanged."));
                            if (effect.Subject == EffectSubject.Other)
                                DrawItemPopup(so.FindProperty(nameof(EffectRule.Item)), "Which");
                            break;
                        case EffectTagEnum.Destroys:
                            EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.Subject)), new GUIContent("Destroys"));
                            if (effect.Subject == EffectSubject.This)
                                EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.OnlyWhenOutOfCharges)),
                                    new GUIContent("Only when out of charges", "The dragged item leaves the backpack only when it has no charges left."));
                            break;
                    }

                    EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.Quantity)), new GUIContent("Quantity", "How many objects are created or added. Create (Nearby) and Add to inventory (Other) only."));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.Chance)), new GUIContent("Chance %", "Chance that the effect happens, 0-100."));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(EffectRule.Time)), new GUIContent("Time", "Seconds until the effect happens after the action has run. The character is free meanwhile. Effects still waiting are lost if the player leaves the location. 0 = instantly."));

                    so.ApplyModifiedProperties();
                }
            }

            if (removed != null)
            {
                action.Effects.Remove(removed);
                EditorUtility.SetDirty(action);
                DestroySub(removed);
            }

            if (GUILayout.Button("+ Effect"))
                ShowAddEffectMenu(rule, action);
        }

        // Objects of this database only
        private void DrawItemPopup(SerializedProperty itemProp, string label)
        {
            var rules = _database.Rules.Where(r => r != null && r.Target != null).ToList();
            var items = rules.Select(r => r.Target).ToList();
            var current = items.IndexOf((Item)itemProp.objectReferenceValue);

            var next = EditorGUILayout.Popup(label, current, rules.Select(ObjectLabel).ToArray());
            if (next != current)
                itemProp.objectReferenceValue = items[next];
        }

        private void ShowAddEffectMenu(InteractionRule rule, ActionRule action)
        {
            var menu = new GenericMenu();
            foreach (EffectTagEnum type in Enum.GetValues(typeof(EffectTagEnum)))
                menu.AddItem(new GUIContent(type.ToString()), false, () => AddEffect(rule, action, type));
            menu.ShowAsContext();
        }

        private void AddEffect(InteractionRule rule, ActionRule action, EffectTagEnum type)
        {
            var effect = AddSub<EffectRule>(rule, type.ToString());
            effect.EffectTag = type;
            effect.Quantity = 1;
            effect.Chance = 100;

            Undo.RecordObject(action, "Add effect");
            action.Effects.Add(effect);
            EditorUtility.SetDirty(action);
            Repaint();
        }

        private static string ActionTitle(ActionRule action)
        {
            var title = ActionName(action.Action);
            return action.Conditions.Count == 0 ? title : $"{title} ({string.Join(", ", action.Conditions.Select(ConditionLabel))})";
        }

        private static string ConditionLabel(ConditionDefinition condition) => condition switch
        {
            HasTagCondition has => has.Tag.DisplayName,
            IsItemCondition isItem => ItemLabel(isItem.Item),
            _ => condition.name
        };

        private static string ActionName(ActionTag tag) => string.IsNullOrEmpty(tag.Name) ? tag.name : tag.Name;

        private static T AddSub<T>(InteractionRule owner, string subName) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = subName;
            AssetDatabase.AddObjectToAsset(asset, owner);
            return asset;
        }

        private static void DestroySub(UnityEngine.Object asset)
        {
            AssetDatabase.RemoveObjectFromAsset(asset);
            UnityEngine.Object.DestroyImmediate(asset, true);
        }

        // Legacy actions and effects are shared files: copy them into the object's own asset
        private static void MakeOwned(InteractionRule rule)
        {
            var path = AssetDatabase.GetAssetPath(rule);
            for (int i = 0; i < rule.ActionRules.Count; i++)
            {
                var action = Own(rule, rule.ActionRules[i], path);
                rule.ActionRules[i] = action;

                for (int j = 0; j < action.Effects.Count; j++)
                    action.Effects[j] = Own(rule, action.Effects[j], path);

                EditorUtility.SetDirty(action);
            }
            EditorUtility.SetDirty(rule);
        }

        private static T Own<T>(InteractionRule rule, T asset, string path) where T : ScriptableObject
        {
            if (AssetDatabase.GetAssetPath(asset) == path)
                return asset;

            var copy = UnityEngine.Object.Instantiate(asset);
            copy.name = asset.name;
            AssetDatabase.AddObjectToAsset(copy, rule);
            return copy;
        }
    }
}
