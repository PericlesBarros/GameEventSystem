namespace GameSystems.Events
{
    public interface IGameEventTarget
    {
        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods
            
        void Process(params object[] data);
        string ToString();
        bool IsValid(out string message);
        bool OnValidate();
        
        #endregion
    }
}