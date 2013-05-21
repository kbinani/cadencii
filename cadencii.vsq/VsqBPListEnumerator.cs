using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace cadencii.vsq
{
    public class VsqBPListEnumerator : IEnumerator<KeyValuePair<int, int>>
    {
        public VsqBPListEnumerator(VsqBPList list)
        {
            list_ = list;
            Reset();
        }

        public KeyValuePair<int, int> Current { get { return GetCurrent(); } }

        object IEnumerator.Current { get { return GetCurrent(); } }

        public void Reset() { index_ = -1; }

        public bool MoveNext()
        {
            index_++;
            return index_ < list_.size();
        }

        public void Dispose() { }

        private KeyValuePair<int, int> GetCurrent()
        {
            return new KeyValuePair<int, int>(list_.getKeyClock(index_), list_.getElementA(index_));
        }

        private VsqBPList list_;
        private int index_;
    }
}
