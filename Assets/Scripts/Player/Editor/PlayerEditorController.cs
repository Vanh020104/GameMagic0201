using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(PlayerController))]
public class PlayerEditorController : Editor
{
   public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var _target = target as PlayerController;
        if (GUILayout.Button("Add Bullet"))
        {
           _target.NormalAttack();
        }
    }
}