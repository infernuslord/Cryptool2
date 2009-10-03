/**
* This .Net msieve wrapper was written by Sven Rech (rech@cryptool.org)
**/

#include <msieve.h>
extern "C" {
#include "wrapper.h"
#include "../../mpqs/mpqs.h"
}

using namespace System;
using namespace System::Collections;

//From demo.c:
extern "C" msieve_factor* factor(char *number, char *savefile_name);
extern "C" char* getNextFactor(msieve_factor** factor);
extern "C" void stop_msieve(msieve_obj *obj);
extern "C" void get_random_seeds(uint32 *seed1, uint32 *seed2);
//From sieve.c:
extern "C" void collect_relations(sieve_conf_t *conf, uint32 target_relations, qs_core_sieve_fcn core_sieve_fcn);
//From relation.c:
extern "C" void save_relation(sieve_conf_t *conf, uint32 sieve_offset,
		uint32 *fb_offsets, uint32 num_factors, 
		uint32 poly_index, uint32 large_prime1, uint32 large_prime2);


//Copy a sieve configuration that can be used in a different thread:
sieve_conf_t *copy_sieve_conf(sieve_conf_t *conf) {
	sieve_conf_t *copy = (sieve_conf_t*)malloc(sizeof(sieve_conf_t));
	msieve_obj *objcopy = (msieve_obj*)malloc(sizeof(msieve_obj));

	*copy = *conf;
	*objcopy = *(conf->obj);
	copy->obj = objcopy;
	copy->slave = 1;	//we are a slave

	//threads shouldn't be allowed to access files or the factor list:
	objcopy->logfile_name = 0;
	objcopy->savefile.file_handle = 0;
	objcopy->factors = 0;
	
	//threads shouldn't be allowed to access these fields:
	copy->poly_a_list = 0;
	copy->poly_list = 0;	
	copy->relation_list = 0;
	copy->num_relations = 0;
	copy->cycle_list = 0;
	copy->num_cycles = 0;
	copy->cycle_table = 0;
	copy->cycle_hashtable = 0;
	copy->cycle_table_size = 0;
	copy->cycle_table_alloc = 0;
	copy->components = 0;
	copy->vertices = 0;

	//deep copies:
	copy->sieve_array = (uint8 *)aligned_malloc(
				(size_t)copy->sieve_block_size, 64);
	for (uint32 i = 0; i < copy->sieve_block_size; i++)
		copy->sieve_array[i] = conf->sieve_array[i];

	copy->factor_base = (fb_t *)xmalloc(objcopy->fb_size * sizeof(fb_t));
	for (uint32 i = 0; i < objcopy->fb_size; i++)
		copy->factor_base[i] = conf->factor_base[i];

	copy->packed_fb = (packed_fb_t *)xmalloc(conf->tf_large_cutoff * sizeof(packed_fb_t));
	for (uint32 i = 0; i < conf->tf_large_cutoff; i++)
		copy->packed_fb[i] = conf->packed_fb[i];

	copy->buckets = (bucket_t *)xcalloc((size_t)(copy->poly_block *
						copy->num_sieve_blocks), 
						sizeof(bucket_t));
	for (uint32 i = 0; i < copy->poly_block * copy->num_sieve_blocks; i++) {
		copy->buckets[i].num_alloc = 1000;
		copy->buckets[i].list = (bucket_entry_t *)
				xmalloc(1000 * sizeof(bucket_entry_t));
	}

	copy->modsqrt_array = (uint32 *)xmalloc(objcopy->fb_size * sizeof(uint32));
	for (uint32 i = 0; i < objcopy->fb_size; i++)
		copy->modsqrt_array[i] = conf->modsqrt_array[i];

	//we need new seeds:
	uint32 seed1, seed2;
	get_random_seeds(&seed1, &seed2);
	copy->obj->seed1 = seed1;
	copy->obj->seed2 = seed2;

	poly_init(copy, copy->num_sieve_blocks * copy->sieve_block_size / 2);

	return copy;
}

namespace Msieve
{
	public delegate void showProgressDelegate(IntPtr conf, int num_relations, int max_relations);
	public delegate void prepareSievingDelegate(IntPtr conf, int update, IntPtr core_sieve_fcn);

