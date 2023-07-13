using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ChunkGeneratorWFC))]
public class TestEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Add a simple label
        myInspector.Add(new Label("Custom Inspector"));
        myInspector.Add(new Vector3IntField() { bindingPath = "dimensions" });
        myInspector.Add(new Button(() => { ((ChunkGeneratorWFC)target).TakeStep(); }) { text = "Generate" });

        // Return the finished inspector UI
        return myInspector;
    }
    /*    public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ChunkGeneratorWFC myScript = (ChunkGeneratorWFC)target;
            if (GUILayout.Button("Generate"))
            {
                myScript.TakeStep();
            }
        }*/
}

