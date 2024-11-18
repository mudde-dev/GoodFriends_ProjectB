using System;

namespace Models;

public enum AnimalKind { Dog, Cat, Rabbit, Fish, Bird };
public enum AnimalMood { Happy, Hungry, Lazy, Sulky, Buzy, Sleepy };
public interface IPet
{
    public Guid PetId { get; set; }

    public AnimalKind Kind { get; set; }
    public AnimalMood Mood { get; set; }
    public string Name { get; set; }

    public IFriend Friend { get; set; }
}


