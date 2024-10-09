using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

public class EditorTest : EditorWindow
{
    [MenuItem("Test/My Editor Window")]
    private static void ShowWindow()
    {
        var window = GetWindow<EditorTest>();
        window.titleContent = new GUIContent("My Editor Window");
        window.Show();
    }
    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        var label = new Label("Hello World!");
        root.Add(label);

        var button = new Button(() => { Debug.Log("Hello World"); });
        root.Add(button);

        Func<VisualElement> makeItem = () =>
        {
            return new Label();
        };
        Action<VisualElement, int> bindItem = (element, index) =>
        {
            (element as Label).text = "Element " + index;
        };

        var listView = new ListView(new[] { "1", "2", "3", "4", "5" }, 20, makeItem, bindItem);
        listView.selectionType = SelectionType.Multiple;
        listView.showAddRemoveFooter = true;
        listView.reorderable = true;
        listView.reorderMode = ListViewReorderMode.Animated;
        listView.showBorder = true;
        listView.showAddRemoveFooter = true;
        listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
        listView.showBoundCollectionSize = true;
        listView.onSelectedIndicesChange += obj =>
        {
            Debug.Log("onSelectedIndicesChanged");
        };
        listView.onItemsChosen += obj =>
        {
            Debug.Log("onItemsChosen");
        };
        listView.onSelectionChange += obj =>
        {
            Debug.Log("onSelectionChange");
        };
        root.Add(listView);
    }
}
