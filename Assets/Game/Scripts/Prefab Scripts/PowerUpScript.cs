using UnityEngine;

// this scrips attaches to a power up prefab giving it a name
public class PowerUpScript : MonoBehaviour
{
    public string powerUpName;

    // return the PU name
    public string GetName()
    {
        return powerUpName;
    }
    
}
