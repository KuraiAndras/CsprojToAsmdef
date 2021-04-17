namespace OtherExternalDependency
{
    public sealed class Addition
    {
        public Addition(int a, int b)
        {
            A = a;
            B = b;
        }

        public int A { get; }
        public int B { get; }

        public int Execute() => A + B;
    }
}
