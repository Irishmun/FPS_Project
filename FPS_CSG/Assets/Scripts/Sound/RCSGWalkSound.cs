using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCSGWalkSound : BaseWalkSound
{
    public override void PlayMaterialWalkSound(Camera viewCamera, float RaycastHeight, AudioSource audio, int layerMask)
    {
        if (Physics.Raycast(viewCamera.transform.position, Vector3.down, out RaycastHit hit, RaycastHeight, gameObject.layer))
        {
            Material res = hit.GetBrushMaterial();
            if (!res)
            {
                res = hit.GetMaterial();
            }
            _MaterialsLookup.PlayWalkSound(res, audio);
        }
    }

}
