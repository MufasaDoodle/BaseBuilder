using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    float soundCooldown = 0f;

    // Start is called before the first frame update
    void Start()
    {
        WorldController.World.RegisterStructureChanged(OnStructureCreated);
        WorldController.World.RegisterTileChanged(OnTileChanged);
    }

    // Update is called once per frame
    void Update()
    {
        Mathf.Clamp(soundCooldown -= Time.deltaTime, 0, 1);
    }

    void OnTileChanged(Tile tile_data)
    {
        //TODO

        if (soundCooldown > 0f)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>($"Sounds/{tile_data.Type}_OnCreated");

        if (ac == null)
        {
            //no specific sound exists for this tile, playing default sound
            ac = Resources.Load<AudioClip>($"Sounds/Tile_Default");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }

    void OnStructureCreated(Structure structure)
    {
        //TODO

        if (soundCooldown > 0f)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>($"Sounds/{structure.ObjectType}_OnCreated");

        if (ac == null)
        {
            //no specific sound exists for this structure, playing default sound
            ac = Resources.Load<AudioClip>($"Sounds/Object_Default");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }
}
