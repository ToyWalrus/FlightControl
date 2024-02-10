using Godot;

namespace FlightControl
{
	public partial class AirportContextMenu : PopupPanel
	{
		private PackedScene connectionRowScene;
		private PackedScene travelerRowScene;

		private Airport airport;

		private Label airportNameLabel;
		private VBoxContainer travelersList;
		private VBoxContainer connectionsList;

		public override void _Ready()
		{
			connectionRowScene = GD.Load<PackedScene>("res://scenes/airport_context_menu/connection_row.tscn");
			travelerRowScene = GD.Load<PackedScene>("res://scenes/airport_context_menu/traveler_destination_count.tscn");

			airportNameLabel = GetNode<Label>("ContentLayout/AirportName");
			travelersList = GetNode<VBoxContainer>("ContentLayout/TabContainer/Travelers");
			connectionsList = GetNode<VBoxContainer>("ContentLayout/TabContainer/Connections");

			PopupHide += OnHide;
		}

		public void Popup(Airport airport)
		{
			this.airport = airport;
			airportNameLabel.Text = airport.AirportName;

			SetPopupPosition();
			UpdateTravelerRows();
			UpdateConnectionRows();

			airport.OnTravellerCountChanged += UpdateTravelerRows;

			Popup();
		}

		private void OnHide()
		{
			if (airport != null)
			{
				airport.OnTravellerCountChanged -= UpdateTravelerRows;
			}
		}


		private void SetPopupPosition()
		{
			Position = (Vector2I)airport.Position;
			Position += new Vector2I(16, 0);
		}

		private void UpdateTravelerRows()
		{
			travelersList.GetNode<Label>("TotalTravelers").Text = $"Total: {airport.TravellerCount}";

			foreach (var child in travelersList.GetChildren())
			{
				if (child is Label) continue;
				child.QueueFree();
			}

			foreach (var travelerDestination in airport.GetTopFinalDestinations(5))
			{
				var travelerRow = travelerRowScene.Instantiate();
				travelerRow.GetNode<Label>("AirportName").Text = travelerDestination.Key.AirportName;
				travelerRow.GetNode<Label>("Count").Text = travelerDestination.Value.ToString();
				travelersList.AddChild(travelerRow);
			}
		}

		private void UpdateConnectionRows()
		{
			foreach (var child in connectionsList.GetChildren())
			{
				child.QueueFree();
			}

			foreach (var connection in airport.GetConnections())
			{
				var connectionRow = connectionRowScene.Instantiate();
				connectionRow.GetNode<Label>("NameContainer/AirportName").Text = connection.GetOther(airport).AirportName;

				var planeIcon = connectionRow.GetNode<TextureRect>("IconContainer/PlaneIcon");
				for (int i = 1; i < connection.PlaneCount; ++i)
				{
					planeIcon.AddSibling(planeIcon.Duplicate());
				}
			}
		}
	}
}
