using UnityEngine;

public class ChangeMoveset : MonoBehaviour
{
    
    public static void SetInitializingEquippment(CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.weapon = movesetDataRef.weapon;
        movesetDataRef.shield = movesetDataRef.shield;
        movesetDataRef.item = movesetDataRef.item;


    }


    public static void SetWeapon(WeaponData weaponRef, CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.weapon = weaponRef;
    }

    public static void SetShield(ShieldData shieldRef, CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.shield = shieldRef;
    }

    public static void SetItem(ItemData itemRef, CharacterMovesetData movesetDataRef)
    {
        movesetDataRef.item = itemRef;

    }

}
