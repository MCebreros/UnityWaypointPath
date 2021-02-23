using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class WaypointElement : VisualElement
{
	public Waypoint data;
	private Label label;
	private Vector3Field vectorField;
	
	public static event Action<WaypointElement> WaypointClickedEvent;
	public static event Action FieldUpdatedEvent;
	
	public void Initialize(string _name,Waypoint waypoint)
	{
		focusable = true;
		name = _name;
		data = waypoint;
		AddToClassList("point");
		RegisterCallback<MouseDownEvent>(OnMouseDown);
			
		label = new Label(name);
		label.style.minWidth = 20;
		Add(label);
		
		vectorField = new Vector3Field();
		vectorField.style.flexGrow = 1;
		vectorField.value = data.position;
		
		vectorField.RegisterCallback<ChangeEvent<Vector3>>(OnFieldChange);
		
		Add(vectorField);
	}

	private void OnFieldChange(ChangeEvent<Vector3> e)
	{
		data.position = e.newValue;
		FieldUpdatedEvent?.Invoke();
	}

	private void OnMouseDown(MouseDownEvent e)
	{
		e.StopPropagation();
		WaypointClickedEvent?.Invoke(this);
	}

	public void Rename(string _name)
	{
		name = _name;
		label.text = _name;
	}

	public void UpdateField(Vector3 value)
	{
		vectorField.value = value;
	}
}
