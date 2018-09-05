using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {

	public GameObject m_GCPAwake;
	public GameObject m_GCP;
	public GameObject m_paintButtons;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void hideGCP() {
		m_GCP.SetActive(false);
	}

	public void showGCP() {
		m_GCP.SetActive(true);
	}

	public void hidePaintButtons() {
		m_paintButtons.SetActive(false);
	}

	public void showPaintButtons() {
		m_paintButtons.SetActive(true);
	}

}
