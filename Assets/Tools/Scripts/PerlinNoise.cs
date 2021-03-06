﻿using UnityEngine;
using System.Collections;

public class PerlinNoise
{
    public const int MaxPermutationValue = 16777216;

    //	private static int[] _permutation = { 
    //		151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
    //		8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,
    //		117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,
    //		165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,
    //		105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,
    //		187,208,89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,
    //		3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,
    //		227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,
    //		221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,
    //		185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,
    //		51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,
    //		121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,
    //		78,66,215,61,156,180
    //	}; 

    private static int[] _permutation = {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
        8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,
        117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,
        165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,
        105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,
        187,208,89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,
        3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,
        227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,
        221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,
        185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,
        51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,
        121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,
        78,66,215,61,156,180,
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
        8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,
        117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,
        165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,
        105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,
        187,208,89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,
        3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,
        227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,
        221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,
        185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,
        51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,
        121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,
        78,66,215,61,156,180
    };

    public static int GetPermutationValue(float x, float y, float z)
    {
        //		int[] p = new int[512];
        //		
        //		for (int i=0; i < 256; i++) 
        //			p [256 + i] = p [i] = _permutation [i];

        int fx = (int)(Mathf.Floor(x));
        int fy = (int)(Mathf.Floor(y));
        int fz = (int)(Mathf.Floor(z));

        return GetPermutationValue(fx, fy, fz);
    }

    public static int GetPermutationValue(int x, int y, int z)
    {
        int X = x & 255;
        int Y = y & 255;
        int Z = z & 255;

        int A = _permutation[X] + Y;
        int AA = _permutation[A] + Z;
        int AB = _permutation[AA];

        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B] + Z;
        int BB = _permutation[BA];

        int C = _permutation[X + 2] + Y;
        int CA = _permutation[C] + Z;
        int CB = _permutation[CA];

        int value = AB + (BB * 256) + (CB * 256 * 256);

        return value;
    }

    // Returns a value between 0 and 1
    public static float GetValue(float x, float y, float z)
    {
        int fx = (int)(Mathf.Floor(x));
        int fy = (int)(Mathf.Floor(y));
        int fz = (int)(Mathf.Floor(z));

        int X = fx & 255;
        int Y = fy & 255;
        int Z = fz & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);

        float u = fade(x);
        float v = fade(y);
        float w = fade(z);

        int A = _permutation[X] + Y;
        int AA = _permutation[A] + Z;
        int AB = _permutation[A + 1] + Z;

        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B] + Z;
        int BB = _permutation[B + 1] + Z;

        return scale(
            lerp(w,
                lerp(v, 
                    lerp(u,
                        grad(_permutation[AA], x, y, z),
                        grad(_permutation[BA], x - 1, y, z)
                        ),
                    lerp(u, 
                        grad(_permutation[AB], x, y - 1, z),
                        grad(_permutation[BB], x - 1, y - 1, z)
                        )
                    ),
                lerp(v, 
                    lerp(u,
                        grad(_permutation[AA + 1], x, y, z - 1),
                        grad(_permutation[BA + 1], x - 1, y, z - 1)
                        ),
                    lerp(u, 
                        grad(_permutation[AB + 1], x, y - 1, z - 1),
                        grad(_permutation[BB + 1], x - 1, y - 1, z - 1)
                        )
                    )
                )
            );
    }

    // Returns a Perlin noise 3d vector as an array of 3 values
    public static float[] Get3DVector(float x, float y, float z)
    {
        int fx = (int)(Mathf.Floor(x));
        int fy = (int)(Mathf.Floor(y));
        int fz = (int)(Mathf.Floor(z));

        int X = fx & 255;
        int Y = fy & 255;
        int Z = fz & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);

        float u = fade(x);
        float v = fade(y);
        float w = fade(z);

        int A = _permutation[X] + Y;
        int AA = _permutation[A] + Z;
        int AB = _permutation[A + 1] + Z;

        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B] + Z;
        int BB = _permutation[B + 1] + Z;

        float[] values = new float[] {
            grad(_permutation[AA], x, y, z),
            grad(_permutation[BA], x - 1, y, z),
            grad(_permutation[AB], x, y - 1, z),
            grad(_permutation[BB], x - 1, y - 1, z),
            grad(_permutation[AA + 1], x, y, z - 1),
            grad(_permutation[BA + 1], x - 1, y, z - 1),
            grad(_permutation[AB + 1], x, y - 1, z - 1),
            grad(_permutation[BB + 1], x - 1, y - 1, z - 1)
        };

        // w for z;
        // v for y;
        // u for x;

        float zValue = lerp(v,
                lerp(u, values[0], values[1]),
                lerp(u, values[2], values[3])
                ) - lerp(v,
                lerp(u, values[4], values[5]),
                lerp(u, values[6], values[7])
                );

        float yValue = lerp(w,
                lerp(u, values[0], values[1]),
                lerp(u, values[4], values[5])
                ) - lerp(w,
                lerp(u, values[2], values[3]),
                lerp(u, values[6], values[7])
                );

        float xValue = lerp(v,
                lerp(w, values[0], values[4]),
                lerp(w, values[2], values[6])
                ) - lerp(v,
                lerp(w, values[1], values[5]),
                lerp(w, values[3], values[7])
                );

        return new float[] { xValue / 2f, yValue / 2f, zValue / 2f };
    }

    private static float fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private static float grad(int hash, float x, float y, float z)
    {
        int h = hash & 15;
        float u = (h < 8) ? x : y;
        float v = (h < 4) ? y : ((h == 12) || (h == 14)) ? x : z;

        return (((h & 1) == 0) ? u : -u) + (((h & 2) == 0) ? v : -v);
    }

    private static float scale(float n)
    {
        return (1 + n) / 2.0f;
    }
}
