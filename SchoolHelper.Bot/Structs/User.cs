namespace SchoolHelper.Bot.Structs
{
    public struct User
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public long Form { get; set; }
        public string FormLetter { get; set; }
        public bool IsBanned { get; set; }

        public bool CompletelyFilled()
        {
            return
                !string.IsNullOrEmpty(Name) &&
                !string.IsNullOrEmpty(Surname) &&
                Form > 0 &&
                !string.IsNullOrEmpty(FormLetter);
        }
    }
}