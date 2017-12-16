﻿// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;

namespace OceanResearch
{
    /// <summary>
    /// Support script for gerstner wave ocean shapes.
    /// Generates a number of gerstner octaves in child gameobjects.
    /// </summary>
    public class ShapeGerstner : MonoBehaviour
    {
        // The number of Gerstner octaves
        public int NumOctaves = 10;
        // Range of amplitudes
        public Vector2 AmplitudeRange = new Vector2(10f, 25f);
        // Range of wavelengths
        public Vector2 WavelengthRange = new Vector2(20f, 50f);
        public float WavelengthDistribution = 1f;
        // General direction of flow, an angle in degrees
        [Range(-180, 180)]
        public float WaveDirectionAngle = 0f;
        // Variance of flow direction, in degrees
        public float WaveDirectionVariance = 29f;
        // Influence factor of wind.
        [Range(0, 1)]
        public float _windStrength = 0.5f;
        // Choppiness of waves. Treat carefully: If set too high, can cause the geometry to overlap itself.
        [Range(0, 5)]
        public float _choppiness = 1.8f;

        // Standard quad mesh, referenced here for convenience.
        public Mesh QuadMesh;
        // Shader to be used to render out a single Gerstner octave.
        public Shader GerstnerOctaveShader;

        public int RandomSeed = 0;

        void Start()
        {
            // Set random seed to get repeatable results
            Random.State randomStateBkp = Random.state;
            Random.InitState( RandomSeed );

            float[] wavelengths = new float[NumOctaves];
            for( int i = 0; i < NumOctaves; i++ )
            {
                float wavelengthSel = Mathf.Pow( Random.value, WavelengthDistribution );
                wavelengths[i] = Mathf.Lerp( WavelengthRange.x, WavelengthRange.y, wavelengthSel );
            }
            System.Array.Sort( wavelengths );

            // Generate the given number of octaves, each generating a GameObject rendering a quad.
            for (int i = 0; i < NumOctaves; i++)
            {
                GameObject GO = new GameObject("Octave" + i);
                GO.layer = gameObject.layer;

                MeshFilter meshFilter = GO.AddComponent<MeshFilter>();
                meshFilter.mesh = QuadMesh;

                GO.transform.parent = transform;
                GO.transform.localPosition = Vector3.zero;
                GO.transform.localRotation = Quaternion.identity;
                GO.transform.localScale = Vector3.one;

                MeshRenderer renderer = GO.AddComponent<MeshRenderer>();
                renderer.material = new Material(GerstnerOctaveShader);

                // Amplitude
                float amp = Mathf.Lerp(AmplitudeRange.x, AmplitudeRange.y, i / (float)Mathf.Max(NumOctaves - 1, 1)) / (float)NumOctaves;
                renderer.material.SetFloat("_Amplitude", amp);

                // Direction
                float angle = WaveDirectionAngle + Random.Range(-WaveDirectionVariance, WaveDirectionVariance);
                if (angle > 180f)
                {
                    angle -= 360f;
                }
                if (angle < -180f)
                {
                    angle += 360f;
                }
                renderer.material.SetFloat("_Angle", angle);

                // Wavelength
                renderer.material.SetFloat( "_Wavelength", wavelengths[i] );
            }

            Random.state = randomStateBkp;
        }

        private void Update()
        {
            Shader.SetGlobalFloat( "_WindStrength", _windStrength );
            Shader.SetGlobalFloat( "_Choppiness", _choppiness );
        }

        static ShapeGerstner _instance;
        public static ShapeGerstner Instance { get { return _instance ?? (_instance = FindObjectOfType<ShapeGerstner>()); } }
    }
}
