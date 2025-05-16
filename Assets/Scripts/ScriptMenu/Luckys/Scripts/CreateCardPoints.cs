#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CreateCardPoints : MonoBehaviour
{
    [MenuItem("Tools/Create Card Points")]
    public static void CreatePoints()
    {
        var parent = Selection.activeTransform;
        if (parent == null)
        {
            return;
        }

        // Xoá con cũ
        foreach (Transform child in parent)
            DestroyImmediate(child.gameObject);

        float spacingX = 250f;
        float spacingY = 280f;

        for (int i = 0; i < 9; i++)
        {
            GameObject point = new GameObject($"Point_{i / 3}_{i % 3}", typeof(RectTransform));
            point.transform.SetParent(parent);
            RectTransform rt = point.GetComponent<RectTransform>();

            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2((i % 3 - 1) * spacingX, (1 - i / 3) * spacingY);
        }

    }
}
#endif
