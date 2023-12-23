/*
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

using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GeneticAlgorithmWorstRandom : GeneticAlgorithm
{
	// During tournament selection this many cars "compete" with each other at once
	private int m_SelectionPressure = 3;
	private int m_Top80Percent;

	protected override void Selection()
	{
		// Stores the selected car IDs for one round
		List<int> pickedCarIdList = new List<int>();

		int paired = 0;

		// The first parent of each pair is fully random
		for (int i = 0; i < PopulationSize; i++)
		{
			CarPairs[i][0] = RandomHelper.NextInt(0, PopulationSize - 1);
		}


		// While not all pairs are ready, new tournament
		while (paired < PopulationSize)
		{
			#region Current tournament initialization
			List<int> tournament = new List<int>();
			for (int i = 0; i < PopulationSize; i++)
			{
				tournament.Add(i);
			}
			#endregion

			// As long as there are enough competitors in the tournament (and more pairs needed), select competitors
			while (tournament.Count >= m_SelectionPressure && paired < PopulationSize)
			{
				// Emptying the selected
				pickedCarIdList.Clear();

				// Until each competitor is selected
				while (pickedCarIdList.Count != m_SelectionPressure)
				{
					int current = tournament[RandomHelper.NextInt(0, tournament.Count - 1)];
					if (!pickedCarIdList.Contains(current))
					{
						pickedCarIdList.Add(current);
						tournament.Remove(current);
					}
				}

				// Pairing
				CarPairs[paired][1] = GetTournamentBestIndex(pickedCarIdList);
				paired++;
			}
		}

#if UNITY_EDITOR
        StringBuilder sb = new StringBuilder();
        foreach (var carPair in CarPairs)
        {
            sb.Append($"{carPair[0]} :: {carPair[1]} \n");
        }
        Debug.Log(sb.ToString());
#endif
    }

	private int GetTournamentBestIndex(List<int> pickedCarIdList)
	{
		int bestIndex = int.MinValue;
		double highestFitness = double.MinValue;
		foreach (var pickedCarId in pickedCarIdList)
		{
			double currentFitness = 0;
			foreach (var stat in FitnessRecords)
			{
				if (stat.Id == pickedCarId)
				{
					currentFitness = stat.Fitness;
				}
			}
			if (currentFitness > highestFitness)
			{
				highestFitness = currentFitness;
				bestIndex = pickedCarId;
			}
		}
		return bestIndex;
	}

	protected override void RecombineAndMutate()
	{
        float mutationRateMinimum = (100 - MutationRate) / 100;
		float mutationRateMaximum = (100 + MutationRate) / 100;
		m_Top80Percent = (int)(PopulationSize * 0.8f);

		for (int i = 0; i < SavedCarNetworks.Length; i++)    // which car
		{
			for (int j = 0; j < SavedCarNetworks[i].Length; j++) // which neuron layer
			{
				for (int k = 0; k < SavedCarNetworks[i][j].Length; k++) // which neuron
				{
					for (int l = 0; l < SavedCarNetworks[i][j][k].Length; l++) // which weight
					{
						if (i == FitnessRecords[0].Id)
						{
							CarNetworks[i].NeuronLayers[j].NeuronWeights[k][l] =
								SavedCarNetworks[i][j][k][l];
						}
						else if (i >= m_Top80Percent)
						{
							// 20% of cars will be randomly reassigned every round.
							CarNetworks[i].NeuronLayers[j].NeuronWeights[k][l] = RandomHelper.NextFloat(-1f, 1f);
						}
						else
						{
							var mutation = RandomHelper.NextFloat(mutationRateMinimum, mutationRateMaximum);
							// 50% chance of inheriting from one parent.
							// carPairs[i] contains indices of both parents
							var index = CarPairs[i][RandomHelper.NextInt(0, 1)];

							// The probability of mutation varies with the MutationChance value
							if (RandomHelper.NextInt(1, 100) <= MutationChance)
							{
								CarNetworks[i].NeuronLayers[j].NeuronWeights[k][l] =
								SavedCarNetworks[index][j][k][l] * mutation;
							}
							else
							{
								CarNetworks[i].NeuronLayers[j].NeuronWeights[k][l] =
									SavedCarNetworks[index][j][k][l];
							}
						}
					}
				}
			}
		}
	}
}
