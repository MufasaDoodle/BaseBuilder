using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseOverTileTypeText : MonoBehaviour
{
    Text myText;
    MouseController mouseController;


    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<Text>();

        if(myText == null)
        {
            Debug.LogError($"MouseOverTileTypeText: No 'Text' UI component on this object");
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

        string s = $"Tile Type: No Tile";

        if(t != null)
        {
            myText.text = $"Tile Type: {t.Type.ToString()}";
        }
        else
        {
            myText.text = s;
        }
    }
}
