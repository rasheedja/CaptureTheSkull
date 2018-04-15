﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyramidController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == this.tag)
        {
            if (other.GetComponent<PlayerController>().IsHoldingSkull())
            {
                other.GetComponent<PlayerController>().DropSkull();
                if (this.tag == "Blue")
                {
                    GameManager.Instance.IncrementBlueScore();
                }
                else
                {
                    GameManager.Instance.IncrementRedScore();
                }
            }
            other.GetComponent<PlayerController>().DecreaseHealth(10);
        }
    }
}
