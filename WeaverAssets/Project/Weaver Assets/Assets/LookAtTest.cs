using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTest : MonoBehaviour {

	public GameObject dest;
	// Use this for initialization
	void Start () 
	{
		
	}


	void LookAt(Vector3 destination)
	{
		transform.eulerAngles = new Vector3(0f,0f,Mathf.Atan2(dest.transform.position.y - transform.position.y,dest.transform.position.x - transform.position.x) * Mathf.Rad2Deg);
	}
	
	// Update is called once per frame
	void Update () {
		LookAt(dest.transform.position);
	}
}
