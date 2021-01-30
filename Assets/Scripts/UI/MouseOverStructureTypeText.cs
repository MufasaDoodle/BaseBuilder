using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseOverStructureTypeText : MonoBehaviour
{
    Text myText;
    MouseController mouseController;


    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<Text>();

        if(myText == null)
        {
            Debug.LogError($"UIMouseOverStructureTypeText: No 'Text' UI component on this object");
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
        string s = "NULL";

        if (t != null && t.Structure != null)
        {
            s = t.Structure.ObjectType;
        }
        myText.text = $"Furniture Type: {s}";
    }
}
