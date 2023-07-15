using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(WFC))]
public class TestEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Add a simple label
        myInspector.Add(new Label("Gamers Only >:)"));
        myInspector.Add(new Vector3IntField() { bindingPath = "dimensions" });
        myInspector.Add(new Button(() => { ((WFC)target).GenerateFull(); }) { text = "Generate Full" });
        myInspector.Add(new Button(() => { ((WFC)target).TakeStep(); }) { text = "Step" });
        myInspector.Add(new Button(() => { ((WFC)target).Clear(); }) { text = "Clear" });

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

