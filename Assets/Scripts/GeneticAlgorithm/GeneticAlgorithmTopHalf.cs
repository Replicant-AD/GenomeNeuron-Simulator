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

using UnityEngine;
using System.Text;

public class GeneticAlgorithmTopHalf : GeneticAlgorithm
{
    private int[] m_TopHalfId;

    protected override void Selection()
    {
        m_TopHalfId = new int[PopulationSize / 2];

        for (int i = 0; i < m_TopHalfId.Length; i++)
        {
            m_TopHalfId[i] = FitnessRecords[i].Id;
        }

        for (int i = 0; i < PopulationSize; i++)
        {
            // Randomly selects one from the top 50%, it will be the left parent
            int random = RandomHelper.NextInt(0, m_TopHalfId.Length - 1);
            CarPairs[i][0] = m_TopHalfId[random];

            do
            {
                // Randomly selects one from the top 50%, it will be the right parent
                // If it is the same as the left parent, randomly selects a new one.
                random = RandomHelper.NextInt(0, m_TopHalfId.Length - 1);
                CarPairs[i][1] = m_TopHalfId[random];
            } while (CarPairs[i][0] == CarPairs[i][1]);
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
}
