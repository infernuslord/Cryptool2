#pragma once
void showProgress(void* conf, int num_relations, int max_relations);
void prepare_sieving(void* conf, int update, void* core_sieve_fcn);

struct relation
{
	uint32 sieve_offset;
	uint32 *fb_offsets;
	uint32 num_factors;
	uint32 poly_index;
	uint32 large_prime1;
	uint32 large_prime2;
};

struct yield_element
{
	int type;	// 0 = relation; 1 = poly
	struct relation rel;
	char polybuf[256];
};

typedef struct
{
	int yield_count;
	int yield_capacity;
	struct yield_element *yield_array;	
} relationYield;