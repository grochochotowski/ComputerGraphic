using GK.Lab7.Geometry;
using System.Windows;

namespace GK.Lab7.Tools
{
    // Tool for selecting and moving vertices of polygons
    public class Cursor : Tool
    {
        private Vertex? dragged;

        // Handles mouse movement
        public override void MouseMoveTo(Point point)
        {
            var win = MainWindow.Instance;
            if (!isDragged)
            {
                if (win.SelectedPolygonIndex != -1)
                {
                    var pol = win.SelectedPolygon;
                    var verts = pol.Vertices;
                    for (int i = 0; i < verts.Count; ++i)
                        verts[i].IsHighlighted = verts[i].Intersects(point);
                }
                return;
            }
            dragged?.SetXY(point.X, point.Y);
        }

        // Handles left mouse button down event
        public override void LeftMouseDown(Point point)
        {
            if (isDragged) return;
            var win = MainWindow.Instance;
            var pols = win.Polygons;

            // Check if a vertex is being clicked
            for (int p = 0; p < pols.Count; ++p)
            {
                var pol = pols[p];
                var vi = pol.IntersectingVertexIndex(point);
                if (vi != -1)
                {
                    win.SelectedPolygonIndex = p;
                    isDragged = true;
                    dragged = pol.Vertices[vi];
                    return;
                }
            }

            int selPolId = win.SelectedPolygonIndex;
            if (selPolId == -1) return;
            var selPol = win.Polygons[selPolId];
            win.Cover();
            selPol.Vertices.Add(new Vertex(point.X, point.Y));
            win.Draw();
        }

        // Handles left mouse button up event
        public override void LeftMouseUp(Point point)
        {
            if (!isDragged) return;
            isDragged = false;
            dragged = null;
        }

        // Handles right mouse button down event
        public override void RightMouseDown(Point point)
        {
            if (isDragged) return;
            var win = MainWindow.Instance;
            var pols = win.Polygons;

            for (int p = 0; p < pols.Count; ++p)
            {
                var pol = pols[p];
                var vi = pol.IntersectingVertexIndex(point);
                if (vi != -1)
                {
                    win.SelectedPolygonIndex = p;
                    win.Cover();
                    pol.Vertices.RemoveAt(vi);
                    win.Draw();
                    break;
                }
            }
        }

        public override void RightMouseUp(Point point) { }
    }
}
