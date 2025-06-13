using UnityEngine;

public class ChangeMoveset : MonoBehaviour
{
    


    public static void SetWeapon(WeaponData weaponRef, CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.weapon = weaponRef;
        
    }

    public static void SetShield(ShieldData shieldRef, CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.shield = shieldRef;
    }

    public static void SetItem()
    {

    }

}
