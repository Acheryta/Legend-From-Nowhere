using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : WeaponController
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
        GameObject spawnedShield = Instantiate(weaponData.Prefab);
        spawnedShield.transform.position = transform.position; //Assign the position to be the parent position(player position)
        spawnedShield.transform.parent = transform; //Spawn below this object
    }
}
