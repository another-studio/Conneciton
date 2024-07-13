using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.Text;

public class Cell : MonoBehaviour
{
    public Material lineMaterial;
    public float lineThickness = 0.08f;
    public float selectedLineThickness = 0.1f;
    public bool isSelected;
    public SpriteRenderer selectedRenderer; 
    public SpriteRenderer hoverRenderer; 
    public List<Cell> connectedCells = new List<Cell>();
    public List<LineRenderer> linesRenderer = new List<LineRenderer>();

    public bool isHoveredOver;
    public Color lineShadowColor;
    public Color lineColor;
    

    public string Name; 



    void Update(){
        Name = GetComponentInChildren<TMP_InputField>().text;
        selectedRenderer.gameObject.SetActive(isSelected);
        hoverRenderer.gameObject.SetActive(isHoveredOver);
        UpdateCollider();

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
            linesRenderer[i].startColor = lineColor;
            linesRenderer[i].endColor = lineColor;

        }

       
    }

    public void AddConnection(){
        var child = new GameObject();
        child.transform.SetParent(transform);
        child.tag = "Line";
        LineRenderer mainLineRenderer = child.transform.AddComponent<LineRenderer>();
        linesRenderer.Add(mainLineRenderer);
        EdgeCollider2D lineCollider = mainLineRenderer.transform.AddComponent<EdgeCollider2D>();
        lineCollider.isTrigger = true;
        lineCollider.edgeRadius = lineThickness/2;
        LineRendererWithShadow shadow = mainLineRenderer.transform.AddComponent<LineRendererWithShadow>();
        shadow.shadowColor = lineShadowColor;
    }


    void UpdateCollider()
    {
        for (int i = 0; i < linesRenderer.Count; i++){
            // Get the positions from the LineRenderer
            int positionCount = linesRenderer[i].positionCount;
            Vector3[] positions = new Vector3[positionCount];
            linesRenderer[i].GetPositions(positions);

            // Convert positions to 2D points
            Vector2[] edgePoints = new Vector2[positionCount];
            for (int j = 0; j < positionCount; j++)
            {
                edgePoints[j] = new Vector2(positions[j].x, positions[j].y);
            }

            // Update the EdgeCollider2D points
            linesRenderer[i].gameObject.GetComponent<EdgeCollider2D>().points = edgePoints;
        }
    }
    
}
