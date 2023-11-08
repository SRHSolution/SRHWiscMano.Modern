namespace SRHWiscMano.Core.Models
{
    public class Patient : IPatient
    {
        public static readonly Patient Empty = new Patient(null, new int?(), "Unspecified");
        private readonly string id;
        private readonly int? age;
        private readonly string gender;

        public Patient(string id, int? age, string gender)
        {
            this.id = id;
            this.age = age;
            this.gender = gender;
        }

        public string Id => id;

        public int? Age => age;
        public string Gender => gender;
    }
}