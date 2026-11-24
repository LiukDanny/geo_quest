using System;
using System.Collections.Generic;
using System.Linq;
using GeoQuest.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "GeoQuest/Config/Action Range Config")]
public class ActionRangeConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public ActionTag Action;
        public float Range = 2f;
    }

    [SerializeField] private float _defaultRange = 2f;
    [SerializeField] private List<Entry> _ranges = new();

    public float GetRange(ActionTag action)
    {
        var entry = _ranges.FirstOrDefault(e => e.Action == action);
        return entry != null ? entry.Range : _defaultRange;
    }
}
