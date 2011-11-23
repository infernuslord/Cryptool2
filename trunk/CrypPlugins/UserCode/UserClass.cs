using System.Numerics;

namespace Cryptool.Plugins.UserCode
{

    public class UserClass
    {
        private Cryptool.Plugins.UserCode.UserCode _userCode;
        public UserClass(Cryptool.Plugins.UserCode.UserCode userCode)
        {
            _userCode = userCode;
        }

        public void UserMethod()
        {
            //USERCODE//
        }

        public void ProgressChanged(double percentage)
        {
            _userCode.ProgressChanged(percentage, 1.0);
        }

        public object input1
        {
            get
            {
                return _userCode.Input1;
            }
        }

        public object input2
        {
            get
            {
                return _userCode.Input2;
            }
        }

        public object input3
        {
            get
            {
                return _userCode.Input3;
            }
        }

        public object input4
        {
            get
            {
                return _userCode.Input4;
            }
        }

        public object input5
        {
            get
            {
                return _userCode.Input5;
            }
        }

        public object output1
        {
            get
            {
                return _userCode.Output1;
            }
            set
            {
                _userCode.Output1 = value;
            }
        }

        public object output2
        {
            get
            {
                return _userCode.Output2;
            }
            set
            {
                _userCode.Output2 = value;
            }
        }

        public object output3
        {
            get
            {
                return _userCode.Output3;
            }
            set
            {
                _userCode.Output3 = value;
            }
        }

        public object output4
        {
            get
            {
                return _userCode.Output4;
            }
            set
            {
                _userCode.Output4 = value;
            }
        }

        public object output5
        {
            get
            {
                return _userCode.Output5;
            }
            set
            {
                _userCode.Output5 = value;
            }
        }
    }
}