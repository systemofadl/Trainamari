using UnityEngine;

namespace Trainamari.Train
{
    /// <summary>
    /// Builds a quick visual reference world at Start: a long ground strip
    /// and a row of pillars stretching forward and backward along Z. Drop on
    /// an empty GameObject in any test scene. So you can SEE the cube moving.
    /// </summary>
    public class TestEnvironment : MonoBehaviour
    {
        [SerializeField] private int pillarsEachSide = 30;
        [SerializeField] private float pillarSpacing = 5f;
        [SerializeField] private float pillarOffsetX = 4f;
        [SerializeField] private float groundLength = 400f;
        [SerializeField] private float groundWidth = 20f;

        private void Start()
        {
            // Ground strip
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "TestGround";
            ground.transform.parent = transform;
            ground.transform.localScale = new Vector3(groundWidth / 10f, 1f, groundLength / 10f);
            ground.transform.position = new Vector3(0f, 0f, 0f);
            // Default URP lit material is already attached. Tint it dim grey via property block.
            Recolor(ground, new Color(0.25f, 0.25f, 0.27f));

            // Pillars along both sides at regular intervals
            for (int i = -pillarsEachSide; i <= pillarsEachSide; i++)
            {
                if (i == 0) continue; // skip origin so the cube starts unobstructed
                CreatePillar(new Vector3(+pillarOffsetX, 1.5f, i * pillarSpacing), AlternateColor(i));
                CreatePillar(new Vector3(-pillarOffsetX, 1.5f, i * pillarSpacing), AlternateColor(i));
            }
        }

        private void CreatePillar(Vector3 pos, Color tint)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.transform.parent = transform;
            pillar.transform.position = pos;
            pillar.transform.localScale = new Vector3(0.6f, 3f, 0.6f);
            Recolor(pillar, tint);
        }

        private static Color AlternateColor(int i)
        {
            // Every 5th pillar is bright so you can count units of distance.
            if (i % 5 == 0) return new Color(0.95f, 0.7f, 0.1f);
            return new Color(0.55f, 0.55f, 0.6f);
        }

        private static void Recolor(GameObject go, Color c)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", c); // URP/Lit
            block.SetColor("_Color", c);     // Built-in fallback
            rend.SetPropertyBlock(block);
        }
    }
}
