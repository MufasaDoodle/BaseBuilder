using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour
{
    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSprites;

    World World
    {
        get
        {
            return WorldController.World;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        characterGameObjectMap = new Dictionary<Character, GameObject>();

        World.RegisterCharacterCreated(OnCharacterCreated);

        foreach(Character c in World.characters)
        {
            OnCharacterCreated(c);
        }

        //FOR DEBUG
        //c.SetDestination(World.GetTileAt(World.Width / 2 + 5, World.Height / 2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadSprites()
    {
        characterSprites = new Dictionary<string, Sprite>();
        //right now, only wood walls are loaded
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Man Blue");

        foreach (var sprite in sprites)
        {
            characterSprites[sprite.name] = sprite;
        }
    }

    public void OnCharacterCreated(Character c)
    {
        //create GameObject linked to this data

        //TODO does not consider multi-tile objects nor rotated objects

        GameObject char_go = new GameObject();

        characterGameObjectMap.Add(c, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(c.X, c.Y, 0);
        char_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sprite = characterSprites["manBlue_stand"];
        sr.sortingLayerName = "Characters";


        c.RegisterCharacterChanged(OnCharacterChanged);
    }
    
    void OnCharacterChanged(Character c)
    {
        //make sure object's graphics are correct
        if (characterGameObjectMap.ContainsKey(c) == false)
        {
            Debug.LogError("OnCharacterChanged - trying to change visuals for character not in map");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];

        char_go.transform.position = new Vector3(c.X, c.Y,0);
    }    
}
