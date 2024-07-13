using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.Text;

public class Cell : MonoBehaviour
{
    public Material lineMaterial;
    public bool isSelected;
    public SpriteRenderer selectedRenderer; 
    public SpriteRenderer hoverRenderer; 
    public List<Cell> connectedCells = new List<Cell>();
    public List<LineRenderer> linesRenderer = new List<LineRenderer>();
    public bool isHoveredOver;
    public Color lineShadowColor;
    public Gradient lineColor;

    public string Name; 
    



    void Update(){
        Name = GetComponentInChildren<TMP_InputField>().text;
        selectedRenderer.gameObject.SetActive(isSelected);
        hoverRenderer.gameObject.SetActive(isHoveredOver);

         for (int i = connectedCells.Count - 1; i >= 0; i--)
        {
            // Check if the connectedCells[i] is missing
            if (connectedCells[i] == null || connectedCells[i] == this)
            {
                Debug.Log("Missing or null GameObject at index: " + i);

                // Destroy the corresponding LineRenderer GameObject
                if (linesRenderer[i] != null)
                {
                    Destroy(linesRenderer[i].gameObject);
                }

                // Remove the entries from the lists
                connectedCells.RemoveAt(i);
                linesRenderer.RemoveAt(i);
            }
        }


        for (int i = 0; i < linesRenderer.Count; i++){
           

            linesRenderer[i].material = lineMaterial;
            linesRenderer[i].widthMultiplier = 0.08f;
            linesRenderer[i].sortingLayerName = "UnderCell";
            linesRenderer[i].SetPosition (0, transform.position);
            linesRenderer[i].SetPosition (1, connectedCells[i].transform.position);
            linesRenderer[i].colorGradient = lineColor;

        }

       
    }

    public void AddConnection(){
        var child = new GameObject();
        child.transform.SetParent(transform);
        linesRenderer.Add(child.transform.AddComponent<LineRenderer>());
        LineRendererWithShadow shadow = child.transform.AddComponent<LineRendererWithShadow>();
        shadow.shadowColor = lineShadowColor;
    }



    
}
