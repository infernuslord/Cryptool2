namespace Cryptool.P2P.Types
{
    public static class Delegates
    {
        #region Delegates

        public delegate void P2PConnectionStateChangeEventHandler(object sender, bool newState);
        public delegate void P2PTryConnectingStateChangeEventHandler(object sender, bool newState);

        public delegate void SystemJoined();
        public delegate void SystemLeft();

        #endregion
    }
}
