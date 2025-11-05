
Actor actor = new Actor("Diego", "Teixeira", DateTime.Now,"Portugal",true);
Character character = new Character("Gerard","loris","dirigeant de la cafetaria", actor);
Console.WriteLine($"Le personnage de {character.FirstName} {character.LastName} est joué par {character.PlayedBy.FirstName} {character.PlayedBy.LastName}");
public class Character
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Description { get; set; }
    public Actor PlayedBy { get; set; }

    public Character(string firstName, string lastName, string description, Actor playedBy)
    {
        FirstName = firstName;
        LastName = lastName;
        Description = description;
        PlayedBy = playedBy;
    }
}
public class Actor
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Country { get; set; }
    public bool IsAlive { get; set; }

    public Actor(string firstName, string lastName, DateTime birthDate, string country, bool isAlive)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Country = country;
        IsAlive = isAlive;
    }
}

public class Episode
{
    public string Title { get; set; }
    public int DurationMinutes { get; set; }
    public int SequenceNumber { get; set; }
    public string Director { get; set; }
    public string Synopsis { get; set; }
    public List<Character> Characters { get; set; } = new List<Character>();
}