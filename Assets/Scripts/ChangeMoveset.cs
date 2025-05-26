using UnityEngine;

public class ChangeMoveset : MonoBehaviour
{
    


    public static void SetWeapon(WeaponData weaponRef, CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.weapon = weaponRef;
        
    }

    //public static void ChangeWeaponAndSetMoveset(WeaponData weaponRef, CharacterMovesetData movesetDataRef)
    //{
    //    movesetDataRef.weapon = weaponRef;
    //    ChangeAnimation.
    //}

    public static void SetShield()
    {

    }

    public static void SetItem()
    {

    }

}
