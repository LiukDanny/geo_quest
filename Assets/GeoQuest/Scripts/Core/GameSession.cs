public class GameSession
{
    public LevelConfig Location { get; private set; }

    public void SelectLocation(LevelConfig location) => Location = location;
}
