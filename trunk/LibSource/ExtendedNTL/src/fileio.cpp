
#include <NTL/fileio.h>

#include <string.h>

#include <NTL/new.h>

NTL_START_IMPL


void OpenWrite(ofstream& s, const char *name)
{
   s.open(name, ios::out);

   if (!s) {
      cerr << "open write error: " << name;
      Error("");
   }
}


void OpenRead(ifstream& s, const char *name)
{
   s.open(name, ios::in);
   if (!s) {
      cerr << "open read error: " << name;
      Error("");
   }
}

static char sbuf[256];

char *FileName(const char* stem, const char *ext)
{
   strcpy_s(sbuf, stem);
   strcat_s(sbuf, "-");
   strcat_s(sbuf, ext);

   return sbuf;
}

char *FileName(const char* stem, const char *ext, long d)
{
   strcpy_s(sbuf, stem);
   strcat_s(sbuf, "-");
   strcat_s(sbuf, ext);
   strcat_s(sbuf, "-");

   char dbuf[6];
   dbuf[5] = '\0';
   long i, dd;
   dd = d;
   for (i = 4; i >= 0; i--) {
      dbuf[i] = IntValToChar(dd % 10);
      dd = dd / 10;
   }

   strcat_s(sbuf, dbuf);

   return sbuf;
}

NTL_END_IMPL
