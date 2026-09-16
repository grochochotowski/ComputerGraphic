using System.Windows;

namespace GK.Lab7.Geometry
{
    // 3x3 homogeneous transformation matrix for 2D transformations
    public class Matrix
    {
        public static Matrix Identity => new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

        private double m11, m12, m21, m22, offsetX, offsetY;

        public Matrix(double m11, double m12, double m21, double m22,
            double offsetX, double offsetY)
        {
            this.m11 = m11;
            this.m12 = m12;
            this.m21 = m21;
            this.m22 = m22;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }

        private Matrix Clone() => new Matrix(m11, m12, m21, m22, offsetX, offsetY);

        // Prepends matrix (multiplies on the left): mat * this
        public void Prepend(Matrix mat)
        {
            var cln = Clone();
            m11 = mat.m11 * cln.m11 + mat.m12 * cln.m21;
            m12 = mat.m11 * cln.m12 + mat.m12 * cln.m22;
            m21 = mat.m21 * cln.m11 + mat.m22 * cln.m21;
            m22 = mat.m21 * cln.m12 + mat.m22 * cln.m22;
            offsetX = mat.offsetX * cln.m11 + mat.offsetY * cln.m21 + cln.offsetX;
            offsetY = mat.offsetX * cln.m12 + mat.offsetY * cln.m22 + cln.offsetY;
        }

        // Transforms a point using the matrix
        public Point Transform(Point pt)
        {
            double retX = pt.X * m11 + pt.Y * m21 + offsetX;
            double retY = pt.X * m12 + pt.Y * m22 + offsetY;
            return new Point(retX, retY);
        }
    }
}
