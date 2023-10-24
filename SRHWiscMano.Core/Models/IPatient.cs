namespace SRHWiscMano.Core.Models
{
    public interface IPatient
    {
        string Id { get; }

        int? Age { get; }

        string Gender { get; }
    }
}