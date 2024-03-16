using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LookTowards : MonoBehaviour
{
    public async Task RotateTowards(float angle, float adjustment)
    {
        transform.rotation = Quaternion.AngleAxis(angle + adjustment, Vector3.forward);
        await Task.Yield();
    }
}
