using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//new
using System;
using System.IO;
using OpenCVForUnity;

/* detect basic 3 gestures */

public class GestureControl : MonoBehaviour {
	//Left hand finger declare
	private GameObject palm;

	//poseDetectorMLtrainning
	LogisticRegression logis_reg_model;
	SVM svm_model;
	int gesture_num = 5;
	int mat_n = 30;

	//poseDetector buffer
	int[] gesture_buff;
	int gesture_buff_len = 10;
	int gesture_buff_idx = 0; 

	GameObject dataMgr;

	//Gesture dictionary
	Dictionary<int, string> gesture_dict = new Dictionary<int, string>();

	// Use this for initialization
	IEnumerator Start () {
		dataMgr = GameObject.Find ("gDataManager");
		gesture_buff_len = dataMgr.GetComponent<DataManager> ().gestBuffer;


		palm = this.transform.GetChild (5).gameObject;
		gesture_buff = new int[gesture_buff_len];

		//Gesture dicitonary establishes
		gesture_dict.Add(0, "palm");
		gesture_dict.Add(1, "pinch");
		gesture_dict.Add(2, "paint");
		gesture_dict.Add(3, "fist");
		gesture_dict.Add(4, "undefined");

		// Read svm
		using(WWW svm_reader = new WWW (System.IO.Path.Combine(Application.streamingAssetsPath, "svm.xml"))) {
			yield return svm_reader;
			System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "svm.xml"), svm_reader.text);

			Debug.Log("Reading SVM");
			Debug.Log (svm_reader.text);
			Debug.Log (File.ReadAllLines (System.IO.Path.Combine (Application.persistentDataPath, "svm.xml")).Length);
			Debug.Log (File.ReadAllText (System.IO.Path.Combine (Application.persistentDataPath, "svm.xml")));

			svm_model = OpenCVForUnity.SVM.load(System.IO.Path.Combine(Application.persistentDataPath, "svm.xml"));

