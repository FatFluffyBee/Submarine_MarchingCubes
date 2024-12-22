using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//abstract base class to build weapons usable by submarine
public abstract class WeaponSystem : MonoBehaviour
{
    public abstract void FireOne();

    public abstract void ReleaseOne();

    public abstract void FireTwo();

    public abstract void ReleaseTwo();
}
