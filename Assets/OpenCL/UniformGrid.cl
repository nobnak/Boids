
typedef struct decl_GridInfo {
    float worldOrigin[3];
	float worldSize[3];
    float cellSize[3];
    int nGridPartitions;
    int maxIndices;
} GridInfo;

typedef struct decl_FlockInfo {
    float sepRange;
    float sepPower;
    float alnRange;
    float alnPower;
    float cohRange;
    float cohPower;
} FlockInfo;

static int3 calcCellPos(float3 pos, GridInfo info) {
    int3 cellPos = (int3)((pos.x - info.worldOrigin[0]) / info.cellSize[0],
                          (pos.y - info.worldOrigin[1]) / info.cellSize[1],
                          (pos.z - info.worldOrigin[2]) / info.cellSize[2]);
	return cellPos;
}

static int calcCellId(int3 cellPos, GridInfo info) {
    int cellId = cellPos.x + info.nGridPartitions * (cellPos.y + info.nGridPartitions * cellPos.z);
    return cellId;
}



kernel void updateGrid(global float* positions,     global int* pointCounters,
                       global int* pointIndices,    GridInfo grInfo) {
	int id = get_global_id(0);
    int id3 = 3 * id;

    float3 pos = (float3)(positions[id3], positions[id3+1], positions[id3+2]);
    int3 cellPos = calcCellPos(pos, grInfo);
    int cellId = calcCellId(cellPos, grInfo);
    
    int oldCount = atomic_inc(&pointCounters[cellId]);
    pointIndices[grInfo.maxIndices * cellId + oldCount] = id;
}

kernel void updateBoids(global float* positions,   global float* velocities,
                         global int* pointCounters, global int* pointIndices,
                         GridInfo grInfo, FlockInfo frInfo) {
    int id = get_global_id(0);
    int id3 = 3 * id;

	int radius = 2;
    float maxSpeed = 0.05f;
    float maxAccel = 0.001f;

    float3 pos = (float3)(positions[id3], positions[id3+1], positions[id3+2]);
    float3 vel = (float3)(velocities[id3], velocities[id3+1], velocities[id3+2]);
    int3 cellPos = calcCellPos(pos.xyz, grInfo);
    int nGrids = grInfo.nGridPartitions;
	float3 worldOrigin = (float3)(grInfo.worldOrigin[0], grInfo.worldOrigin[1], grInfo.worldOrigin[2]);
	float3 worldSize = (float3)(grInfo.worldSize[0], grInfo.worldSize[1], grInfo.worldSize[2]);

    int sepCounter = 0;
    float3 sepSum = (float3)(0, 0, 0);
    int alnCounter = 0;
    float3 alnSum = (float3)(0, 0, 0);
    int cohCounter = 0;
    float3 cohSum = (float3)(0, 0, 0);
    
	for (int z = -radius; z <= radius; z++) {
		for (int y = -radius; y <= radius; y++) {
			for (int x = -radius; x <= radius; x++) {
                int3 mateCellPos = cellPos + (int3)(x, y, z);
                if (mateCellPos.x < 0 || nGrids <= mateCellPos.x
                    || mateCellPos.y < 0 || nGrids <= mateCellPos.y
                    || mateCellPos.z < 0 || nGrids <= mateCellPos.z) {
                    continue;
                }
                int cellId = calcCellId(mateCellPos, grInfo);
                int nIndices = pointCounters[cellId];
                int indexOffset = grInfo.maxIndices * cellId;
                for (int i = 0; i < nIndices; i++) {
                    int matePosId3 = 3 * pointIndices[indexOffset + i];
                    if (matePosId3 == id3) {
                        continue;
                    }
                    float3 matePos = (float3)(positions[matePosId3], positions[matePosId3+1], positions[matePosId3+2]);
                    float3 mateVel = (float3)(velocities[matePosId3], velocities[matePosId3+1], velocities[matePosId3+2]);
                    float3 toMate = matePos - pos;
                    float sqDist2mate = dot(toMate, toMate);
                    float dist2mate = sqrt(sqDist2mate);
                    
                    if (0 < dist2mate && dist2mate < frInfo.sepRange) {
                        sepCounter++;
                        sepSum += -toMate / sqDist2mate;
                    }
                    if (0 < dist2mate && dist2mate < frInfo.alnRange) {
                        alnCounter++;
                        alnSum += mateVel;
                    }
                    if (0 < dist2mate && dist2mate < frInfo.cohRange) {
                        cohCounter++;
                        cohSum += matePos;
                    }
                }
			}
		}
	}

    float3 accelerator = (float3)(0, 0, 0);
    if (sepCounter > 0) {
        sepSum *= 1.0f / sepCounter;
        float sepLen = length(sepSum);
        if (sepLen > maxAccel) {
            sepSum *= maxAccel / sepLen;
        }
        accelerator += sepSum * frInfo.sepPower;
    }
    if (alnCounter > 0) {
        alnSum *= 1.0f / alnCounter;
        float alnLen = length(alnSum);
        if (alnLen > maxAccel) {
            alnSum *= maxAccel / alnLen;
        }
        accelerator += alnSum * frInfo.alnPower;
    }
    if (cohCounter > 0) {
        cohSum *= 1.0f / cohCounter;
        cohSum -= pos;
        float cohLen = length(cohSum);
        if (cohLen > 0) {
            cohSum *= maxSpeed / cohLen;
        }
        accelerator += cohSum * frInfo.cohPower;
    }
    
    float accLen = length(accelerator);
    if (accLen > maxAccel) {
        accelerator *= maxAccel / accLen;
    }
    vel += accelerator;
    float velLen = length(vel);
    if (velLen > maxSpeed) {
        vel *= maxSpeed / velLen;
    }
	pos += vel;

    velocities[id3]     = vel.x;
    velocities[id3+1]   = vel.y;
    velocities[id3+2]   = vel.z;
    positions[id3]		= pos.x;
    positions[id3+1]    = pos.y;
    positions[id3+2]    = pos.z;
}

kernel void boundary(global float* positions, GridInfo grInfo) {
	int id = get_global_id(0);
	int id3 = 3 * id;

	float3 pos = (float3)(positions[id3], positions[id3+1], positions[id3+2]);
	float3 worldMin = (float3)(grInfo.worldOrigin[0], grInfo.worldOrigin[1], grInfo.worldOrigin[2]);
	float3 worldSize = (float3)(grInfo.worldSize[0], grInfo.worldSize[1], grInfo.worldSize[2]);

	pos = pos - worldSize * floor((pos - worldMin) / worldSize);

	positions[id3]		= pos.x;
	positions[id3+1]	= pos.y;
	positions[id3+2]	= pos.z;
}
