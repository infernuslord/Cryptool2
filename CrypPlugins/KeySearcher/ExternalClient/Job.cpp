#include "Job.h"

Job::Job():
    Key(NULL)
{
}

Job::~Job()
{
    if(Key != NULL)
    {
        delete[] Key;
    }
}

