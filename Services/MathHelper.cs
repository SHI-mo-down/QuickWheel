using System.Windows;

namespace QuickWheel.Services
{
    public static class MathHelper
    {
        public const double DegreesToRadians = Math.PI / 180.0;
        public const double RadiansToDegrees = 180.0 / Math.PI;

        public static Point PolarToCartesian(double centerX, double centerY, double radius, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * DegreesToRadians;
            return new Point(
                centerX + radius * Math.Cos(angleInRadians),
                centerY + radius * Math.Sin(angleInRadians));
        }

        public static double CartesianToAngle(double x, double y, double centerX, double centerY)
        {
            double dx = x - centerX;
            double dy = y - centerY;
            double angle = Math.Atan2(dy, dx) * RadiansToDegrees;
            if (angle < 0) angle += 360;
            return angle;
        }

        public static int GetSectorIndex(double x, double y, double centerX, double centerY, int totalSectors, double deadZoneRadius)
        {
            double dx = x - centerX;
            double dy = y - centerY;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < deadZoneRadius)
                return -1;

            double angle = CartesianToAngle(x, y, centerX, centerY);
            double sectorAngle = 360.0 / totalSectors;
            int index = (int)(angle / sectorAngle);
            index = index % totalSectors;
            return index;
        }

        public static (Point startPoint, Point endPoint) GetSectorPoints(
            double centerX, double centerY, 
            double innerRadius, double outerRadius, 
            double startAngle, double sweepAngle)
        {
            double startRad = startAngle * DegreesToRadians;
            double endRad = (startAngle + sweepAngle) * DegreesToRadians;

            var innerStart = new Point(
                centerX + innerRadius * Math.Cos(startRad),
                centerY + innerRadius * Math.Sin(startRad));

            var outerStart = new Point(
                centerX + outerRadius * Math.Cos(startRad),
                centerY + outerRadius * Math.Sin(startRad));

            var innerEnd = new Point(
                centerX + innerRadius * Math.Cos(endRad),
                centerY + innerRadius * Math.Sin(endRad));

            var outerEnd = new Point(
                centerX + outerRadius * Math.Cos(endRad),
                centerY + outerRadius * Math.Sin(endRad));

            return (innerStart, innerEnd);
        }

        public static string CreateSectorPathData(
            double centerX, double centerY,
            double innerRadius, double outerRadius,
            double startAngle, double sweepAngle)
        {
            bool isLargeArc = sweepAngle > 180;

            double startRad = (startAngle - 90) * DegreesToRadians;
            double endRad = (startAngle + sweepAngle - 90) * DegreesToRadians;

            double x1 = centerX + innerRadius * Math.Cos(startRad);
            double y1 = centerY + innerRadius * Math.Sin(startRad);
            double x2 = centerX + outerRadius * Math.Cos(startRad);
            double y2 = centerY + outerRadius * Math.Sin(startRad);
            double x3 = centerX + outerRadius * Math.Cos(endRad);
            double y3 = centerY + outerRadius * Math.Sin(endRad);
            double x4 = centerX + innerRadius * Math.Cos(endRad);
            double y4 = centerY + innerRadius * Math.Sin(endRad);

            string sweepFlag = sweepAngle > 180 ? "1" : "0";

            return $"M {x1:F1} {y1:F1} L {x2:F1} {y2:F1} A {outerRadius:F1} {outerRadius:F1} 0 {sweepFlag} 1 {x3:F1} {y3:F1} L {x4:F1} {y4:F1} A {innerRadius:F1} {innerRadius:F1} 0 {sweepFlag} 0 {x1:F1} {y1:F1} Z";
        }

        public static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }

        public static bool IsPointInCircle(double px, double py, double cx, double cy, double radius)
        {
            double dx = px - cx;
            double dy = py - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        public static bool IsPointInAnnulus(double px, double py, double cx, double cy, double innerRadius, double outerRadius)
        {
            double dx = px - cx;
            double dy = py - cy;
            double distanceSquared = dx * dx + dy * dy;
            return distanceSquared >= innerRadius * innerRadius && distanceSquared <= outerRadius * outerRadius;
        }
    }
}
