using System.Windows;

namespace GK.Lab7.Tools.LiveTransformations
{
    // Live translation transformation tool
    public class Translation : LiveTransformation
    {
        // Handles mouse move event for translation
        public override void MouseMoveTo(Point point)
        {
            if (!isDragged || startVertices == null) return;
            Vector startToPoint = point - mouseStart;

            // Get main window and selected polygon
            var win = MainWindow.Instance;
            var pol = win.SelectedPolygon;

            // Apply translation
            win.Cover();
            pol.RestoreVerticesFrom(startVertices);
            pol.Translate(startToPoint.X, startToPoint.Y);
            win.Draw();
        }

        // Handles left mouse button down event
        public override void LeftMouseDown(Point point)
        {
            var win = MainWindow.Instance;
            if (win.SelectedPolygonIndex == -1) return;
            if (isDragged) return;
            mouseStart = point;
            startVertices = win.SelectedPolygon.CloneVertices();
            isDragged = true;
        }
    }
}
