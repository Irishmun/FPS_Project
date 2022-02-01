using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkSound : BaseWalkSound
{
    public override void PlayMaterialWalkSound(Camera viewCamera, float RaycastHeight, AudioSource audio, int layerMask)
    {
        Debug.DrawLine(viewCamera.transform.position, viewCamera.transform.position + new Vector3(0, -RaycastHeight, 0), Color.cyan, 1, false);
        if (Physics.Raycast(viewCamera.transform.position, Vector3.down, out RaycastHit hit, RaycastHeight, layerMask))
        {
            Material res = hit.GetMaterial();
            _MaterialsLookup.PlayWalkSound(res, audio);
        }
    }
}


