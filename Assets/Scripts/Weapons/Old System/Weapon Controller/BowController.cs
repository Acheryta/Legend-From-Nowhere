using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowController : WeaponController
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
        GameObject spawnedArrow = Instantiate(weaponData.Prefab);
        spawnedArrow.transform.position = transform.position; //Assign the position to be the parent position(player position)
        spawnedArrow.GetComponent<BowBehavior>().DirectionChecker(pm.lastMovedVector); //reference and set the direction
    }
}
