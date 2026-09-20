namespace Script
{
    public class GameState 
    {
        private static GameState instance;

        private GameState() { }

        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState(); 
                }
                return instance;
            }
        }
        
        public bool IsLevelClear { get; private set; }
        
        public void LevelClear()
        {
            IsLevelClear = true;
        }
        
        public void StartLevel()
        {
            IsLevelClear = false;
        }
    }
}