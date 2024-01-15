using System.Collections.Generic;
using Godot;

namespace FlightControl
{
	public partial class AirportPropertiesPopup : PopupPanel
	{
		const string DESTINATION_COUNT_PATH = "Layout/VBoxContainer/Indented/VBoxContainer";

		private Airport airport;
		private readonly HashSet<string> destinationCountNodePaths = new();

		public void Popup(Airport airport)
		{
			SetAirport(airport);
			UpdateLabels(null);
			Popup();
		}

		public void SetAirport(Airport newAirport)
		{
			if (airport != null)
			{
				airport.OnTravellerCountChanged -= UpdateLabels;
			}

			airport = newAirport;

			if (airport != null)
			{
				airport.OnTravellerCountChanged += UpdateLabels;
				UpdateLabels(null);
			}
		}


		private void UpdateLabels(object sender)
		{
			CheckAllExistingDestinationCountNodes();

			SetLabel("Layout/Title/Airport", $"({airport.AirportName})");
			SetLabel("Layout/VBoxContainer/Travellers/Count", airport.TravellerCount.ToString());

			var destinations = airport.GetDestinations();
			foreach (var (destination, count) in destinations)
			{
				GD.Print($"Destination: {destination.AirportName}, Count: {count}");
				SetDestinationCountNode(destination.AirportName, count);
			}

			foreach (var nodeName in destinationCountNodePaths)
			{
				GD.Print($"Removing node at {DESTINATION_COUNT_PATH}/{nodeName}");
				GetNode($"{DESTINATION_COUNT_PATH}/{nodeName}").QueueFree();
			}
		}

		private void SetLabel(string path, string text, Node parent = null)
		{
			parent ??= this;
			var node = parent.GetNode<Label>(path);
			if (node != null)
			{
				node.Text = text;
			}
		}

		private void SetDestinationCountNode(string destination, int count)
		{
			var node = GetDestinationCountNode(destination);
			SetLabel("Label", destination, node);
			SetLabel("Count", count.ToString(), node);
		}

		private Node GetDestinationCountNode(string destination)
		{
			var nodeName = $"DestinationCount_{destination}";

			var path = $"{DESTINATION_COUNT_PATH}/{nodeName}";
			if (destinationCountNodePaths.Contains(path))
			{
				// Remove the node from the list so we don't remove the node from the scene
				destinationCountNodePaths.Remove(nodeName);
				return GetNode(path);
			}

			var template = GetNode($"{DESTINATION_COUNT_PATH}/DestinationCountTemplate");
			var newNode = template.Duplicate() as CanvasItem;
			newNode.Name = nodeName;
			newNode.Visible = true;
			template.GetParent().AddChild(newNode);
			return newNode;
		}

		private void CheckAllExistingDestinationCountNodes()
		{
			destinationCountNodePaths.Clear();

			var destinationCountPath = "Layout/VBoxContainer/Indented/VBoxContainer";
			var nodes = GetNode(destinationCountPath).GetChildren();

			foreach (var node in nodes)
			{
				if (node.Name == "DestinationCountTemplate") continue;
				destinationCountNodePaths.Add(node.Name);
			}
		}
	}
}
