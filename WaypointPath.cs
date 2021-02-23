using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
	public bool drawBezier;
	public List<Waypoint> points;

	//List of points meant to be used at runtime
	public List<Vector3> worldPoints;

	public int Count
	{
		get
		{
			if (worldPoints != null)
				return worldPoints.Count;
			return 0;
		}
	}

	public Vector3 this[int key] => worldPoints[key];

	public bool InRange(int index)
	{
		return index >= 0 && index < Count;
	}
	
	void Start()
	{
		worldPoints = new List<Vector3>();
		UpdateWorldPoints();
	}

	//If, for some reason, the entire path needs to be repositioned at runtime
	//Call this after moving it so the worldPoint list contains the correct positions
	public void UpdateWorldPoints()
	{
		worldPoints.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			worldPoints.Add(transform.position + points[i].position);
		}
	}
}

[System.Serializable]
public class Waypoint
{
	public Vector3 position;
	public List<int> adjacentPoints;
}