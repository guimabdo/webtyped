using System;

namespace ExternalLib.Models
{
    public class ExternalModel<TKey>
    {
        public TKey Id { get; set; }
        public int? NullNumber1 { get; set; }
        public bool? NullBool1 { get; set; }
    }

    public class ExternalModel : ExternalModel<int>
    {
        public string Name { get; set; }
    }
}


namespace ExternalLib.Models2
{
    public class ExternalModel2
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
