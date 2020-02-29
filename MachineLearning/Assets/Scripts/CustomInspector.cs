using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CartIO))]
public class CustomInspector : Editor
{

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CartIO movement = (CartIO)target;

        if (GUILayout.Button("Left")) {
            movement.MoveLeft(50);
        }
        
        if (GUILayout.Button("Right")) {
            movement.MoveRight(50);
        }

        if (GUILayout.Button("test")) {
            movement.Reset();
        }
        
    }
}
