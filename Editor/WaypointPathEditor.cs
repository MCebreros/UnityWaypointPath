using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(WaypointPath))]
public class WaypointPathEditor : Editor
{
	private Vector3 cubeSize = new Vector3(0.25f, 0.25f, 0.25f);
	
	private VisualElement rootElement;
	private VisualElement box;
	private ScrollView actionContainer;

	private List<WaypointElement> pointElements;
	
	private WaypointElement selectedVisualPoint;
	
	private Color linesColor = new Color(1f,0.75f,1f,1f);
	private ToolbarToggle bezierToggle;

	private WaypointPath t;

	void OnEnable()
	{
		rootElement = new VisualElement();
		var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
			"Assets/Editor/WaypointPathStyles.uss");
		rootElement.styleSheets.Add(styleSheet);
		pointElements = new List<WaypointElement>();

		WaypointElement.WaypointClickedEvent += OnWaypointClicked;
		WaypointElement.FieldUpdatedEvent += OnFieldUpdated;
	}

	void OnDisable()
	{
		WaypointElement.WaypointClickedEvent -= OnWaypointClicked;
		WaypointElement.FieldUpdatedEvent -= OnFieldUpdated;
	}

	public override VisualElement CreateInspectorGUI()
	{
		t = target as WaypointPath;
		
		var root = rootElement;
		root.Clear();

		box = new Box();
		box.style.minHeight = 100;
		box.RegisterCallback<MouseDownEvent>(OnBoxClicked);
		
		var toolbar = new Toolbar();
		
		var addButton = new ToolbarButton(() => {AddWaypoint(target as WaypointPath);})
		{
			text = "Add Point"
		};
		
		bezierToggle = new ToolbarToggle(){text = "Draw Bezier"};
		bezierToggle.value = t.drawBezier;
		bezierToggle.RegisterCallback<ChangeEvent<bool>>(OnToggleValueChange);
		
		var spacer = new ToolbarSpacer();
		spacer.style.flexGrow = 1;
		
		var clearButton = new ToolbarButton(ClearWaypoints)
		{
			text = "Clear Points"
		};
		
		toolbar.Add(addButton);
		toolbar.Add(bezierToggle);
		toolbar.Add(spacer);
		toolbar.Add(clearButton);
		
		actionContainer = new ScrollView();
		actionContainer.showHorizontal = false;
		box.Add(toolbar);
		box.Add(actionContainer);
		
		//TODO: Add previously added tasks to the box
		
		root.Add(box);
		
		var button = new Button(() => {t.UpdateWorldPoints();});
		button.text = "Update World Points";
		root.Add(button);

		if (t.points != null)
		{
			for (int i = 0; i < t.points.Count; i++)
				CreateWaypointElement(i.ToString(),t.points[i]);
		}

		return root;
	}

	private void OnToggleValueChange(ChangeEvent<bool> e)
	{
		t.drawBezier = e.newValue;
		serializedObject.Update();
		MarkSceneAsDirty();
	}
	
	private void AddWaypoint(WaypointPath waypointPath)
	{
		if(waypointPath.points == null)
			waypointPath.points = new List<Waypoint>();
		
		var newWaypoint = new Waypoint();
		newWaypoint.position = Vector3.left;
		CreateWaypointElement(waypointPath.points.Count.ToString(),newWaypoint);
		
		waypointPath.points.Add(newWaypoint);
		serializedObject.Update();
		MarkSceneAsDirty();
	}

	private void DeleteWaypoint(KeyDownEvent e, WaypointElement pointVisual)
	{
		e.StopPropagation();
		if (e.keyCode == KeyCode.Delete && pointVisual != null)
		{
			if (!pointElements.Contains(pointVisual))
				return;

			//Remove it from the view
			box.Remove(pointVisual);
			int index = pointElements.IndexOf(pointVisual);
			
			//If the point to be deleted is the currently selected point then deselect it
			if (selectedVisualPoint == pointVisual)
			{
				selectedVisualPoint = null;
			}

			//Remove it from the data structures
			pointElements.Remove(pointVisual);
			t.points.RemoveAt(index);
			serializedObject.Update();
			MarkSceneAsDirty();
			
			//Rename points
			for (int i = 0; i < pointElements.Count; i++)
			{
				pointElements[i].Rename(i.ToString());
			}
		}
	}

	private void CreateWaypointElement(string _name,Waypoint waypoint)
	{
		var pointVisual = new WaypointElement();
		pointVisual.Initialize(_name,waypoint);
		pointVisual.RegisterCallback<KeyDownEvent,WaypointElement>(DeleteWaypoint,pointVisual);
		box.Add(pointVisual);
		pointElements.Add(pointVisual);
	}

	private void ClearWaypoints()
	{
		t.points.Clear();
		serializedObject.Update();
		foreach (var p in pointElements)
			box.Remove(p);
		pointElements.Clear();
		selectedVisualPoint = null;
	}

	private void OnWaypointClicked(WaypointElement waypoint)
	{
		selectedVisualPoint = waypoint;
	}

	private void OnFieldUpdated()
	{
		serializedObject.Update();
		MarkSceneAsDirty();
	}

	private void OnBoxClicked(MouseDownEvent e)
	{
		e.StopPropagation();
		selectedVisualPoint = null;
	}

	void OnSceneGUI()
	{
		if (t == null || t.gameObject == null || t.points == null)
			return;

		
		for (int i = 0; i < t.points.Count; i++)
		{
			var p1 = t.points[i].position + t.transform.position;
			Handles.color = Color.magenta;
			Handles.DrawWireCube(p1,cubeSize);

			if (i < t.points.Count - 1)
			{
				var p2 = t.points[i + 1].position + t.transform.position;
				Handles.color = linesColor;
				Handles.DrawLine(p1,p2);
			}
		}

		if (selectedVisualPoint != null)
		{
			var waypoint = selectedVisualPoint.data;
			
			waypoint.position = Handles.PositionHandle(
				                      t.transform.position+waypoint.position,
				                      Quaternion.identity) - t.transform.position;
			selectedVisualPoint.UpdateField(waypoint.position);
		}

		if (bezierToggle != null && bezierToggle.value)
		{
			Handles.color = Color.cyan;
			int pointCount = t.points.Count;
			for (int i = 0; i < pointCount; i += 2)
			{
				if (pointCount - (i+2) <= 0)
					break;

				var start = t.points[i].position + t.transform.position;
				var control = t.points[i + 1].position + t.transform.position;
				var end = t.points[i + 2].position + t.transform.position;
				for (int j = 0; j < 16; j++)
				{
					
					Vector3 from = Utils.Bezier2D(start,control,end,j/16f);
					from.z = t.transform.position.z;
					Vector3 to = Utils.Bezier2D(start,control,end, (j + 1) / 16f);
					to.z = t.transform.position.z;
					Handles.DrawLine(from,to);
				}
			}
		}
	}

	private void MarkSceneAsDirty()
	{
		if(!EditorApplication.isPlaying)
			EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
		PrefabUtility.RecordPrefabInstancePropertyModifications(t);
	}
}