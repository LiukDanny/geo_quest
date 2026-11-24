using UnityEngine;
using UnityEngine.Rendering;

public class YSortGroup : MonoBehaviour
{
    private SortingGroup group;

    void Awake()
    {
        group = GetComponent<SortingGroup>();
    }

    void LateUpdate()
    {
        group.sortingOrder = -(int)(transform.position.y * 100);
    }
}
