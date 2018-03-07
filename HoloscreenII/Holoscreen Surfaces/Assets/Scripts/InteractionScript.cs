﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InteractionScript : MonoBehaviour {
	/**
	 * Child indexes of hand represent:
	 * 0 - thumb
	 * 1 - index
	 * 2 - middle
	 * 3 - pinky
	 * 4 - ring
	 * 5 - palm for another hand
	 * 6- forearm
	 */
	private GameObject hand_l,hand_r;

	//Left hand finger declare
	private GameObject thumb_l, indexfinger_l, middlefinger_l, ringfinger_l, palm_l;
	private GameObject thumb_l_2, indexfinger_l_2, middlefinger_l_2, ringfinger_l_2;
	//Right hand finger declare
	private GameObject thumb_r, indexfinger_r, middlefinger_r, ringfinger_r, palm_r;
	private GameObject thumb_r_2, indexfinger_r_2, middlefinger_r_2, ringfinger_r_2;

	private GameObject grabHolder;
	private DataManager dataManager;

	//Boolean var to record current grab motion
	bool grabbed = false;

	private int sizeOfSpeedQ = 10; 
	private Queue<Vector3> speedList = new Queue<Vector3>();
	private float colliderReenableTime = 1.5f;
	private float add;
	private float restoreColliderTimer;

	private Vector3 prePos;

	public Material primaryMaterial;
	public Material secondaryMaterial;

	// Use this for initialization
	void Start () {
		Debug.Log ("Script started");

		//0.21 finger to palm
		dataManager = GameObject.Find ("gDataManager").GetComponent<DataManager> ();

		hand_l = GameObject.Find ("Hand_l").gameObject;
		hand_r = GameObject.Find ("Hand_r").gameObject;

		for (int i=0; i < sizeOfSpeedQ; i++)
			speedList.Enqueue(new Vector3(0,0,0));

		thumb_l = hand_l.transform.GetChild (0).gameObject;
		indexfinger_l = hand_l.transform.GetChild (1).gameObject;
		middlefinger_l = hand_l.transform.GetChild (2).gameObject;
		ringfinger_l = hand_l.transform.GetChild (4).gameObject;
		palm_l = hand_l.transform.GetChild (5).gameObject;

		thumb_l_2 = thumb_l.transform.GetChild (2).gameObject;
		indexfinger_l_2 = indexfinger_l.transform.GetChild (2).gameObject;
		middlefinger_l_2 = middlefinger_l.transform.GetChild (2).gameObject;
		ringfinger_l_2 = ringfinger_l.transform.GetChild (2).gameObject;

		thumb_r = hand_r.transform.GetChild (0).gameObject;
		indexfinger_r = hand_r.transform.GetChild (1).gameObject;
		middlefinger_r = hand_r.transform.GetChild (2).gameObject;
		ringfinger_r = hand_r.transform.GetChild (4).gameObject;
		palm_r = hand_r.transform.GetChild (5).gameObject;

		thumb_r_2 = thumb_r.transform.GetChild (2).gameObject;
		indexfinger_r_2 = indexfinger_r.transform.GetChild (2).gameObject;
		middlefinger_r_2 = middlefinger_r.transform.GetChild (2).gameObject;
		ringfinger_r_2 = ringfinger_r.transform.GetChild (2).gameObject;

		grabHolder = palm_l.transform.GetChild (0).gameObject;

		prePos = new Vector3 (0, 0, 0);

		//new 03/06
		palm_l.GetComponent<Collider> ().isTrigger = true;
		palm_l.GetComponent<Rigidbody> ().detectCollisions = false;
	}

	bool isMoving(float thredValue){
		//check movement on 3 axis
		Vector3 tmp = this.transform.position - prePos;
		prePos = this.transform.position;
		if ((Math.Abs (tmp [0]) < thredValue) &&
			(Math.Abs (tmp [1]) < thredValue) &&
			(Math.Abs (tmp [2]) < thredValue))
			return true;
		return false;
	}

	// Update is called once per frame
	void Update () {
		Debug.Log ("Script updated");

		//Bounce prevetion mechinism
		this.GetComponent<Rigidbody> ().velocity = new Vector3(Math.Min(this.GetComponent<Rigidbody> ().velocity.x, 1f),Math.Min(this.GetComponent<Rigidbody> ().velocity.y, 1f),Math.Min(this.GetComponent<Rigidbody> ().velocity.z, 1f));
		this.GetComponent<Rigidbody> ().velocity = new Vector3(Math.Max(this.GetComponent<Rigidbody> ().velocity.x, -1f),Math.Max(this.GetComponent<Rigidbody> ().velocity.y, -1f),Math.Max(this.GetComponent<Rigidbody> ().velocity.z, -1f));

		//Grab Detection: 
		//With rigidbody
		Collider c = this.GetComponent<Collider>();
		float dist_thumb_index_l = Vector3.Distance(thumb_l_2.transform.position, indexfinger_l_2.transform.position);
		float dist_thumb_index_r = Vector3.Distance(thumb_r_2.transform.position, indexfinger_r_2.transform.position);

		Vector3 indexfinger_0_palm_r = indexfinger_r.transform.GetChild(0).position - palm_r.transform.position;
		Vector3 indexfinger_2_thumb1_r = indexfinger_r.transform.GetChild(2).position - palm_r.transform.position;
		float curve_indexfinger_r = Vector3.Angle(indexfinger_2_thumb1_r, indexfinger_0_palm_r);

		//Determine whethere a grab happnens. Left hand has priority here.
		if ((dist_thumb_index_l < 0.065) && 
			c.bounds.Intersects (thumb_l_2.GetComponent<Collider> ().bounds) &&
			(c.bounds.Intersects (indexfinger_l_2.GetComponent<Collider> ().bounds) ||
				c.bounds.Intersects (middlefinger_l_2.GetComponent<Collider> ().bounds) ||
				c.bounds.Intersects (ringfinger_l_2.GetComponent<Collider> ().bounds))) {

			//Record current velocity and delete the oldest velocity
			this.GetComponent<Collider> ().isTrigger = true;
			this.GetComponent<Rigidbody> ().useGravity = false;
			this.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			this.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
			this.GetComponent<Rigidbody> ().Sleep ();
			this.transform.SetParent(grabHolder.transform);

			//Throw
			Vector3 initial_v = new Vector3 ();
			initial_v = palm_l.GetComponent<Rigidbody> ().velocity;
			speedList.Dequeue ();
			speedList.Enqueue (initial_v);
			restoreColliderTimer = Time.time;
			grabbed = true;
			//this.GetComponent<Rigidbody> ().isKinematic = true;
			dataManager.setLeftHandGrab (true);


			//New
			palm_l.GetComponent<Collider> ().isTrigger = true;
			palm_l.GetComponent<Rigidbody> ().detectCollisions = false;
		}  /*else if (dist_thumb_index_r < 0.060 && (curve_indexfinger_r>15) &&
			(c.bounds.Intersects (thumb_r_2.GetComponent<Collider> ().bounds) || c.bounds.Contains (thumb_r_2.transform.position)) &&
			((c.bounds.Intersects (indexfinger_r_2.GetComponent<Collider> ().bounds) ||
				c.bounds.Intersects (middlefinger_r_2.GetComponent<Collider> ().bounds) ||
				c.bounds.Intersects (ringfinger_r_2.GetComponent<Collider> ().bounds)))) {*/
		/*this.GetComponent<Rigidbody> ().isKinematic = true;
		this.transform.parent = palm_r.transform;
		Vector3 initial_v = new Vector3 ();
		initial_v = palm_r.GetComponent<Rigidbody> ().velocity;
		speedList.Dequeue ();
		speedList.Enqueue (initial_v);
		grabbed = true;
		for (int i = 0; i < 2; i++) {
			palm_l.GetComponent<Collider> ().isTrigger = true;
			palm_r.GetComponent<Collider> ().isTrigger = true;
		}
		restoreColliderTimer = Time.time;
		*/
		//} 
		else if (grabbed && dist_thumb_index_l > 0.075f) {
			grabbed = false;
			//this.GetComponent<Rigidbody>().useGravity = true;
			this.transform.parent = null;
			this.GetComponent<Rigidbody>().useGravity = true;
			this.GetComponent<Collider> ().isTrigger = false;

			//this.GetComponent<Rigidbody> ().isKinematic = false;
			//New
			this.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0);
			//New
			dataManager.setLeftHandGrab (false);

			// New
			/*	
			restoreColliderTimer = Time.time;
			int num_speed = speedList.Count;
			Vector3 average = new Vector3 (0, 0, 0);
			for (int i = 0; i < sizeOfSpeedQ; i++) {
				average += speedList.Dequeue ();
				speedList.Enqueue (new Vector3 (0, 0, 0));
			}
			this.GetComponent<Rigidbody> ().velocity = 0.9f * (average / num_speed);
			restoreColliderTimer = Time.time;
			*/

		} else if(!grabbed) {
			float diff = Time.time - restoreColliderTimer;


			//Wait a certain interval to re-enable the collider of the hand
			if (diff > colliderReenableTime) {
				Debug.Log("Re-enabled");
			}
		}

		//Hover feature
		float dist_obj_palm_l = Vector3.Distance(this.transform.position, palm_l.transform.position);
		float threshold = 0.25f;

		if (c.bounds.Intersects (thumb_l.transform.GetChild(0).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (thumb_l.transform.GetChild(1).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (thumb_l.transform.GetChild(2).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (indexfinger_l.transform.GetChild(0).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (indexfinger_l.transform.GetChild(1).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (indexfinger_l.transform.GetChild(2).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (middlefinger_l.transform.GetChild(0).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (middlefinger_l.transform.GetChild(1).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (middlefinger_l.transform.GetChild(2).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (ringfinger_l.transform.GetChild(0).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (ringfinger_l.transform.GetChild(1).GetComponent<Collider> ().bounds) ||
			c.bounds.Intersects (ringfinger_l.transform.GetChild(2).GetComponent<Collider> ().bounds)) {
			secondaryMaterial.mainTexture = primaryMaterial.mainTexture;
			this.GetComponent<Renderer> ().material = secondaryMaterial;
		} else {
			this.GetComponent<Renderer> ().material = primaryMaterial;
		}

		//drawLineToHand ();

	}

	private void disableCollider(GameObject finger){
		for (int i = 0; i < 2; i++) {
			finger.transform.GetChild (i).GetComponent<Collider> ().isTrigger = true;
			Debug.Log (finger.transform.GetChild (i).name);
		}
		return;
	}

	private void enableCollider(GameObject finger){
		for (int i=0; i<2; i++)
			finger.transform.GetChild (i).GetComponent<Collider>().isTrigger = false;
		return;
	}

	private void OnDrawGizmosSelected () {
		if(hand_l != null) {
			// Draws a blue line from this transform to the target
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (transform.position, hand_l.transform.position);
		}
	}
}