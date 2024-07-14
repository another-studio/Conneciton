using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Required for accessing UI components
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting; // Required for UI elements

public class TouchManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction touchPositionAction;
    private InputAction touchPressAction;
    private InputAction doubleTapAction;

    public GameObject cellPrefab;
    private GameObject currentCell;
    private GameObject connectingCell;
    public GameObject selectionZonePrefab; // Prefab or GameObject for visualizing selection zone
    private GameObject selectionZoneInstance; // Instance of the selection zone visualization
    private GameObject parentObject; 
    private List<Cell> hoveredOverCell = new List<Cell>();
    public List<GameObject> selectedObjects = new List<GameObject>();

    
    public RectTransform singleSelectUI;
    public LineRenderer lineRenderer;

    private Vector2 dragStartPosition;
    private Vector2 dragEndPosition;
    private float touchStartTime;
    public float minSize;
    public float maxSize;
    public float moveSpeed;
    private float lastTapTime = 0f; // Time of the last tap
    public float doubleTapThreshold = 0.3f; // Time threshold for double tap in seconds

    private bool isTouching;
    private bool isCreating;
    private bool isSelecting;
    private bool isConnecting;
    private bool isDragging;

    public bool isMultiSelecting; 


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions.FindAction("TouchPress");
        touchPositionAction = playerInput.actions.FindAction("TouchPosition");
        doubleTapAction = playerInput.actions.FindAction("DoubleTap");
    }

    private void OnEnable()
    {
        touchPressAction.started += OnTouchStarted;
        touchPressAction.canceled += OnTouchCanceled;
    }

    private void OnDisable()
    {
        touchPressAction.started -= OnTouchStarted;
        touchPressAction.canceled -= OnTouchCanceled;
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (Touchscreen.current.touches.Count == 1)
        {
            if (ClickedOnUi() == false && isConnecting == false && isMultiSelecting == false){  
            Vector3 position = Camera.main.ScreenToWorldPoint(touchPositionAction.ReadValue<Vector2>());
            position.z = 0;

            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Cell") && selectedObjects.Count <= 1)
            {
                if(IsDoubleTap() == false){
                    if (selectedObjects != null){
                        OnCellDeselect();
                        selectedObjects.Clear();
                    }

                    selectedObjects.Add(hit.collider.gameObject);
                    OnCellSelect(context);
                }else{
                    CreateConnection();
                }
            }else if(hit.collider != null && hit.collider.gameObject.CompareTag("Cell") && selectedObjects.Count > 1 && IsDoubleTap() == true){
                Group();
            }
            else if (hit.collider == null && !isSelecting)
            {
                if(IsDoubleTap() == false){
                    CreateCell(context);
                    touchStartTime = Time.time;
                    isCreating = true;
                }else{
                    CreateCell(context);
                }
            }
            else if(isSelecting && hit.collider == null)
            {
                OnCellDeselect();   
                selectedObjects.Clear();
            }
            isTouching = true;            
            }
       
            if(isMultiSelecting == true){
                StartDrag();
            }
        }
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(touchPositionAction.ReadValue<Vector2>());
        position.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);

        isTouching = false;
        isCreating = false;
        
        TMP_InputField inputField = currentCell.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }

        if(isMultiSelecting == true){
            EndDrag();
        }

        for (int i = 0; i < selectedObjects.Count; i++){   
            selectedObjects[i].transform.SetParent(null);
        }

        if (isConnecting == true && hit.collider != null && hit.collider.gameObject.CompareTag("Cell"))
        {
            FinishConnection(hit.collider.gameObject);
        }
        else
        {
            isConnecting = false;
        }
    }


    private void CreateCell(InputAction.CallbackContext context)
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(touchPositionAction.ReadValue<Vector2>());
        position.z = 0;
        currentCell = Instantiate(cellPrefab, position, Quaternion.identity);
    }

    private void OnCellSelect(InputAction.CallbackContext context)
    {
        if(selectedObjects.Count == 1){
            singleSelectUI.gameObject.SetActive(true);
            singleSelectUI.anchoredPosition = new Vector2(selectedObjects[0].transform.position.x, selectedObjects[0].transform.position.y -1.5f);
            singleSelectUI.SetParent(selectedObjects[0].transform);
        }


        foreach (GameObject cell in selectedObjects){
            cell.GetComponent<Cell>().isSelected = true;
        }        
        isSelecting = true;


    }

    public void OnCellDeselect()
    {
        if (selectedObjects.Count > 1){
            foreach (GameObject cell in selectedObjects){
                cell.transform.SetParent(null);
            }   
            DestroyImmediate(parentObject);
        }

        singleSelectUI.gameObject.SetActive(false);
        singleSelectUI.SetParent(null);

        foreach (GameObject cell in selectedObjects){
            cell.GetComponent<Cell>().isSelected = false;
        }

        selectedObjects.Clear();
        isSelecting = false;   
    }

    public void CreateConnection(){
        if(isSelecting == true){
            connectingCell = selectedObjects[0]; 
            isConnecting = true;
        }
    }
    
    void FinishConnection(GameObject targetCell){
        bool noPreviousConnection = false;
        
        for (int i = 0; i < connectingCell.GetComponent<Cell>().connectedCells.Count; i++){
            if(connectingCell.GetComponent<Cell>().connectedCells[i].gameObject == targetCell)
                noPreviousConnection = true;
        } 


        if (connectingCell != null && targetCell != connectingCell)
        {
            connectingCell.GetComponent<Cell>().connectedCells.Add(targetCell.GetComponent<Cell>());
            targetCell.GetComponent<Cell>().connectedCells.Add(connectingCell.GetComponent<Cell>());

            connectingCell.GetComponent<Cell>().AddConnection();
            targetCell.GetComponent<Cell>().AddConnection();
        }
        isConnecting = false;
    }

    private void Update()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(touchPositionAction.ReadValue<Vector2>());
        position.z = 0;

        if (isCreating && currentCell != null)
        {
            float touchDuration = Time.time - touchStartTime;
            float scaleMultiplier = Mathf.Lerp(minSize, maxSize, touchDuration);
            currentCell.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1);
        }

        //Moving a single
        if (isSelecting && isTouching && selectedObjects[0] != null && selectedObjects.Count == 1 && isConnecting == false){       
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
            if(parentObject == null)
                parentObject = new GameObject();

            parentObject.transform.position = position;
            for (int i = 0; i < selectedObjects.Count; i++){
                selectedObjects[i].transform.SetParent(parentObject.transform);
            }

            if (hit.collider != null && hit.collider.gameObject == selectedObjects[0].gameObject)
                parentObject.transform.position = Vector3.Lerp(parentObject.transform.position, position, moveSpeed * Time.deltaTime);
        }

        //Moving multiple at once
        if(isSelecting && isTouching && selectedObjects.Count > 1 && isConnecting == false){

            if(parentObject == null)
                parentObject = new GameObject();

            parentObject.transform.position = position;

            for (int i = 0; i < selectedObjects.Count; i++){
                selectedObjects[i].transform.SetParent(parentObject.transform);
            }

            for (int i = 0; i < selectedObjects.Count; i++){
                if (Physics2D.Raycast(position, Vector2.zero).collider.gameObject != null && Physics2D.Raycast(position, Vector2.zero).collider.gameObject == selectedObjects[i].gameObject){
                    parentObject.transform.position = Vector3.Lerp(parentObject.transform.position, position, moveSpeed * Time.deltaTime);
                }
            }
        }

        if(isDragging){
            dragEndPosition = position;

            // Update selection zone visualization
            Vector2 center = (dragStartPosition + dragEndPosition) / 2f;
            Vector2 size = new Vector2(Mathf.Abs(dragEndPosition.x - dragStartPosition.x), Mathf.Abs(dragEndPosition.y - dragStartPosition.y));
            selectionZoneInstance.transform.position = center;
            selectionZoneInstance.transform.localScale = size;
        }

        lineRenderer.enabled = isConnecting;
        if(isConnecting){
            lineRenderer.SetPosition(0, connectingCell.transform.position);
            lineRenderer.SetPosition(1, position);
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero); 
        }

        if(isTouching){
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero); 
            if(isConnecting && hit.collider != null && hit.collider.gameObject.CompareTag("Cell") && hit.collider.gameObject != connectingCell.gameObject)
            {
                if(!hoveredOverCell.Contains(hit.collider.GetComponent<Cell>()))
                    hoveredOverCell.Add(hit.collider.GetComponent<Cell>());

                hoveredOverCell[0].isHoveredOver = true;
            }
            else 
            {
                if(hoveredOverCell.Count > 0){
                    hoveredOverCell[0].isHoveredOver = false;
                    hoveredOverCell.Clear();
               }

            }
        }else{
            if(hoveredOverCell.Count > 0){
                foreach(Cell cell in hoveredOverCell){
                    cell.isHoveredOver = false;
                }
            }    
            hoveredOverCell.Clear();
        }

        if(Input.GetKeyDown(KeyCode.G))
            Group();
        
        if(isDragging){
            Vector2 center = (dragStartPosition + dragEndPosition) / 2f;
            Vector2 size = new Vector2(Mathf.Abs(dragEndPosition.x - dragStartPosition.x), Mathf.Abs(dragEndPosition.y - dragStartPosition.y));
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

            for(int i = 0; i < hits.Length; i++){
                if(!hoveredOverCell.Contains(hits[i].GetComponent<Cell>()))
                    hoveredOverCell.Add(hits[i].GetComponent<Cell>());

               hits[i].GetComponent<Cell>().isHoveredOver = true;
                
            }
        }



    }

    void StartDrag()
    {
        dragStartPosition = Camera.main.ScreenToWorldPoint(touchPositionAction.ReadValue<Vector2>());
        isDragging = true;

        selectionZoneInstance = Instantiate(selectionZonePrefab, dragStartPosition, Quaternion.identity);
    }

    void EndDrag()
    {
        isDragging = false;

        Vector2 center = (dragStartPosition + dragEndPosition) / 2f;
        Vector2 size = new Vector2(Mathf.Abs(dragEndPosition.x - dragStartPosition.x), Mathf.Abs(dragEndPosition.y - dragStartPosition.y));
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);


        selectedObjects.Clear();
        foreach (Collider2D hit in hits)
        {
            selectedObjects.Add(hit.gameObject);
            hit.gameObject.GetComponent<Cell>().isSelected = true;
            isSelecting = true;
        }
        isMultiSelecting = false;
        Destroy(selectionZoneInstance);
    }

    public void Group(){
        if (selectedObjects.Count > 1){
            for (int i = 0; i < selectedObjects.Count; i++){
                for (int j = 0; j < selectedObjects.Count; j++){
                    if (selectedObjects[i].GetComponent<Cell>().connectedCells.Contains(selectedObjects[j].GetComponent<Cell>()) == false){
                        selectedObjects[i].GetComponent<Cell>().connectedCells.Add(selectedObjects[j].GetComponent<Cell>());
                        selectedObjects[i].GetComponent<Cell>().AddConnection();      
                    }
                }
            }
        }
    }

    private bool ClickedOnUi(){
 
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPositionAction.ReadValue<Vector2>();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (var item in results)
        {
            if (item.gameObject.CompareTag("UI"))
            {
                return true;
            }
        }

        return false;
}

    private bool IsDoubleTap()
    {
        float currentTime = Time.time;
        if (currentTime - lastTapTime <= doubleTapThreshold)
        {
            lastTapTime = 0f; // Reset last tap time to avoid multiple detections
            Debug.Log("was a double tap");
            return true;
        }
        lastTapTime = currentTime;
        Debug.Log("was not a double tap");
        return false;
    }

    public void onMultiSelect(){
        isMultiSelecting = !isMultiSelecting;
        OnCellDeselect();   
        selectedObjects.Clear();
    }


}