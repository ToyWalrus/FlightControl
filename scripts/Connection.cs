using System.Collections.Generic;
using Godot;

namespace FlightControl
{
	[Tool]
	public partial class Connection : Node2D
	{
		[Export]
		public Airport PortA
		{
			get => _portA;
			set
			{
				UnsetConnections();
				_portA = value;
				SetConnections();

				QueueRedraw();
			}
		}
		private Airport _portA;

		[Export]
		public Airport PortB
		{
			get => _portB;
			set
			{
				UnsetConnections();
				_portB = value;
				SetConnections();

				QueueRedraw();
			}
		}
		private Airport _portB;

		[Export]
		public Color HighlightColor
		{
			get => _highlightColor;
			set
			{
				_highlightColor = value;
				QueueRedraw();
			}
		}
		private Color _highlightColor = Colors.Red;

		public bool ShouldHighlight { get; set; } = false;

		private readonly HashSet<Plane> planes = new();

		public void AddPlane(Plane plane)
		{
			if (planes.Add(plane))
			{
				plane.SetConnection(this);
			}
		}

		public bool HasConnection(Airport a)
		{
			return a == PortA || a == PortB;
		}

		public bool Connects(Airport a, Airport b)
		{
			return (a == PortA && b == PortB) || (a == PortB && b == PortA);
		}

		public bool IsValid()
		{
			return PortA != null && PortB != null;
		}

		public Airport GetOther(Airport a)
		{
			if (!IsValid()) return null;
			if (a == PortA) return PortB;
			if (a == PortB) return PortA;
			return null;
		}

		public override void _Draw()
		{
			if (!IsValid()) return;
			if (Engine.IsEditorHint())
			{
				DrawLine(PortA.GlobalPosition, PortB.GlobalPosition, HighlightColor, 4);
			}
			else
			{
				DrawLine(PortA.GlobalPosition, PortB.GlobalPosition, ShouldHighlight ? HighlightColor : Colors.White, 2);
			}
		}

		private void SetConnections()
		{
			if (IsValid())
			{
				PortA.AddConnection(this);
				PortB.AddConnection(this);
			}
		}

		private void UnsetConnections()
		{
			if (IsValid())
			{
				PortA.RemoveConnection(this);
				PortB.RemoveConnection(this);
			}
		}

		public static int CalculateCost(Airport a, Airport b)
		{
			return 0;
		}
	}
}
