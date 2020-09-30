namespace Unity.GoQL
{
    struct Discrimator
    {
        public string type;
        public string value;
        public override string ToString()
        {
            return $"{type}:{value}";
        }
    }
}

