using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RenderStoredSprite : MonoBehaviour
{
    public SpriteStoreSO storedSprite;
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = storedSprite.sprite;
    }
}
