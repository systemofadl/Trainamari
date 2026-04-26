using UnityEngine;
using System.Collections.Generic;

namespace Trainamari.Train
{
    /// <summary>
    /// Endless ground + pillar runway. Pillars are recycled in a ring around the
    /// target's Z position so the world never runs out, no matter the speed.
    /// Drop on an empty GameObject, assign Target = the cube.
    /// </summary>
    public class InfiniteEnvironment : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private int pillarCountPerSide = 60;
        [SerializeField] private float pillarSpacing = 8f;
        [SerializeField] private float pillarOffsetX = 5f;
        [SerializeField] private float groundWidth = 30f;

        private readonly List<Transform> leftPillars = new();
        private readonly List<Transform> rightPillars = new();
        private float ringLength;
        private Transform ground;

        private void Start()
        {
            ringLength = pillarCountPerSide * pillarSpacing;

            // Big ground plane that we'll keep centered under the target.
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
            ground.name = "InfiniteGround";
            ground.parent = transform;
            ground.localScale = new Vector3(groundWidth / 10f, 1f, ringLength * 2f / 10f);
            Recolor(ground.gameObject, new Color(0.22f, 0.22f, 0.25f));

            for (int i = 0; i < pillarCountPerSide; i++)
            {
                leftPillars.Add(MakePillar(-pillarOffsetX, i, true));
                rightPillars.Add(MakePillar(+pillarOffsetX, i, false));
            }
        }

        private Transform MakePillar(float x, int index, bool isLeft)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            p.parent = transform;
            p.localScale = new Vector3(0.6f, 3f, 0.6f);
            p.position = new Vector3(x, 1.5f, index * pillarSpacing);
            // Every 5th pillar is a yellow distance marker.
            Recolor(p.gameObject, (index % 5 == 0)
                ? new Color(0.95f, 0.7f, 0.1f)
                : new Color(0.55f, 0.55f, 0.6f));
            return p;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Slide the ground so it's always centered under the target.
            var gp = ground.position;
            gp.z = target.position.z;
            ground.position = gp;

            // Recycle any pillar that has fallen too far behind the target.
            float behindThreshold = target.position.z - ringLength * 0.5f;
            float aheadThreshold  = target.position.z + ringLength * 0.5f;
            RecycleRing(leftPillars, behindThreshold, aheadThreshold);
            RecycleRing(rightPillars, behindThreshold, aheadThreshold);
        }

        private void RecycleRing(List<Transform> ring, float behind, float ahead)
        {
            foreach (var p in ring)
            {
                if (p.position.z < behind)
                {
                    var pos = p.position; pos.z += ringLength; p.position = pos;
                }
                else if (p.position.z > ahead)
                {
                    var pos = p.position; pos.z -= ringLength; p.position = pos;
                }
            }
        }

        private static void Recolor(GameObject go, Color c)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", c);
            block.SetColor("_Color", c);
            rend.SetPropertyBlock(block);
        }
    }
}
