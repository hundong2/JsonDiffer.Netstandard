namespace JsonDiffer
{
    public enum ChangeMode
    {
        Changed,
        Added,
        Removed,
        Same
    }
    public static class SignInformation
    {
        public const string Changed = "*";
        public const string Added = "+";
        public const string Same = "@";
        public const string Removed = "-";
    }
}
