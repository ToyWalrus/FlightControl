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

		public override void _Draw()
		{
			if (Engine.IsEditorHint())
			{
				var absolutePosition = WorldRect.Position - GlobalPosition;
				DrawRect(new Rect2(absolutePosition, WorldRect.Size), Colors.Red, false);
			}
		}
	}
}
