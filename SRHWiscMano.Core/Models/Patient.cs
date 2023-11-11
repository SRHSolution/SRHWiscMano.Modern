namespace SRHWiscMano.Core.Models
{
    public class Patient : IPatient
    {
        public static readonly Patient Empty = new(string.Empty, new int?(), "Unspecified");
        public string Id { get; }
        public int? Age { get; }
        public string Gender { get; }

        public Patient(string id, int? age, string gender)
        {
            this.Id = id;
            this.Age = age;
            this.Gender = gender;
        }
    }
}