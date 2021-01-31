using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StructureActions
{
    public static void Door_UpdateAction(Structure structure, float deltaTime)
    {
        //Debug.Log("Door UpdateAction");

        if(structure.GetParameter("is_opening") >= 1)
        {
            structure.ChangeParameter("openness", deltaTime * 4);
            if(structure.GetParameter("openness") >= 1)
            {
                structure.SetParameter("is_opening", 0);
            }
        }
        else
        {
            structure.ChangeParameter("openness", deltaTime * -4);
        }

        structure.SetParameter("openness", Mathf.Clamp01(structure.GetParameter("openness")));

        if (structure.OnChanged != null)
        {
            structure.OnChanged(structure);
        }
    }

    public static Enterability Door_IsEnterable(Structure structure)
    {
        structure.SetParameter("is_opening", 1);

        if(structure.GetParameter("openness") >= 1)
        {
            return Enterability.Yes;
        }

        return Enterability.Soon;
    }
}
