﻿using UnityEngine;
using System.Collections.Generic;

public class JumpBridge : MonoBehaviour
{
    [SerializeField]
    private float maxCollisionTime = 0.2f;
    [SerializeField]
    private float cooldownMultiplier = 0.1f;

    private float collisionTime = 0f;

    private List<GameObject> allParts = new List<GameObject>();
    private List<GameObject> playerCollisionParts = new List<GameObject>();

    void Awake()
    {
        WorldInfo.info.addResetMethod(() => playerCollisionParts.Clear(), "bridgeReset" + gameObject.GetInstanceID());
    }

    public void RegisterPart(GameObject go)
    {
        allParts.Add(go);
    }

    public void CollisionEnter(GameObject go)
    {
        if (!playerCollisionParts.Contains(go))
            playerCollisionParts.Add(go);
    }

    public void CollisionLeave(GameObject go)
    {
        if (playerCollisionParts.Contains(go))
            playerCollisionParts.Remove(go);
    }

    void Update()
    {
        if (playerCollisionParts.Count > 0)
            collisionTime = Mathf.Min(collisionTime + Time.deltaTime, maxCollisionTime);
        else
            collisionTime = Mathf.Max(collisionTime - Time.deltaTime * cooldownMultiplier, 0f);

        foreach (GameObject go in allParts)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = new Color(1f, 1f - collisionTime / maxCollisionTime, 1f - collisionTime / maxCollisionTime);
        }

        if (collisionTime >= maxCollisionTime)
            GameInfo.info.reset();
    }
}