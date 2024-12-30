using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//Generate a 2D or 3D noise map
public static class NoiseGenerator 
{
    //no longer needed, cause the settings are contained in chunkSO and there is no different functions for creating noise
    //simplify the initialisation of different noise settings
    public static float [,,] Generate3DNoiseMap(ChunkSettingsSO chunkSO, Vector3 posOffset) {
        switch (chunkSO.noiseType) {
            case NoiseType.Shore_Plateau:
                return GeneratePlateauNoiseMap3D(chunkSO.chunkSize + Vector3Int.one, chunkSO.noiseScale, chunkSO.persistance, 
                chunkSO.lacunarity, chunkSO.octaves, chunkSO.seed, chunkSO.posOffset + posOffset/chunkSO.meshScale, chunkSO.xBound, chunkSO.yBound, chunkSO.zBound,
                 chunkSO);
            case NoiseType.Perlin3D:
            default:
                return GenerateMapPerlin3D(chunkSO.chunkSize + Vector3Int.one, chunkSO.noiseScale, chunkSO.persistance, 
                chunkSO.lacunarity, chunkSO.octaves, chunkSO.seed, chunkSO.posOffset + posOffset/chunkSO.meshScale);
        }
    } 
    
    //Generate a 2D noise map 
    public static float[,] GenerateNoiseMap2D(Vector2Int size, float noiseScale, float persistence, float lacunarity, int octaves, int seed, Vector2 offset, Vector2 squichiness) {
        float[,] perlinArray = new float[size.x, size.y];
        Vector2[] octaveOffsets = new Vector2[octaves];
        Vector2 halfSize = new Vector2((size.x - 1) / 2, (size.y - 1) / 2);

        System.Random prng = new System.Random(seed); //seeded randomness

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        //Initialization of offset and max amplitude for the final inverse lerp
        for(int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x - halfSize.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y - halfSize.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        //Pour chaque coordonnée, on calcule le bruit de perin pour le nombre d'octaves
        for (int x = 0; x < size.x; x++) {
            for(int y = 0; y < size.y; y++) {
                float noiseValue = 0;
                frequency = 1;
                amplitude = 1;

                for(int i = 0; i < octaves; i++) {
                    float xCoord = (x + octaveOffsets[i].x) * squichiness.x / noiseScale * frequency;
                    float yCoord = (y + octaveOffsets[i].y) * squichiness.y / noiseScale * frequency;

                    noiseValue += Mathf.PerlinNoise(xCoord, yCoord) * amplitude;
                    frequency *= lacunarity;
                    amplitude *= persistence;
                }
                perlinArray[x, y] = Mathf.InverseLerp(0, maxPossibleHeight * 0.7f, noiseValue);
            }
        }
        return perlinArray;
    }

    //Generate a 3D noise map, same principal than 2D but with an added dimension
    public static float[,,] GenerateMapPerlin3D(Vector3Int size, float noiseScale, float persistence, float lacunarity, int octaves, int seed, Vector3 offset) {
        System.Random prng = new System.Random(seed);
        float[,,] perlinArray = new float[size.x, size.y, size.z];
        Vector3[] octaveOffsets = new Vector3[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        Vector3 halfSize = new Vector3((size.x - 1) / 2, (size.y - 1) / 2, (size.z - 1)/2 );

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(0, 100000) + offset.x - halfSize.x;
            float offsetY = prng.Next(0, 100000) + offset.y - halfSize.y;
            float offsetZ = prng.Next(0, 100000) + offset.z - halfSize.z; 

            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        for(int x = 0; x < size.x; x++)
            for(int y = 0; y < size.y; y++)
                for(int z = 0; z < size.z; z++) {
                    float noiseValue = 0;
                    amplitude = 1;
                    frequency = 1;

                    for(int i = 0; i < octaves; i++) {
                        float xCoord = (x + octaveOffsets[i].x) / noiseScale * frequency;
                        float yCoord = (y + octaveOffsets[i].y) / noiseScale * frequency;
                        float zCoord = (z + octaveOffsets[i].z) / noiseScale * frequency;

                        noiseValue += PerlinNoise3D(xCoord, yCoord, zCoord) * amplitude;

                        frequency *= lacunarity;
                        amplitude *= persistence;
                    }
                    perlinArray[x, y, z] = Mathf.InverseLerp(0f, maxPossibleHeight * 0.7f, noiseValue);
                }
        
        return perlinArray;
    }

    public static float[,,] GeneratePlateauNoiseMap3D(Vector3Int size, float noiseScale, float persistence, float lacunarity, int octaves, int seed, Vector3 offset, Vector4 xBound, 
    Vector4 yBound, Vector4 zBound, ChunkSettingsSO chunkSO) {
        Vector3 halfSize = new Vector3((size.x - 1) / 2, (size.y - 1) / 2, (size.z - 1)/2 );
        System.Random prng = new System.Random(seed);
        float[,,] perlinArray = new float[size.x, size.y, size.z];
        

        Vector3[] octaveOffsets = new Vector3[octaves];
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        //Initialize offset to not recalculate them every loop	
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x - halfSize.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y - halfSize.y;
            float offsetZ = prng.Next(-100000, 100000) + offset.z - halfSize.z; 
            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        //main loop for perlin values
        for(int x = 0; x < size.x; x++)
            for(int y = 0; y < size.y; y++) 
                for(int z = 0; z < size.z; z++) {
                    //We remove out of bounds pos to avoid useless calculations
                    Vector3 posWithOffset = new Vector3(x, y, z) + offset;
                    if(IsCoordinateInBound(posWithOffset, xBound, yBound, zBound, out float result)) {
                        perlinArray[x, y, z] = result;
                        continue;
                    }

                    //Main Noise Loop
                    float noiseValue = 0;
                    amplitude = 1;
                    frequency = 1;

                    for(int i = 0; i < octaves; i++) {
                        float xCoord = (x + octaveOffsets[i].x) * chunkSO.noiseSquichiness.x / noiseScale * frequency;
                        float yCoord = (y + octaveOffsets[i].y) * chunkSO.noiseSquichiness.y / noiseScale * frequency;
                        float zCoord = (z + octaveOffsets[i].z) * chunkSO.noiseSquichiness.z / noiseScale * frequency;

                        noiseValue += PerlinNoise3D(xCoord, yCoord, zCoord) * amplitude;

                        frequency *= lacunarity;
                        amplitude *= persistence;
                    }

                    //Normalizing noise values to work between 0 and 1
                    perlinArray[x, y, z] = Mathf.InverseLerp(0f, maxPossibleHeight * 0.7f, noiseValue);

                    if(chunkSO.useEaseCurve) {
                        perlinArray[x, y, z] = chunkSO.noiseEaseCurve.Evaluate(perlinArray[x, y, z]);
                    }
                }

        //Plateau Noise, Multiply the noise of a whole collection of y layers according to a modulo, 
        if(chunkSO.addPlateau) {
            AddPlateauToNoiseMap(ref perlinArray, offset, chunkSO);
        }

        //Top cutout for a natural looking top from above
        if(chunkSO.addTopCutout) {
            AddTopCutout(ref perlinArray, offset, chunkSO);
        }           

        if(chunkSO.addEdgeMapLerp) { // add lerp to edges of map
            AddLerpedEdges(ref perlinArray, offset, chunkSO);
        }
        return perlinArray;
    }
    
    //Custom function to return a 3D perlin noise (usually only exists in 2D), it has a tendency to do straight line along origin 
    //but its fast and efficient
    private static float PerlinNoise3D(float x, float y, float z) {
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

    //check if pos is out of bounds
    private static bool IsCoordinateInBound(Vector3 pos, Vector4 xBound, Vector4 yBound, Vector4 zBound, out float boundResult) {
        if(pos.y < yBound.x) {
            boundResult = yBound.z;
            return true;
        }

        if(pos.y > yBound.y) {
            boundResult = yBound.w;
            return true;
        }

        if(pos.x < xBound.x) {
            boundResult = xBound.z;
            return true;
        }

        if(pos.x > xBound.y) {
            boundResult = xBound.w;
            return true;
        }

        if(pos.z < zBound.x) {
            boundResult = zBound.z;
            return true;
        }

        if(pos.z > zBound.y) {
            boundResult = zBound.w;
            return true;
        }
        boundResult = -1;
        return false;
    } 

    private static bool IsCoordinateInBound(Vector3 pos, Vector2 xBound, Vector2 yBound, Vector2 zBound) {
        if(pos.y < yBound.x) {
            return true;
        }

        if(pos.y > yBound.y) {
            return true;
        }

        if(pos.x < xBound.x) {
            return true;
        }

        if(pos.x > xBound.y) {
            return true;
        }

        if(pos.z < zBound.x) {
            return true;
        }

        if(pos.z > zBound.y) {
            return true;
        }
        return false;
    } 

    //add lerped plateau to specific modulo
    private static void AddPlateauToNoiseMap(ref float[,,] perlinArray, Vector3 posOffset, ChunkSettingsSO chunkSO) {
        float minModulo = chunkSO.bottomPlateauWidth;
        float maxModulo = chunkSO.plateauModulo - chunkSO.topPlateauWidth;

        for(int y = 0; y < chunkSO.chunkSize.y + 1; y++) {
            int yPos = y + (int)posOffset.y;
            float modulo = yPos % chunkSO.plateauModulo; 

            if(modulo < 0) {
                modulo += chunkSO.plateauModulo;
            }

            //ratio is how close to the plateau modulo and in range the pos is, so we can lerp the plateau over a range of y values
            if(modulo <= minModulo || modulo >= maxModulo) {
                float ratio;
                if(modulo <= minModulo) {
                    ratio = modulo / (minModulo + 1);
                } else if(modulo == 0) {
                    ratio = 0;
                } else {
                    ratio = (chunkSO.plateauModulo - modulo) / (chunkSO.topPlateauWidth + 1);
                }

                //we then modify all cells of the y row
                for(int x = 0; x < chunkSO.chunkSize.x + 1; x++)
                    for(int z = 0; z < chunkSO.chunkSize.z + 1; z++) {
                        perlinArray[x, y, z] *= Mathf.Lerp(1, chunkSO.plateauModifMax, ratio);
                    }
            }
        }
    }

    //Make a max height for each point to get a surfacy look where structure stops before reaching the top
    //We compare the ypos with a 2d noise which input are x and z. we then determine a max yThreshold for the column, and we lerp the value accordingly
    private static void AddTopCutout(ref float[,,] perlinArray, Vector3 posOffset, ChunkSettingsSO chunkSO) {
        float[,] topCutoutMap = GenerateNoiseMap2D(new Vector2Int(chunkSO.chunkSize.x + 1, chunkSO.chunkSize.z + 1), chunkSO.cutoutNoiseScale, chunkSO.cutoutNoisePersistance, chunkSO.cutoutNoiseLacunarity, 
        chunkSO.cutoutNoiseOctaves, chunkSO.seed, new Vector2(posOffset.x, posOffset.z) + chunkSO.cutoutNoiseOffset, chunkSO.cutoutMapSquichiness);
        
        for(int x = 0; x < chunkSO.chunkSize.x + 1; x++) {
            for(int z = 0; z < chunkSO.chunkSize.z + 1; z++){
                float yThreshold = topCutoutMap[x, z] * (chunkSO.cutoutThresholdRange.y - chunkSO.cutoutThresholdRange.x) + chunkSO.cutoutThresholdRange.x;
                for(int y = 0; y < chunkSO.chunkSize.y + 1; y++) {
                    if(y + posOffset.y > yThreshold) {
                        if(y + posOffset.y < yThreshold + chunkSO.cutoutThresholdLerpLength) { //dans la range du lerp
                            perlinArray[x, y, z] = Mathf.Lerp(perlinArray[x, y, z], chunkSO.yBound.w, (y + posOffset.y - yThreshold) /  chunkSO.cutoutThresholdLerpLength);
                        } else {
                            perlinArray[x, y, z] = chunkSO.yBound.w;;
                        }
                    }
                }
            }
        }
    }

    private static void AddLerpedEdges(ref float[,,] perlinArray, Vector3 posOffset, ChunkSettingsSO chunkSO) { //todo might be a better a way to wrap this
        Vector2 xLerpBound = new Vector2(chunkSO.xBound.x + chunkSO.lerpRange, chunkSO.xBound.y - chunkSO.lerpRange);
        Vector2 yLerpBound = new Vector2(chunkSO.yBound.x + chunkSO.lerpRange, chunkSO.yBound.y - chunkSO.lerpRange);
        Vector2 zLerpBound = new Vector2(chunkSO.zBound.x + chunkSO.lerpRange, chunkSO.zBound.y - chunkSO.lerpRange);
        
        for(int x = 0; x < chunkSO.chunkSize.x + 1; x++) {
            for(int y = 0; y < chunkSO.chunkSize.y + 1; y++) {
                for(int z = 0; z < chunkSO.chunkSize.z + 1; z++) {
                    float xPos = x + posOffset.x;
                    float yPos = y + posOffset.y;
                    float zPos = z + posOffset.z;

                    float noise = perlinArray[x, y, z];  

                    //lerp on edge noise with the edge value to get more natural looking seem, even around multiple seams 
                    if(xPos <= xLerpBound.x) {
                        float ratio = - (xPos - xLerpBound.x) / chunkSO.lerpRange;
                        ratio *= ratio * ratio;
                        noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.xBound.z : chunkSO.xBound.z ,ratio);
                    }

                    if(xPos >= xLerpBound.y) {
                        float ratio = (xPos - xLerpBound.y) / chunkSO.lerpRange;
                        ratio *= ratio * ratio;
                        noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.xBound.w : chunkSO.xBound.w ,ratio);
                    }

                    if(yPos <= yLerpBound.x) {
                        float ratio = - (yPos - yLerpBound.x) / chunkSO.lerpRange;
                        ratio *= ratio * ratio;
                        noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.yBound.z : chunkSO.yBound.z ,ratio);
                    }
                    if(yPos >= yLerpBound.y) {
                        float ratio = (yPos - yLerpBound.y) / chunkSO.lerpRange;
                        ratio *= ratio * ratio;
                        noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.yBound.w : chunkSO.yBound.w ,ratio);
                    }

                    if(zPos <= zLerpBound.x) {
                        float ratio = - (zPos - zLerpBound.x) / chunkSO.lerpRange;
                        ratio *= ratio * ratio;
                        noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.zBound.z : chunkSO.zBound.z ,ratio);
                    }

                    if(zPos >= zLerpBound.y) {
                        float ratio = (zPos - zLerpBound.y) / chunkSO.lerpRange;
                        ratio *= ratio * ratio;
                        noise = Mathf.Lerp(noise, chunkSO.invert? 1 - chunkSO.zBound.w : chunkSO.zBound.w ,ratio);
                    }

                    perlinArray[x, y, z] = noise;
                }
            }  
        }   
    }
}
