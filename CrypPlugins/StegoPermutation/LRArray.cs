using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.StegoPermutation
{
    class LRArray
    {
        private ulong[] fl_;  // Free indices Left (including current element) in bsearch interval
        private bool[] tg_;   // tags: tg[i]==true if and only if index i is free
        private ulong n_;    // total number of indices
        private ulong f_;    // number of free indices

        public LRArray(ulong n)
        {
            n_ = n;
            fl_ = new ulong[n_];
            tg_ = new bool[n_];
            free_all();
        }

        ~LRArray()
        {
            fl_ = null;
            tg_ = null;
        }

        public ulong num_free() { return f_; }
        public ulong num_set() { return n_ - f_; }
        public bool is_free(ulong i) { return tg_[i]; }
        public bool is_set(ulong i) { return !tg_[i]; }

        /// <summary>
        /// Set elements of fl[0,...,n-2] according to all indices free.
        /// The element fl[n-1] needs to be set to 1 afterwards.
        /// Work is O(n).
        /// </summary>
        private void init_rec(ulong i0, ulong i1)
        {
            if ((i1 - i0) != 0)
            {
                ulong t = (i1 + i0) / 2;
                init_rec(i0, t);
                init_rec(t + 1, i1);
            }
            fl_[i1] = i1 - i0 + 1;
        }

        /// <summary>
        /// Mark all indices as free.
        /// </summary>
        public void free_all()
        {
            f_ = n_;
            for (ulong j = 0; j < n_; ++j) tg_[j] = true;
            init_rec(0, n_ - 1);
            fl_[n_ - 1] = 1;
        }

        /// <summary>
        /// Mark all indices as set.
        /// </summary>
        public void set_all()
        {
            f_ = 0;
            for (ulong j = 0; j < n_; ++j) tg_[j] = false;
            for (ulong j = 0; j < n_; ++j) fl_[j] = 0;
        }

        /// <summary>
        /// Return the k-th ( 0 &lt;= k &lt; num_free() ) free index.
        /// Return ~0UL if k is out of bounds.
        /// Work is O(log(n)).
        /// </summary>
        public ulong get_free_idx(ulong k)
        {
            if (k >= num_free()) return ~0UL;

            ulong i0 = 0, i1 = n_ - 1;
            while (true)
            {
                ulong t = (i1 + i0) / 2;
                if ((fl_[t] == k + 1) && (tg_[t])) return t;

                if (fl_[t] > k)  // left:
                {
                    i1 = t;
                }
                else   // right:
                {
                    i0 = t + 1; k -= fl_[t];
                }
            }
        }

        /// <summary>
        /// Return the k-th ( 0 &lt;= k &lt; num_free() ) free index.
        /// Return ~0UL if k is out of bounds.
        /// Change the arrays and fl[] and tg[] reflecting that index i will be set afterwards.
        /// Work is O(log(n)).
        /// </summary>
        public ulong get_free_idx_chg(ulong k)
        {
            if (k >= num_free()) return ~0UL;

            --f_;

            ulong i0 = 0, i1 = n_ - 1;
            while (true)
            {
                ulong t = (i1 + i0) / 2;

                if ((fl_[t] == k + 1) && (tg_[t]))
                {
                    --fl_[t];
                    tg_[t] = false;
                    return t;
                }

                if (fl_[t] > k)  // left:
                {
                    --fl_[t];
                    i1 = t;
                }
                else    // right:
                {
                    i0 = t + 1; k -= fl_[t];
                }
            }
        }

        /// <summary>
        /// Return the k-th ( 0 &lt;= k &lt; num_set() ) set index.
        /// Return ~0UL if k is out of bounds.
        /// Work is O(log(n)).
        /// </summary>
        public ulong get_set_idx(ulong k)
        {
            if (k >= num_set()) return ~0UL;

            ulong i0 = 0, i1 = n_ - 1;
            while (true)
            {
                ulong t = (i1 + i0) / 2;
                // how many elements to the left are set:
                ulong slt = t - i0 + 1 - fl_[t];

                if ((slt == k + 1) && (tg_[t] == false)) return t;

                if (slt > k)  // left:
                {
                    i1 = t;
                }
                else   // right:
                {
                    i0 = t + 1; k -= slt;
                }
            }
        }

        /// <summary>
        /// Return the k-th ( 0 &lt;= k &lt; num_set() ) set index.
        /// Return ~0UL if k is out of bounds.
        /// Change the arrays and fl[] and tg[] reflecting that index i will be freed afterwards.
        /// Work is O(log(n)).
        /// </summary>
        public ulong get_set_idx_chg(ulong k)
        {
            if (k >= num_set()) return ~0UL;

            ++f_;

            ulong i0 = 0, i1 = n_ - 1;
            while (true)
            {
                ulong t = (i1 + i0) / 2;
                // how many elements to the left are set:
                ulong slt = t - i0 + 1 - fl_[t];

                if ((slt == k + 1) && (tg_[t] == false))
                {
                    ++fl_[t];
                    tg_[t] = true;
                    return t;
                }

                if (slt > k)  // left:
                {
                    ++fl_[t];
                    i1 = t;
                }
                else   // right:
                {
                    i0 = t + 1; k -= slt;
                }
            }
        }


        // The methods num_[FS][LR][IE](ulong i) return the number of
        // Free/Set indices Left/Right if (absolute) index i, Including/Excluding i.
        // Return ~0UL if i >= n.

        /// <summary>
        /// Return number of Free indices Left of (absolute) index i (Excluding i).
        /// Work is O(log(n)).
        /// </summary>
        public ulong num_FLE(ulong i)
        {
            if (i >= n_) { return ~0UL; }  // out of bounds

            ulong i0 = 0, i1 = n_ - 1;
            ulong ns = i;  // number of set element left to i (including i)
            while (true)
            {
                if (i0 == i1) break;

                ulong t = (i1 + i0) / 2;
                if (i <= t)  // left:
                {
                    i1 = t;
                }
                else   // right:
                {
                    ns -= fl_[t];
                    i0 = t + 1;
                }
            }

            return i - ns;
        }

        /// <summary>
        /// Return number of Free indices Left of (absolute) index i (Including i).
        /// </summary>
        public ulong num_FLI(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return num_FLE(i) + (tg_[i] ? 1UL : 0UL);
        }

        /// <summary>
        /// Return number of Free indices Right of (absolute) index i (Excluding i).
        /// </summary>
        public ulong num_FRE(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return num_free() - num_FLI(i);
        }

        /// <summary>
        /// Return number of Free indices Right of (absolute) index i (Including i).
        /// </summary>
        public ulong num_FRI(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return num_free() - num_FLE(i);
        }

        /// <summary>
        /// Return number of Set indices Left of (absolute) index i (Excluding i).
        /// </summary>
        public ulong num_SLE(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return i - num_FLE(i);
        }

        /// <summary>
        /// Return number of Set indices Left of (absolute) index i (Including i).
        /// </summary>
        public ulong num_SLI(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return i - num_FLE(i) + (!tg_[i] ? 1UL : 0UL);
        }

        /// <summary>
        /// Return number of Set indices Right of (absolute) index i (Excluding i).
        /// </summary>
        public ulong num_SRE(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return num_set() - num_SLI(i);
        }

        /// <summary>
        /// Return number of Set indices Right of (absolute) index i (Including i).
        /// </summary>
        public ulong num_SRI(ulong i)
        {
            if (i >= n_) { return ~0UL; }
            return num_set() - i + num_FLE(i);
        }
    }
}