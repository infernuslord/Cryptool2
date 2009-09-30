/*--------------------------------------------------------------------
This source distribution is placed in the public domain by its author,
Jason Papadopoulos. You may use it for any purpose, free of charge,
without having to notify anyone. I disclaim any responsibility for any
errors.

Optionally, please be nice and tell me if you find this source to be
useful. Again optionally, if you add to the functionality present here
please consider making those additions public too, so that others may 
benefit from your work.	

$Id: prime_delta.c 23 2009-07-20 02:59:07Z jasonp_sf $
--------------------------------------------------------------------*/

#include <common.h>

const uint8 prime_delta[PRECOMPUTED_NUM_PRIMES] = {
 2, 1, 2, 2, 4, 2, 4, 2, 4, 6, 2, 6, 4, 2, 4, 6, 6, 2, 6, 4, 2, 6, 4, 6, 8,
 4, 2, 4, 2, 4,14, 4, 6, 2,10, 2, 6, 6, 4, 6, 6, 2,10, 2, 4, 2,12,12, 4, 2,
 4, 6, 2,10, 6, 6, 6, 2, 6, 4, 2,10,14, 4, 2, 4,14, 6,10, 2, 4, 6, 8, 6, 6,
 4, 6, 8, 4, 8,10, 2,10, 2, 6, 4, 6, 8, 4, 2, 4,12, 8, 4, 8, 4, 6,12, 2,18,
 6,10, 6, 6, 2, 6,10, 6, 6, 2, 6, 6, 4, 2,12,10, 2, 4, 6, 6, 2,12, 4, 6, 8,
10, 8,10, 8, 6, 6, 4, 8, 6, 4, 8, 4,14,10,12, 2,10, 2, 4, 2,10,14, 4, 2, 4,
14, 4, 2, 4,20, 4, 8,10, 8, 4, 6, 6,14, 4, 6, 6, 8, 6,12, 4, 6, 2,10, 2, 6,
10, 2,10, 2, 6,18, 4, 2, 4, 6, 6, 8, 6, 6,22, 2,10, 8,10, 6, 6, 8,12, 4, 6,
 6, 2, 6,12,10,18, 2, 4, 6, 2, 6, 4, 2, 4,12, 2, 6,34, 6, 6, 8,18,10,14, 4,
 2, 4, 6, 8, 4, 2, 6,12,10, 2, 4, 2, 4, 6,12,12, 8,12, 6, 4, 6, 8, 4, 8, 4,
14, 4, 6, 2, 4, 6, 2, 6,10,20, 6, 4, 2,24, 4, 2,10,12, 2,10, 8, 6, 6, 6,18,
 6, 4, 2,12,10,12, 8,16,14, 6, 4, 2, 4, 2,10,12, 6, 6,18, 2,16, 2,22, 6, 8,
 6, 4, 2, 4, 8, 6,10, 2,10,14,10, 6,12, 2, 4, 2,10,12, 2,16, 2, 6, 4, 2,10,
 8,18,24, 4, 6, 8,16, 2, 4, 8,16, 2, 4, 8, 6, 6, 4,12, 2,22, 6, 2, 6, 4, 6,
14, 6, 4, 2, 6, 4, 6,12, 6, 6,14, 4, 6,12, 8, 6, 4,26,18,10, 8, 4, 6, 2, 6,
22,12, 2,16, 8, 4,12,14,10, 2, 4, 8, 6, 6, 4, 2, 4, 6, 8, 4, 2, 6,10, 2,10,
 8, 4,14,10,12, 2, 6, 4, 2,16,14, 4, 6, 8, 6, 4,18, 8,10, 6, 6, 8,10,12,14,
 4, 6, 6, 2,28, 2,10, 8, 4,14, 4, 8,12, 6,12, 4, 6,20,10, 2,16,26, 4, 2,12,
 6, 4,12, 6, 8, 4, 8,22, 2, 4, 2,12,28, 2, 6, 6, 6, 4, 6, 2,12, 4,12, 2,10,
 2,16, 2,16, 6,20,16, 8, 4, 2, 4, 2,22, 8,12, 6,10, 2, 4, 6, 2, 6,10, 2,12,
10, 2,10,14, 6, 4, 6, 8, 6, 6,16,12, 2, 4,14, 6, 4, 8,10, 8, 6, 6,22, 6, 2,
10,14, 4, 6,18, 2,10,14, 4, 2,10,14, 4, 8,18, 4, 6, 2, 4, 6, 2,12, 4,20,22,
12, 2, 4, 6, 6, 2, 6,22, 2, 6,16, 6,12, 2, 6,12,16, 2, 4, 6,14, 4, 2,18,24,
10, 6, 2,10, 2,10, 2,10, 6, 2,10, 2,10, 6, 8,30,10, 2,10, 8, 6,10,18, 6,12,
12, 2,18, 6, 4, 6, 6,18, 2,10,14, 6, 4, 2, 4,24, 2,12, 6,16, 8, 6, 6,18,16,
 2, 4, 6, 2, 6, 6,10, 6,12,12,18, 2, 6, 4,18, 8,24, 4, 2, 4, 6, 2,12, 4,14,
30,10, 6,12,14, 6,10,12, 2, 4, 6, 8, 6,10, 2, 4,14, 6, 6, 4, 6, 2,10, 2,16,
12, 8,18, 4, 6,12, 2, 6, 6, 6,28, 6,14, 4, 8,10, 8,12,18, 4, 2, 4,24,12, 6,
 2,16, 6, 6,14,10,14, 4,30, 6, 6, 6, 8, 6, 4, 2,12, 6, 4, 2, 6,22, 6, 2, 4,
18, 2, 4,12, 2, 6, 4,26, 6, 6, 4, 8,10,32,16, 2, 6, 4, 2, 4, 2,10,14, 6, 4,
 8,10, 6,20, 4, 2, 6,30, 4, 8,10, 6, 6, 8, 6,12, 4, 6, 2, 6, 4, 6, 2,10, 2,
16, 6,20, 4,12,14,28, 6,20, 4,18, 8, 6, 4, 6,14, 6, 6,10, 2,10,12, 8,10, 2,
10, 8,12,10,24, 2, 4, 8, 6, 4, 8,18,10, 6, 6, 2, 6,10,12, 2,10, 6, 6, 6, 8,
 6,10, 6, 2, 6, 6, 6,10, 8,24, 6,22, 2,18, 4, 8,10,30, 8,18, 4, 2,10, 6, 2,
 6, 4,18, 8,12,18,16, 6, 2,12, 6,10, 2,10, 2, 6,10,14, 4,24, 2,16, 2,10, 2,
10,20, 4, 2, 4, 8,16, 6, 6, 2,12,16, 8, 4, 6,30, 2,10, 2, 6, 4, 6, 6, 8, 6,
 4,12, 6, 8,12, 4,14,12,10,24, 6,12, 6, 2,22, 8,18,10, 6,14, 4, 2, 6,10, 8,
 6, 4, 6,30,14,10, 2,12,10, 2,16, 2,18,24,18, 6,16,18, 6, 2,18, 4, 6, 2,10,
 8,10, 6, 6, 8, 4, 6, 2,10, 2,12, 4, 6, 6, 2,12, 4,14,18, 4, 6,20, 4, 8, 6,
 4, 8, 4,14, 6, 4,14,12, 4, 2,30, 4,24, 6, 6,12,12,14, 6, 4, 2, 4,18, 6,12,
 8, 6, 4,12, 2,12,30,16, 2, 6,22,14, 6,10,12, 6, 2, 4, 8,10, 6, 6,24,14, 6,
 4, 8,12,18,10, 2,10, 2, 4, 6,20, 6, 4,14, 4, 2, 4,14, 6,12,24,10, 6, 8,10,
 2,30, 4, 6, 2,12, 4,14, 6,34,12, 8, 6,10, 2, 4,20,10, 8,16, 2,10,14, 4, 2,
12, 6,16, 6, 8, 4, 8, 4, 6, 8, 6, 6,12, 6, 4, 6, 6, 8,18, 4,20, 4,12, 2,10,
 6, 2,10,12, 2, 4,20, 6,30, 6, 4, 8,10,12, 6, 2,28, 2, 6, 4, 2,16,12, 2, 6,
10, 8,24,12, 6,18, 6, 4,14, 6, 4,12, 8, 6,12, 4, 6,12, 6,12, 2,16,20, 4, 2,
10,18, 8, 4,14, 4, 2, 6,22, 6,14, 6, 6,10, 6, 2,10, 2, 4, 2,22, 2, 4, 6, 6,
12, 6,14,10,12, 6, 8, 4,36,14,12, 6, 4, 6, 2,12, 6,12,16, 2,10, 8,22, 2,12,
 6, 4, 6,18, 2,12, 6, 4,12, 8, 6,12, 4, 6,12, 6, 2,12,12, 4,14, 6,16, 6, 2,
10, 8,18, 6,34, 2,28, 2,22, 6, 2,10,12, 2, 6, 4, 8,22, 6, 2,10, 8, 4, 6, 8,
 4,12,18,12,20, 4, 6, 6, 8, 4, 2,16,12, 2,10, 8,10, 2, 4, 6,14,12,22, 8,28,
 2, 4,20, 4, 2, 4,14,10,12, 2,12,16, 2,28, 8,22, 8, 4, 6, 6,14, 4, 8,12, 6,
 6, 4,20, 4,18, 2,12, 6, 4, 6,14,18,10, 8,10,32, 6,10, 6, 6, 2, 6,16, 6, 2,
12, 6,28, 2,10, 8,16, 6, 8, 6,10,24,20,10, 2,10, 2,12, 4, 6,20, 4, 2,12,18,
10, 2,10, 2, 4,20,16,26, 4, 8, 6, 4,12, 6, 8,12,12, 6, 4, 8,22, 2,16,14,10,
 6,12,12,14, 6, 4,20, 4,12, 6, 2, 6, 6,16, 8,22, 2,28, 8, 6, 4,20, 4,12,24,
20, 4, 8,10, 2,16, 2,12,12,34, 2, 4, 6,12, 6, 6, 8, 6, 4, 2, 6,24, 4,20,10,
 6, 6,14, 4, 6, 6, 2,12, 6,10, 2,10, 6,20, 4,26, 4, 2, 6,22, 2,24, 4, 6, 2,
 4, 6,24, 6, 8, 4, 2,34, 6, 8,16,12, 2,10, 2,10, 6, 8, 4, 8,12,22, 6,14, 4,
26, 4, 2,12,10, 8, 4, 8,12, 4,14, 6,16, 6, 8, 4, 6, 6, 8, 6,10,12, 2, 6, 6,
16, 8, 6, 6,12,10, 2, 6,18, 4, 6, 6, 6,12,18, 8, 6,10, 8,18, 4,14, 6,18,10,
 8,10,12, 2, 6,12,12,36, 4, 6, 8, 4, 6, 2, 4,18,12, 6, 8, 6, 6, 4,18, 2, 4,
 2,24, 4, 6, 6,14,30, 6, 4, 6,12, 6,20, 4, 8, 4, 8, 6, 6, 4,30, 2,10,12, 8,
10, 8,24, 6,12, 4,14, 4, 6, 2,28,14,16, 2,12, 6, 4,20,10, 6, 6, 6, 8,10,12,
14,10,14,16,14,10,14, 6,16, 6, 8, 6,16,20,10, 2, 6, 4, 2, 4,12, 2,10, 2, 6,
22, 6, 2, 4,18, 8,10, 8,22, 2,10,18,14, 4, 2, 4,18, 2, 4, 6, 8,10, 2,30, 4,
30, 2,10, 2,18, 4,18, 6,14,10, 2, 4,20,36, 6, 4, 6,14, 4,20,10,14,22, 6, 2,
30,12,10,18, 2, 4,14, 6,22,18, 2,12, 6, 4, 8, 4, 8, 6,10, 2,12,18,10,14,16,
14, 4, 6, 6, 2, 6, 4, 2,28, 2,28, 6, 2, 4, 6,14, 4,12,14,16,14, 4, 6, 8, 6,
 4, 6, 6, 6, 8, 4, 8, 4,14,16, 8, 6, 4,12, 8,16, 2,10, 8, 4, 6,26, 6,10, 8,
 4, 6,12,14,30, 4,14,22, 8,12, 4, 6, 8,10, 6,14,10, 6, 2,10,12,12,14, 6, 6,
18,10, 6, 8,18, 4, 6, 2, 6,10, 2,10, 8, 6, 6,10, 2,18,10, 2,12, 4, 6, 8,10,
12,14,12, 4, 8,10, 6, 6,20, 4,14,16,14,10, 8,10,12, 2,18, 6,12,10,12, 2, 4,
 2,12, 6, 4, 8, 4,44, 4, 2, 4, 2,10,12, 6, 6,14, 4, 6, 6, 6, 8, 6,36,18, 4,
 6, 2,12, 6, 6, 6, 4,14,22,12, 2,18,10, 6,26,24, 4, 2, 4, 2, 4,14, 4, 6, 6,
 8,16,12, 2,42, 4, 2, 4,24, 6, 6, 2,18, 4,14, 6,28,18,14, 6,10,12, 2, 6,12,
30, 6, 4, 6, 6,14, 4, 2,24, 4, 6, 6,26,10,18, 6, 8, 6, 6,30, 4,12,12, 2,16,
 2, 6, 4,12,18, 2, 6, 4,26,12, 6,12, 4,24,24,12, 6, 2,12,28, 8, 4, 6,12, 2,
18, 6, 4, 6, 6,20,16, 2, 6, 6,18,10, 6, 2, 4, 8, 6, 6,24,16, 6, 8,10, 6,14,
22, 8,16, 6, 2,12, 4, 2,22, 8,18,34, 2, 6,18, 4, 6, 6, 8,10, 8,18, 6, 4, 2,
 4, 8,16, 2,12,12, 6,18, 4, 6, 6, 6, 2, 6,12,10,20,12,18, 4, 6, 2,16, 2,10,
14, 4,30, 2,10,12, 2,24, 6,16, 8,10, 2,12,22, 6, 2,16,20,10, 2,12,12,18,10,
12, 6, 2,10, 2, 6,10,18, 2,12, 6, 4, 6, 2,24,28, 2, 4, 2,10, 2,16,12, 8,22,
 2, 6, 4, 2,10, 6,20,12,10, 8,12, 6, 6, 6, 4,18, 2, 4,12,18, 2,12, 6, 4, 2,
16,12,12,14, 4, 8,18, 4,12,14, 6, 6, 4, 8, 6, 4,20,12,10,14, 4, 2,16, 2,12,
30, 4, 6,24,20,24,10, 8,12,10,12, 6,12,12, 6, 8,16,14, 6, 4, 6,36,20,10,30,
12, 2, 4, 2,28,12,14, 6,22, 8, 4,18, 6,14,18, 4, 6, 2, 6,34,18, 2,16, 6,18,
 2,24, 4, 2, 6,12, 6,12,10, 8, 6,16,12, 8,10,14,40, 6, 2, 6, 4,12,14, 4, 2,
 4, 2, 4, 8, 6,10, 6, 6, 2, 6, 6, 6,12, 6,24,10, 2,10, 6,12, 6, 6,14, 6, 6,
52,20, 6,10, 2,10, 8,10,12,12, 2, 6, 4,14,16, 8,12, 6,22, 2,10, 8, 6,22, 2,
22, 6, 8,10,12,12, 2,10, 6,12, 2, 4,14,10, 2, 6,18, 4,12, 8,18,12, 6, 6, 4,
 6, 6,14, 4, 2,12,12, 4, 6,18,18,12, 2,16,12, 8,18,10,26, 4, 6, 8, 6, 6, 4,
 2,10,20, 4, 6, 8, 4,20,10, 2,34, 2, 4,24, 2,12,12,10, 6, 2,12,30, 6,12,16,
12, 2,22,18,12,14,10, 2,12,12, 4, 2, 4, 6,12, 2,16,18, 2,40, 8,16, 6, 8,10,
 2, 4,18, 8,10, 8,12, 4,18, 2,18,10, 2, 4, 2, 4, 8,28, 2, 6,22,12, 6,14,18,
 4, 6, 8, 6, 6,10, 8, 4, 2,18,10, 6,20,22, 8, 6,30, 4, 2, 4,18, 6,30, 2, 4,
 8, 6, 4, 6,12,14,34,14, 6, 4, 2, 6, 4,14, 4, 2, 6,28, 2, 4, 6, 8,10, 2,10,
 2,10, 2, 4,30, 2,12,12,10,18,12,14,10, 2,12, 6,10, 6,14,12, 4,14, 4,18, 2,
10, 8, 4, 8,10,12,18,18, 8, 6,18,16,14, 6, 6,10,14, 4, 6, 2,12,12, 4, 6, 6,
12, 2,16, 2,12, 6, 4,14, 6, 4, 2,12,18, 4,36,18,12,12, 2, 4, 2, 4, 8,12, 4,
36, 6,18, 2,12,10, 6,12,24, 8, 6, 6,16,12, 2,18,10,20,10, 2, 6,18, 4, 2,40,
 6, 2,16, 2, 4, 8,18,10,12, 6, 2,10, 8, 4, 6,12, 2,10,18, 8, 6, 4,20, 4, 6,
36, 6, 2,10, 6,24, 6,14,16, 6,18, 2,10,20,10, 8, 6, 4, 6, 2,10, 2,12, 4, 2,
 4, 8,10, 6,12,18,14,12,16, 8, 6,16, 8, 4, 2, 6,18,24,18,10,12, 2, 4,14,10,
 6, 6, 6,18,12, 2,28,18,14,16,12,14,24,12,22, 6, 2,10, 8, 4, 2, 4,14,12, 6,
 4, 6,14, 4, 2, 4,30, 6, 2, 6,10, 2,30,22, 2, 4, 6, 8, 6, 6,16,12,12, 6, 8,
 4, 2,24,12, 4, 6, 8, 6, 6,10, 2, 6,12,28,14, 6, 4,12, 8, 6,12, 4, 6,14, 6,
12,10, 6, 6, 8, 6, 6, 4, 2, 4, 8,12, 4,14,18,10, 2,16, 6,20, 6,10, 8, 4,30,
36,12, 8,22,12, 2, 6,12,16, 6, 6, 2,18, 4,26, 4, 8,18,10, 8,10, 6,14, 4,20,
22,18,12, 8,28,12, 6, 6, 8, 6,12,24,16,14, 4,14,12, 6,10,12,20, 6, 4, 8,18,
12,18,10, 2, 4,20,10,14, 4, 6, 2,10,24,18, 2, 4,20,16,14,10,14, 6, 4, 6,20,
 6,10, 6, 2,12, 6,30,10, 8, 6, 4, 6, 8,40, 2, 4, 2,12,18, 4, 6, 8,10, 6,18,
18, 2,12,16, 8, 6, 4, 6, 6, 2,52,14, 4,20,16, 2, 4, 6,12, 2, 6,12,12, 6, 4,
14,10, 6, 6,14,10,14,16, 8, 6,12, 4, 8,22, 6, 2,18,22, 6, 2,18, 6,16,14,10,
 6,12, 2, 6, 4, 8,18,12,16, 2, 4,14, 4, 8,12,12,30,16, 8, 4, 2, 6,22,12, 8,
10, 6, 6, 6,14, 6,18,10,12, 2,10, 2, 4,26, 4,12, 8, 4,18, 8,10,14,16, 6, 6,
 8,10, 6, 8, 6,12,10,20,10, 8, 4,12,26,18, 4,12,18, 6,30, 6, 8, 6,22,12, 2,
 4, 6, 6, 2,10, 2, 4, 6, 6, 2, 6,22,18, 6,18,12, 8,12, 6,10,12, 2,16, 2,10,
 2,10,18, 6,20, 4, 2, 6,22, 6, 6,18, 6,14,12,16, 2, 6, 6, 4,14,12, 4, 2,18,
16,36,12, 6,14,28, 2,12, 6,12, 6, 4, 2,16,30, 8,24, 6,30,10, 2,18, 4, 6,12,
 8,22, 2, 6,22,18, 2,10, 2,10,30, 2,28, 6,14,16, 6,20,16, 2, 6, 4,32, 4, 2,
 4, 6, 2,12, 4, 6, 6,12, 2, 6, 4, 6, 8, 6, 4,20, 4,32,10, 8,16, 2,22, 2, 4,
 6, 8, 6,16,14, 4,18, 8, 4,20, 6,12,12, 6,10, 2,10, 2,12,28,12,18, 2,18,10,
 8,10,48, 2, 4, 6, 8,10, 2,10,30, 2,36, 6,10, 6, 2,18, 4, 6, 8,16,14,16, 6,
14, 4,20, 4, 6, 2,10,12, 2, 6,12, 6, 6, 4,12, 2, 6, 4,12, 6, 8, 4, 2, 6,18,
10, 6, 8,12, 6,22, 2, 6,12,18, 4,14, 6, 4,20, 6,16, 8, 4, 8,22, 8,12, 6, 6,
16,12,18,30, 8, 4, 2, 4, 6,26, 4,14,24,22, 6, 2, 6,10, 6,14, 6, 6,12,10, 6,
 2,12,10,12, 8,18,18,10, 6, 8,16, 6, 6, 8,16,20, 4, 2,10, 2,10,12, 6, 8, 6,
10,20,10,18,26, 4, 6,30, 2, 4, 8, 6,12,12,18, 4, 8,22, 6, 2,12,34, 6,18,12,
 6, 2,28,14,16,14, 4,14,12, 4, 6, 6, 2,36, 4, 6,20,12,24, 6,22, 2,16,18,12,
12,18, 2, 6, 6, 6, 4, 6,14, 4, 2,22, 8,12, 6,10, 6, 8,12,18,12, 6,10, 2,22,
14, 6, 6, 4,18, 6,20,22, 2,12,24, 4,18,18, 2,22, 2, 4,12, 8,12,10,14, 4, 2,
18,16,38, 6, 6, 6,12,10, 6,12, 8, 6, 4, 6,14,30, 6,10, 8,22, 6, 8,12,10, 2,
10, 2, 6,10, 2,10,12,18,20, 6, 4, 8,22, 6, 6,30, 6,14, 6,12,12, 6,10, 2,10,
30, 2,16, 8, 4, 2, 6,18, 4, 2, 6, 4,26, 4, 8, 6,10, 2, 4, 6, 8, 4, 6,30,12,
 2, 6, 6, 4,20,22, 8, 4, 2, 4,72, 8, 4, 8,22, 2, 4,14,10, 2, 4,20, 6,10,18,
 6,20,16, 6, 8, 6, 4,20,12,22, 2, 4, 2,12,10,18, 2,22, 6,18,30, 2,10,14,10,
 8,16,50, 6,10, 8,10,12, 6,18, 2,22, 6, 2, 4, 6, 8, 6, 6,10,18, 2,22, 2,16,
14,10, 6, 2,12,10,20, 4,14, 6, 4,36, 2, 4, 6,12, 2, 4,14,12, 6, 4, 6, 2, 6,
 4,20,10, 2,10, 6,12, 2,24,12,12, 6, 6, 4,24, 2, 4,24, 2, 6, 4, 6, 8,16, 6,
 2,10,12,14, 6,34, 6,14, 6, 4, 2,30,22, 8, 4, 6, 8, 4, 2,28, 2, 6, 4,26,18,
22, 2, 6,16, 6, 2,16,12, 2,12, 4, 6, 6,14,10, 6, 8,12, 4,18, 2,10, 8,16, 6,
 6,30, 2,10,18, 2,10, 8, 4, 8,12,24,40, 2,12,10, 6,12, 2,12, 4, 2, 4, 6,18,
14,12, 6, 4,14,30, 4, 8,10, 8, 6,10,18, 8, 4,14,16, 6, 8, 4, 6, 2,10, 2,12,
 4, 2, 4, 6, 8, 4, 6,32,24,10, 8,18,10, 2, 6,10, 2, 4,18, 6,12, 2,16, 2,22,
 6, 6, 8,18, 4,18,12, 8, 6, 4,20, 6,30,22,12, 2, 6,18, 4,62, 4, 2,12, 6,10,
 2,12,12,28, 2, 4,14,22, 6, 2, 6, 6,10,14, 4, 2,10, 6, 8,10,14,10, 6, 2,12,
22,18, 8,10,18,12, 2,12, 4,12, 2,10, 2, 6,18, 6, 6,34, 6, 2,12, 4, 6,18,18,
 2,16, 6, 6, 8, 6,10,18, 8,10, 8,10, 2, 4,18,26,12,22, 2, 4, 2,22, 6, 6,14,
16, 6,20,10,12, 2,18,42, 4,24, 2, 6,10,12, 2, 6,10, 8, 4, 6,12,12, 8, 4, 6,
12,30,20, 6,24, 6,10,12, 2,10,20, 6, 6, 4,12,14,10,18,12, 8, 6,12, 4,14,10,
 2,12,30,16, 2,12, 6, 4, 2, 4, 6,26, 4,18, 2, 4, 6,14,54, 6,52, 2,16, 6, 6,
12,26, 4, 2, 6,22, 6, 2,12,12, 6,10,18, 2,12,12,10,18,12, 6, 8, 6,10, 6, 8,
 4, 2, 4,20,24, 6, 6,10,14,10, 2,22, 6,14,10,26, 4,18, 8,12,12,10,12, 6, 8,
16, 6, 8, 6, 6,22, 2,10,20,10, 6,44,18, 6,10, 2, 4, 6,14, 4,26, 4, 2,12,10,
 8, 4, 8,12, 4,12, 8,22, 8, 6,10,18, 6, 6, 8, 6,12, 4, 8,18,10,12, 6,12, 2,
 6, 4, 2,16,12,12,14,10,14, 6,10,12, 2,12, 6, 4, 6, 2,12, 4,26, 6,18, 6,10,
 6, 2,18,10, 8, 4,26,10,20, 6,16,20,12,10, 8,10, 2,16, 6,20,10,20, 4,30, 2,
 4, 8,16, 2,18, 4, 2, 6,10,18,12,14,18, 6,16,20, 6, 4, 8, 6, 4, 6,12, 8,10,
 2,12, 6, 4, 2, 6,10, 2,16,12,14,10, 6, 8, 6,28, 2, 6,18,30,34, 2,16,12, 2,
18,16, 6, 8,10, 8,10, 8,10,44, 6, 6, 4,20, 4, 2, 4,14,28, 8, 6,16,14,30, 6,
30, 4,14,10, 6, 6, 8, 4,18,12, 6, 2,22,12, 8, 6,12, 4,14, 4, 6, 2, 4,18,20,
 6,16,38,16, 2, 4, 6, 2,40,42,14, 4, 6, 2,24,10, 6, 2,18,10,12, 2,16, 2, 6,
16, 6, 8, 4, 2,10, 6, 8,10, 2,18,16, 8,12,18,12, 6,12,10, 6, 6,18,12,14, 4,
 2,10,20, 6,12, 6,16,26, 4,18, 2, 4,32,10, 8, 6, 4, 6, 6,14, 6,18, 4, 2,18,
10, 8,10, 8,10, 2, 4, 6, 2,10,42, 8,12, 4, 6,18, 2,16, 8, 4, 2,10,14,12,10,
20, 4, 8,10,38, 4, 6, 2,10,20,10,12, 6,12,26,12, 4, 8,28, 8, 4, 8,24, 6,10,
 8, 6,16,12, 8,10,12, 8,22, 6, 2,10, 2, 6,10, 6, 6, 8, 6, 4,14,28, 8,16,18,
 8, 4, 6,20, 4,18, 6, 2,24,24, 6, 6,12,12, 4, 2,22, 2,10, 6, 8,12, 4,20,18,
 6, 4,12,24, 6, 6,54, 8, 6, 4,26,36, 4, 2, 4,26,12,12, 4, 6, 6, 8,12,10, 2,
12,16,18, 6, 8, 6,12,18,10, 2,54, 4, 2,10,30,12, 8, 4, 8,16,14,12, 6, 4, 6,
12, 6, 2, 4,14,12, 4,14, 6,24, 6, 6,10,12,12,20,18, 6, 6,16, 8, 4, 6,20, 4,
32, 4,14,10, 2, 6,12,16, 2, 4, 6,12, 2,10, 8, 6, 4, 2,10,14, 6, 6,12,18,34,
 8,10, 6,24, 6, 2,10,12, 2,30,10,14,12,12,16, 6, 6, 2,18, 4, 6,30,14, 4, 6,
 6, 2, 6, 4, 6,14, 6, 4, 8,10,12, 6,32,10, 8,22, 2,10, 6,24, 8, 4,30, 6, 2,
12,16, 8, 6, 4, 6, 8,16,14, 6, 6, 4, 2,10,12, 2,16,14, 4, 2, 4,20,18,10, 2,
10, 6,12,30, 8,18,12,10, 2, 6, 6, 4,12,12, 2, 4,12,18,24, 2,10, 6, 8,16, 8,
 6,12,10,14, 6,12, 6, 6, 4, 2,24, 4, 6, 8, 6, 4, 2, 4, 6,14, 4, 8,10,24,24,
12, 2, 6,12,22,30, 2, 6,18,10, 6, 6, 8, 4, 2, 6,10, 8,10, 6, 8,16, 6,14, 6,
 4,24, 8,10, 2,12, 6, 4,36, 2,22, 6, 8, 6,10, 8, 6,12,10,14,10, 6,18,12, 2,
12, 4,26,10,14,16,18, 8,18,12,12, 6,16,14,24,10,12, 8,22, 6, 2,10,60, 6, 2,
 4, 8,16,14,10, 6,24, 6,12,18,24, 2,30, 4, 2,12, 6,10, 2, 4,14, 6,16, 2,10,
 8,22,20, 6, 4,32, 6,18, 4, 2, 4, 2, 4, 8,52,14,22, 2,22,20,10, 8,10, 2, 6,
 4,14, 4, 6,20, 4, 6, 2,12,12, 6,12,16, 2,12,10, 8, 4, 6, 2,28,12, 8,10,12,
 2, 4,14,28, 8, 6, 4, 2, 4, 6, 2,12,58, 6,14,10, 2, 6,28,32, 4,30, 8, 6, 4,
 6,12,12, 2, 4, 6, 6,14,16, 8,30, 4, 2,10, 8, 6, 4, 6,26, 4,12, 2,10,18,12,
12,18, 2, 4,12, 8,12,10,20, 4, 8,16,12, 8, 6,16, 8,10,12,14, 6, 4, 8,12, 4,
20, 6,40, 8,16, 6,36, 2, 6, 4, 6, 2,22,18, 2,10, 6,36,14,12, 4,18, 8, 4,14,
10, 2,10, 8, 4, 2,18,16,12,14,10,14, 6, 6,42,10, 6, 6,20,10, 8,12, 4,12,18,
 2,10,14,18,10,18, 8, 6, 4,14, 6,10,30,14, 6, 6, 4,12,38, 4, 2, 4, 6, 8,12,
10, 6,18, 6,50, 6, 4, 6,12, 8,10,32, 6,22, 2,10,12,18, 2, 6, 4,30, 8, 6, 6,
18,10, 2, 4,12,20,10, 8,24,10, 2, 6,22, 6, 2,18,10,12, 2,30,18,12,28, 2, 6,
 4, 6,14, 6,12,10, 8, 4,12,26,10, 8, 6,16, 2,10,18,14, 6, 4, 6,14,16, 2, 6,
 4,12,20, 4,20, 4, 6,12, 2,36, 4, 6, 2,10, 2,22, 8, 6,10,12,12,18,14,24,36,
 4,20,24,10, 6, 2,28, 6,18, 8, 4, 6, 8, 6, 4, 2,12,28,18,14,16,14,18,10, 8,
 6, 4, 6, 6, 8,22,12, 2,10,18, 6, 2,18,10, 2,12,10,18,32, 6, 4, 6, 6, 8, 6,
 6,10,20, 6,12,10, 8,10,14, 6,10,14, 4, 2,22,18, 2,10, 2, 4,20, 4, 2,34, 2,
12, 6,10, 2,10,18, 6,14,12,12,22, 8, 6,16, 6, 8, 4,12, 6, 8, 4,36, 6, 6,20,
24, 6,12,18,10, 2,10,26, 6,16, 8, 6, 4,24,18, 8,12,12,10,18,12, 2,24, 4,12,
18,12,14,10, 2, 4,24,12,14,10, 6, 2, 6, 4, 6,26, 4, 6, 6, 2,22, 8,18, 4,18,
 8, 4,24, 2,12,12, 4, 2,52, 2,18, 6, 4, 6,12, 2, 6,12,10, 8, 4, 2,24,10, 2,
10, 2,12, 6,18,40, 6,20,16, 2,12, 6,10,12, 2, 4, 6,14,12,12,22, 6, 8, 4, 2,
16,18,12, 2, 6,16, 6, 2, 6, 4,12,30, 8,16, 2,18,10,24, 2, 6,24, 4, 2,22, 2,
16, 2, 6,12, 4,18, 8, 4,14, 4,18,24, 6, 2, 6,10, 2,10,38, 6,10,14, 6, 6,24,
 4, 2,12,16,14,16,12, 2, 6,10,26, 4, 2,12, 6, 4,12, 8,12,10,18, 6,14,28, 2,
 6,10, 2, 4,14,34, 2, 6,22, 2,10,14, 4, 2,16, 8,10, 6, 8,10, 8, 4, 6, 2,16,
 6, 6,18,30,14, 6, 4,30, 2,10,14, 4,20,10, 8, 4, 8,18, 4,14, 6, 4,24, 6, 6,
18,18, 2,36, 6,10,14,12, 4, 6, 2,30, 6, 4, 2, 6,28,20, 4,20,12,24,16,18,12,
14, 6, 4,12,32,12, 6,10, 8,10, 6,18, 2,16,14, 6,22, 6,12, 2,18, 4, 8,30,12,
 4,12, 2,10,38,22, 2, 4,14, 6,12,24, 4, 2, 4,14,12,10, 2,16, 6,20, 4,20,22,
12, 2, 4, 2,12,22,24, 6, 6, 2, 6, 4, 6, 2,10,12,12, 6, 2, 6,16, 8, 6, 4,18,
12,12,14, 4,12, 6, 8, 6,18, 6,10,12,14, 6, 4, 8,22, 6, 2,28,18, 2,18,10, 6,
14,10, 2,10,14, 6,10, 2,22, 6, 8, 6,16,12, 8,22, 2, 4,14,18,12, 6,24, 6,10,
 2,12,22,18, 6,20, 6,10,14, 4, 2, 6,12,22,14,12, 4, 6, 8,22, 2,10,12, 8,40,
 2, 6,10, 8, 4,42,20, 4,32,12,10, 6,12,12, 2,10, 8, 6, 4, 8, 4,26,18, 4, 8,
28, 6,18, 6,12, 2,10, 6, 6,14,10,12,14,24, 6, 4,20,22, 2,18, 4, 6,12, 2,16,
18,14, 6, 6, 4, 6, 8,18, 4,14,30, 4,18, 8,10, 2, 4, 8,12, 4,12,18, 2,12,10,
 2,16, 8, 4,30, 2, 6,28, 2,10, 2,18,10,14, 4,26, 6,18, 4,20, 6, 4, 8,18, 4,
12,26,24, 4,20,22, 2,18,22, 2, 4,12, 2, 6, 6, 6, 4, 6,14, 4,24,12, 6,18, 2,
12,28,14, 4, 6, 8,22, 6,12,18, 8, 4,20, 6, 4, 6, 2,18, 6, 4,12,12, 8,28, 6,
 8,10, 2,24,12,10,24, 8,10,20,12, 6,12,12, 4,14,12,24,34,18, 8,10, 6,18, 8,
 4, 8,16,14, 6, 4, 6,24, 2, 6, 4, 6, 2,16, 6, 6,20,24, 4, 2, 4,14, 4,18, 2,
 6,12, 4,14, 4, 2,18,16, 6, 6, 2,16,20, 6, 6,30, 4, 8, 6,24,16, 6, 6, 8,12,
30, 4,18,18, 8, 4,26,10, 2,22, 8,10,14, 6, 4,18, 8,12,28, 2, 6, 4,12, 6,24,
 6, 8,10,20,16, 8,30, 6, 6, 4, 2,10,14, 6,10,32,22,18, 2, 4, 2, 4, 8,22, 8,
18,12,28, 2,16,12,18,14,10,18,12, 6,32,10,14, 6,10, 2,10, 2, 6,22, 2, 4, 6,
 8,10, 6,14, 6, 4,12,30,24, 6, 6, 8, 6, 4, 2, 4, 6, 8, 6, 6,22,18, 8, 4, 2,
18, 6, 4, 2,16,18,20,10, 6, 6,30, 2,12,28, 6, 6, 6, 2,12,10, 8,18,18, 4, 8,
18,10, 2,28, 2,10,14, 4, 2,30,12,22,26,10, 8, 6,10, 8,16,14, 6, 6,10,14, 6,
 4, 2,10,12, 2, 6,10, 8, 4, 2,10,26,22, 6, 2,12,18, 4,26, 4, 8,10, 6,14,10,
 2,18, 6,10,20, 6, 6, 4,24, 2, 4, 8, 6,16,14,16,18, 2, 4,12, 2,10, 2, 6,12,
10, 6, 6,20, 6, 4, 6,38, 4, 6,12,14, 4,12, 8,10,12,12, 8, 4, 6,14,10, 6,12,
 2,10,18, 2,18,10, 8,10, 2,12, 4,14,28, 2,16, 2,18, 6,10, 6, 8,16,14,30,10,
20, 6,10,24, 2,28, 2,12,16, 6, 8,36, 4, 8, 4,14,12,10, 8,12, 4, 6, 8, 4, 6,
14,22, 8, 6, 4, 2,10, 6,20,10, 8, 6, 6,22,18, 2,16, 6,20, 4,26, 4,14,22,14,
 4,12, 6, 8, 4, 6, 6,26,10, 2,18,18, 4, 2,16, 2,18, 4, 6, 8, 4, 6,12, 2, 6,
 6,28,38, 4, 8,16,26, 4, 2,10,12, 2,10, 8, 6,10,12, 2,10, 2,24, 4,30,26, 6,
 6,18, 6, 6,22, 2,10,18,26, 4,18, 8, 6, 6,12,16, 6, 8,16, 6, 8,16, 2,42,58,
 8, 4, 6, 2, 4, 8,16, 6,20, 4,12,12, 6,12, 2,10, 2, 6,22, 2,10, 6, 8, 6,10,
14, 6, 6, 4,18, 8,10, 8,16,14,10, 2,10, 2,12, 6, 4,20,10, 8,52, 8,10, 6, 2,
10, 8,10, 6, 6, 8,10, 2,22, 2, 4, 6,14, 4, 2,24,12, 4,26,18, 4, 6,14,30, 6,
 4, 6, 2,22, 8, 4, 6, 2,22, 6, 8,16, 6,14, 4, 6,18, 8,12, 6,12,24,30,16, 8,
34, 8,22, 6,14,10,18,14, 4,12, 8, 4,36, 6, 6, 2,10, 2, 4,20, 6, 6,10,12, 6,
 2,40, 8, 6,28, 6, 2,12,18, 4,24,14, 6, 6,10,20,10,14,16,14,16, 6, 8,36, 4,
12,12, 6,12,50,12, 6, 4, 6, 6, 8, 6,10, 2,10, 2,18,10,14,16, 8, 6, 4,20, 4,
 2,10, 6,14,18,10,38,10,18, 2,10, 2,12, 4, 2, 4,14, 6,10, 8,40, 6,20, 4,12,
 8, 6,34, 8,22, 8,12,10, 2,16,42,12, 8,22, 8,22, 8, 6,34, 2, 6, 4,14, 6,16,
 2,22, 6, 8,24,22, 6, 2,12, 4, 6,14, 4, 8,24, 4, 6, 6, 2,22,20, 6, 4,14, 4,
 6, 6, 8, 6,10, 6, 8, 6,16,14, 6, 6,22, 6,24,32, 6,18, 6,18,10, 8,30,18, 6,
16,12, 6,12, 2, 6, 4,12, 8, 6,22, 8, 6, 4,14,10,18,20,10, 2, 6, 4, 2,28,18,
 2,10, 6, 6, 6,14,40,24, 2, 4, 8,12, 4,20, 4,32,18,16, 6,36, 8, 6, 4, 6,14,
 4, 6,26, 6,10,14,18,10, 6, 6,14,10, 6, 6,14, 6,24, 4,14,22, 8,12,10, 8,12,
18,10,18, 8,24,10, 8, 4,24, 6,18, 6, 2,10,30, 2,10, 2, 4, 2,40, 2,28, 8, 6,
 6,18, 6,10,14, 4,18,30,18, 2,12,30, 6,30, 4,18,12, 2, 4,14, 6,10, 6, 8, 6,
10,12, 2, 6,12,10, 2,18, 4,20, 4, 6,14, 6, 6,22, 6, 6, 8,18,18,10, 2,10, 2,
 6, 4, 6,12,18, 2,10, 8, 4,18, 2, 6, 6, 6,10, 8,10, 6,18,12, 8,12, 6, 4, 6,
14,16, 2,12, 4, 6,38, 6, 6,16,20,28,20,10, 6, 6,14, 4,26, 4,14,10,18,14,28,
 2, 4,14,16, 2,28, 6, 8, 6,34, 8, 4,18, 2,16, 8, 6,40, 8,18, 4,30, 6,12, 2,
30, 6,10,14,40,14,10, 2,12,10, 8, 4, 8, 6, 6,28, 2, 4,12,14,16, 8,30,16,18,
 2,10,18, 6,32, 4,18, 6, 2,12,10,18, 2, 6,10,14,18,28, 6, 8,16, 2, 4,20,10,
 8,18,10, 2,10, 8, 4, 6,12, 6,20, 4, 2, 6, 4,20,10,26,18,10, 2,18, 6,16,14,
 4,26, 4,14,10,12,14, 6, 6, 4,14,10, 2,30,18,22, 2,16, 2, 4, 8, 6, 6,16, 2,
 6,12,10, 8,12, 4,14, 4, 6,20,10,12, 2, 6, 6, 4, 2,10, 2,30,16,12,20,18, 4,
 6, 2, 4, 8,16,14,18,22, 6, 2,22, 6, 6,18, 2,10,36, 8, 4, 6,20, 4,12, 6,14,
 4, 2,28,24, 8, 4, 6,12,30,18,32,22, 8,36, 6, 4,12, 2,12, 4, 6,20,10,18,18,
 8, 6, 4,24, 8,10,14, 6, 4, 8,12,16, 2,16, 6, 8,16,12,14,10,30,14, 4,12, 8,
12, 6,10, 2,12,28, 6,12,12,20,10, 2,10,14, 6, 6,30, 4, 8,12, 4, 2,10,14, 4,
26,18,12,10, 6, 8, 4,12, 6,24,18, 8,10, 2,12, 4,12,12, 6, 2,22, 2, 4, 2,12,
16,14,10, 2,16,18,32, 4, 6,20,22, 8,10, 2,10, 6, 2, 4,14, 6,24, 4, 8, 4, 6,
12,12, 8, 6,10,12, 8,10, 2,10,12, 6,12,12,20,28,20,10,14,10, 8,10, 6, 2, 4,
14, 6, 6,12, 6,12,10,14,10,14,16, 8,10,26, 4, 2, 6, 4,14, 4, 6,12, 8, 6,30,
18,12, 6,12,16,12,12, 2,28, 6,14,10,36, 2, 4, 6, 8,12,22,18, 2,30,18,22,20,
18,10,38, 6, 4, 2,24, 4, 6, 6, 2,10, 6,14,10, 8, 4,24,14,16,14,22, 6,20,10,
14, 4,12,12, 2,16, 8, 6, 6,18, 4, 6,14,22, 6, 2,42,16, 2,10, 6, 2, 4, 6, 8,
10,20,16,30, 8,10, 8,10, 2,30, 6, 6,36,10, 8,16, 6, 2,12,28, 2, 4, 6,18,12,
 6, 8,10, 2, 4,50, 4,20, 4,30, 8, 4, 6,12, 2,24, 4, 8,18, 6, 4, 6, 8,10, 2,
 4, 2,40,18,36,30,30, 8,16,14, 6,12,28, 2,22, 2, 4,12,30,12, 6, 2, 4,14,10,
 2,18,22,12,18, 2,10,18,32, 6, 4, 2, 6,10,20,12,10, 6,12,20,12, 6, 4, 2,16,
 2,16, 6,14, 4, 2,16, 2, 6,16, 6, 8, 4, 8,22,18, 8,12, 4, 8, 6,24,22, 6, 2,
12,30, 6,10,12, 6, 2,22, 6, 2,12, 6,22, 8,12,22, 2,10, 6,18,12, 2, 6,12,18,
 6, 4,20,22, 8,12,24,16,14,10,30,18, 2, 6, 4,14,10, 2,12,10,12, 6, 2,16,12,
 2, 6,12,10, 2,10, 6, 2,12,12,16,20,10,12, 8,30,10,14, 4, 6, 8, 6, 4,20,18,
24, 4,12, 8, 4, 2,24, 6,24,10, 2, 4, 6, 2, 6, 6, 6, 4,24, 2,10,12, 2, 6,10,
 8, 6,10,18, 2, 6, 4,20,24,10,12, 2,12, 6,24, 4,36,14,16, 8,22, 6, 8, 4, 2,
 6,22,20,16,12,18, 2,12,16, 6, 6,12, 6,12, 2, 6,12,10, 8,16, 8, 6,16, 8,12,
 4, 6, 6,20,12,12, 4, 6,20, 4,12, 2,10, 2, 6,30,22, 6, 2, 4,38,10, 2, 4, 2,
22, 2,16, 2, 6,10,20, 6,24, 4,12,14,12, 4,38,10,30, 6, 2,12,12, 4, 6,30,14,
 4, 8,18,36, 4, 6,20, 4, 2,12,10, 2, 6,10,12, 6,12, 8, 6, 6,24, 4,30,20, 6,
36,10, 2,12, 6, 4, 8, 6, 4,12, 8, 6,12, 4, 6,14, 4,20,12, 4, 6,18, 2, 4,18,
 2,16,12,30, 6, 6, 8,40, 8,48, 6,16,18,14,12, 6,18, 4,20,10, 2, 6,10, 8,30,
 4,12,20, 6,12, 6, 6,34, 6, 6,18, 6, 8,10,12, 6, 8,10, 2, 4,24, 6, 8,22, 6,
 2,12, 6,10,12, 6,24, 6,14,12,36, 4,24, 2,10, 8,10, 6,14,10,32, 4, 8,10,12,
26,18, 4, 6,20, 4,20, 6,16, 6, 2,30,12, 6,10, 2, 6,10,12, 8, 4, 2, 6,10,12,
26,22, 8, 6, 4,14, 6, 6,30, 4, 6,14, 4, 2,28, 2, 6,22, 8, 4,18,18,18, 2,12,
 6, 4,20,10, 6, 6,14,10,12, 2,12,30,34,12, 8, 6, 4, 2,10, 2,16,12, 2,10, 8,
18,24, 6, 4,12,14, 4, 8, 4,14, 4, 6, 6,20, 6, 4, 8,18,52, 2, 4,12, 8, 4,38,
 4,26,24,16,12, 6, 2,12,12,16, 2, 6, 6, 4,12,14,16, 8,12,18,16, 6, 8,10, 6,
14,10,12, 2,10, 2, 4,24, 6,42,24, 8,10, 6, 6, 6, 2,12, 4,14, 6, 6,28, 6, 2,
10,12,12, 6,20, 4, 6,14, 4, 2,12,10,12,24, 6, 8, 6, 6, 4,24,12,20,16,14,30,
18, 6, 4,26,12, 4, 6, 2, 6, 4, 2,28, 8,40, 2,10, 8, 4,20, 6,18,10, 2, 4,44,
 6,18,12, 6, 4, 6, 2,22, 6,14,30,10,24, 2,10, 8,16,18, 2,18,22, 8,10, 6, 6,
14, 4, 8,18, 4, 2,18,18,18, 6, 4,24,18, 2,16, 6, 6,18,20,16,20, 4,14, 6, 4,
20,18,10, 2, 6,10,24, 2,10,24, 6, 6,24, 6,12, 2,28,12,14, 6, 6,12, 6,22,12,
12, 8,36, 4,12,14, 4,20,10,12,24, 2, 4, 6,12, 2, 4, 2,10,12,26, 6,16, 8, 4,
 8,10, 8, 6,34, 2,12,16,24, 6, 2,10, 2,18, 4, 8, 6,16, 6, 2, 6, 6, 6, 4,14,
 4,20, 6, 4,20, 6,12,22, 6, 2,10,12, 2, 6, 4, 8,12, 4,14,12,10,14, 4,12,26,
10,14, 4,26, 6,30, 4,18,18, 8, 6,16, 8,10,14,10, 8,10,20,22,20,16, 2,18, 6,
 4, 6, 6,12, 2,10,26, 4, 8,18,18, 6,18, 6, 4, 6,24, 6,20,34,26,10, 2,28,12,
 8,10,12, 2, 6,22, 2,12,16, 2, 6, 6,10,14,16,20, 6, 4,38, 6,10, 6, 8,16,42,
 2, 6, 4, 6, 6, 6,14,16,14, 4,20,10, 2, 4, 8,18,10,12,36, 2,10,42, 8, 4,20,
24,16, 8,22, 6, 8, 4, 2, 6,22, 6, 6, 8,28, 2,10,18,14, 6, 4,18, 8,10,14, 4,
12, 8,10,12,14, 4, 2,12,12, 4, 6,18,30,12,38, 6,12,10, 2,18,10,12, 8, 4, 8,
 6, 4, 2,24,12,18, 4, 2, 4, 2,58,12, 8,24,10, 2, 4, 6, 6,12, 2, 4,14, 6, 6,
16,12, 2, 4,32, 4,24, 6, 6, 8,10, 2,22,18,12,20, 6,30, 4,30, 6, 2, 4,14, 6,
 4,14,16, 2,12,10, 2, 6,12,12,10, 6, 8,22, 8,12,12, 6,16, 6,18,20,22,18, 2,
22, 2,16, 2,22,14,10,20,10,32, 4, 8,10, 6, 2,22, 6,12, 2, 6, 4, 2, 4,14,12,
24,10, 2,12,16, 2, 4, 6,14, 6,10,12, 2,16,14,34,12, 2, 6, 6, 6, 4,20,10,26,
12,12, 4, 2, 4, 8,10, 2, 4, 2,22, 6, 6,14, 4,18,12,26, 6,10, 8,16, 2, 4,20,
10, 6,42, 2,10, 6, 8,24,12, 6, 4, 6,12, 2,28, 8,12,18,18, 6,46, 8,10, 6,14,
 4, 2, 6, 4, 6,42, 8,10, 8,10, 2,18, 4, 6,12,12, 2, 4,20,10,12,12, 8, 4,26,
18,22, 8, 6,16,14,16, 2,18,10, 2, 6, 6,10,14, 4, 2,30, 4, 2, 4, 8,10, 6, 2,
12,16, 6,56,10, 2,12,10, 8,12, 6, 4,14,10, 2, 4, 8, 6, 4,20, 6,12,22, 6,32,
10, 2,10,12,14, 6,28,36, 6, 6, 2,12, 4, 6, 6, 8,22, 2,18,10, 2, 6, 4,20,10,
 8, 4, 6,14,18, 6,42,22, 2, 4, 2,28, 2, 4,18, 6, 6, 6,12, 2,24,10,36, 6, 2,
12,10,26,24,18,16, 6, 6,14,24,12, 4, 8, 6,12, 4, 8,16,20,40,26, 4,12, 2, 6,
 4, 2,10,14,10, 2, 4,26,12,28, 2,16,26, 6,10, 2, 6,10, 6, 8, 6, 6, 6,10,12,
 6,20,40,20, 4, 2,16,12, 6,12, 8, 4,18, 2,12,10,26,12,16, 2,18,24,12, 4,14,
22,20,10,14,12, 4,18,12, 8,10,12, 6,30,14, 4,24, 6,30, 6, 6, 2, 6,22,32, 6,
 4, 6, 6,20,16, 2,10, 8,12,10, 2, 6,10, 8,16,36, 8, 6, 4, 2,28, 2,28,12, 2,
10, 6,14,10, 6, 6, 6, 8, 6, 4,14,18, 4, 6,12, 2,10,18, 8,30,40, 2,18, 4, 6,
14,18, 6, 4,12, 6,12, 6,14,10,26, 6,16, 2,16,30, 2,10, 2,42, 6,28,14, 6,10,
 2,12,18,12, 6,10,12,12,20, 6, 4, 2,10, 6,12,12,14,12,34, 6, 2,12,10, 6, 8,
 6, 4,12,38, 6,10,18, 2,28, 2, 6,12,30,16, 2,10, 8, 4, 2,16,18,26, 4, 6, 8,
18,22, 6,20, 4, 6,12, 2, 6,12, 4,18, 6, 2,22,12, 8, 6,16,18,30,12,24, 2,10,
 2, 6, 6, 4, 6,36,14, 6,22, 2,58, 8,12, 6,10, 2,40, 8, 6,28, 2, 4,14, 6, 6,
18,10, 8, 4,14, 4, 8,30, 4, 6, 8, 6, 6,18, 4, 2, 4,14,12,18,10, 2, 4,12, 2,
10, 8,10,14,10,18,12, 8, 6,10,14,10, 8,22, 2, 6,22,12, 6, 8,12,28, 2,48,12,
 4,18, 8,10,14,10,14, 4,12,30,24, 6, 8, 6, 4, 8,54, 4, 2,10,12, 8,10,12,12,
18, 2,24, 4, 8,22,12,20, 4,12, 2,12,16, 2,28, 2, 6,24,10, 2,28, 2, 4,20, 4,
12, 6,14, 4, 6,14,22,24,20, 4,14, 6, 6,10,30, 8,10,18, 2, 6, 6,16, 2, 6, 6,
 4, 2,24, 4, 2,24,10, 6, 2,10, 2, 6,22, 8, 4, 8, 6, 4,18, 2,18, 4, 8,16,26,
 4, 6, 8,22,20,16, 8, 4, 6,24, 6,14,12,16, 2,12, 4,14,10, 2, 4,12,18,32,10,
14,24,12,40, 8,34,12,14, 4,18, 2,28,12,20, 6,10, 2,40,18,14,12, 4,36, 6, 2,
22, 6,14,10,24,42, 2,16, 2,34, 8, 6, 4, 2, 4,14,40, 8,12, 6,24,18, 4, 6, 2,
 6, 4, 2, 4, 2,24,10, 8, 6, 6,10,14, 6,16,18,14,18,24, 4, 6, 6, 8, 4,20,10,
 6,12, 2,12, 4,14, 6, 6, 6, 4,14,16,36,14, 6, 4,14, 4, 6,24, 8, 4,20,10,14,
12,34, 8,10, 6, 6, 6,14, 4,14,12, 6,10,18,14,10,12, 6, 2, 6, 6,28, 2, 4,24,
 6, 2, 4, 8,16, 6,20, 4, 2,10, 2,10, 8,64, 6, 8,12, 4,14,12,10, 2,12, 6,10,
18,24, 6, 2,10, 8, 6,16,20, 4,14, 6, 6,12, 6, 4, 6, 2, 4, 8,22, 6, 8, 4, 2,
16,18,14, 6,22,14,10,14, 4, 6, 2, 4,14,10,12, 8,16, 8,10, 8,24,40, 6,12, 2,
 6,18, 4, 2, 4,30, 2,30, 4, 8,18,12,12, 4, 2, 4,14,36,16,18, 2,12,10, 6,12,
18, 2,18, 6, 6,22,18,38, 6,10,18, 2,10, 8, 6,16,24,14, 6, 4, 6,14,16,24, 6,
12, 8,12,10,14,46, 2,16, 2,22, 6, 2,10, 2,10, 2, 6, 4,20,10, 6,30, 8, 6, 6,
 4,30, 8, 6, 6, 6,22,36, 2, 4, 8, 6, 6, 4,14,12,10,20, 4, 2, 4,30, 6,14,16,
12,30, 2, 4, 6, 8,30,10, 8,34,18,12, 8,22,20, 4,14,10,20, 6, 4, 2,10,14, 4,
26, 6,36,12,18, 4, 8, 6, 4, 6, 2,28, 6, 6,24, 8,10,26, 6,24, 4, 8,24,10,20,
 4, 2,10,14,16, 2, 6, 6, 4, 6, 8,18,28,14, 6,16,14, 6, 4, 6, 6, 8, 4, 2, 4,
12, 2,12, 6,12,28, 2, 6,12,10,14, 4,44, 6,10, 2,12,12,30, 4,12, 2, 6,10,12,
 2,10, 2,10, 6, 8,10, 6,14,16, 8, 6,12,10, 2,10, 8,12,10,18, 8, 4, 2, 4,26,
 6,22, 6,14,10, 6, 2,28, 6, 8,46, 6, 6,18, 6, 6, 8, 6,10,18, 2, 6,12,18,10,
 8,12,30,10, 2,10, 2, 4, 6,18, 2, 4,20,12, 4, 6, 8,34, 6, 6,24,12, 8,36,16,
 2, 6, 4, 2, 4, 6,20, 6,24, 4, 2, 4,18,20, 6,22, 8,46,18, 2,16,20,22, 2,24,
22, 2,16,24,20,16, 2, 4, 8,10, 2,10,14, 4, 8,18, 4, 8, 4,14,10, 2,24,16, 8,
 6,16,20,10, 2, 6, 4,30, 2,16,32, 6,12,10,24, 8,12,18,16, 2,12, 6, 4,12, 6,
 2,28,18, 2,22, 6, 6, 6, 2, 6,16,14, 6,30,16, 2,10, 2, 4,12, 2,12,10,14, 6,
10, 8,28, 2,36, 6,16,14, 4,20,24, 6, 4, 8, 4,18, 8, 4,14, 4, 6, 2,24,16,14,
 4,26,16, 2,10,32, 6, 4, 6,12, 6,36, 8,12, 4, 2, 4, 8, 6, 4,20,12,10,24,12,
 2,12,10, 6,12, 2, 6,18, 4, 6, 6, 6, 8,24, 6,10,12,30,14,10, 8,12, 6,10,12,
 2,18, 6, 4, 8, 4,24,20, 4, 8,10,12, 8,12,16, 6,14, 4, 8, 4,18,50, 6, 6, 4,
 6, 8, 6,10,26,10, 6, 2,10, 2,10, 6,38,12, 4, 8,10,20, 6, 6, 6,18,10, 2,12,
16, 2,12,12, 4,26,10, 6,20,18,40,12, 8,10,12, 2,18,12,10, 2,10,26, 4, 6,12,
 8, 4,30, 6, 2, 6,16,24,24,18,12,12, 8, 6, 4, 8,10, 8, 6, 4,20,10,26, 4,24,
 6, 2,12,42,18, 6, 4,26, 6,28, 6, 2,10, 8, 6, 6,10, 8,10, 2,22, 2, 4,20, 4,
 6,36,14, 4,20,22, 6,14, 6,10, 8, 4, 2, 4,14,18,34, 8,22,14,10,24, 6, 2,10,
 2, 6,10,26,18,10,18,24,18, 2,24,40, 2, 4, 6, 2, 6,10,26, 6,12,12, 6, 4,36,
 2,10,12,24, 2, 4, 8,10, 6, 2, 4,24, 2, 4,36, 2,22,14,24,18,42, 6,10, 2,24,
16,12, 2, 4, 2,10, 2,10, 8, 4,36, 8, 4,12,18, 6, 6,14,22, 2, 6,24, 6,10,24,
20,22, 6,14,36,28, 6, 8, 6,24, 6,12,28, 2,18, 4, 2, 4,20,22, 8,10, 2,18, 4,
 8,10,14,10, 6, 8, 6, 6,12,16,12,14,10,18, 2,10,24,24, 6,12, 2,22, 6,20,22,
 2, 4,12, 2, 6,36, 6,22, 6, 2,28,12,18, 2, 4,14, 6, 4, 2,10, 2,16, 2,10, 8,
 6,10,18,12, 6,14, 4, 6,18,12,26, 4, 6,14, 6,10,12, 2, 4, 2,10,24, 8,10,32,
10, 8,10, 6, 2,18,12,28,30, 2,18, 4, 6,14, 6, 4, 8,22, 8,30,18,10,26, 4, 2,
22, 8, 4, 8, 6, 4,26, 4,12,20,18, 6,12,10,18, 2, 4, 6, 2,12,28, 6,20, 6,16,
 8, 6, 6, 4, 6,20,12, 6, 4,20, 6,16, 6,32,10,18, 2
};
