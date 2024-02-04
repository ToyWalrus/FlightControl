using System.Collections.Generic;
using FlightControl.Systems;
using Godot;

namespace FlightControl
{
	[Tool]
	public partial class CameraController : Camera2D
	{
		[Export]
		public float ZoomSpeed = 0.05f;
		[Export]
		public float ZoomMin = 0.8f;
		[Export]
		public float ZoomMax = 1.2f;

		[Export]
		public Rect2 WorldRect
		{
			get => _worldRect;
			set { _worldRect = value; QueueRedraw(); }
		}
		private Rect2 _worldRect = new(Vector2.Zero, new Vector2(2000, 2000));

		[Export]
		public Vector2 LatLonOffset
		{
			get => _latLonOffset;
			set { _latLonOffset = value; QueueRedraw(); }
		}
		private Vector2 _latLonOffset = new(0, 0);

		[Export]
		public bool DisplayAirports
		{
			get => _displayAirports;
			set { _displayAirports = value; QueueRedraw(); }
		}
		private bool _displayAirports = false;

		private bool dragging = false;
		private Vector2 viewportSize = Vector2.Zero;

		public override void _Ready()
		{
			viewportSize = GetViewportRect().Size;
		}

		public override void _Input(InputEvent @event)
		{
			HandleDrag(@event);
			HandleZoom(@event);
		}

		private void HandleDrag(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
			{
				dragging = mouseButton.Pressed;

				if (dragging)
				{
					var mousePos = GetGlobalMousePosition();
					GD.Print($"Lat lon, {CoordsToLatLon(mousePos)}");
				}
			}
			else if (@event is InputEventMouseMotion mouseMotion && dragging)
			{
				// 1: get self size
				// 2: get updated position
				// 3: ensure that the camera is not dragged outside the viewport
				// 4: set the new position

				var newPos = GlobalPosition - mouseMotion.Relative / Zoom;
				GlobalPosition = ClampPosition(newPos);
			}
		}

