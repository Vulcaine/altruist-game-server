public class CharacterSessionContext
{
    public string CharacterId { get; set; }

    public CharacterSessionContext(string characterId)
    {
        CharacterId = characterId;
    }
}
