using System;
using System.Windows;

namespace GK.Lab7.Tools.LiveTransformations
{
    // Live rotation transformation tool
    public class Rotation : LiveTransformation
    {
        // Handles mouse move event for rotation
        public override void MouseMoveTo(Point point)
        {
            if (!isDragged || startVertices == null) return; // Check if dragging is in progress

            // Get main window instance and transformation center
            var win = MainWindow.Instance;

            // Center of transformation
            var center = win.TransformationPoint;

            // Vectors from center to start mouse position and current point
            Vector centerToStart = center.Subtract(mouseStart);
            Vector centerToPoint = center.Subtract(point);

            // Calculate angles
            double angCenPt = Math.Atan2(centerToPoint.Y, centerToPoint.X);
            double angCenSt = Math.Atan2(centerToStart.Y, centerToStart.X);
            var pol = win.SelectedPolygon;

            // Apply rotation
            win.Cover();
            pol.RestoreVerticesFrom(startVertices);
            pol.Rotate(center.X, center.Y, angCenPt - angCenSt);
            win.Draw();
        }
    }
}
