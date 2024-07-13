using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererWithShadow : MonoBehaviour
{
    public Vector3 shadowOffset = new Vector3(0.1f, -0.1f, 0f); // Offset for the shadow
    public Color shadowColor = Color.black; // Color of the shadow

    private LineRenderer mainLineRenderer;
    private LineRenderer shadowLineRenderer;

    void Start()
    {
        // Get the main line renderer
        mainLineRenderer = GetComponent<LineRenderer>();

        // Create a new GameObject for the shadow
        GameObject shadowObject = new GameObject("Shadow");
        shadowObject.transform.SetParent(transform);

        // Add a LineRenderer component to the shadow object
        shadowLineRenderer = shadowObject.AddComponent<LineRenderer>();

        // Copy the settings from the main line renderer to the shadow line renderer
        shadowLineRenderer.widthMultiplier = mainLineRenderer.widthMultiplier;
        shadowLineRenderer.positionCount = mainLineRenderer.positionCount;
        shadowLineRenderer.material = mainLineRenderer.material;
        shadowLineRenderer.startColor = shadowColor;
        shadowLineRenderer.endColor = shadowColor;

        // Ensure the shadow is rendered behind the main line
        shadowLineRenderer.sortingLayerID = mainLineRenderer.sortingLayerID;
        shadowLineRenderer.sortingOrder = mainLineRenderer.sortingOrder - 1;
    }

    void Update()
    {
        shadowLineRenderer.enabled = mainLineRenderer.enabled;

        // Update the shadow line renderer positions
        Vector3[] positions = new Vector3[mainLineRenderer.positionCount];
        mainLineRenderer.GetPositions(positions);

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] += shadowOffset;
        }

        shadowLineRenderer.SetPositions(positions);
    }
}
