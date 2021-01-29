using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StructureActions
{
    public static void Door_UpdateAction(Structure structure, float deltaTime)
    {
        //Debug.Log("Door UpdateAction");

        if(structure.structureParameters["is_opening"] >= 1)
        {
            structure.structureParameters["openness"] += deltaTime * 4;
            if(structure.structureParameters["openness"] >= 1)
            {
                structure.structureParameters["is_opening"] = 0;
            }
        }
        else
        {
            structure.structureParameters["openness"] -= deltaTime * 4;
        }

        structure.structureParameters["openness"] = Mathf.Clamp01(structure.structureParameters["openness"]);

        if (structure.OnChanged != null)
        {
            structure.OnChanged(structure);
        }
    }

    public static Enterability Door_IsEnterable(Structure structure)
    {
        structure.structureParameters["is_opening"] = 1;

        if(structure.structureParameters["openness"] >= 1)
        {
            return Enterability.Yes;
        }

        return Enterability.Soon;
    }
}
