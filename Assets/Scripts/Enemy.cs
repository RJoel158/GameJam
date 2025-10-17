using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100.0f;
    
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        Debug.Log(health);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
