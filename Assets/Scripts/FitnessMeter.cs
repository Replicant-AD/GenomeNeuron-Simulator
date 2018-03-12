﻿using UnityEngine;

public class FitnessMeter : MonoBehaviour
{

	[SerializeField] private GameObject waypointsRoot;
	[SerializeField] private Transform carCenterPoint;

	private Transform[] waypoints;
	private Transform prevPoint, currentPoint, nextPoint;
	private int nextPointIndex = 1;

	// AbsoluteFitness: a palyahoz viszonyitott fitness.
	[HideInInspector] public double absoluteFitness = 0;

	// RelativeFitness: a currentPointhoz viszonyitott tavolsag.
	double relativeFitness = 0;
	// SavedFitness: a mar elhagyott waypointok tavolsaganak osszege.
	double savedFitness = 0;
	// Az auto sorszama - tobb autot managel a CarGameController osztaly
	private int carIndex;

	void Start()
	{
		carIndex = CarGameManager.Instance.carIndexF++;
		CarGameManager.Instance.AllCarFitness[carIndex] = absoluteFitness;

		waypoints = new Transform[waypointsRoot.transform.childCount];

		int index = 0;
		foreach (Transform wp in waypointsRoot.transform)
		{
			waypoints[index++] = wp;
		}

		// Szamon tartjuk az autohoz legkozelebbi 3 wayPoint-ot.
		prevPoint = waypoints[waypoints.Length - 1];
		currentPoint = waypoints[0];
		nextPoint = waypoints[1];
	}

	void CheckNegativeFitness()
	{
		double prevCarDistance = Vector3.Distance(prevPoint.position, carCenterPoint.position);
		double nextCarDistance = Vector3.Distance(carCenterPoint.position, nextPoint.position);
		double centerCarDistance = Vector3.Distance(currentPoint.position, carCenterPoint.position);

		relativeFitness = centerCarDistance;
		if (prevCarDistance < nextCarDistance && relativeFitness > 0)
		{
			relativeFitness *= -1;
		}
	}

	// Eggyel elore lepteti a kornyezo pontok indexet.
	void FollowingPoints()
	{
		// Ha az index tulfutna a waypointok szaman, akkor az elejetol kezdi (kor a palya).
		nextPointIndex = (nextPointIndex + 1) > waypoints.Length - 1 ? 0 : (nextPointIndex + 1);

		prevPoint = currentPoint;
		currentPoint = nextPoint;
		nextPoint = waypoints[nextPointIndex];

		CheckNegativeFitness();
	}

	// Eggyel vissza lepteti a kornyezo pontok indexet.
	void PreviousPoints()
	{
		// Ha az index tul alacsony lenne, akkor a vegerol kezdi (kor a palya).
		int prevIndex = (nextPointIndex - 3) < 0 ? (((nextPointIndex - 3) + waypoints.Length) % waypoints.Length) : (nextPointIndex - 3);
		nextPointIndex = (nextPointIndex - 1) < 0 ? (((nextPointIndex - 1) + waypoints.Length) % waypoints.Length) : (nextPointIndex - 1);

		nextPoint = currentPoint;
		currentPoint = prevPoint;
		prevPoint = waypoints[prevIndex];

		CheckNegativeFitness();
	}

	void Update()
	{
		CalculateFitness();
	}

	// Kiszamolja az auto fitness-et.
	private void CalculateFitness()
	{
		double centerCarDistance = Vector3.Distance(currentPoint.position, carCenterPoint.position);
		double prevCenterDistance = Vector3.Distance(prevPoint.position, currentPoint.position);
		double centerNextDistance = Vector3.Distance(currentPoint.position, nextPoint.position);
		double prevCarDistance = Vector3.Distance(prevPoint.position, carCenterPoint.position);
		double nextCarDistance = Vector3.Distance(carCenterPoint.position, nextPoint.position);

		relativeFitness = centerCarDistance;

		// Ha a prevPointhoz van kozelebb az auto, akkor visszafele halad.
		if (prevCarDistance < nextCarDistance && relativeFitness > 0)
		{
			relativeFitness *= -1;
		}

		// Ha a currentPoint es a nextPoint tavolsa kisebb, mint a
		// currentPoint es a carCenterPoint tavolsaga,
		// es a nextPointhoz van kozelebb az auto, akkor az atment a nextPointon.
		if (centerCarDistance > centerNextDistance && nextCarDistance < prevCarDistance)
		{
			savedFitness += centerNextDistance;
			FollowingPoints();
		}

		// Ha a currentPoint es a prevPoint tavolsa kisebb, mint a
		// currentPoint es a carCenterPoint tavolsaga,
		// es a prevPointhoz van kozelebb az auto, akkor az atment a prevPointon.
		if (centerCarDistance > prevCenterDistance && prevCarDistance < nextCarDistance)
		{
			savedFitness -= prevCenterDistance;
			PreviousPoints();
		}

		// Az autonak a palyahoz viszonyitott elorehaladasa.
		absoluteFitness = savedFitness + relativeFitness;

		// Az auto fitness erteket atadja a CarGameManager-nek
		CarGameManager.Instance.AllCarFitness[carIndex] = absoluteFitness;
	}

}
