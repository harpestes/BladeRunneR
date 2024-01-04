using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private HeroKnight player;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Die from " + collision.name);
        player.Die();
    }
}
