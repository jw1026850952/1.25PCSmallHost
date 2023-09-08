using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public class FaultRecordInfo
    {
        public FaultRecordInfo()
        { }

        #region Model
        private int _id;
        private string _subject;
        private string _childsubject;
        private string _faulttype;
        private string _fault;

        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 故障对象
        /// </summary>
        public string Subject
        {
            set { _subject = value; }
            get { return _subject; }
        }

        public string ChildSubject
        {
            set { _childsubject = value; }
            get { return _childsubject; }
        }

        public string FaultType
        {
            set { _faulttype = value; }
            get { return _faulttype; }
        }

        /// <summary>
        /// 故障内容
        /// </summary>
        public string Fault
        {
            set { _fault = value; }
            get { return _fault; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            FaultRecordInfo other = (FaultRecordInfo)obj;

            return _id == other._id &&
                   _subject == other._subject &&
                   _childsubject == other._childsubject &&
                   _faulttype == other._faulttype &&
                   _fault == other._fault;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _id;
                hash = hash * 23 + (_subject != null ? _subject.GetHashCode() : 0);
                hash = hash * 23 + (_childsubject != null ? _childsubject.GetHashCode() : 0);
                hash = hash * 23 + (_faulttype != null ? _faulttype.GetHashCode() : 0);
                hash = hash * 23 + (_fault != null ? _fault.GetHashCode() : 0);
                return hash;
            }
        }
    }



    public class FaultRecordEqualityComparer : IEqualityComparer<FaultRecordInfo>
    {
        public bool Equals(FaultRecordInfo x, FaultRecordInfo y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(FaultRecordInfo obj)
        {
            return obj.ID.GetHashCode();
        }
    }
}
