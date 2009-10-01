#pragma once
void showProgress(int num_relations, int max_relations);

struct relation
{
	uint32 sieve_offset;
	uint32 *fb_offsets;
	uint32 num_factors;
	uint32 poly_index;
	uint32 large_prime1;
	uint32 large_prime2;
};

typedef struct
{
	int relation_count;
	int relation_capacity;
	struct relation *relations;
	char polybuf[256];
} relationYield;