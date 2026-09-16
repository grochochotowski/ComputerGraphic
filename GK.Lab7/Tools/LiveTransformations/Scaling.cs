using System.Windows;

namespace GK.Lab7.Tools.LiveTransformations
{
    // Live scaling transformation tool
    public class Scaling : LiveTransformation
    {
        public override void MouseMoveTo(Point point)
        {
            if (!isDragged || startVertices == null) return; // Check if dragging is in progress

            // Get main window and transformation center
            var win = MainWindow.Instance;

            // Get center point for scaling
            var center = win.TransformationPoint;

            // Calculate scaling factors
            Vector centerToStart = center.Subtract(mouseStart);
            Vector centerToPoint = center.Subtract(point);
            var pol = win.SelectedPolygon;

            // Apply transformation
            win.Cover();
            pol.RestoreVerticesFrom(startVertices);
            pol.Scale(center.X, center.Y, centerToPoint.X / centerToStart.X, centerToPoint.Y / centerToStart.Y);
            win.Draw();
        }
    }
}
