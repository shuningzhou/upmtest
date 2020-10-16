using UnityEngine;

namespace ParallelUnity.DebugTools
{
    public static class DebugDraw
    {
        public static Color ColliderOutlineColor = Color.green;

        public static void DrawHemiSphere(Vector3 origin, float radius, Vector3 offset)
        {
            float radius2 = 0;
            float delta = radius;
            for (float deltaAngle = 0f; deltaAngle <= radius + 0.1f; deltaAngle += delta)
            {
                radius2 = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(deltaAngle, 2));
                DrawCircleForHS(origin + offset * deltaAngle, radius2);
            }
            float dir = Mathf.Sign(offset.y);
            //DrawCircleForHS(origin + offset * radius, radius2 / 3);

            for (int i = 0; i < 181; i = i + 90)
            {
                DrawArcForHS(origin, radius * dir, i);
            }
        }

        static float GetDeltaForLoop(float radius, bool isDotted)
        {
            float delta = 0.1f;
            if (isDotted && radius > 0.2f)
            {//
                delta = delta / radius;
            }
            return delta;
        }

        public static void DrawLineConnectingHS(Vector3 point1, Vector3 point2, float radius)
        {
            Vector3 origin = (point1 - point2) / 2 + point1;
            Vector3 direction = (point1 - point2).normalized;
            Vector3 tangentToDirection = Vector3.zero;
            Vector3.OrthoNormalize(ref direction, ref tangentToDirection);
            Vector3 tangendOffset = tangentToDirection * (radius);
            Vector3 basePositionOfLine = point1 + tangendOffset;
            Vector3 endPositionOfLine = point2 + tangendOffset;
            if (radius == 0)
            {
                Gizmos.DrawSphere(Vector3.zero, 0.05f);
            }
            float delta = GetDeltaForLoop(radius, true);
            Vector3 beginPosition = basePositionOfLine;

            for (int i = 0; i < 5; i++)
            {
                Quaternion connectingLinesAngularOffset = Quaternion.AngleAxis(90 * i, direction);
                Gizmos.DrawLine(basePositionOfLine, endPositionOfLine);
                basePositionOfLine = connectingLinesAngularOffset * tangendOffset + point1;
                endPositionOfLine = connectingLinesAngularOffset * tangendOffset + point2;
            }
        }

        public static void DrawHemispheresOfCapsule(Vector3 point1, Vector3 point2, float radius)
        {
            if(radius < 0.01f)
            {
                return;
            }
            
            Vector3 directionFromPoints = point2 - point1;

            Quaternion rotationOfHS = Quaternion.FromToRotation(Vector3.down, directionFromPoints.normalized);
            Gizmos.matrix = Matrix4x4.TRS(point1, rotationOfHS, new Vector3(1, 1, 1));
            DrawHemisphereUpOrDown(Vector3.zero, radius);
            Gizmos.matrix = Matrix4x4.TRS(point2, rotationOfHS, new Vector3(1, 1, 1));
            DrawHemisphereUpOrDown(Vector3.zero, radius, -1);
        }

        static void DrawHemisphereUpOrDown(Vector3 origin, float radius, int sign = 1)
        {
            Vector3 offset = new Vector3(0, 1, 0) * sign;
            DrawHemiSphere(origin, radius, offset);
        }

        static void DrawCircleForHS(Vector3 origin, float radius)
        {
            float delta = 0.1f;

            float x = radius * Mathf.Cos(0f);
            float z = radius * Mathf.Sin(0f);

            Vector3 beginPosition = (origin + new Vector3(x, 0, z));
            Vector3 endPosition = beginPosition;
            Vector3 lastPosition = beginPosition;

            for (float deltaAngle = 0f; deltaAngle < Mathf.PI * 2; deltaAngle += delta)
            {
                x = radius * Mathf.Cos(deltaAngle);
                z = radius * Mathf.Sin(deltaAngle);
                endPosition = (origin + new Vector3(x, 0, z));
                Gizmos.DrawLine(beginPosition, endPosition);
                beginPosition = endPosition;
            }
            Gizmos.DrawLine(beginPosition, lastPosition);
        }

        static void DrawArcForHS(Vector3 origin, float radius, float alfhaOffset)
        {
            float delta = 0.1f;
            float x = radius * Mathf.Cos(0f);
            float y = radius * Mathf.Sin(0f);
            Quaternion arcRotation = Quaternion.AngleAxis(alfhaOffset, Vector3.up);
            Vector3 beginPosition = arcRotation * origin + arcRotation * (new Vector3(radius * Mathf.Cos(0), radius * Mathf.Sin(0)));
            Vector3 endPosition = beginPosition;
            Vector3 lastPosition = arcRotation * origin + arcRotation * (new Vector3(radius * Mathf.Cos(Mathf.PI), radius * Mathf.Sin(Mathf.PI)));
            for (int i = 0; i < 1; i++)
            {

                for (float deltaAngle = 0f; deltaAngle <= Mathf.PI; deltaAngle += delta)
                {

                    x = radius * Mathf.Cos(deltaAngle);
                    y = radius * Mathf.Sin(deltaAngle);
                    endPosition = arcRotation * origin + arcRotation * (new Vector3(x, y));
                    Gizmos.DrawLine(beginPosition, endPosition);
                    beginPosition = endPosition;
                }
                Gizmos.DrawLine(beginPosition, lastPosition);
            }
        }

    }
}
