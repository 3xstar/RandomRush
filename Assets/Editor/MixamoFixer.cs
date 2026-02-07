using UnityEngine;
using UnityEditor;
using System.Linq;

public class MixamoPrefixRemover : EditorWindow
{
    private GameObject selectedModel;
    private string prefixToRemove = "mixamorig7:"; // Стандартный префикс Mixamo (можете изменить)

    [MenuItem("Tools/Mixamo Tools/Remove Bone Prefixes")]
    public static void ShowWindow()
    {
        GetWindow<MixamoPrefixRemover>("Mixamo Prefix Remover");
    }

    private void OnGUI()
    {
        GUILayout.Label("Remove Mixamo Bone Prefixes", EditorStyles.boldLabel);

        selectedModel = (GameObject)EditorGUILayout.ObjectField("Target Model", selectedModel, typeof(GameObject), true);
        prefixToRemove = EditorGUILayout.TextField("Prefix to Remove", prefixToRemove);

        if (GUILayout.Button("Remove Prefixes"))
        {
            if (selectedModel == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a model first!", "OK");
                return;
            }

            RemovePrefixes();
        }
    }

    private void RemovePrefixes()
    {
        SkinnedMeshRenderer skinnedMesh = selectedModel.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMesh == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found in the model!");
            return;
        }

        Transform rootBone = skinnedMesh.rootBone;
        if (rootBone == null)
        {
            Debug.LogError("No root bone found!");
            return;
        }

        Undo.RecordObject(selectedModel, "Remove Mixamo Prefixes");

        // Переименовываем все кости в иерархии
        Transform[] allBones = rootBone.GetComponentsInChildren<Transform>();
        int renamedCount = 0;

        foreach (Transform bone in allBones)
        {
            if (bone.name.StartsWith(prefixToRemove))
            {
                string newName = bone.name.Replace(prefixToRemove, "");
                Debug.Log($"Renaming: {bone.name} → {newName}");
                bone.name = newName;
                renamedCount++;
            }
        }

        // Обновляем ссылки в SkinnedMeshRenderer
        skinnedMesh.bones = skinnedMesh.bones
            .Select(bone => bone != null ? selectedModel.transform.FindDeepChild(bone.name) : null)
            .ToArray();

        EditorUtility.SetDirty(selectedModel);
        Debug.Log($"Success! Renamed {renamedCount} bones.");
    }
}

// Расширение для поиска дочерних объектов по имени (включая вложенные)
public static class TransformExtensions
{
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result != null) return result;

        foreach (Transform child in parent)
        {
            result = child.FindDeepChild(name);
            if (result != null) return result;
        }
        return null;
    }
}