		private void HandleZoom(InputEvent @event)
		{
			// We only want the zoom event to fire on the "pressed" action, otherwise it will fire on "released" as well
			if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.WheelUp && Zoom.X < ZoomMax)
				{
					Zoom = ClampZoom(Zoom + Vector2.One * ZoomSpeed);
					GlobalPosition = ClampPosition(GlobalPosition);
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown && Zoom.X > ZoomMin)
				{
					Zoom = ClampZoom(Zoom - Vector2.One * ZoomSpeed);
					GlobalPosition = ClampPosition(GlobalPosition);
				}
			}
		}

		private Vector2 ClampZoom(Vector2 newZoom)
		{
			var z = Mathf.Clamp(newZoom.X, ZoomMin, ZoomMax);
			return new Vector2(z, z);
		}

		private Vector2 ClampPosition(Vector2 newPos)
		{
			// Divide by 2 because the camera anchor is the center
			var size = viewportSize / 2 / Zoom;
			var limitX = WorldRect.Size.X - size.X;
			var limitY = WorldRect.Size.Y - size.Y;

			var minX = Mathf.Min(size.X, limitX);
			var minY = Mathf.Min(size.Y, limitY);
			var maxX = Mathf.Max(size.X, limitX);
			var maxY = Mathf.Max(size.Y, limitY);

			// Need to pre-organize min/max values to ensure that in case
			// zoom is too big, min/max values are still correct for clamp
			var x = Mathf.Clamp(newPos.X, minX, maxX);
			var y = Mathf.Clamp(newPos.Y, minY, maxY);
			return new Vector2(x, y);
		}

		private Vector2 LatLonToCoords(double lat, double lon)
		{
			var adjustedLatLonOffset = GetAdjustedLatLonOffset();
			var x = (float)(WorldRect.Size.X / 360.0 * (180 + lon)) + adjustedLatLonOffset.X;
			var y = (float)(WorldRect.Size.Y / 180.0 * (90 - lat)) + adjustedLatLonOffset.Y;
			return new Vector2(x, y);
		}

		private Vector2 CoordsToLatLon(Vector2 coords)
		{
			var adjustedLatLonOffset = GetAdjustedLatLonOffset();
			var lat = (float)(90 - ((coords.Y - adjustedLatLonOffset.Y) / (WorldRect.Size.Y / 180.0)));
			var lon = (float)(((coords.X - adjustedLatLonOffset.X) / (WorldRect.Size.X / 360.0)) - 180);
			return new Vector2(lat, lon);
		}

		private Vector2 GetAdjustedLatLonOffset()
		{
			var latStep = WorldRect.Size.Y / 18;
			var lonStep = WorldRect.Size.X / 36;
			return new Vector2(LatLonOffset.X * lonStep, LatLonOffset.Y * latStep);
		}

		private Vector2 AbsolutePosition => WorldRect.Position - GlobalPosition;

		public override void _Draw()
		{
			if (Engine.IsEditorHint())
			{
				DrawRect(new Rect2(AbsolutePosition, WorldRect.Size), Colors.Red, false);
				DrawLatitudeLongitudeLines();

				if (DisplayAirports)
				{
					DrawAirports();
				}
			}
		}

		private void DrawLatitudeLongitudeLines()
		{
			var absolutePosition = AbsolutePosition;
			var latStep = WorldRect.Size.Y / 18;
			var lonStep = WorldRect.Size.X / 36;
			var fontSize = 96;
			var color = Colors.Maroon;
			var adjustedLatLonOffset = GetAdjustedLatLonOffset();

			for (int i = 0; i < 18; i++)
			{
				var y = i * latStep + absolutePosition.Y + adjustedLatLonOffset.Y;
				if (y < absolutePosition.Y)
				{
					y += WorldRect.Size.Y;
				}
				else if (y > absolutePosition.Y + WorldRect.Size.Y)
				{
					y -= WorldRect.Size.Y;
				}
				DrawLine(new Vector2(absolutePosition.X, y), new Vector2(absolutePosition.X + WorldRect.Size.X, y), color);
				DrawString(ThemeDB.FallbackFont, new Vector2(absolutePosition.X + 5, y - 10), $"{90 - i * 10}", HorizontalAlignment.Left, -1, fontSize);
			}

			for (int i = 0; i < 36; i++)
			{
				var x = i * lonStep + absolutePosition.X + adjustedLatLonOffset.X;
				if (x < absolutePosition.X)
				{
					x += WorldRect.Size.X;
				}
				else if (x > absolutePosition.X + WorldRect.Size.X)
				{
					x -= WorldRect.Size.X;
				}
				DrawLine(new Vector2(x, absolutePosition.Y), new Vector2(x, absolutePosition.Y + WorldRect.Size.Y), color);
				DrawString(ThemeDB.FallbackFont, new Vector2(x + 5, absolutePosition.Y + WorldRect.Size.Y - 20), $"{i * 10 - 180}", HorizontalAlignment.Left, -1, fontSize);
			}
		}

		private void DrawAirports()
		{
			var absolutePosition = AbsolutePosition;
			var airportsDrawnInLatLon = new Dictionary<int, List<int>>();

			foreach (var airport in AirportSpawnSystem.Airports)
			{
				var latKey = Mathf.FloorToInt(airport.lat);
				var lonKey = Mathf.FloorToInt(airport.lon);
				if (airportsDrawnInLatLon.ContainsKey(latKey))
				{
					if (airportsDrawnInLatLon[latKey].Contains(lonKey))
					{
						continue;
					}
					else
					{
						airportsDrawnInLatLon[latKey].Add(lonKey);
					}
				}
				else
				{
					airportsDrawnInLatLon.Add(latKey, new List<int> { lonKey });
				}

				var coords = LatLonToCoords(airport.lat, airport.lon);
				var x = coords.X + absolutePosition.X;
				var y = coords.Y + absolutePosition.Y;
				DrawCircle(new Vector2(x, y), 5, Colors.White);
			}
		}
	}
}
