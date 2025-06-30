using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggScript : MonoBehaviour
{
    // Aplica damage jucatorului la contact si se distruge
    private void OnCollisionEnter2D(Collision2D target)
    {
        if (target.gameObject.tag == MyTags.PLAYER_TAG)
        {
            target.gameObject.GetComponent<PlayerDamage>().DealDamage();
        }
        gameObject.SetActive(false);
    }
}
