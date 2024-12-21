using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator 
{
    public static float [,,] Generate3DNoiseMap(ChunkSettingsSO chunkSO, Vector3 posOffset)
    {
        switch (chunkSO.noiseType)
        {
            case NoiseType.Perlin3D:
                return GenerateMapPerlin3D(chunkSO.chunkSize + Vector3Int.one, chunkSO.noiseScale, chunkSO.persistance, 
                chunkSO.lacunarity, chunkSO.octaves, chunkSO.seed, chunkSO.posOffset + posOffset/chunkSO.meshScale);
            case NoiseType.Shore_Plateau:
                return GeneratePlateauNoiseMap3D(chunkSO.chunkSize + Vector3Int.one, chunkSO.noiseScale, chunkSO.persistance, 
                chunkSO.lacunarity, chunkSO.octaves, chunkSO.seed, chunkSO.posOffset + posOffset/chunkSO.meshScale, chunkSO.xBound, chunkSO.yBound, chunkSO.zBound,
                 chunkSO);
            default:
                return GenerateMapPerlin3D(chunkSO.chunkSize + Vector3Int.one, chunkSO.noiseScale, chunkSO.persistance, 
                chunkSO.lacunarity, chunkSO.octaves, chunkSO.seed, chunkSO.posOffset + posOffset/chunkSO.meshScale);
        }
    } 
    public static float[,] GenerateNoiseMap2D(Vector2Int size, float noiseScale, float persistence, float lacunarity, int octaves, int seed, Vector2 offset, Vector2 squichiness)
    {
        float[,] perlinTab = new float[size.x, size.y];
        Vector2[] octaveOffsets = new Vector2[octaves];
        Vector2 halfSize = new Vector2((size.x - 1) / 2, (size.y - 1) / 2);

        System.Random prng = new System.Random(seed);
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x - halfSize.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y - halfSize.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        for (int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                float noiseValue = 0;
                frequency = 1;
                amplitude = 1;

                for(int i = 0; i < octaves; i++)
                {
                    float xCoord = (x + octaveOffsets[i].x) * squichiness.x / noiseScale * frequency;
                    float yCoord = (y + octaveOffsets[i].y) * squichiness.y / noiseScale * frequency;

                    noiseValue += Mathf.PerlinNoise(xCoord, yCoord) * amplitude;
                    frequency *= lacunarity;
                    amplitude *= persistence;
                }
                perlinTab[x, y] = Mathf.InverseLerp(0, maxPossibleHeight * 0.7f, noiseValue);
            }
        }
        return perlinTab;
    }


    public static float[,,] GenerateMapPerlin3D(Vector3Int size, float noiseScale, float persistence, float lacunarity, int octaves, int seed, Vector3 offset) 
    {
        System.Random prng = new System.Random(seed);

        float[,,] perlinTab = new float[size.x, size.y, size.z];
        Vector3[] octaveOffsets = new Vector3[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        Vector3 halfSize = new Vector3((size.x - 1) / 2, (size.y - 1) / 2, (size.z - 1)/2 );

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(0, 100000) + offset.x - halfSize.x;
            float offsetY = prng.Next(0, 100000) + offset.y - halfSize.y;
            float offsetZ = prng.Next(0, 100000) + offset.z - halfSize.z; 

            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        for(int x = 0; x < size.x; x++)
            for(int y = 0; y < size.y; y++)
                for(int z = 0; z < size.z; z++)
                {
                    float noiseValue = 0;
                    amplitude = 1;
                    frequency = 1;

                    for(int i = 0; i < octaves; i++) 
                    {
                        float xCoord = (x + octaveOffsets[i].x) / noiseScale * frequency;
                        float yCoord = (y + octaveOffsets[i].y) / noiseScale * frequency;
                        float zCoord = (z + octaveOffsets[i].z) / noiseScale * frequency;

                        noiseValue += PerlinNoise3D(xCoord, yCoord, zCoord) * amplitude;

                        frequency *= lacunarity;
                        amplitude *= persistence;
                    }

                    //count += noiseValue;
                    perlinTab[x, y, z] = Mathf.InverseLerp(0f, maxPossibleHeight * 0.7f, noiseValue);
                }

        //count /= (size * size * size);
        //Debug.Log(count);
        
        return perlinTab;
    }

    public static float[,,] GeneratePlateauNoiseMap3D(Vector3Int size, float noiseScale, float persistence, float lacunarity, int octaves, int seed, Vector3 offset, Vector4 xBound, 
    Vector4 yBound, Vector4 zBound, ChunkSettingsSO chunkSO) 
    {
        Vector3 halfSize = new Vector3((size.x - 1) / 2, (size.y - 1) / 2, (size.z - 1)/2 );
        
        System.Random prng = new System.Random(seed);

        float[,,] perlinTab = new float[size.x, size.y, size.z];
        float[,] topCutoutMap = GenerateNoiseMap2D(new Vector2Int(size.x, size.z), chunkSO.cutoutNoiseScale, chunkSO.cutoutNoisePersistance, chunkSO.cutoutNoiseLacunarity, 
        chunkSO.cutoutNoiseOctaves, seed, new Vector2(offset.x, offset.z) + chunkSO.cutoutNoiseOffset, chunkSO.cutoutMapSquichiness);
        Vector3[] octaveOffsets = new Vector3[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        //Initialize offset to not recalculate them every loop	
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x - halfSize.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y - halfSize.y;
            float offsetZ = prng.Next(-100000, 100000) + offset.z - halfSize.z; 

            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        //check if out of bound -> not bother with calculus // To Remove later cause lerping between edges like the cutout noise
        for(int x = 0; x < size.x; x++)
            for(int y = 0; y < size.y; y++) 
                for(int z = 0; z < size.z; z++)
                {
                    //jump to next if out of bound to optimize. less elegant but does the work
                    
                    if(y + offset.y < yBound.x)
                    {
                        perlinTab[x, y, z] = yBound.z;
                        continue;
                    }

                    if(y + offset.y > yBound.y)
                    {
                        perlinTab[x, y, z] = yBound.w;
                        continue;
                    }

                    if(x + offset.x < xBound.x)
                    {
                        perlinTab[x, y, z] = xBound.z;
                        continue;
                    }

                    if(x + offset.x > xBound.y)
                    {
                        perlinTab[x, y, z] = xBound.w;
                        continue;
                    }

                    if(z + offset.z < zBound.x)
                    {
                        perlinTab[x, y, z] = zBound.z;
                        continue;
                    }

                    if(z + offset.z > zBound.y)
                    {
                        perlinTab[x, y, z] = zBound.w;
                        continue;
                    }

                    //Main Noise Loop
                    float noiseValue = 0;
                    amplitude = 1;
                    frequency = 1;

                    for(int i = 0; i < octaves; i++) 
                    {
                        float xCoord = (x + octaveOffsets[i].x) * chunkSO.noiseSquichiness.x / noiseScale * frequency;
                        float yCoord = (y + octaveOffsets[i].y) * chunkSO.noiseSquichiness.y / noiseScale * frequency;
                        float zCoord = (z + octaveOffsets[i].z) * chunkSO.noiseSquichiness.z / noiseScale * frequency;

                        noiseValue += PerlinNoise3D(xCoord, yCoord, zCoord) * amplitude;

                        frequency *= lacunarity;
                        amplitude *= persistence;
                    }

                    //Normalizing values to work between 0 and 1
                    perlinTab[x, y, z] = Mathf.InverseLerp(0f, maxPossibleHeight * 0.7f, noiseValue);

                    if(chunkSO.useEaseCurve)
                    {
                        perlinTab[x, y, z] = chunkSO.noiseEaseCurve.Evaluate(perlinTab[x, y, z]);
                    }
                }

        //Plateau Noise, Multiply the noise of a whole collection of y layers according to a modulo, 
        if(chunkSO.addPlateau)
        {
            float minModulo = chunkSO.bottomPlateauWidth;
            float maxModulo = chunkSO.plateauModulo - chunkSO.topPlateauWidth;
            float plateauWidth = chunkSO.bottomPlateauWidth + chunkSO.topPlateauWidth + 1;

             for(int y = 0; y < size.y; y++) //pour optimiser, on s'assure d'avoir un bon y sans iterer dans x et y
             {
                int yPos = y + (int)offset.y;
                float modulo = yPos % chunkSO.plateauModulo; 

                if(modulo < 0) modulo = chunkSO.plateauModulo + modulo;

                if(modulo <= minModulo || modulo >= maxModulo)
                {
                    float ratio;

                    if(modulo <= minModulo)
                        ratio = modulo / (minModulo + 1);
                    else if(modulo == 0)
                        ratio = 0;
                    else
                        ratio = (chunkSO.plateauModulo - modulo) / (chunkSO.topPlateauWidth + 1);

                    for(int x = 0; x < size.x; x++)
                        for(int z = 0; z < size.z; z++)
                        {
                            perlinTab[x, y, z] *= Mathf.Lerp(1, chunkSO.plateauModifMax, ratio);
                        }
                }
             }
        }

        //Make a max height for each point to get a surfacy look where structure stops before reaching the top
        //We compare the ypos with a 2d noise which input are x and z. we then determine a max yThreshold for the column, and we lerp the value accordingly
        if(chunkSO.addTopCutout)
        {
            for(int x = 0; x < size.x; x++)
                for(int z = 0; z < size.z; z++)
                {
                    float yThreshold = topCutoutMap[x, z] * (chunkSO.cutoutThresholdRange.y - chunkSO.cutoutThresholdRange.x) + chunkSO.cutoutThresholdRange.x;

                    for(int y = 0; y < size.y; y++)
                        if(y + offset.y > yThreshold)
                        {
                            if(y + offset.y < yThreshold + chunkSO.cutoutThresholdLerpLength) //dans la range du lerp
                            {
                                perlinTab[x, y, z] = Mathf.Lerp(perlinTab[x, y, z], yBound.w, (y + offset.y - yThreshold) /  chunkSO.cutoutThresholdLerpLength);
                            }
                            else  //hors de la range du lerp
                            {
                                perlinTab[x, y, z] = yBound.w;;
                            }
                        }
                }
            
        }           

        if(chunkSO.addEdgeMapLerp) // add lerp to edges of map
        {
            Vector2 xLerpBound = new Vector2(chunkSO.xBound.x + chunkSO.lerpRange, chunkSO.xBound.y - chunkSO.lerpRange);
            Vector2 yLerpBound = new Vector2(chunkSO.yBound.x + chunkSO.lerpRange, chunkSO.yBound.y - chunkSO.lerpRange);
            Vector2 zLerpBound = new Vector2(chunkSO.zBound.x + chunkSO.lerpRange, chunkSO.zBound.y - chunkSO.lerpRange);

            for(int x = 0; x < size.x; x++)
            {
                float xPos = x + (int)offset.x;

                if(xPos < xBound.x || xPos > xBound.y)
                    continue;
                    
                 for(int y = 0; y< size.y; y++)
                 {
                    float yPos = y + (int)offset.y;

                    if(yPos < yBound.x || yPos > yBound.y)
                        continue;

                    for(int z = 0; z < size.z; z++)
                    {
                        float zPos = z + (int)offset.z;

                        if(zPos < zBound.x || zPos > zBound.y)
                            continue;
                        
                        float count = 0;
                        float noise = perlinTab[x, y, z];                       

                        if(xPos <= xLerpBound.x)
                        {
                            float ratio = (xPos - xLerpBound.x) / chunkSO.lerpRange;
                            ratio *= ratio * ratio;
                            noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.xBound.z : chunkSO.xBound.z ,-ratio);
                            count++;
                        }
                        if(xPos >= xLerpBound.y)
                        {
                            float ratio = (xPos - xLerpBound.y) / chunkSO.lerpRange;
                            ratio *= ratio * ratio;
                            noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.xBound.w : chunkSO.xBound.w ,ratio);
                            count++;
                        }

                        if(yPos <= yLerpBound.x)
                        {
                            float ratio = (yPos - yLerpBound.x) / chunkSO.lerpRange;
                            ratio *= ratio * ratio;
                            noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.yBound.z : chunkSO.yBound.z ,-ratio);
                            count++;
                        }
                        if(yPos >= yLerpBound.y)
                        {
                            float ratio = (yPos - yLerpBound.y) / chunkSO.lerpRange;
                            ratio *= ratio * ratio;
                            noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.yBound.w : chunkSO.yBound.w ,ratio);
                            count++;
                        }

                        if(zPos <= zLerpBound.x)
                        {
                            float ratio = (zPos - zLerpBound.x) / chunkSO.lerpRange;
                            ratio *= ratio * ratio;
                            noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.zBound.z : chunkSO.zBound.z ,-ratio);
                            count++;
                        }

                        if(zPos >= zLerpBound.y)
                        {
                            float ratio = (zPos - zLerpBound.y) / chunkSO.lerpRange;
                            ratio *= ratio * ratio;
                            noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.zBound.w : chunkSO.zBound.w ,ratio);
                            count++;
                        }

                        perlinTab[x, y, z] = noise;
                    }
                 }  
            }   
        }
        return perlinTab;
    }
    public static float PerlinNoise3D(float x, float y, float z) 
    {
        float noise = 0.0f;

        // Get all permutations of noise for each individual axis
        float noiseXY = Mathf.PerlinNoise(x, y);
        float noiseXZ = Mathf.PerlinNoise(x, z);
        float noiseYZ = Mathf.PerlinNoise(y, z);

        // Reverse of the permutations of noise for each individual axis
        float noiseYX = Mathf.PerlinNoise(y, x);
        float noiseZX = Mathf.PerlinNoise(z, x);
        float noiseZY = Mathf.PerlinNoise(z, y);

        // Use the average of the noise functions
        noise += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;
        return noise;
    }
}
