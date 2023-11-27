using System.Collections.Generic;
using UnityEngine;

public static class PoissonDisk
{
   public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
   {
      float cellSize = radius / Mathf.Sqrt(2);

      int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize),
         Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

      List<Vector2> points = new();
      List<Vector2> spawnPoints = new() { sampleRegionSize/2 };

      while (spawnPoints.Count > 0)
      {
         int spawnIndex = Random.Range(0, spawnPoints.Count);
         Vector2 spawnCentre = spawnPoints[spawnIndex];

         bool isCandidateAccepted = false;

         for (int i = 0; i < numSamplesBeforeRejection; i++)
         {
            float angle = Random.value * Mathf.PI * 2;
            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

            Vector2 canditate = spawnCentre + dir * Random.Range(radius, 2 * radius);

            if (IsValid(canditate, sampleRegionSize, radius, cellSize, points, grid))
            {
               points.Add(canditate);
               spawnPoints.Add(canditate);
               grid[(int)(canditate.x / cellSize), (int)(canditate.y / cellSize)] = points.Count; // 1 based Index
               isCandidateAccepted = true;
               break;
            }
         }

         if (!isCandidateAccepted)
            spawnPoints.RemoveAt(spawnIndex);
      }

      return points;
   }

   private static bool IsValid(Vector2 canditate, Vector2 sampleRegionSize, float radius, float cellSize, List<Vector2> points, int[,] grid)
   {
      if (canditate.x >= 0 && canditate.x < sampleRegionSize.x && canditate.y >= 0 && canditate.y < sampleRegionSize.y)
      {
         int cellX = (int)(canditate.x / cellSize);
         int cellY = (int)(canditate.y / cellSize);

         int searchStartX = Mathf.Max(0, cellX - 2); //bunlar gridteki celler
         int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
         
         int searchStartY = Mathf.Max(0, cellY - 2);
         int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);
         
         for (int x = searchStartX; x <= searchEndX; x++)
         {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
               int pointIndex = grid[x, y] - 1; //Negative one means no point in that cell

               if (pointIndex != -1)
               {
                  float sqrDist = (canditate - points[pointIndex]).sqrMagnitude;

                  if (sqrDist < radius * radius) //candidate is too close to the point
                     return false;
               }
            }
         }

         return true;
      }

      return false;
   }
}
