using System;
using System.Linq;
using UnityEngine;

// A combo box that expands in-place when opened (as opposed to producing a popup)
public class InlineComboBox<T>
{
    private T[] values;
    private string[] labels;
    private bool opened;
    private int selectedIndex;

    public InlineComboBox(T[] values, string[] labels = null)
    {
        opened = false;
        selectedIndex = 0;
        this.values = values;
        if (labels != null)
            this.labels = labels;
        else
            this.labels = values.Select(v => v.ToString()).ToArray();
    }

    public void show(GUILayoutOption options)
    {
        if (opened)
        {
            GUILayout.BeginVertical(options);
            for (int i = 0; i < values.Length; i++)
            {
                if (GUILayout.Button("→ " + labels[i].ToString(), options))
                {
                    opened = false;
                    selectedIndex = i;
                }
            }
            GUILayout.EndVertical();
        }
        else
        {
            if (GUILayout.Button("↓ " + labels[selectedIndex].ToString() + " ↓", options))
            {
                opened = true;
            }
        }
    }

    public int getIndex()
    {
        return selectedIndex;
    }

    public T get()
    {
        return values[selectedIndex];
    }

}
