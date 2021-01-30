using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseOverRoomIndexText : MonoBehaviour
{
    Text myText;
    MouseController mouseController;


    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<Text>();

        if(myText == null)
        {
            Debug.LogError($"UIMouseOverRoomIndex: No 'Text' UI component on this object");
            this.enabled = false;
            return;
        }

        mouseController = FindObjectOfType<MouseController>();
        if(mouseController == null)
        {
            Debug.LogError("No mouse controller found");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Tile t = mouseController.GetMouseOverTile();
        
        if(t != null)
        {
            myText.text = $"Room Index: {t.World.rooms.IndexOf(t.room).ToString()}";
        }
        else
        {
            myText.text = $"Room Index: N/A";
        }
    }
}
