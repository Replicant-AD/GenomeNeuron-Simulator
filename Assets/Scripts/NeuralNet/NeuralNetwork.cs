﻿/*
Copyright (C) 2021 zoltanvi

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;

[System.Serializable]
public class NeuralNetwork : MonoBehaviour
{
    public NeuronLayer[] NeuronLayers { get; set; }

    private int m_CarId;
    private int m_HiddenLayerCount;
    private int m_NeuronCount;
    private int m_InputCount;
    private float[][] m_TransferData;
    private float[] m_CarInputs;
    private float m_Bias;

    public CarController CarController;

    // The car is controlled based on the array elements
    // control[0] = steering , control[1] = acceleration 
    private float[] m_Control = new float[2];

    private void Start()
    {
        m_Bias = Master.Instance.Manager.Bias;
        m_CarId = this.gameObject.GetComponent<CarController>().Id;
        m_HiddenLayerCount = Master.Instance.Manager.Configuration.LayersCount;
        m_NeuronCount = Master.Instance.Manager.Configuration.NeuronPerLayerCount;

        if (Master.Instance.Manager.Configuration.Navigator)
        {
            m_InputCount = Master.Instance.Manager.CarSensorCount + 4;
        }
        else
        {
            m_InputCount = Master.Instance.Manager.CarSensorCount + 1;
        }

        m_TransferData = new float[m_HiddenLayerCount][];
        for (int i = 0; i < m_TransferData.Length; i++)
        {
            m_TransferData[i] = new float[m_NeuronCount];
        }

        // Hidden layers, +1 output layer
        NeuronLayers = new NeuronLayer[m_HiddenLayerCount + 1];

        switch (m_HiddenLayerCount)
        {
            // Initialize the hidden layers.
            // If zero hidden layer -> there is only the output layer
            case 0:
                NeuronLayers[0] = new NeuronLayerTanh(2, m_InputCount, m_Bias);
                break;

            // If one hidden layer -> first layer gets the input,
            // second layer is the output layer.
            case 1:
                NeuronLayers[0] = new NeuronLayerTanh(m_NeuronCount, m_InputCount, m_Bias);
                NeuronLayers[1] = new NeuronLayerTanh(2, m_NeuronCount, m_Bias);
                break;

            // If two or more hidden layers -> first layer gets the input,
            // the other ones get the output from the previous layer
            // and the last layer is the output layer.
            default:
                NeuronLayers[0] = new NeuronLayerTanh(m_NeuronCount, m_InputCount, m_Bias);
                for (int i = 1; i < NeuronLayers.Length - 1; i++)
                {
                    NeuronLayers[i] = new NeuronLayerTanh(m_NeuronCount, m_NeuronCount, m_Bias);
                }
                NeuronLayers[NeuronLayers.Length - 1] = new NeuronLayerTanh(2, m_NeuronCount, m_Bias);
                break;
        }

        if (!Master.Instance.Manager.IsLoad) return;

        for (int i = 0; i < NeuronLayers.Length; i++)
        {
            for (int j = 0; j < NeuronLayers[i].NeuronWeights.Length; j++)
            {
                for (int k = 0; k < NeuronLayers[i].NeuronWeights[j].Length; k++)
                {
                    NeuronLayers[i].NeuronWeights[j][k] = Master.Instance.Manager.Save.SavedCarNetworks[m_CarId][i][j][k];
                }
            }
        }
    }


    private void FixedUpdate()
    {
        // The inputs array contains the car's sensor datas and it's current speed.
        m_CarInputs = Master.Instance.Manager.Cars[m_CarId].Inputs;

        switch (m_HiddenLayerCount)
        {
            // If zero hidden layer -> there is only the output layer
            case 0:
                m_Control = NeuronLayers[0].CalculateLayer(m_CarInputs);
                break;

            // If one hidden layer -> first layer gets the input,
            // second layer is the output layer.
            case 1:
                m_Control = NeuronLayers[1].CalculateLayer(
                       NeuronLayers[0].CalculateLayer(m_CarInputs));
                break;

            // If two or more hidden layers -> first layer gets the input,
            // the other ones get the output from the previous layer
            // and the last layer is the output layer.
            default:
                m_TransferData[0] = NeuronLayers[0].CalculateLayer(m_CarInputs);
                for (int i = 1; i < m_TransferData.Length; i++)
                {
                    m_TransferData[i] = NeuronLayers[i].CalculateLayer(m_TransferData[i - 1]);
                }
                m_Control = NeuronLayers[NeuronLayers.Length - 1].CalculateLayer(m_TransferData[m_TransferData.Length - 1]);
                break;
        }

        CarController.Steer = m_Control[0];
        CarController.Accelerate = m_Control[1];
    }
}

