using UnityEngine;
using System.Collections.Generic;

namespace Trainamari.Train
{
    /// <summary>
    /// Catmull-Rom spline for track following.
    /// The train follows this spline at its current distance along the track.
    /// 
    /// Supports closed loops and calculates curvature at each point
    /// for derailment physics.
    /// </summary>
    public class TrackSpline : MonoBehaviour
    {
        [Header("Track Settings")]
        [SerializeField] private bool closedLoop = true;
        [SerializeField] private float trackWidth = 2f;
        [SerializeField] private int interpolationSteps = 100; // segments between control points
        [SerializeField] private float railGauge = 1.435f;    // standard gauge in meters (for visuals)
        
        [Header("Control Points")]
        [SerializeField] private Vector3[] controlPoints;
        
        // Cached spline data
        private Vector3[] splinePoints;
        private float[] distances;          // cumulative distance at each spline point
        private float totalLength;
        private float[] curvatures;          // curvature at each spline point
        
        public float GetLength() => totalLength;
        public float[] GetCurvatures() => curvatures;
        
        /// <summary>
        /// Set control points from external code (e.g., procedural generation).
        /// </summary>
        public void SetControlPoints(Vector3[] points)
        {
            controlPoints = points;
            Recalculate();
        }
        
        /// <summary>
        /// Get world position at a distance along the track.
        /// </summary>
        public Vector3 GetPointAtDistance(float distance)
        {
            if (splinePoints == null || splinePoints.Length == 0) return Vector3.zero;
            
            // Wrap distance for closed loops
            if (closedLoop)
            {
                distance = ((distance % totalLength) + totalLength) % totalLength;
            }
            else
            {
                distance = Mathf.Clamp(distance, 0f, totalLength);
            }
            
            // Find the two spline points we're between
            int index = FindSegmentIndex(distance);
            float segmentDistance = distance - distances[index];
            float segmentLength = distances[index + 1] - distances[index];
            float t = segmentLength > 0 ? segmentDistance / segmentLength : 0f;
            
            return Vector3.Lerp(splinePoints[index], splinePoints[index + 1], t);
        }
        
        /// <summary>
        /// Get the forward direction at a distance along the track.
        /// </summary>
        public Vector3 GetDirectionAtDistance(float distance)
        {
            float delta = 0.5f;
            Vector3 p1 = GetPointAtDistance(distance - delta);
            Vector3 p2 = GetPointAtDistance(distance + delta);
            return (p2 - p1).normalized;
        }
        
        /// <summary>
        /// Recalculate all spline data from control points.
        /// </summary>
        public void Recalculate()
        {
            if (controlPoints == null || controlPoints.Length < 2)
            {
                Debug.LogWarning("[TrackSpline] Need at least 2 control points");
                return;
            }
            
            // Build interpolated spline points using Catmull-Rom
            List<Vector3> points = new List<Vector3>();
            
            int numSegments = closedLoop ? controlPoints.Length : controlPoints.Length - 1;
            
            for (int i = 0; i < numSegments; i++)
            {
                Vector3 p0 = GetControlPoint(i - 1);
                Vector3 p1 = GetControlPoint(i);
                Vector3 p2 = GetControlPoint(i + 1);
                Vector3 p3 = GetControlPoint(i + 2);
                
                for (int step = 0; step < interpolationSteps; step++)
                {
                    float t = step / (float)interpolationSteps;
                    points.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }
            
            // Add final point for non-closed splines
            if (!closedLoop)
            {
                points.Add(controlPoints[controlPoints.Length - 1]);
            }
            
            splinePoints = points.ToArray();
            
            // Calculate cumulative distances
            distances = new float[splinePoints.Length];
            distances[0] = 0f;
            totalLength = 0f;
            
            for (int i = 1; i < splinePoints.Length; i++)
            {
                float segDist = Vector3.Distance(splinePoints[i - 1], splinePoints[i]);
                totalLength += segDist;
                distances[i] = totalLength;
            }
            
            // Calculate curvatures (rate of direction change)
            curvatures = new float[splinePoints.Length];
            for (int i = 1; i < splinePoints.Length - 1; i++)
            {
                Vector3 prev = (splinePoints[i] - splinePoints[i - 1]).normalized;
                Vector3 next = (splinePoints[i + 1] - splinePoints[i]).normalized;
                float angle = Vector3.Angle(prev, next);
                curvatures[i] = angle / 180f; // normalize to 0-1 range
            }
            
            // Edge curvatures
            curvatures[0] = curvatures.Length > 1 ? curvatures[1] : 0f;
            curvatures[curvatures.Length - 1] = curvatures.Length > 1 ? curvatures[curvatures.Length - 2] : 0f;
            
            Debug.Log($"[TrackSpline] Generated {splinePoints.Length} points, total length: {totalLength:F1}m");
        }
        
        /// <summary>
        /// Get a control point, wrapping for closed loops and clamping for open.
        /// </summary>
        private Vector3 GetControlPoint(int index)
        {
            if (closedLoop)
            {
                return controlPoints[((index % controlPoints.Length) + controlPoints.Length) % controlPoints.Length];
            }
            return controlPoints[Mathf.Clamp(index, 0, controlPoints.Length - 1)];
        }
        
        /// <summary>
        /// Catmull-Rom spline interpolation.
        /// </summary>
        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
        
        /// <summary>
        /// Find which spline segment a distance falls into using binary search.
        /// </summary>
        private int FindSegmentIndex(float distance)
        {
            int low = 0;
            int high = distances.Length - 2;
            
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (distances[mid] <= distance)
                    low = mid;
                else
                    high = mid - 1;
            }
            
            return low;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Draw the track spline in the editor for visualization.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (splinePoints == null || splinePoints.Length < 2) return;
            
            Gizmos.color = Color.yellow;
            for (int i = 0; i < splinePoints.Length - 1; i++)
            {
                Gizmos.DrawLine(splinePoints[i], splinePoints[i + 1]);
            }
            
            // Draw control points
            if (controlPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var cp in controlPoints)
                {
                    Gizmos.DrawWireSphere(cp + transform.position, 1f);
                }
            }
            
            // Draw station markers if any
            Gizmos.color = Color.cyan;
            // Station markers will be added by StationManager
        }
        #endif
    }
}