	public ref struct callback_struct
	{
	public:
		showProgressDelegate^ showProgress;
		prepareSievingDelegate^ prepareSieving;
	};

	public ref class msieve 
	{
	private:
		static char* stringToCharA(String^ str)
		{
			if (!str)
				return 0;
			char* ch = (char*)malloc(str->Length + 1);
			for (int i = 0; i < str->Length; i++)
				ch[i] = (char)str[i];
			ch[str->Length] = 0;
			return ch;
		}

	public:
		static callback_struct^ callbacks;

		//initialize msieve with callback functions:
		static void initMsieve(callback_struct^ cb)
		{
			callbacks = cb;
		}

		//factorize the number:
		static ArrayList^ factorize(String^ number, String^ savefile)
		{
			ArrayList^ factor_list = gcnew ArrayList;
			char* num = stringToCharA(number);	
			char* save = stringToCharA(savefile);
			msieve_factor* factors = factor(num, save);

			while (factors != 0)
			{
				char* f = getNextFactor(&factors);
				String^ fa = gcnew String(f);
				free(f);
				factor_list->Add(fa);
			}

			free(num);
			return factor_list;
		}

		//stop msieve:
		static void stop(IntPtr obj)
		{
			stop_msieve((msieve_obj*)obj.ToPointer());
		}

		//clone this conf (the clone can be used to run the sieving in a different thread):
		static IntPtr cloneSieveConf(IntPtr conf)
		{
			return IntPtr(copy_sieve_conf((sieve_conf_t*)conf.ToPointer()));
		}

		//free this conf (shouldn't be the conf file that belongs to the main thread):
		static void freeSieveConf(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			if (!c->slave)
				return;
			free(c->obj);
			free(c->next_poly_action);
			free(c->curr_b);
			free(c->poly_b_small[0]);
			free(c->poly_b_array);
			aligned_free(c->sieve_array);
			free(c->factor_base);
			free(c->packed_fb);
			for (uint32 i = 0; i < c->poly_block * c->num_sieve_blocks; i++)
				free(c->buckets[i].list);
			free(c->buckets);
			free(c->modsqrt_array);
			if (c->yield != 0)
			{
				for (int j = 0; j < c->yield->yield_count; j++)			
					if (c->yield->yield_array[j].type == 0)
						free(c->yield->yield_array->rel.fb_offsets);
				free(c->yield->yield_array);
				free(c->yield);
			}
			free(c);
		}

		static void collectRelations(IntPtr conf, int target_relations, IntPtr core_sieve_fcn)
		{
			collect_relations((sieve_conf_t*)conf.ToPointer(), target_relations, (qs_core_sieve_fcn)core_sieve_fcn.ToPointer());
		}
		
		//get the yield in the thread of "conf" (shoudn't be the main thread):
		static IntPtr getYield(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			if (!c->slave)
				return IntPtr::Zero;
			relationYield* yield = c->yield;
			c->yield = 0;
			return IntPtr((void*)yield);
		}

		//stores the yield in the thread of "conf" (should be the main thread), and destroys the yield:
		static void saveYield(IntPtr conf, IntPtr yield)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			if (c->slave)
				return;			
			relationYield* y = (relationYield*)yield.ToPointer();

			for (int j = 0; j < y->yield_count; j++)
			{
				if (y->yield_array[j].type == 1)
					savefile_write_line(&c->obj->savefile, y->yield_array[j].polybuf);
				else
				{
					relation* rel = &y->yield_array[j].rel;
					save_relation(c, rel->sieve_offset, rel->fb_offsets, rel->num_factors, rel->poly_index, rel->large_prime1, rel->large_prime2);
					free(rel->fb_offsets);
				}
			}

			free(y->yield_array);
			free(y);
		}

		static IntPtr getObjFromConf(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			return IntPtr(c->obj);
		}
	};

}

extern "C" void showProgress(void* conf, int num_relations, int max_relations)
{	
	Msieve::msieve::callbacks->showProgress(IntPtr(conf), num_relations, max_relations);
}

extern "C" void prepare_sieving(void* conf, int update, void* core_sieve_fcn)
{
	Msieve::msieve::callbacks->prepareSieving(IntPtr(conf), update, IntPtr(core_sieve_fcn));
}
