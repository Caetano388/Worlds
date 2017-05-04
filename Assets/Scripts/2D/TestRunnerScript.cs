﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestRunnerScript : MonoBehaviour {

	public List<AutomatedTest> tests = new List<AutomatedTest> ();

	private int _prevTestIndex = -1;
	private int _testIndex = 0;

	private int _successes = 0;

	// Use this for initialization
	void Start () {

//		Manager.RecordingEnabled = true;

//		tests.Add (new SaveLoadTest (407252633, 80, 1, 2, 0, false, true));
//		tests.Add (new SaveLoadTest (407252633, 100000, 20000, 5));
		tests.Add (new SaveLoadTest ("after 5 polities", 783909167, (World world) => {
			return world.PolityCount > 5;
		}, 200000, 5));
//		tests.Add (new SaveLoadTest ("after 5 polities", 783909167, (World world) => {
//			return world.PolityCount > 5;
//		}, 20000, 5));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 2000, 20, 60000));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 200, 20, 78000));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 20, 20, 81400));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 2, 20, 81740));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 5, 10));

		#if DEBUG
		Manager.TracingData.GroupId = 64643160086;
		Manager.TracingData.PolityId = 195618245070;
		Manager.TracingData.Longitude = 245;
		Manager.TracingData.Latitude = 72;
		#endif

//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 2, 20, 81740, true));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 5, 10, 0, true));
//		tests.Add (new SaveLoadTest ("after 0 polities", 783909167, (World world) => {
//			return world.PolityCount > 0;
//		}, 1, 10, 0, true));
//		tests.Add (new SaveLoadTest ("after 5 polities", 407252633, (World world) => {
//			return world.PolityCount > 5;
//		}, 5, 20, 0, true));
//		tests.Add (new SaveLoadTest ("after 5 polities", 407252633, (World world) => {
//			return world.PolityCount > 5;
//		}, 5, 98, 79200, true));
//		tests.Add (new SaveLoadTest ("after 5 polities", 407252633, (World world) => {
//			return world.PolityCount > 5;
//		}, 5, 2000, 20000, true));
//		tests.Add (new SaveLoadTest ("after 5 polities", 407252633, (World world) => {
//			return world.PolityCount > 5;
//		}, 5, 2000, 0, true));

//		tests.Add (new LanguageGenerationTest());

		Debug.Log ("Running Tests...\n");
	}
	
	// Update is called once per frame
	void Update () {

		Manager.ExecuteTasks (100);

		if (_testIndex == tests.Count) {

			Debug.Log ("\nFinished Tests!");
			Debug.Log (_successes + " of " + tests.Count + " Succeded");
			Debug.Break ();

		} else {

			AutomatedTest test = tests [_testIndex];

			if (_prevTestIndex != _testIndex) {
				
				Debug.Log ("Executing test: " + _testIndex + " - " + test.Name);

				_prevTestIndex = _testIndex;
			}

			test.Run ();

			_successes += (test.State == TestState.Succeded) ? 1 : 0;

			if ((test.State == TestState.Succeded) || (test.State == TestState.Failed))
				_testIndex++;
		}
	}
}
