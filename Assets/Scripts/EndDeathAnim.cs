using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDeathAnim : MonoBehaviour {

    LifeBehaviour life;

	// Use this for initialization
	void Start () {
        life = GetComponentInParent<LifeBehaviour>();
	}
	
	public void OnEndDeathAnimation()
    {
        if(life)
        {
            life.OnEndDeathAnim();
        }
    }
}
