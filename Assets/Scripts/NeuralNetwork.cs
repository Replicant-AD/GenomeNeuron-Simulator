﻿using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{

	public NeuronLayer[] NeuronLayers { get; set; }

	private int carIndex;
	private int hiddenLayerCount;
	private int neuronCount;
	private int inputCount;
	private double[][] transferData;
	private double[] carInputs;
	private double bias;

	public CarController carController;

	// Az auto iranyitasa a tombelemek alapjan tortenik
	// control[0] = kanyarodas , control[1] = gyorsulas 
	double[] control = new double[2];


	void Start()
	{
		bias = GameManager.Instance.Bias;
		carIndex = this.gameObject.GetComponent<CarController>().carStats.index;
		hiddenLayerCount = GameManager.Instance.HiddenLayerCount;
		neuronCount = GameManager.Instance.NeuronPerLayerCount;
		inputCount = GameManager.Instance.CarsRayCount + 1;

		transferData = new double[hiddenLayerCount][];
		for (int i = 0; i < transferData.Length; i++)
		{
			transferData[i] = new double[neuronCount];
		}

		// hidden layers, +1 output layer
		NeuronLayers = new NeuronLayer[hiddenLayerCount + 1];


		// Initialize the hidden layers.
		// If zero hidden layer -> there is only the output layer
		if (hiddenLayerCount == 0)
		{
			NeuronLayers[0] = new NeuronLayer(2, inputCount, bias);
		}
		// If one hidden layer -> first layer gets the input,
		// second layer is the output layer.
		else if (hiddenLayerCount == 1)
		{
			NeuronLayers[0] = new NeuronLayer(neuronCount, inputCount, bias);
			NeuronLayers[1] = new NeuronLayer(2, neuronCount, bias);
		}
		// If two or more hidden layers -> first layer gets the input,
		// the other ones get the output from the previous layer
		// and the last layer is the output layer.
		else if (hiddenLayerCount >= 2)
		{
			NeuronLayers[0] = new NeuronLayer(neuronCount, inputCount, bias);
			for (int i = 1; i < NeuronLayers.Length - 1; i++)
			{
				NeuronLayers[i] = new NeuronLayer(neuronCount, neuronCount, bias);
			}
			NeuronLayers[NeuronLayers.Length - 1] = new NeuronLayer(2, neuronCount, bias);
		}

	}


	void FixedUpdate()
	{
		// The inputs array contains the car's sensor datas and it's current speed.
		carInputs = GameManager.Instance.Cars[carIndex].Inputs;

		// If zero hidden layer -> there is only the output layer
		if (hiddenLayerCount == 0)
		{
			control = NeuronLayers[0].CalculateLayer(carInputs);
		}
		// If one hidden layer -> first layer gets the input,
		// second layer is the output layer.
		else if (hiddenLayerCount == 1)
		{
			control = NeuronLayers[1].CalculateLayer(
				NeuronLayers[0].CalculateLayer(carInputs));
		}
		// If two or more hidden layers -> first layer gets the input,
		// the other ones get the output from the previous layer
		// and the last layer is the output layer.
		else if (hiddenLayerCount >= 2)
		{
			transferData[0] = NeuronLayers[0].CalculateLayer(carInputs);
			for (int i = 1; i < transferData.Length; i++)
			{
				transferData[i] = NeuronLayers[i].CalculateLayer(transferData[i - 1]);
			}
			control = NeuronLayers[NeuronLayers.Length - 1].CalculateLayer(transferData[transferData.Length - 1]);
		}


		carController.steer = control[0];
		carController.accelerate = control[1];

	}

}
