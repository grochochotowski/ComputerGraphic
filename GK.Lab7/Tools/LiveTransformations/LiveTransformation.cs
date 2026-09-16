using System.Windows;

namespace GK.Lab7.Tools.LiveTransformations
{
    // Abstract base class for live transformation tools
    public abstract class LiveTransformation : Tool
    {
        protected Point[]? startVertices;
        protected Point mouseStart;

        // Handles left mouse button down event
        public override void LeftMouseDown(Point point)
        {
            var win = MainWindow.Instance;
            if (win.SelectedPolygonIndex == -1) return;
            mouseStart = point;
            startVertices = win.SelectedPolygon.CloneVertices();
            isDragged = true;
        }
    }
}
