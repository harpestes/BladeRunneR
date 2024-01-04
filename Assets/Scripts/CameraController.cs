using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    private Vector3 pos;

    void Start()
    {
        if (!player)
        {
            player = FindObjectOfType<HeroKnight>().transform;
        }
    }

    void Update()
    {
        pos.x = player.position.x;
        pos.y = player.position.y + 3f;
        pos.z = -10f;

        transform.position = pos;
    }
}