			Debug.Log(svm_model.empty());
		}

		// Write data files
		/*
		string data_path = "jar:file://" + Application.dataPath + "!/assets/Data/";

		for (int i = 0; i < gesture_num; i++) {
			string fname = "handDataG" + i.ToString () + "_new.txt";
			string data_fname = data_path + fname;

			WWW file_reader = new WWW (data_fname);
			yield return file_reader;

			System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, fname), file_reader.text);
		}

		// Gesture train
		gestureDetectorMLtrain();
		*/
	}
	
	// Update is called once per frame
	void Update () {
		//Update gesture buffer array
		if (svm_model != null) {
			gesture_buff [gesture_buff_idx++] = gestureDetectorMLpredict ();
			gesture_buff_idx = (gesture_buff_idx) % gesture_buff_len;
		}
	}


	/* 	
	//Method aborted
	private bool checkPosePointing(){
		Vector3 palm_plane_norm, palm_plane_up, palm_plane_right, vec_bone1, vec_bone2, proj_bone1, proj_bone2;
		palm_plane_norm = palm.transform.forward;
		palm_plane_up = palm.transform.up;
		palm_plane_right = palm.transform.right;

		vec_bone1 = indexfinger_bone1.transform.position - palm.transform.position;
		vec_bone2 = indexfinger_bone2.transform.position - palm.transform.position;
		proj_bone1.y = Vector3.ProjectOnPlane(vec_bone1, palm_plane_norm).magnitude;
		proj_bone2.x = Vector3.ProjectOnPlane(vec_bone2, palm_plane_right).magnitude;
		proj_bone2.y = Vector3.ProjectOnPlane(vec_bone2, palm_plane_norm).magnitude;
		proj_bone2.z = Vector3.ProjectOnPlane(vec_bone2, palm_plane_up).magnitude;

		//Debug.Log ("x: " + proj_bone2.x.ToString("F3") + "  y: " + proj_bone2.y.ToString("F3") + "  z: " + proj_bone2.z.ToString("F3"));
		if ((proj_bone2.y/proj_bone1.y)>1.027) {
			return true;
		}

		return false;
	}
	*/

	/* 	gestureDetectorMLtrain
	*	Input: GameObject obj
	*	Output: None
	*	Summary: 1. Process text file to OpenCV mat 2.train the model and store model in disk
	*/
	private bool gestureDetectorMLtrain(){
		Debug.Log ("Training");

		//Data process
		Mat all_data = new Mat();
		Mat all_label = new Mat ();
		for (int i = 0; i < gesture_num; i++) {
			string fname = "handDataG" + i.ToString () + "_new.txt";
			string[] file_lines = File.ReadAllLines (System.IO.Path.Combine(Application.persistentDataPath, fname));

			int mat_m = file_lines.Length;
			Mat cur_data = Mat.zeros(mat_m,mat_n,CvType.CV_32F);
			Mat cur_label = Mat.ones(mat_m, 1, CvType.CV_32S);

			/* assign labels for cur_label */
			Core.multiply(cur_label,new Scalar((double)(i)),cur_label);
			int row = 0;

			foreach(string cur_line in file_lines) {
				Debug.Log (cur_line);
				float[] cur_line_data = Array.ConvertAll (cur_line.Split (','), float.Parse);
				cur_data.put(row,0,cur_line_data);
				row++;
			}
				
			all_data.push_back (cur_data);
			all_label.push_back (cur_label);
		}
		TrainData data_lable = OpenCVForUnity.TrainData.create (all_data, OpenCVForUnity.Ml.ROW_SAMPLE,all_label);

		//Debug usage
		//Debug.Log("Debug print(GestureControl.poseDetectorMLtrainning()):----------");

		//logistic reg model starts
		/*
		logis_reg_model = OpenCVForUnity.LogisticRegression.create ();
		logis_reg_model.setLearningRate (0.0001);
		logis_reg_model.setRegularization(OpenCVForUnity.LogisticRegression.REG_L2);
		logis_reg_model.setIterations (100);
		logis_reg_model.setTrainMethod (OpenCVForUnity.LogisticRegression.BATCH);
		logis_reg_model.setMiniBatchSize (8);
		Debug.Log("Train success: " + logis_reg_model.train(all_data, OpenCVForUnity.Ml.ROW_SAMPLE,all_label));
		//Debug.Log("Train error: " + logis_reg_model.calcError(data_lable,true,all_label));
		
		//test
		Mat result = Mat.zeros(1,1,CvType.CV_32S);
		int idx = 1500;
		logis_reg_model.predict (all_data.row (idx), result, 0);
		Debug.Log ("Data is: " + all_data.get(idx,0)[0]);
		Debug.Log ("Theta is: " + logis_reg_model.get_learnt_thetas().get(0,0)[0]);
		Debug.Log ("Theta is: " + logis_reg_model.get_learnt_thetas().get(0,1)[0]);

		Debug.Log ("Label should be: " + all_label.get(idx,0)[0]);
		Debug.Log ("Predicted label is: " + result.get (0, 0) [0]);
		*/

		//logistic reg model ends

		//svm model starts
		svm_model = OpenCVForUnity.SVM.create();
		svm_model.setKernel (SVM.INTER);
		svm_model.setC (20);
		bool ret_val = svm_model.train (data_lable);
		Debug.Log ("SVM train success : " + ret_val);

		//Debug usage
		/*
		Mat res = Mat.ones(1,1,CvType.CV_32S);
		int idx = 2000;
		svm_model.predict (all_data.row (idx), res, 0);
		Debug.Log ("Label should be: " + all_label.get(idx,0)[0]);
		Debug.Log("SVM predicted: " + res.get (0, 0) [0]);
		*/

		//Trainning error printed out
		//Debug.Log("SVM trainning error: " + svm_model.calcError (data_lable,true,all_label));

		//svm model ends
		return ret_val;
	}

	/* 	gestureDetectorMLpredict
	*	Input: None
	*	Output: None
	*	Summary: 1. Collect current hand data 2. Generate an instant prediction for current gesture
	*/
	private int gestureDetectorMLpredict(){
		//initialize/declare necessary normals and vector array
		Vector3[] vec_bone2 = new Vector3[5];
		Vector3[] vec_bone1 = new Vector3[5];
		float[] cur_data_array = new float[30];
		Mat cur_data_mat = Mat.zeros(1,mat_n,CvType.CV_32F);

		Vector3 palm_plane_norm = palm.transform.forward;
		Vector3 palm_plane_up = palm.transform.up;
		Vector3 palm_plane_right = palm.transform.right;

		//collect current hand data
		for (int i = 0; i < 5; i++) {
			Vector3 vec_palm_bone2 = this.transform.GetChild (i).GetChild (2).position - palm.transform.position;
			Vector3 vec_palm_bone1 = this.transform.GetChild (i).GetChild (1).position - palm.transform.position;
			vec_bone2 [i].x = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_right).magnitude;
			vec_bone2 [i].y = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_norm).magnitude;
			vec_bone2 [i].z = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_up).magnitude;
			vec_bone1 [i].x = Vector3.ProjectOnPlane (vec_palm_bone1, palm_plane_right).magnitude;
			vec_bone1 [i].y = Vector3.ProjectOnPlane (vec_palm_bone1, palm_plane_norm).magnitude;
			vec_bone1 [i].z = Vector3.ProjectOnPlane (vec_palm_bone1, palm_plane_up).magnitude;
			cur_data_array [i * 6] = vec_bone2 [i].x;
			cur_data_array [i * 6 + 1] = vec_bone2 [i].y;
			cur_data_array [i * 6 + 2] = vec_bone2 [i].z;
			cur_data_array [i * 6 + 3] = vec_bone1 [i].x;
			cur_data_array [i * 6 + 4] = vec_bone1 [i].y;
			cur_data_array [i * 6 + 5] = vec_bone1 [i].z;
		}
		cur_data_mat.put (0, 0, cur_data_array);

		//predict
		Mat result = Mat.ones(1,1,CvType.CV_32S);

		svm_model.predict (cur_data_mat, result, 0);

		//Debug usage
		string cur_gesture = gesture_dict[((int)result.get (0, 0) [0])];
		//Debug.Log ("Prediction gesture is " + cur_gesture);
		//Debug.Log ("Predicted label is: " + cur_gesture);
		//Debug.Log (result.get (0, 0) [0]);

		return ((int)result.get (0, 0) [0]);
	}

	/* 	bufferedGesture
	*	Input: None
	*	Output: None
	*	Summary: Output mode gesture in last detector_buff_len frames to reduce noise
	*/
	public string bufferedGesture(){
		int[] gesture_hist = new int[gesture_dict.Count];
		for (int i = 0; i < gesture_buff_len; i++) {
			gesture_hist [gesture_buff [i]] += 1;
		}

		int modeGesture = 0;
		for (int i = 0; i < gesture_hist.Length; i++){
			if (gesture_hist[i] >= gesture_hist[modeGesture])
				modeGesture = i;
		}
		return gesture_dict [modeGesture];
	}


	/* 	handDataGenerator
	*	Input: None
	*	Output: None
	*	Summary: Output current gesture data into a .txt file
	*/
	private void handDataGenerator(){
		Vector3[] vec_bone2 = new Vector3[5];
		Vector3 palm_plane_norm, palm_plane_up, palm_plane_right;
		palm_plane_norm = palm.transform.forward;
		palm_plane_up = palm.transform.up;
		palm_plane_right = palm.transform.right;

		string temp = "";
		for (int i = 0; i < 5; i++) {
			Vector3 vec_palm_bone2 = this.transform.GetChild (i).GetChild (2).position - palm.transform.position;
			vec_bone2 [i].x = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_right).magnitude;
			vec_bone2 [i].y = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_norm).magnitude;
			vec_bone2 [i].z = Vector3.ProjectOnPlane (vec_palm_bone2, palm_plane_up).magnitude;
			temp += vec_bone2 [i].x.ToString ("F10") + "," + vec_bone2 [i].y.ToString ("F10") + "," + vec_bone2 [i].z.ToString ("F10") ;
			if (i < 4)
				temp += ",";
			else
				temp += "\n";
		}
		//System.IO.File.AppendAllText(System.IO.Path.Combine(Application.persistentDataPath, "handDataG_new.txt"), temp);
	}
}
