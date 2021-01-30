using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    public GameObject circleCursorPrefab;

    [Range(1, 6)]
    public float minZoomSize = 2f;
    [Range(20, 100)]
    public float maxZoomSize = 25f;

    Vector3 lastMousePosition;
    Vector3 currentMousePosition;
    Vector3 dragStartPosition;

    List<GameObject> dragPreviews;

    // Start is called before the first frame update
    void Start()
    {
        dragPreviews = new List<GameObject>();
    }

    public Vector3 GetMousePosition()
    {
        return currentMousePosition;
    }

    public Tile GetMouseOverTile()
    {
        return WorldController.World.GetTileAt(Mathf.RoundToInt(currentMousePosition.x), Mathf.RoundToInt(currentMousePosition.y));
    }

    // Update is called once per frame
    void Update()
    {
        currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMousePosition.z = 0;

        //UpdateCursor();
        UpdateDragging();
        UpdateCameraMovement();

        lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition.z = 0;
    }

    private void UpdateCameraMovement()
    {
        //check RMB or MMB for mouse dragging
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            Vector3 diff = lastMousePosition - currentMousePosition;
            Camera.main.transform.Translate(diff);
        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoomSize, maxZoomSize);
    }

    private void UpdateDragging()
    {
        //If over UI element, break
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //Start LMB drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = currentMousePosition;
        }

        int start_x = Mathf.RoundToInt(dragStartPosition.x);
        int end_x = Mathf.RoundToInt(currentMousePosition.x);

        if (end_x < start_x)
        {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }

        int start_y = Mathf.RoundToInt(dragStartPosition.y);
        int end_y = Mathf.RoundToInt(currentMousePosition.y);

        if (end_y < start_y)
        {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }

        //clean up old drag previews
        while (dragPreviews.Count > 0)
        {
            GameObject go = dragPreviews[0];
            dragPreviews.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        //checks if LMB is currently dragging
        if (Input.GetMouseButton(0))
        {
            //display preview of drag area
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        //display the building hint on this tile position
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        go.transform.SetParent(this.transform, true);
                        dragPreviews.Add(go);
                    }
                }
            }
        }

        //end LMB drag
        if (Input.GetMouseButtonUp(0))
        {
            BuildModeController bmc = FindObjectOfType<BuildModeController>();

            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.World.GetTileAt(x, y);

                    if (t != null)
                    {
                        bmc.DoBuild(t);
                    }
                }
            }
        }
    }
}